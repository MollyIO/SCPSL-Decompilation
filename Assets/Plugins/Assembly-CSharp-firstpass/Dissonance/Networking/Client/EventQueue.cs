using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Datastructures;
using Dissonance.Threading;

namespace Dissonance.Networking.Client
{
	internal class EventQueue
	{
		private enum EventType
		{
			PlayerJoined = 0,
			PlayerLeft = 1,
			PlayerEnteredRoom = 2,
			PlayerExitedRoom = 3,
			PlayerStartedSpeaking = 4,
			PlayerStoppedSpeaking = 5,
			VoiceData = 6,
			TextMessage = 7
		}

		private struct NetworkEvent
		{
			public readonly EventType Type;

			private string _playerName;

			private CodecSettings _codecSettings;

			private string _room;

			private ReadOnlyCollection<string> _allRooms;

			private readonly VoicePacket _voicePacket;

			private readonly TextMessage _textMessage;

			public string PlayerName
			{
				get
				{
					return _playerName;
				}
				set
				{
					_playerName = value;
				}
			}

			public CodecSettings CodecSettings
			{
				get
				{
					Check(EventType.PlayerJoined);
					return _codecSettings;
				}
				set
				{
					Check(EventType.PlayerJoined);
					_codecSettings = value;
				}
			}

			public string Room
			{
				get
				{
					Check(EventType.PlayerEnteredRoom, EventType.PlayerExitedRoom);
					return _room;
				}
				set
				{
					Check(EventType.PlayerEnteredRoom, EventType.PlayerExitedRoom);
					_room = value;
				}
			}

			[NotNull]
			public ReadOnlyCollection<string> AllRooms
			{
				get
				{
					Check(EventType.PlayerEnteredRoom, EventType.PlayerExitedRoom);
					return _allRooms;
				}
				set
				{
					Check(EventType.PlayerEnteredRoom, EventType.PlayerExitedRoom);
					_allRooms = value;
				}
			}

			public VoicePacket VoicePacket
			{
				get
				{
					Check(EventType.VoiceData);
					return _voicePacket;
				}
			}

			public TextMessage TextMessage
			{
				get
				{
					Check(EventType.TextMessage);
					return _textMessage;
				}
			}

			public NetworkEvent(EventType type)
			{
				Type = type;
				_playerName = null;
				_room = null;
				_allRooms = null;
				_codecSettings = default(CodecSettings);
				_voicePacket = default(VoicePacket);
				_textMessage = default(TextMessage);
			}

			public NetworkEvent(VoicePacket voice)
				: this(EventType.VoiceData)
			{
				_voicePacket = voice;
			}

			public NetworkEvent(TextMessage text)
				: this(EventType.TextMessage)
			{
				_textMessage = text;
			}

			private void Check(EventType type)
			{
			}

			private void Check(EventType typeA, EventType typeB)
			{
			}
		}

		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(EventQueue).Name);

		private readonly ReadonlyLockedValue<List<NetworkEvent>> _queuedEvents = new ReadonlyLockedValue<List<NetworkEvent>>(new List<NetworkEvent>());

		private readonly IRecycler<byte[]> _byteArrayPool;

		[NotNull]
		private readonly IRecycler<List<RemoteChannel>> _channelsListPool;

		private const int MinWarnPacketCountThreshold = 12;

		private static readonly TimeSpan MinWarnDispatchTimeThreshold = TimeSpan.FromMilliseconds(64.0);

		private int _voicePacketWarnThreshold = 12;

		private int _pendingVoicePackets;

		private DateTime _previousFlush = DateTime.MaxValue;

		public event Action<string, CodecSettings> PlayerJoined;

		public event Action<string> PlayerLeft;

		public event Action<RoomEvent> PlayerEnteredRoom;

		public event Action<RoomEvent> PlayerExitedRoom;

		public event Action<VoicePacket> VoicePacketReceived;

		public event Action<TextMessage> TextMessageReceived;

		public event Action<string> PlayerStartedSpeaking;

		public event Action<string> PlayerStoppedSpeaking;

		internal event Action<string> OnEnqueuePlayerLeft;

		public EventQueue([NotNull] IRecycler<byte[]> byteArrayPool, [NotNull] IRecycler<List<RemoteChannel>> channelsListPool)
		{
			if (byteArrayPool == null)
			{
				throw new ArgumentNullException("byteArrayPool");
			}
			if (channelsListPool == null)
			{
				throw new ArgumentNullException("channelsListPool");
			}
			_byteArrayPool = byteArrayPool;
			_channelsListPool = channelsListPool;
		}

		public bool DispatchEvents(DateTime? utcNow = null)
		{
			PreDispatchLog((!utcNow.HasValue) ? DateTime.UtcNow : utcNow.Value);
			bool flag = false;
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				List<NetworkEvent> value = unlocker.Value;
				for (int i = 0; i < value.Count; i++)
				{
					NetworkEvent networkEvent = value[i];
					switch (networkEvent.Type)
					{
					case EventType.PlayerJoined:
						flag |= InvokeEvent(networkEvent.PlayerName, networkEvent.CodecSettings, this.PlayerJoined);
						break;
					case EventType.PlayerLeft:
						flag |= InvokeEvent(networkEvent.PlayerName, this.PlayerLeft);
						break;
					case EventType.PlayerStartedSpeaking:
						flag |= InvokeEvent(networkEvent.PlayerName, this.PlayerStartedSpeaking);
						break;
					case EventType.PlayerStoppedSpeaking:
						flag |= InvokeEvent(networkEvent.PlayerName, this.PlayerStoppedSpeaking);
						break;
					case EventType.VoiceData:
						flag |= InvokeEvent(networkEvent.VoicePacket, this.VoicePacketReceived);
						_pendingVoicePackets--;
						if (networkEvent.VoicePacket.Channels != null)
						{
							networkEvent.VoicePacket.Channels.Clear();
							_channelsListPool.Recycle(networkEvent.VoicePacket.Channels);
						}
						_byteArrayPool.Recycle(networkEvent.VoicePacket.EncodedAudioFrame.Array);
						break;
					case EventType.TextMessage:
						flag |= InvokeEvent(networkEvent.TextMessage, this.TextMessageReceived);
						break;
					case EventType.PlayerEnteredRoom:
					{
						RoomEvent arg2 = CreateRoomEvent(networkEvent, true);
						flag |= InvokeEvent(arg2, this.PlayerEnteredRoom);
						break;
					}
					case EventType.PlayerExitedRoom:
					{
						RoomEvent arg = CreateRoomEvent(networkEvent, false);
						flag |= InvokeEvent(arg, this.PlayerExitedRoom);
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				value.Clear();
				return flag;
			}
		}

		private void PreDispatchLog(DateTime utcNow)
		{
			TimeSpan timeSpan = utcNow - _previousFlush;
			_previousFlush = utcNow;
			if (_pendingVoicePackets < _voicePacketWarnThreshold)
			{
				_voicePacketWarnThreshold = Math.Max(12, _voicePacketWarnThreshold - 1);
				return;
			}
			_voicePacketWarnThreshold *= 4;
			if (timeSpan > MinWarnDispatchTimeThreshold)
			{
				Log.Warn("Large number of received packets pending dispatch ({0}). Possibly due to long frame times (last frame was {1}ms)", _pendingVoicePackets, timeSpan.TotalMilliseconds);
			}
			else
			{
				Log.Warn("Large number of received packets pending dispatch ({0}). Possibly due to network congension (last frame was {1}ms)", _pendingVoicePackets, timeSpan.TotalMilliseconds);
			}
		}

		private static RoomEvent CreateRoomEvent(NetworkEvent @event, bool joined)
		{
			return new RoomEvent
			{
				PlayerName = @event.PlayerName,
				Room = @event.Room,
				Joined = joined,
				Rooms = @event.AllRooms
			};
		}

		private static bool InvokeEvent<T>(T arg, [CanBeNull] Action<T> handler)
		{
			try
			{
				if (handler != null)
				{
					handler(arg);
				}
			}
			catch (Exception p)
			{
				Log.Error("Exception invoking event handler: {0}", p);
				return true;
			}
			return false;
		}

		private static bool InvokeEvent<T1, T2>(T1 arg1, T2 arg2, [CanBeNull] Action<T1, T2> handler)
		{
			try
			{
				if (handler != null)
				{
					handler(arg1, arg2);
				}
			}
			catch (Exception p)
			{
				Log.Error("Exception invoking event handler: {0}", p);
				return true;
			}
			return false;
		}

		public void EnqueuePlayerJoined(string playerName, CodecSettings codecSettings)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(EventType.PlayerJoined)
				{
					PlayerName = playerName,
					CodecSettings = codecSettings
				});
			}
		}

		public void EnqueuePlayerLeft(string playerName)
		{
			if (this.OnEnqueuePlayerLeft != null)
			{
				this.OnEnqueuePlayerLeft(playerName);
			}
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(EventType.PlayerLeft)
				{
					PlayerName = playerName
				});
			}
		}

		public void EnqueuePlayerEnteredRoom([NotNull] string playerName, [NotNull] string room, [NotNull][ItemNotNull] ReadOnlyCollection<string> allRooms)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(EventType.PlayerEnteredRoom)
				{
					PlayerName = playerName,
					Room = room,
					AllRooms = allRooms
				});
			}
		}

		public void EnqueuePlayerExitedRoom([NotNull] string playerName, [NotNull] string room, [NotNull][ItemNotNull] ReadOnlyCollection<string> allRooms)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(EventType.PlayerExitedRoom)
				{
					PlayerName = playerName,
					Room = room,
					AllRooms = allRooms
				});
			}
		}

		public void EnqueueStartedSpeaking(string playerName)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(EventType.PlayerStartedSpeaking)
				{
					PlayerName = playerName
				});
			}
		}

		public void EnqueueStoppedSpeaking(string playerName)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(EventType.PlayerStoppedSpeaking)
				{
					PlayerName = playerName
				});
			}
		}

		public void EnqueueVoiceData(VoicePacket data)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				_pendingVoicePackets++;
				unlocker.Value.Add(new NetworkEvent(data));
			}
		}

		public void EnqueueTextData(TextMessage data)
		{
			using (ReadonlyLockedValue<List<NetworkEvent>>.Unlocker unlocker = _queuedEvents.Lock())
			{
				unlocker.Value.Add(new NetworkEvent(data));
			}
		}
	}
}
