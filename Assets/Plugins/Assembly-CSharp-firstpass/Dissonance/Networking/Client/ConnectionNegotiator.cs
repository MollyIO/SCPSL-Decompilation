using System;
using System.Threading;

namespace Dissonance.Networking.Client
{
	internal class ConnectionNegotiator<TPeer> : ISession where TPeer : struct
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(ConnectionNegotiator<TPeer>).Name);

		private static readonly TimeSpan HandshakeRequestInterval = TimeSpan.FromSeconds(2.0);

		private readonly ISendQueue<TPeer> _sender;

		private readonly string _playerName;

		private readonly CodecSettings _codecSettings;

		private DateTime _lastHandshakeRequest = DateTime.MinValue;

		private bool _running;

		private int _connectionStateValue;

		public ConnectionState State
		{
			get
			{
				return (ConnectionState)_connectionStateValue;
			}
		}

		public uint SessionId { get; private set; }

		public ushort? LocalId { get; private set; }

		public string LocalName
		{
			get
			{
				return _playerName;
			}
		}

		public ConnectionNegotiator([NotNull] ISendQueue<TPeer> sender, string playerName, [NotNull] CodecSettings codecSettings)
		{
			_sender = sender;
			_playerName = playerName;
			_codecSettings = codecSettings;
		}

		public void ReceiveHandshakeResponseHeader(ref PacketReader reader)
		{
			uint session;
			ushort clientId;
			reader.ReadHandshakeResponseHeader(out session, out clientId);
			SessionId = session;
			LocalId = clientId;
			if (Interlocked.CompareExchange(ref _connectionStateValue, 2, 1) == 1)
			{
				Log.Info("Received handshake response from server, joined session '{0}'", SessionId);
			}
		}

		public void Start()
		{
			if (State == ConnectionState.Disconnected)
			{
				throw Log.CreatePossibleBugException("Attempted to restart a ConnectionNegotiator after it has been disconnected", "92F0B2EB-282A-4558-B3BD-6656F83A06E3");
			}
			_running = true;
		}

		public void Stop()
		{
			_running = false;
			_connectionStateValue = 3;
		}

		public void Update(DateTime utcNow)
		{
			if (_running)
			{
				bool flag = State == ConnectionState.Negotiating && utcNow - _lastHandshakeRequest > HandshakeRequestInterval;
				if (State == ConnectionState.None || flag)
				{
					SendHandshake(utcNow);
				}
			}
		}

		private void SendHandshake(DateTime utcNow)
		{
			Log.AssertAndThrowPossibleBug(State != ConnectionState.Disconnected, "39533F23-2DAC-4340-9A7D-960904464E23", "Attempted to begin connection negotiation with a client which is disconnected");
			_lastHandshakeRequest = utcNow;
			_sender.EnqueueReliable(new PacketWriter(new ArraySegment<byte>(_sender.SendBufferPool.Get())).WriteHandshakeRequest(_playerName, _codecSettings).Written);
			Interlocked.CompareExchange(ref _connectionStateValue, 1, 0);
		}
	}
}
