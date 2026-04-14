using System;
using System.Collections.Generic;

namespace Dissonance.Networking.Server
{
	internal class BroadcastingClientCollection<TPeer> : BaseClientCollection<TPeer>
	{
		private readonly IServer<TPeer> _server;

		private readonly byte[] _tmpSendBuffer = new byte[1024];

		private readonly List<TPeer> _tmpConnectionBuffer = new List<TPeer>();

		private readonly List<ClientInfo<TPeer>> _tmpClientBuffer = new List<ClientInfo<TPeer>>();

		private readonly List<ClientInfo<TPeer>> _tmpClientBufferHandshake = new List<ClientInfo<TPeer>>();

		private readonly Dictionary<string, List<ClientInfo<TPeer>>> _tmpClientByRoomsBufferHandshake = new Dictionary<string, List<ClientInfo<TPeer>>>();

		public BroadcastingClientCollection(IServer<TPeer> server)
		{
			_server = server;
		}

		protected override void OnRemovedClient(ClientInfo<TPeer> client)
		{
			base.OnRemovedClient(client);
			PacketWriter packetWriter = new PacketWriter(_tmpSendBuffer);
			packetWriter.WriteRemoveClient(_server.SessionId, client.PlayerId);
			Broadcast(packetWriter.Written);
		}

		protected override void OnAddedClient(ClientInfo<TPeer> client)
		{
			base.OnAddedClient(client);
			_server.AddClient(client);
		}

		public void ProcessHandshakeRequest(TPeer source, ref PacketReader reader)
		{
			string name;
			CodecSettings codecSettings;
			reader.ReadHandshakeRequest(out name, out codecSettings);
			if (name == null)
			{
				Log.Warn("Ignoring a handshake with a null player name");
				return;
			}
			ClientInfo<TPeer> info;
			ClientInfo<TPeer> info2;
			if ((TryGetClientInfoByName(name, out info) | TryFindClientByConnection(source, out info2)) && !EqualityComparer<ClientInfo<TPeer>>.Default.Equals(info, info2))
			{
				if (info2 != null && info2.IsConnected)
				{
					RemoveClient(info2);
				}
				if (info != null && info.IsConnected)
				{
					RemoveClient(info);
				}
			}
			ushort? id = PlayerIds.GetId(name);
			ushort id2 = ((!id.HasValue) ? PlayerIds.Register(name) : id.Value);
			ClientInfo<TPeer> orCreateClientInfo = GetOrCreateClientInfo(id2, name, codecSettings, source);
			PacketWriter packetWriter = new PacketWriter(_tmpSendBuffer);
			_tmpClientBufferHandshake.Clear();
			packetWriter.WriteHandshakeResponse(_server.SessionId, orCreateClientInfo.PlayerId, _tmpClientBufferHandshake, _tmpClientByRoomsBufferHandshake);
			_server.SendReliable(source, packetWriter.Written);
			GetClients(_tmpClientBufferHandshake);
			for (int i = 0; i < _tmpClientBufferHandshake.Count; i++)
			{
				SendFakeClientState(source, _tmpClientBufferHandshake[i]);
			}
		}

		private void SendFakeClientState(TPeer destination, [NotNull] ClientInfo<TPeer> clientInfo)
		{
			PacketWriter packetWriter = new PacketWriter(_tmpSendBuffer);
			packetWriter.WriteClientState(_server.SessionId, clientInfo.PlayerName, clientInfo.PlayerId, clientInfo.CodecSettings, clientInfo.Rooms);
			_server.SendReliable(destination, packetWriter.Written);
		}

		public override void ProcessClientState(TPeer source, ref PacketReader reader)
		{
			Broadcast(reader.All);
			base.ProcessClientState(source, ref reader);
		}

		public override void ProcessDeltaChannelState(ref PacketReader reader)
		{
			Broadcast(reader.All);
			base.ProcessDeltaChannelState(ref reader);
		}

		private void Broadcast(ArraySegment<byte> packet)
		{
			_tmpConnectionBuffer.Clear();
			_tmpClientBuffer.Clear();
			GetClients(_tmpClientBuffer);
			for (int i = 0; i < _tmpClientBuffer.Count; i++)
			{
				ClientInfo<TPeer> clientInfo = _tmpClientBuffer[i];
				_tmpConnectionBuffer.Add(clientInfo.Connection);
			}
			_server.SendReliable(_tmpConnectionBuffer, packet);
			_tmpConnectionBuffer.Clear();
			_tmpClientBuffer.Clear();
		}

		public void RemoveClient(TPeer connection)
		{
			ClientInfo<TPeer> info;
			if (TryFindClientByConnection(connection, out info))
			{
				RemoveClient(info);
			}
		}
	}
}
