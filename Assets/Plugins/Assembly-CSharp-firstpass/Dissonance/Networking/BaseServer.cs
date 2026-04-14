using System;
using System.Collections.Generic;
using Dissonance.Networking.Server;

namespace Dissonance.Networking
{
	public abstract class BaseServer<TServer, TClient, TPeer> : IServer<TPeer> where TServer : BaseServer<TServer, TClient, TPeer> where TClient : BaseClient<TServer, TClient, TPeer> where TPeer : struct
	{
		protected readonly Log Log;

		private bool _disconnected;

		private bool _error;

		private readonly ServerRelay<TPeer> _relay;

		private readonly BroadcastingClientCollection<TPeer> _clients;

		private readonly uint _sessionId;

		internal TrafficCounter RecvHandshakeRequest { get; private set; }

		internal TrafficCounter RecvClientState { get; private set; }

		internal TrafficCounter RecvPacketRelay { get; private set; }

		internal TrafficCounter RecvDeltaChannelState { get; private set; }

		internal TrafficCounter SentTraffic { get; private set; }

		public uint SessionId
		{
			get
			{
				return _sessionId;
			}
		}

		protected BaseServer()
		{
			Log = Logs.Create(LogCategory.Network, GetType().Name);
			RecvClientState = new TrafficCounter();
			RecvHandshakeRequest = new TrafficCounter();
			RecvPacketRelay = new TrafficCounter();
			SentTraffic = new TrafficCounter();
			RecvDeltaChannelState = new TrafficCounter();
			Random random = new Random();
			while (_sessionId == 0)
			{
				_sessionId = (uint)random.Next();
			}
			_clients = new BroadcastingClientCollection<TPeer>(this);
			_relay = new ServerRelay<TPeer>(this, _clients);
			Log.Info("Constructing host. SessionId:{0}", _sessionId);
		}

		public virtual void Connect()
		{
			Log.Info("Connected");
		}

		public virtual void Disconnect()
		{
			if (!_disconnected)
			{
				_disconnected = true;
				_clients.Stop();
				Log.Info("Disconnected");
			}
		}

		protected void FatalError([NotNull] string reason)
		{
			Log.Error(reason);
			_error = true;
		}

		protected void ClientDisconnected(TPeer connection)
		{
			_clients.RemoveClient(connection);
		}

		public virtual ServerState Update()
		{
			if (_disconnected)
			{
				return ServerState.Error;
			}
			_error |= RunUpdate();
			if (_error)
			{
				Disconnect();
				return ServerState.Error;
			}
			return ServerState.Ok;
		}

		private bool RunUpdate()
		{
			try
			{
				ReadMessages();
				return false;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);
				return true;
			}
		}

		protected abstract void SendReliable(TPeer connection, ArraySegment<byte> packet);

		protected abstract void SendUnreliable(TPeer connection, ArraySegment<byte> packet);

		public virtual void SendUnreliable([NotNull] List<TPeer> connections, ArraySegment<byte> packet)
		{
			if (connections == null)
			{
				throw new ArgumentNullException("connections");
			}
			SentTraffic.Update(packet.Count * connections.Count);
			for (int i = 0; i < connections.Count; i++)
			{
				SendUnreliable(connections[i], packet);
			}
		}

		public virtual void SendReliable([NotNull] List<TPeer> connections, ArraySegment<byte> packet)
		{
			if (connections == null)
			{
				throw new ArgumentNullException("connections");
			}
			SentTraffic.Update(packet.Count * connections.Count);
			for (int i = 0; i < connections.Count; i++)
			{
				SendReliable(connections[i], packet);
			}
		}

		void IServer<TPeer>.SendReliable(TPeer connection, ArraySegment<byte> packet)
		{
			SentTraffic.Update(packet.Count);
			SendReliable(connection, packet);
		}

		void IServer<TPeer>.SendUnreliable(List<TPeer> connections, ArraySegment<byte> packet)
		{
			SendUnreliable(connections, packet);
		}

		void IServer<TPeer>.SendReliable(List<TPeer> connections, ArraySegment<byte> packet)
		{
			SendReliable(connections, packet);
		}

		protected abstract void ReadMessages();

		public void NetworkReceivedPacket(TPeer source, ArraySegment<byte> data)
		{
			if (_disconnected)
			{
				Log.Warn("Received a packet with a disconnected server, dropping packet");
				return;
			}
			PacketReader reader = new PacketReader(data);
			MessageTypes messageType;
			if (!reader.ReadPacketHeader(out messageType))
			{
				Log.Warn("Discarding packet - incorrect magic number.");
				return;
			}
			switch (messageType)
			{
			case MessageTypes.HandshakeRequest:
				RecvHandshakeRequest.Update(data.Count);
				_clients.ProcessHandshakeRequest(source, ref reader);
				break;
			case MessageTypes.ClientState:
				if (CheckSessionId(ref reader, source))
				{
					RecvClientState.Update(data.Count);
					_clients.ProcessClientState(source, ref reader);
				}
				break;
			case MessageTypes.ServerRelayReliable:
			case MessageTypes.ServerRelayUnreliable:
				if (CheckSessionId(ref reader, source))
				{
					RecvPacketRelay.Update(data.Count);
					_relay.ProcessPacketRelay(ref reader, messageType == MessageTypes.ServerRelayReliable);
				}
				break;
			case MessageTypes.DeltaChannelState:
				if (CheckSessionId(ref reader, source))
				{
					RecvDeltaChannelState.Update(data.Count);
					_clients.ProcessDeltaChannelState(ref reader);
				}
				break;
			case MessageTypes.VoiceData:
			case MessageTypes.TextData:
			case MessageTypes.HandshakeResponse:
			case MessageTypes.ErrorWrongSession:
			case MessageTypes.RemoveClient:
			case MessageTypes.HandshakeP2P:
				Log.Error("Server received packet '{0}'. This should only ever be received by the client", messageType);
				break;
			default:
				Log.Error("Ignoring a packet with an unknown header: '{0}'", messageType);
				break;
			}
		}

		private bool CheckSessionId(ref PacketReader reader, TPeer source)
		{
			uint num = reader.ReadUInt32();
			if (num != _sessionId)
			{
				Log.Warn("Received a packet with incorrect session ID. Expected {0}, got {1}. Resetting client.", _sessionId, num);
				PacketWriter packetWriter = new PacketWriter(new byte[7]);
				packetWriter.WriteErrorWrongSession(_sessionId);
				SendUnreliable(source, packetWriter.Written);
				return false;
			}
			return true;
		}

		protected virtual void AddClient(ClientInfo<TPeer> client)
		{
		}

		void IServer<TPeer>.AddClient(ClientInfo<TPeer> client)
		{
			AddClient(client);
		}
	}
}
