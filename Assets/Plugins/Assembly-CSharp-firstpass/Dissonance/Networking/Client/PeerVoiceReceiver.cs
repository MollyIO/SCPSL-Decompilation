using System;
using System.Collections.Generic;
using Dissonance.Audio.Playback;
using Dissonance.Datastructures;
using Dissonance.Extensions;

namespace Dissonance.Networking.Client
{
	internal class PeerVoiceReceiver
	{
		private struct ChannelsMetadata
		{
			public readonly bool IsPositional;

			public readonly float AmplitudeMultiplier;

			public readonly ChannelPriority Priority;

			public ChannelsMetadata(bool isPositional, float amplitudeMultiplier, ChannelPriority priority)
			{
				IsPositional = isPositional;
				AmplitudeMultiplier = amplitudeMultiplier;
				Priority = priority;
			}

			public ChannelsMetadata CombineWith(ChannelsMetadata other)
			{
				return new ChannelsMetadata(IsPositional & other.IsPositional, Math.Max(AmplitudeMultiplier, other.AmplitudeMultiplier), (ChannelPriority)Math.Max((int)Priority, (int)other.Priority));
			}
		}

		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PeerVoiceReceiver).Name);

		private readonly string _name;

		private readonly EventQueue _events;

		private readonly Rooms _localListeningRooms;

		private readonly ConcurrentPool<byte[]> _byteArrPool;

		private readonly ConcurrentPool<List<RemoteChannel>> _channelListPool;

		private readonly ushort _localId;

		private readonly string _localName;

		private DateTime _lastReceiptTime;

		private ushort _remoteSequenceNumber;

		private uint _localSequenceNumber;

		private int _currentChannelSession;

		private readonly Dictionary<int, int> _expectedPerChannelSessions = new Dictionary<int, int>();

		private readonly List<int> _tmpCompositeIdBuffer = new List<int>();

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public bool Open { get; private set; }

		public PeerVoiceReceiver(string remoteName, ushort localId, string localName, EventQueue events, Rooms listeningRooms, ConcurrentPool<byte[]> byteArrPool, ConcurrentPool<List<RemoteChannel>> channelListPool)
		{
			_name = remoteName;
			_localId = localId;
			_localName = localName;
			_events = events;
			_localListeningRooms = listeningRooms;
			_byteArrPool = byteArrPool;
			_channelListPool = channelListPool;
		}

		public void CheckTimeout(DateTime utcNow, TimeSpan timeout)
		{
			if (Open && utcNow - _lastReceiptTime > timeout)
			{
				StopSpeaking();
			}
		}

		public void StopSpeaking()
		{
			Log.AssertAndThrowPossibleBug(Open, "E8A0D33E-8C74-45F9-AA8C-3889012498D7", "Attempted to stop speaking when not speaking");
			Open = false;
			_events.EnqueueStoppedSpeaking(Name);
		}

		private void StartSpeaking(ushort startSequenceNumber, int channelSession, DateTime utcNow)
		{
			Log.AssertAndThrowPossibleBug(!Open, "E8A0D33E-8C74-45F9-AA8C-3889012498D7", "Attempted to start speaking when already speaking");
			_currentChannelSession = channelSession;
			_remoteSequenceNumber = startSequenceNumber;
			_localSequenceNumber = 0u;
			_lastReceiptTime = utcNow;
			Open = true;
			_events.EnqueueStartedSpeaking(Name);
		}

		public void ReceivePacket(ref PacketReader reader, DateTime utcNow)
		{
			VoicePacketOptions options;
			ushort sequenceNumber;
			ushort numChannels;
			reader.ReadVoicePacketHeader2(out options, out sequenceNumber, out numChannels);
			if (!IsPacketFromPreviousSession(_currentChannelSession, options.ChannelSession))
			{
				List<RemoteChannel> list = _channelListPool.Get();
				bool allClosing;
				bool forceReset;
				ChannelsMetadata channelsMetadata;
				ReadChannels(ref reader, numChannels, out allClosing, out forceReset, out channelsMetadata, list);
				if (UpdateSpeakerState(allClosing, forceReset, options.ChannelSession, sequenceNumber, utcNow))
				{
					ArraySegment<byte> encodedAudioFrame = reader.ReadByteSegment().CopyTo(_byteArrPool.Get());
					_events.EnqueueVoiceData(new VoicePacket(Name, channelsMetadata.Priority, channelsMetadata.AmplitudeMultiplier, channelsMetadata.IsPositional, encodedAudioFrame, _localSequenceNumber, list));
				}
				if (Open && allClosing)
				{
					StopSpeaking();
				}
			}
		}

		private void ReadChannels(ref PacketReader reader, ushort numChannels, out bool allClosing, out bool forceReset, out ChannelsMetadata channelsMetadata, [NotNull] ICollection<RemoteChannel> channelsOut)
		{
			channelsMetadata = new ChannelsMetadata(true, 0f, ChannelPriority.None);
			allClosing = true;
			forceReset = true;
			channelsOut.Clear();
			_tmpCompositeIdBuffer.Clear();
			for (int i = 0; i < numChannels; i++)
			{
				ChannelBitField bitfield;
				ushort recipient;
				reader.ReadVoicePacketChannel(out bitfield, out recipient);
				RemoteChannel? remoteChannel = IsChannelToLocalPlayer(bitfield, recipient);
				if (remoteChannel.HasValue)
				{
					channelsOut.Add(remoteChannel.Value);
					int num = (int)bitfield.Type | (recipient << 8);
					_tmpCompositeIdBuffer.Add(num);
					channelsMetadata = channelsMetadata.CombineWith(new ChannelsMetadata(bitfield.IsPositional, bitfield.AmplitudeMultiplier, bitfield.Priority));
					allClosing &= bitfield.IsClosing;
					forceReset &= HasChannelSessionChanged(num, bitfield.SessionId);
				}
			}
			forceReset &= Open;
			RemoveChannelsExcept(_tmpCompositeIdBuffer);
			_tmpCompositeIdBuffer.Clear();
		}

		private bool HasChannelSessionChanged(int compositeId, int expectedValue)
		{
			bool flag = false;
			bool flag2 = false;
			int value;
			if (!_expectedPerChannelSessions.TryGetValue(compositeId, out value))
			{
				flag2 = true;
			}
			else if (value != expectedValue)
			{
				flag = true;
			}
			if (flag2 || flag)
			{
				_expectedPerChannelSessions[compositeId] = expectedValue;
			}
			return flag;
		}

		private RemoteChannel? IsChannelToLocalPlayer(ChannelBitField channel, ushort recipient)
		{
			PlaybackOptions options = new PlaybackOptions(channel.IsPositional, channel.AmplitudeMultiplier, channel.Priority);
			if (channel.Type == ChannelType.Player)
			{
				if (recipient != _localId)
				{
					return null;
				}
				return new RemoteChannel(_localName, ChannelType.Player, options);
			}
			if (channel.Type == ChannelType.Room)
			{
				string text = _localListeningRooms.Name(recipient);
				if (text == null)
				{
					return null;
				}
				return new RemoteChannel(text, ChannelType.Room, options);
			}
			throw Log.CreatePossibleBugException(string.Format("Unknown ChannelType variant '{0}'", channel.Type), "1884B2CF-35AA-46BD-93C7-80F14D8D25D8");
		}

		private void RemoveChannelsExcept([NotNull] List<int> keys)
		{
			int count = keys.Count;
			keys.Sort();
			foreach (KeyValuePair<int, int> expectedPerChannelSession in _expectedPerChannelSessions)
			{
				int num = keys.BinarySearch(0, count, expectedPerChannelSession.Key, Comparer<int>.Default);
				if (num < 0)
				{
					keys.Add(expectedPerChannelSession.Key);
				}
			}
			for (int i = count; i < keys.Count; i++)
			{
				_expectedPerChannelSessions.Remove(keys[i]);
			}
			keys.RemoveRange(count, keys.Count - count);
		}

		private bool UpdateSpeakerState(bool allClosing, bool forceReset, int channelSession, ushort sequenceNumber, DateTime utcNow)
		{
			if ((forceReset || _currentChannelSession != channelSession) && Open)
			{
				if (forceReset)
				{
				}
				StopSpeaking();
			}
			if (!allClosing && !Open)
			{
				StartSpeaking(sequenceNumber, channelSession, utcNow);
			}
			if (Open && !UpdateSequenceNumber(sequenceNumber, utcNow))
			{
				return false;
			}
			return Open;
		}

		private bool UpdateSequenceNumber(ushort sequenceNumber, DateTime utcNow)
		{
			int num = _remoteSequenceNumber.WrappedDelta(sequenceNumber);
			if (_localSequenceNumber + num < 0)
			{
				return false;
			}
			_localSequenceNumber = (uint)(_localSequenceNumber + num);
			_remoteSequenceNumber = sequenceNumber;
			_lastReceiptTime = utcNow;
			return true;
		}

		private static bool IsPacketFromPreviousSession(int currentChannelSession, int packetChannelSession)
		{
			int num = currentChannelSession.WrappedDelta(packetChannelSession, 4);
			return num < 0;
		}
	}
}
