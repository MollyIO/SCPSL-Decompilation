using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissonance.Networking
{
	internal class BaseClientCollection<TPeer> : IClientCollection<TPeer>
	{
		protected readonly Log Log;

		protected readonly ClientIdCollection PlayerIds = new ClientIdCollection();

		protected readonly RoomClientsCollection<TPeer> ClientsInRooms = new RoomClientsCollection<TPeer>();

		private readonly Dictionary<ushort, ClientInfo<TPeer>> _clientsByPlayerId = new Dictionary<ushort, ClientInfo<TPeer>>();

		private readonly Dictionary<string, ClientInfo<TPeer>> _clientsByName = new Dictionary<string, ClientInfo<TPeer>>();

		private readonly List<string> _tmpRoomList = new List<string>();

		protected BaseClientCollection()
		{
			Log = Logs.Create(LogCategory.Network, GetType().Name);
		}

		public virtual void Stop()
		{
			List<ClientInfo<TPeer>> list = new List<ClientInfo<TPeer>>();
			GetClients(list);
			foreach (ClientInfo<TPeer> item in list)
			{
				RemoveClient(item);
			}
			Log.AssertAndLogError(!PlayerIds.Items.Any(), "E8313B54-97FE-43F6-BC8D-7E0D52D01C7A", "{0} player(s) were not properly removed from the session", PlayerIds.Items.Count());
			PlayerIds.Clear();
			int num = ClientsInRooms.ClientCount();
			Log.AssertAndLogError(num == 0, "441F07AE-A25F-4968-B028-DABA51794B45", "{0} player(s) were not properly removed from the session", num);
			ClientsInRooms.Clear();
			Log.AssertAndLogError(_clientsByPlayerId.Count == 0, "17F67420-9874-4A2E-ABDF-3EF0C4037378", "{0} player(s) were not properly removed from the session", _clientsByPlayerId.Count);
			_clientsByPlayerId.Clear();
		}

		protected virtual void OnAddedClient([NotNull] ClientInfo<TPeer> client)
		{
		}

		protected virtual void OnRemovedClient([NotNull] ClientInfo<TPeer> client)
		{
		}

		[NotNull]
		protected ClientInfo<TPeer> GetOrCreateClientInfo(ushort id, [NotNull] string name, CodecSettings codecSettings, [CanBeNull] TPeer connection)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			ClientInfo<TPeer> info;
			if (TryGetClientInfoById(id, out info))
			{
				return info;
			}
			info = new ClientInfo<TPeer>(name, id, codecSettings, connection);
			_clientsByPlayerId[id] = info;
			_clientsByName[name] = info;
			OnAddedClient(info);
			return info;
		}

		protected void RemoveClient([NotNull] ClientInfo<TPeer> client)
		{
			client.IsConnected = false;
			PlayerIds.Unregister(client.PlayerName);
			_clientsByPlayerId.Remove(client.PlayerId);
			_clientsByName.Remove(client.PlayerName);
			for (int num = client.Rooms.Count - 1; num >= 0; num--)
			{
				LeaveRoom(client.Rooms[num], client);
			}
			OnRemovedClient(client);
		}

		public bool TryGetClientInfoById(ushort player, out ClientInfo<TPeer> info)
		{
			return _clientsByPlayerId.TryGetValue(player, out info);
		}

		public bool TryGetClientInfoByName([CanBeNull] string name, out ClientInfo<TPeer> info)
		{
			if (name == null)
			{
				info = null;
				return false;
			}
			return _clientsByName.TryGetValue(name, out info);
		}

		public bool TryGetClientsInRoom(string room, out List<ClientInfo<TPeer>> clients)
		{
			return ClientsInRooms.TryGetClientsInRoom(room, out clients);
		}

		public bool TryGetClientsInRoom(ushort roomId, out List<ClientInfo<TPeer>> clients)
		{
			return ClientsInRooms.TryGetClientsInRoom(roomId, out clients);
		}

		protected void GetClients(List<ClientInfo<TPeer>> output)
		{
			foreach (ClientInfo<TPeer> value in _clientsByPlayerId.Values)
			{
				output.Add(value);
			}
		}

		[ContractAnnotation("=> true, info:notnull; => false, info:null")]
		protected bool TryFindClientByConnection(TPeer connection, [CanBeNull] out ClientInfo<TPeer> info)
		{
			foreach (ClientInfo<TPeer> value in _clientsByPlayerId.Values)
			{
				if (value != null && connection.Equals(value.Connection))
				{
					info = value;
					return true;
				}
			}
			info = null;
			return false;
		}

		protected void ClearRooms()
		{
			ClientsInRooms.Clear();
		}

		protected virtual void OnClientEnteredRoom([NotNull] ClientInfo<TPeer> client, string room)
		{
		}

		protected virtual void OnClientExitedRoom([NotNull] ClientInfo<TPeer> client, string room)
		{
		}

		protected void JoinRoom([NotNull] string room, [NotNull] ClientInfo<TPeer> client)
		{
			if (room == null)
			{
				throw new ArgumentNullException("room");
			}
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			ClientsInRooms.Add(room, client);
			if (client.AddRoom(room))
			{
				OnClientEnteredRoom(client, room);
			}
		}

		private void LeaveRoom([NotNull] string room, [NotNull] ClientInfo<TPeer> client)
		{
			if (room == null)
			{
				throw new ArgumentNullException("room");
			}
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			ClientsInRooms.Remove(room, client);
			if (client.RemoveRoom(room))
			{
				OnClientExitedRoom(client, room);
			}
		}

		public virtual void ProcessClientState([CanBeNull] TPeer source, ref PacketReader reader)
		{
			ClientInfo clientInfo = reader.ReadClientStateHeader();
			ClientInfo<TPeer> orCreateClientInfo = GetOrCreateClientInfo(clientInfo.PlayerId, clientInfo.PlayerName, clientInfo.CodecSettings, source);
			while (orCreateClientInfo.Rooms.Count > 0)
			{
				LeaveRoom(orCreateClientInfo.Rooms[orCreateClientInfo.Rooms.Count - 1], orCreateClientInfo);
			}
			_tmpRoomList.Clear();
			reader.ReadClientStateRooms(_tmpRoomList);
			for (int i = 0; i < _tmpRoomList.Count; i++)
			{
				JoinRoom(_tmpRoomList[i], orCreateClientInfo);
			}
			_tmpRoomList.Clear();
		}

		public virtual void ProcessDeltaChannelState(ref PacketReader reader)
		{
			bool joined;
			ushort peer;
			string name;
			reader.ReadDeltaChannelState(out joined, out peer, out name);
			ClientInfo<TPeer> info;
			if (!TryGetClientInfoById(peer, out info))
			{
				Log.Warn("Received a DeltaChannelState for an unknown peer");
			}
			else if (joined)
			{
				JoinRoom(name, info);
			}
			else
			{
				LeaveRoom(name, info);
			}
		}
	}
}
