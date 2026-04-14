using System.Collections.Generic;

namespace Dissonance.Networking.Client
{
	internal class TextSender<TPeer> where TPeer : struct
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(TextSender<TPeer>).Name);

		private readonly ISession _session;

		private readonly ISendQueue<TPeer> _sender;

		private readonly IClientCollection<TPeer?> _peers;

		private readonly List<ClientInfo<TPeer?>> _tmpDests = new List<ClientInfo<TPeer?>>();

		public TextSender(ISendQueue<TPeer> sender, ISession session, IClientCollection<TPeer?> peers)
		{
			_session = session;
			_sender = sender;
			_peers = peers;
		}

		public void Send(string data, ChannelType type, string recipient)
		{
			List<ClientInfo<TPeer?>> clients;
			if (!_session.LocalId.HasValue)
			{
				Log.Warn("Attempted to send a text message before connected to Dissonance session");
			}
			else if (type == ChannelType.Player)
			{
				ClientInfo<TPeer?> info;
				if (!_peers.TryGetClientInfoByName(recipient, out info))
				{
					Log.Warn("Attempted to send text message to unknown player '{0}'", recipient);
					return;
				}
				byte[] array = _sender.SendBufferPool.Get();
				PacketWriter packetWriter = new PacketWriter(array);
				packetWriter.WriteTextPacket(_session.SessionId, _session.LocalId.Value, type, info.PlayerId, data);
				_tmpDests.Clear();
				_tmpDests.Add(info);
				_sender.EnqueueReliableP2P(_session.LocalId.Value, _tmpDests, packetWriter.Written);
				_tmpDests.Clear();
			}
			else if (_peers.TryGetClientsInRoom(recipient, out clients))
			{
				byte[] array2 = _sender.SendBufferPool.Get();
				PacketWriter packetWriter2 = new PacketWriter(array2);
				packetWriter2.WriteTextPacket(_session.SessionId, _session.LocalId.Value, type, recipient.ToRoomId(), data);
				_sender.EnqueueReliableP2P(_session.LocalId.Value, clients, packetWriter2.Written);
			}
		}
	}
}
