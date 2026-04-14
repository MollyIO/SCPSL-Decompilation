using System;
using System.Collections.Generic;
using Dissonance.Threading;

namespace Dissonance.Networking.Client
{
	internal class VoiceSender<TPeer> where TPeer : struct
	{
		private struct ChannelDelta
		{
			public readonly bool Open;

			public readonly ChannelType Type;

			public readonly ChannelProperties Properties;

			public readonly ushort RecipientId;

			public readonly string RecipientName;

			public ChannelDelta(bool open, ChannelType type, ChannelProperties properties, ushort recipientId, string recipientName)
			{
				Open = open;
				Type = type;
				Properties = properties;
				RecipientId = recipientId;
				RecipientName = recipientName;
			}
		}

		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(VoiceSender<TPeer>).Name);

		private readonly ISendQueue<TPeer> _sender;

		private readonly ISession _session;

		private readonly IClientCollection<TPeer?> _peers;

		private readonly EventQueue _events;

		private readonly PlayerChannels _playerChannels;

		private readonly RoomChannels _roomChannels;

		private byte _channelSessionId;

		private readonly ReadonlyLockedValue<List<OpenChannel>> _openChannels = new ReadonlyLockedValue<List<OpenChannel>>(new List<OpenChannel>());

		private readonly ReadonlyLockedValue<List<ChannelDelta>> _deltas = new ReadonlyLockedValue<List<ChannelDelta>>(new List<ChannelDelta>());

		private readonly List<KeyValuePair<string, ChannelProperties>> _pendingPlayerChannels = new List<KeyValuePair<string, ChannelProperties>>();

		private ushort _sequenceNumber;

		private readonly HashSet<ClientInfo<TPeer?>> _tmpDestsSet = new HashSet<ClientInfo<TPeer?>>();

		private readonly List<ClientInfo<TPeer?>> _tmpDestsList = new List<ClientInfo<TPeer?>>();

		private bool _hadId;

		private int _noIdSendCount;

		public VoiceSender([NotNull] ISendQueue<TPeer> sender, [NotNull] ISession session, [NotNull] IClientCollection<TPeer?> peers, [NotNull] EventQueue events, [NotNull] PlayerChannels playerChannels, [NotNull] RoomChannels roomChannels)
		{
			if (sender == null)
			{
				throw new ArgumentNullException("sender");
			}
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			if (peers == null)
			{
				throw new ArgumentNullException("peers");
			}
			if (events == null)
			{
				throw new ArgumentNullException("events");
			}
			if (playerChannels == null)
			{
				throw new ArgumentNullException("playerChannels");
			}
			if (roomChannels == null)
			{
				throw new ArgumentNullException("roomChannels");
			}
			_sender = sender;
			_session = session;
			_peers = peers;
			_playerChannels = playerChannels;
			_roomChannels = roomChannels;
			_events = events;
			_playerChannels.OpenedChannel += OpenPlayerChannel;
			_playerChannels.ClosedChannel += ClosePlayerChannel;
			_roomChannels.OpenedChannel += OpenRoomChannel;
			_roomChannels.ClosedChannel += CloseRoomChannel;
			foreach (KeyValuePair<ushort, PlayerChannel> playerChannel in playerChannels)
			{
				OpenPlayerChannel(playerChannel.Value.TargetId, playerChannel.Value.Properties);
			}
			foreach (KeyValuePair<ushort, RoomChannel> roomChannel in roomChannels)
			{
				OpenRoomChannel(roomChannel.Value.TargetId, roomChannel.Value.Properties);
			}
			_events.PlayerJoined += OnPlayerJoined;
			_events.PlayerLeft += OnPlayerLeft;
		}

		public void Stop()
		{
			_playerChannels.OpenedChannel -= OpenPlayerChannel;
			_playerChannels.ClosedChannel -= ClosePlayerChannel;
			_roomChannels.OpenedChannel -= OpenRoomChannel;
			_roomChannels.ClosedChannel -= CloseRoomChannel;
			_events.PlayerJoined -= OnPlayerJoined;
			_events.PlayerLeft -= OnPlayerLeft;
			using (ReadonlyLockedValue<List<OpenChannel>>.Unlocker unlocker = _openChannels.Lock())
			{
				unlocker.Value.Clear();
			}
			using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker2 = _deltas.Lock())
			{
				unlocker2.Value.Clear();
			}
		}

		private void OnPlayerJoined([NotNull] string name, CodecSettings codecSettings)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			for (int num = _pendingPlayerChannels.Count - 1; num >= 0; num--)
			{
				KeyValuePair<string, ChannelProperties> keyValuePair = _pendingPlayerChannels[num];
				if (keyValuePair.Key == name)
				{
					OpenPlayerChannel(keyValuePair.Key, keyValuePair.Value);
					_pendingPlayerChannels.RemoveAt(num);
				}
			}
		}

		private void OnPlayerLeft([NotNull] string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			using (ReadonlyLockedValue<List<OpenChannel>>.Unlocker unlocker = _openChannels.Lock())
			{
				using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker2 = _deltas.Lock())
				{
					List<OpenChannel> value = unlocker.Value;
					List<ChannelDelta> value2 = unlocker2.Value;
					for (int i = 0; i < value2.Count; i++)
					{
						ChannelDelta d = value2[i];
						if (d.Type == ChannelType.Player && d.RecipientName == name)
						{
							ApplyChannelDelta(d, unlocker);
							value2.RemoveAt(i);
							i--;
						}
					}
					for (int num = value.Count - 1; num >= 0; num--)
					{
						OpenChannel openChannel = value[num];
						if (openChannel.Type == ChannelType.Player && openChannel.Name == name)
						{
							value.RemoveAt(num);
							if (!openChannel.IsClosing)
							{
								_pendingPlayerChannels.Add(new KeyValuePair<string, ChannelProperties>(openChannel.Name, openChannel.Config));
							}
						}
					}
				}
			}
		}

		private void OpenPlayerChannel([NotNull] string player, [NotNull] ChannelProperties config)
		{
			if (player == null)
			{
				throw new ArgumentNullException("player");
			}
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			ClientInfo<TPeer?> info;
			if (!_peers.TryGetClientInfoByName(player, out info))
			{
				_pendingPlayerChannels.Add(new KeyValuePair<string, ChannelProperties>(player, config));
				return;
			}
			using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker = _deltas.Lock())
			{
				unlocker.Value.Add(new ChannelDelta(true, ChannelType.Player, config, info.PlayerId, info.PlayerName));
			}
		}

		private void ClosePlayerChannel([NotNull] string player, [NotNull] ChannelProperties config)
		{
			if (player == null)
			{
				throw new ArgumentNullException("player");
			}
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			for (int num = _pendingPlayerChannels.Count - 1; num >= 0; num--)
			{
				if (_pendingPlayerChannels[num].Key == player && object.ReferenceEquals(config, _pendingPlayerChannels[num].Value))
				{
					_pendingPlayerChannels.RemoveAt(num);
				}
			}
			ClientInfo<TPeer?> info;
			if (!_peers.TryGetClientInfoByName(player, out info))
			{
				return;
			}
			using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker = _deltas.Lock())
			{
				unlocker.Value.Add(new ChannelDelta(false, ChannelType.Player, config, info.PlayerId, info.PlayerName));
			}
		}

		private void OpenRoomChannel([NotNull] string room, [NotNull] ChannelProperties config)
		{
			if (room == null)
			{
				throw new ArgumentNullException("room");
			}
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker = _deltas.Lock())
			{
				unlocker.Value.Add(new ChannelDelta(true, ChannelType.Room, config, room.ToRoomId(), room));
			}
		}

		private void CloseRoomChannel([NotNull] string room, [NotNull] ChannelProperties config)
		{
			if (room == null)
			{
				throw new ArgumentNullException("room");
			}
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker = _deltas.Lock())
			{
				unlocker.Value.Add(new ChannelDelta(false, ChannelType.Room, config, room.ToRoomId(), room));
			}
		}

		private void OpenChannel(ChannelType type, [NotNull] ChannelProperties config, ushort recipient, [NotNull] string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			using (ReadonlyLockedValue<List<OpenChannel>>.Unlocker unlocker = _openChannels.Lock())
			{
				List<OpenChannel> value = unlocker.Value;
				bool flag = false;
				for (int i = 0; i < value.Count; i++)
				{
					OpenChannel openChannel = value[i];
					if (openChannel.Type == type && object.ReferenceEquals(openChannel.Config, config) && openChannel.Recipient == recipient)
					{
						value[i] = openChannel.AsOpen();
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					value.Add(new OpenChannel(type, 0, config, false, recipient, name));
				}
			}
		}

		private void CloseChannel(ChannelType type, [NotNull] ChannelProperties properties, ushort id)
		{
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			using (ReadonlyLockedValue<List<OpenChannel>>.Unlocker unlocker = _openChannels.Lock())
			{
				List<OpenChannel> value = unlocker.Value;
				for (int i = 0; i < value.Count; i++)
				{
					OpenChannel openChannel = value[i];
					if (!openChannel.IsClosing && openChannel.Type == type && openChannel.Recipient == id && object.ReferenceEquals(openChannel.Config, properties))
					{
						value[i] = openChannel.AsClosing();
					}
				}
			}
		}

		private void ClearClosedChannels()
		{
			using (ReadonlyLockedValue<List<OpenChannel>>.Unlocker unlocker = _openChannels.Lock())
			{
				List<OpenChannel> value = unlocker.Value;
				for (int num = value.Count - 1; num >= 0; num--)
				{
					if (value[num].IsClosing)
					{
						value.RemoveAt(num);
					}
				}
			}
		}

		public void Send(ArraySegment<byte> encodedAudio)
		{
			if (encodedAudio.Array == null)
			{
				throw new ArgumentNullException("encodedAudio");
			}
			if (!_session.LocalId.HasValue)
			{
				if (_hadId)
				{
					Log.Warn("Attempted to send voice but client ID has been unset");
				}
				else if (_noIdSendCount++ >= 99 && _noIdSendCount % 50 == 0)
				{
					Log.Warn("Attempted to send voice before assigned a client ID by the host ({0} packets discarded so far)", _noIdSendCount);
				}
				return;
			}
			_hadId = true;
			_noIdSendCount = 0;
			using (ReadonlyLockedValue<List<OpenChannel>>.Unlocker unlocker = _openChannels.Lock())
			{
				List<OpenChannel> value = unlocker.Value;
				ApplyChannelDeltas(unlocker);
				if (value.Count == 0)
				{
					return;
				}
				List<ClientInfo<TPeer?>> voiceDestinations = GetVoiceDestinations(value);
				if (voiceDestinations.Count > 0)
				{
					ArraySegment<byte> written = new PacketWriter(_sender.SendBufferPool.Get()).WriteVoiceData(_session.SessionId, _session.LocalId.Value, _sequenceNumber, _channelSessionId, value, encodedAudio).Written;
					_sequenceNumber++;
					_sender.EnqueueUnreliableP2P(_session.LocalId.Value, voiceDestinations, written);
					ClearClosedChannels();
					if (value.Count == 0)
					{
						_channelSessionId++;
					}
					for (int i = 0; i < value.Count; i++)
					{
						value[i] = value[i].AsSent();
					}
				}
				voiceDestinations.Clear();
			}
		}

		[NotNull]
		private List<ClientInfo<TPeer?>> GetVoiceDestinations([NotNull] IList<OpenChannel> openChannels)
		{
			_tmpDestsSet.Clear();
			_tmpDestsList.Clear();
			for (int i = 0; i < openChannels.Count; i++)
			{
				OpenChannel openChannel = openChannels[i];
				if (openChannel.Type == ChannelType.Player)
				{
					ClientInfo<TPeer?> info;
					if (_peers.TryGetClientInfoById(openChannel.Recipient, out info) && _tmpDestsSet.Add(info))
					{
						_tmpDestsList.Add(info);
					}
					continue;
				}
				if (openChannel.Type == ChannelType.Room)
				{
					List<ClientInfo<TPeer?>> clients;
					if (!_peers.TryGetClientsInRoom(openChannel.Recipient, out clients))
					{
						continue;
					}
					for (int j = 0; j < clients.Count; j++)
					{
						ClientInfo<TPeer?> item = clients[j];
						if (_tmpDestsSet.Add(item))
						{
							_tmpDestsList.Add(item);
						}
					}
					continue;
				}
				throw Log.CreatePossibleBugException(string.Format("Attempted to send to a channel with an unknown type '{0}'", openChannel.Type), "CF735F3F-F954-4F05-9C5D-5153AB1E30E7");
			}
			_tmpDestsSet.Clear();
			return _tmpDestsList;
		}

		private void ApplyChannelDeltas([NotNull] ReadonlyLockedValue<List<OpenChannel>>.Unlocker openChannels)
		{
			bool flag = AreAllChannelsClosing(openChannels);
			using (ReadonlyLockedValue<List<ChannelDelta>>.Unlocker unlocker = _deltas.Lock())
			{
				for (int i = 0; i < unlocker.Value.Count; i++)
				{
					ApplyChannelDelta(unlocker.Value[i], openChannels);
					if (!flag)
					{
						flag |= AreAllChannelsClosing(openChannels);
					}
				}
				unlocker.Value.Clear();
			}
			if (!AreAllChannelsClosing(openChannels) && flag)
			{
				_channelSessionId++;
			}
		}

		private static bool AreAllChannelsClosing([NotNull] ReadonlyLockedValue<List<OpenChannel>>.Unlocker openChannels)
		{
			if (openChannels.Value.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < openChannels.Value.Count; i++)
			{
				if (!openChannels.Value[i].IsClosing)
				{
					return false;
				}
			}
			return true;
		}

		private void ApplyChannelDelta(ChannelDelta d, ReadonlyLockedValue<List<OpenChannel>>.Unlocker openChannels)
		{
			if (d.Open)
			{
				OpenChannel(d.Type, d.Properties, d.RecipientId, d.RecipientName);
			}
			else
			{
				CloseChannel(d.Type, d.Properties, d.RecipientId);
			}
		}
	}
}
