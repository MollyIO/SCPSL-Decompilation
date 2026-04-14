using System;
using System.Collections.Generic;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking.Client;

namespace Dissonance.Networking
{
	public abstract class BaseClient<TServer, TClient, TPeer> : IClient<TPeer> where TServer : BaseServer<TServer, TClient, TPeer> where TClient : BaseClient<TServer, TClient, TPeer> where TPeer : struct
	{
		protected readonly Log Log;

		private bool _disconnected;

		private bool _error;

		private readonly EventQueue _events;

		private readonly SlaveClientCollection<TPeer> _peers;

		private readonly ConnectionNegotiator<TPeer> _serverNegotiator;

		private readonly SendQueue<TPeer> _sendQueue;

		private readonly PacketDelaySimulator _lossSimulator;

		private readonly VoiceReceiver<TPeer> _voiceReceiver;

		private readonly VoiceSender<TPeer> _voiceSender;

		private readonly TextReceiver<TPeer> _textReceiver;

		private readonly TextSender<TPeer> _textSender;

		private readonly TrafficCounter _recvRemoveClient = new TrafficCounter();

		private readonly TrafficCounter _recvVoiceData = new TrafficCounter();

		private readonly TrafficCounter _recvTextData = new TrafficCounter();

		private readonly TrafficCounter _recvHandshakeResponse = new TrafficCounter();

		private readonly TrafficCounter _recvHandshakeP2P = new TrafficCounter();

		private readonly TrafficCounter _recvClientState = new TrafficCounter();

		private readonly TrafficCounter _recvDeltaState = new TrafficCounter();

		private readonly TrafficCounter _sentServer = new TrafficCounter();

		public bool IsConnected
		{
			get
			{
				return !_error && !_disconnected && _serverNegotiator.State == ConnectionState.Connected;
			}
		}

		[NotNull]
		internal TrafficCounter RecvRemoveClient
		{
			get
			{
				return _recvRemoveClient;
			}
		}

		[NotNull]
		internal TrafficCounter RecvVoiceData
		{
			get
			{
				return _recvVoiceData;
			}
		}

		[NotNull]
		internal TrafficCounter RecvTextData
		{
			get
			{
				return _recvTextData;
			}
		}

		[NotNull]
		internal TrafficCounter RecvHandshakeResponse
		{
			get
			{
				return _recvHandshakeResponse;
			}
		}

		[NotNull]
		internal TrafficCounter RecvHandshakeP2P
		{
			get
			{
				return _recvHandshakeP2P;
			}
		}

		[NotNull]
		internal TrafficCounter RecvClientState
		{
			get
			{
				return _recvClientState;
			}
		}

		[NotNull]
		internal TrafficCounter RecvDeltaState
		{
			get
			{
				return _recvDeltaState;
			}
		}

		[NotNull]
		internal TrafficCounter SentServerTraffic
		{
			get
			{
				return _sentServer;
			}
		}

		public event Action<string, CodecSettings> PlayerJoined
		{
			add
			{
				_events.PlayerJoined += value;
			}
			remove
			{
				_events.PlayerJoined -= value;
			}
		}

		public event Action<string> PlayerLeft
		{
			add
			{
				_events.PlayerLeft += value;
			}
			remove
			{
				_events.PlayerLeft -= value;
			}
		}

		public event Action<RoomEvent> PlayerEnteredRoom
		{
			add
			{
				_events.PlayerEnteredRoom += value;
			}
			remove
			{
				_events.PlayerEnteredRoom -= value;
			}
		}

		public event Action<RoomEvent> PlayerExitedRoom
		{
			add
			{
				_events.PlayerExitedRoom += value;
			}
			remove
			{
				_events.PlayerExitedRoom -= value;
			}
		}

		public event Action<VoicePacket> VoicePacketReceived
		{
			add
			{
				_events.VoicePacketReceived += value;
			}
			remove
			{
				_events.VoicePacketReceived -= value;
			}
		}

		public event Action<TextMessage> TextMessageReceived
		{
			add
			{
				_events.TextMessageReceived += value;
			}
			remove
			{
				_events.TextMessageReceived -= value;
			}
		}

		public event Action<string> PlayerStartedSpeaking
		{
			add
			{
				_events.PlayerStartedSpeaking += value;
			}
			remove
			{
				_events.PlayerStartedSpeaking -= value;
			}
		}

		public event Action<string> PlayerStoppedSpeaking
		{
			add
			{
				_events.PlayerStoppedSpeaking += value;
			}
			remove
			{
				_events.PlayerStoppedSpeaking -= value;
			}
		}

		protected BaseClient([NotNull] ICommsNetworkState network)
		{
			if (network == null)
			{
				throw new ArgumentNullException("network");
			}
			Log = Logs.Create(LogCategory.Network, GetType().Name);
			ConcurrentPool<byte[]> concurrentPool = new ConcurrentPool<byte[]>(32, () => new byte[1024]);
			ConcurrentPool<List<RemoteChannel>> concurrentPool2 = new ConcurrentPool<List<RemoteChannel>>(32, () => new List<RemoteChannel>(8));
			_sendQueue = new SendQueue<TPeer>(this, concurrentPool);
			_serverNegotiator = new ConnectionNegotiator<TPeer>(_sendQueue, network.PlayerName, network.CodecSettings);
			_lossSimulator = new PacketDelaySimulator();
			_events = new EventQueue(concurrentPool, concurrentPool2);
			_peers = new SlaveClientCollection<TPeer>(_sendQueue, _serverNegotiator, _events, network.Rooms, network.PlayerName, network.CodecSettings);
			_peers.OnClientJoined += OnAddedClient;
			_peers.OnClientIntroducedP2P += OnMetClient;
			_voiceReceiver = new VoiceReceiver<TPeer>(_serverNegotiator, _peers, _events, network.Rooms, concurrentPool, concurrentPool2);
			_voiceSender = new VoiceSender<TPeer>(_sendQueue, _serverNegotiator, _peers, _events, network.PlayerChannels, network.RoomChannels);
			_textReceiver = new TextReceiver<TPeer>(_events, network.Rooms, _peers);
			_textSender = new TextSender<TPeer>(_sendQueue, _serverNegotiator, _peers);
		}

		public abstract void Connect();

		protected void Connected()
		{
			_serverNegotiator.Start();
		}

		public virtual void Disconnect()
		{
			if (!_disconnected)
			{
				_disconnected = true;
				_sendQueue.Stop();
				_serverNegotiator.Stop();
				_voiceReceiver.Stop();
				_voiceSender.Stop();
				_peers.Stop();
				_events.DispatchEvents();
				Log.Info("Disconnected");
			}
		}

		protected void FatalError(string reason)
		{
			Log.Error(reason);
			_error = true;
		}

		public virtual ClientStatus Update()
		{
			if (_disconnected)
			{
				return ClientStatus.Error;
			}
			if (!_error)
			{
				_error |= RunUpdate(DateTime.UtcNow);
			}
			if (_error)
			{
				Disconnect();
				return ClientStatus.Error;
			}
			return ClientStatus.Ok;
		}

		private bool RunUpdate(DateTime utcNow)
		{
			bool result = false;
			try
			{
				_serverNegotiator.Update(utcNow);
				ReadMessages();
				_sendQueue.Update();
				_voiceReceiver.Update(utcNow);
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);
				result = true;
			}
			finally
			{
				if (_events.DispatchEvents())
				{
					result = true;
				}
			}
			return result;
		}

		public void SendVoiceData(ArraySegment<byte> encodedAudio)
		{
			_voiceSender.Send(encodedAudio);
		}

		public void SendTextData(string data, ChannelType type, string recipient)
		{
			_textSender.Send(data, type, recipient);
		}

		public ushort? NetworkReceivedPacket(ArraySegment<byte> data)
		{
			if (_disconnected)
			{
				Log.Warn("Received a packet with a disconnected client, dropping packet");
				return null;
			}
			if (_lossSimulator.ShouldLose(data))
			{
				return null;
			}
			return ProcessReceivedPacket(data);
		}

		private ushort? ProcessReceivedPacket(ArraySegment<byte> data)
		{
			PacketReader reader = new PacketReader(data);
			MessageTypes messageType;
			if (!reader.ReadPacketHeader(out messageType))
			{
				Log.Warn("Discarding packet - incorrect magic number.");
				return null;
			}
			switch (messageType)
			{
			case MessageTypes.VoiceData:
				if (CheckSessionId(ref reader, messageType))
				{
					_voiceReceiver.ReceiveVoiceData(ref reader);
					_recvVoiceData.Update(reader.Read.Count);
				}
				break;
			case MessageTypes.TextData:
				if (CheckSessionId(ref reader, messageType))
				{
					_textReceiver.ProcessTextMessage(ref reader);
					_recvTextData.Update(reader.Read.Count);
				}
				break;
			case MessageTypes.HandshakeResponse:
				_serverNegotiator.ReceiveHandshakeResponseHeader(ref reader);
				_peers.ReceiveHandshakeResponseBody(ref reader);
				_recvHandshakeResponse.Update(reader.Read.Count);
				if (_serverNegotiator.LocalId.HasValue)
				{
					OnServerAssignedSessionId(_serverNegotiator.SessionId, _serverNegotiator.LocalId.Value);
				}
				break;
			case MessageTypes.RemoveClient:
				if (CheckSessionId(ref reader, messageType))
				{
					_peers.ProcessRemoveClient(ref reader);
					_recvRemoveClient.Update(reader.Read.Count);
				}
				break;
			case MessageTypes.ClientState:
				if (CheckSessionId(ref reader, messageType))
				{
					_peers.ProcessClientState(null, ref reader);
					_recvClientState.Update(reader.Read.Count);
				}
				break;
			case MessageTypes.DeltaChannelState:
				if (CheckSessionId(ref reader, messageType))
				{
					_peers.ProcessDeltaChannelState(ref reader);
					_recvDeltaState.Update(reader.Read.Count);
				}
				break;
			case MessageTypes.ErrorWrongSession:
			{
				uint num = reader.ReadUInt32();
				if (_serverNegotiator.SessionId != num)
				{
					FatalError(string.Format("Kicked from session - wrong session ID. Mine:{0} Theirs:{1}", _serverNegotiator.SessionId, num));
				}
				break;
			}
			case MessageTypes.HandshakeP2P:
				if (CheckSessionId(ref reader, messageType))
				{
					ushort peerId;
					reader.ReadhandshakeP2P(out peerId);
					_recvHandshakeP2P.Update(reader.Read.Count);
					return peerId;
				}
				break;
			case MessageTypes.HandshakeRequest:
			case MessageTypes.ServerRelayReliable:
			case MessageTypes.ServerRelayUnreliable:
				Log.Error("Client received packet '{0}'. This should only ever be received by the server", messageType);
				break;
			}
			return null;
		}

		private bool CheckSessionId(ref PacketReader reader, MessageTypes type)
		{
			uint num = reader.ReadUInt32();
			if (_serverNegotiator.SessionId != num)
			{
				Log.Warn("Received a '{0}' packet with incorrect session ID. Expected {1}, got {2}", type, _serverNegotiator.SessionId, num);
				return false;
			}
			return true;
		}

		protected abstract void ReadMessages();

		protected abstract void SendReliable(ArraySegment<byte> packet);

		protected abstract void SendUnreliable(ArraySegment<byte> packet);

		protected virtual void SendReliableP2P([NotNull] List<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet)
		{
			if (destinations.Count > 0)
			{
				SentServerTraffic.Update(packet.Count);
				byte[] array = _sendQueue.SendBufferPool.Get();
				PacketWriter packetWriter = new PacketWriter(array);
				packetWriter.WriteRelay(_serverNegotiator.SessionId, destinations, packet, true);
				((IClient<TPeer>)this).SendReliable(packetWriter.Written);
				_sendQueue.SendBufferPool.Put(array);
			}
		}

		protected virtual void SendUnreliableP2P([NotNull] List<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet)
		{
			if (destinations.Count > 0)
			{
				SentServerTraffic.Update(packet.Count);
				byte[] array = _sendQueue.SendBufferPool.Get();
				PacketWriter packetWriter = new PacketWriter(array);
				packetWriter.WriteRelay(_serverNegotiator.SessionId, destinations, packet, false);
				((IClient<TPeer>)this).SendUnreliable(packetWriter.Written);
				_sendQueue.SendBufferPool.Put(array);
			}
		}

		protected virtual void OnServerAssignedSessionId(uint session, ushort id)
		{
		}

		protected virtual void OnAddedClient([NotNull] ClientInfo<TPeer?> client)
		{
		}

		protected virtual void OnMetClient([NotNull] ClientInfo<TPeer?> client)
		{
			if (!Log.AssertAndLogError(IsConnected, "704E1AA4-1802-4FA6-B8BD-4CB780DD82F2", "Attempted to call IntroduceP2P before connected to Dissonance session") && !Log.AssertAndLogError(_serverNegotiator.LocalId.HasValue, "9B611EAA-B2D9-4C96-A619-976B61F5A76B", "No LocalId assigned even though server negotiator is connected") && client.Connection.HasValue)
			{
				ArraySegment<byte> written = new PacketWriter(_sendQueue.SendBufferPool.Get()).WriteHandshakeP2P(_serverNegotiator.SessionId, _serverNegotiator.LocalId.Value).Written;
				_sendQueue.EnqueueReliableP2P(_serverNegotiator.LocalId.Value, new List<ClientInfo<TPeer?>> { client }, written);
			}
		}

		protected void ReceiveHandshakeP2P(ushort id, TPeer connection)
		{
			if (!IsConnected)
			{
				Log.Error("Attempted to call IntroduceP2P before connected to Dissonance session");
			}
			else
			{
				_peers.IntroduceP2P(id, connection);
			}
		}

		[NotNull]
		protected static byte[] WriteHandshakeP2P(uint sessionId, ushort clientId)
		{
			ArraySegment<byte> written = new PacketWriter(new byte[9]).WriteHandshakeP2P(sessionId, clientId).Written;
			return written.ToArray();
		}

		void IClient<TPeer>.SendReliable(ArraySegment<byte> packet)
		{
			SendReliable(packet);
		}

		void IClient<TPeer>.SendUnreliable(ArraySegment<byte> packet)
		{
			SendUnreliable(packet);
		}

		void IClient<TPeer>.SendReliableP2P([NotNull] List<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet)
		{
			SendReliableP2P(destinations, packet);
		}

		void IClient<TPeer>.SendUnreliableP2P([NotNull] List<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet)
		{
			SendUnreliableP2P(destinations, packet);
		}
	}
}
