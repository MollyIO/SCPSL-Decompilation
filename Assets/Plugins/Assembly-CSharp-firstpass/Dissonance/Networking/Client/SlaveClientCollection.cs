using System;
using System.Collections.Generic;

namespace Dissonance.Networking.Client
{
	internal class SlaveClientCollection<TPeer> : BaseClientCollection<TPeer?> where TPeer : struct
	{
		private readonly ISendQueue<TPeer> _sender;

		private readonly ISession _session;

		private readonly EventQueue _events;

		private readonly Rooms _localRooms;

		private readonly string _playerName;

		private readonly CodecSettings _codecSettings;

		private readonly List<KeyValuePair<ushort, TPeer>> _pendingIntroductions = new List<KeyValuePair<ushort, TPeer>>();

		public event Action<ClientInfo<TPeer?>> OnClientJoined;

		public event Action<ClientInfo<TPeer?>> OnClientIntroducedP2P;

		public SlaveClientCollection([NotNull] ISendQueue<TPeer> sender, [NotNull] ISession session, [NotNull] EventQueue events, [NotNull] Rooms localRooms, [NotNull] string playerName, CodecSettings codecSettings)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			if (sender == null)
			{
				throw new ArgumentNullException("sender");
			}
			if (events == null)
			{
				throw new ArgumentNullException("events");
			}
			if (localRooms == null)
			{
				throw new ArgumentNullException("localRooms");
			}
			if (playerName == null)
			{
				throw new ArgumentNullException("playerName");
			}
			_session = session;
			_sender = sender;
			_events = events;
			_localRooms = localRooms;
			_playerName = playerName;
			_codecSettings = codecSettings;
		}

		protected override void OnAddedClient(ClientInfo<TPeer?> client)
		{
			_events.EnqueuePlayerJoined(client.PlayerName, client.CodecSettings);
			bool flag = false;
			for (int num = _pendingIntroductions.Count - 1; num >= 0; num--)
			{
				if (_pendingIntroductions[num].Key == client.PlayerId)
				{
					if (!flag)
					{
						bool flag2 = !client.Connection.HasValue;
						client.Connection = _pendingIntroductions[num].Value;
						if (flag2 && this.OnClientIntroducedP2P != null)
						{
							this.OnClientIntroducedP2P(client);
						}
						flag = true;
					}
					_pendingIntroductions.RemoveAt(num);
				}
			}
			if (this.OnClientJoined != null)
			{
				this.OnClientJoined(client);
			}
			base.OnAddedClient(client);
		}

		protected override void OnRemovedClient(ClientInfo<TPeer?> client)
		{
			if (client.PlayerName != _playerName)
			{
				_events.EnqueuePlayerLeft(client.PlayerName);
			}
			base.OnRemovedClient(client);
		}

		protected override void OnClientEnteredRoom(ClientInfo<TPeer?> client, string room)
		{
			_events.EnqueuePlayerEnteredRoom(client.PlayerName, room, client.Rooms);
		}

		protected override void OnClientExitedRoom(ClientInfo<TPeer?> client, string room)
		{
			_events.EnqueuePlayerExitedRoom(client.PlayerName, room, client.Rooms);
		}

		public void ProcessRemoveClient(ref PacketReader reader)
		{
			ushort clientId;
			reader.ReadRemoveClient(out clientId);
			ClientInfo<TPeer?> info;
			if (TryGetClientInfoById(clientId, out info))
			{
				RemoveClient(info);
			}
		}

		public void ReceiveHandshakeResponseBody(ref PacketReader reader)
		{
			Dictionary<string, List<ushort>> dictionary = new Dictionary<string, List<ushort>>();
			List<ClientInfo> list = new List<ClientInfo>();
			reader.ReadHandshakeResponseBody(list, dictionary);
			PlayerIds.Load(list);
			List<ClientInfo<TPeer?>> list2 = new List<ClientInfo<TPeer?>>();
			GetClients(list2);
			for (int i = 0; i < list2.Count; i++)
			{
				if (PlayerIds.GetName(list2[i].PlayerId) != list2[i].PlayerName)
				{
					RemoveClient(list2[i]);
				}
			}
			foreach (ClientInfo item in list)
			{
				GetOrCreateClientInfo(item.PlayerId, item.PlayerName, item.CodecSettings, null);
			}
			ClearRooms();
			foreach (KeyValuePair<string, List<ushort>> item2 in dictionary)
			{
				foreach (ushort item3 in item2.Value)
				{
					ClientInfo<TPeer?> info;
					if (!TryGetClientInfoById(item3, out info))
					{
						Log.Warn("Attempted to add an unknown client '{0}' into room '{1}'", item3, item2.Key);
					}
					else
					{
						JoinRoom(item2.Key, info);
					}
				}
			}
			SendClientState();
		}

		private void SendClientState()
		{
			ushort? localId = _session.LocalId;
			if (!Log.AssertAndLogError(localId.HasValue, "EBC361ED-780A-4DE0-944D-3D4D983B785D", "Attempting to send local client state before assigned an ID by the server"))
			{
				PacketWriter packetWriter = new PacketWriter(_sender.SendBufferPool.Get());
				packetWriter.WriteClientState(_session.SessionId, _playerName, localId.Value, _codecSettings, _localRooms);
				_sender.EnqueueReliable(packetWriter.Written);
				_localRooms.JoinedRoom -= SendJoinRoom;
				_localRooms.JoinedRoom += SendJoinRoom;
				_localRooms.LeftRoom -= SendLeaveRoom;
				_localRooms.LeftRoom += SendLeaveRoom;
			}
		}

		private void SendLeaveRoom(string room)
		{
			ushort? localId = _session.LocalId;
			if (!Log.AssertAndLogError(localId.HasValue, "7F29AD74-7F03-46BA-A776-F63F25A39FC5", "Attempted to send channel state delta, but local client ID is null"))
			{
				PacketWriter packetWriter = new PacketWriter(_sender.SendBufferPool.Get());
				packetWriter.WriteDeltaChannelState(_session.SessionId, false, localId.Value, room);
				_sender.EnqueueReliable(packetWriter.Written);
			}
		}

		private void SendJoinRoom(string room)
		{
			ushort? localId = _session.LocalId;
			if (!Log.AssertAndLogError(localId.HasValue, "73A33580-B876-4D16-9578-5FB417BA98F5", "Attempted to send channel state delta, but local client ID is null"))
			{
				PacketWriter packetWriter = new PacketWriter(_sender.SendBufferPool.Get());
				packetWriter.WriteDeltaChannelState(_session.SessionId, true, localId.Value, room);
				_sender.EnqueueReliable(packetWriter.Written);
			}
		}

		public override void Stop()
		{
			_localRooms.JoinedRoom -= SendJoinRoom;
			_localRooms.LeftRoom -= SendLeaveRoom;
			base.Stop();
		}

		public void IntroduceP2P(ushort id, TPeer connection)
		{
			if (!TryIntroduceP2P(id, connection))
			{
				_pendingIntroductions.Add(new KeyValuePair<ushort, TPeer>(id, connection));
			}
		}

		private bool TryIntroduceP2P(ushort id, TPeer connection)
		{
			ClientInfo<TPeer?> info;
			if (TryGetClientInfoById(id, out info))
			{
				bool flag = !info.Connection.HasValue;
				info.Connection = connection;
				if (flag && this.OnClientIntroducedP2P != null)
				{
					this.OnClientIntroducedP2P(info);
				}
				return true;
			}
			return false;
		}
	}
}
