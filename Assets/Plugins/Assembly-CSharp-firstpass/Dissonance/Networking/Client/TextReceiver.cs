using System;

namespace Dissonance.Networking.Client
{
	internal class TextReceiver<TPeer> where TPeer : struct
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(TextReceiver<TPeer>).Name);

		private readonly EventQueue _events;

		private readonly Rooms _rooms;

		private readonly IClientCollection<TPeer?> _peers;

		public TextReceiver([NotNull] EventQueue events, [NotNull] Rooms rooms, [NotNull] IClientCollection<TPeer?> peers)
		{
			if (events == null)
			{
				throw new ArgumentNullException("events");
			}
			if (rooms == null)
			{
				throw new ArgumentNullException("rooms");
			}
			if (peers == null)
			{
				throw new ArgumentNullException("peers");
			}
			_events = events;
			_rooms = rooms;
			_peers = peers;
		}

		public void ProcessTextMessage(ref PacketReader reader)
		{
			TextPacket textPacket = reader.ReadTextPacket();
			ClientInfo<TPeer?> info;
			if (_peers.TryGetClientInfoById(textPacket.Sender, out info))
			{
				string txtMessageRecipient = GetTxtMessageRecipient(textPacket.RecipientType, textPacket.Recipient);
				if (txtMessageRecipient == null)
				{
					Log.Warn("Received a text message for a null recipient from '{0}'", info.PlayerName);
				}
				else
				{
					_events.EnqueueTextData(new TextMessage(info.PlayerName, textPacket.RecipientType, txtMessageRecipient, textPacket.Text));
				}
			}
		}

		[CanBeNull]
		private string GetTxtMessageRecipient(ChannelType txtRecipientType, ushort txtRecipient)
		{
			switch (txtRecipientType)
			{
			case ChannelType.Player:
			{
				ClientInfo<TPeer?> info;
				if (!_peers.TryGetClientInfoById(txtRecipient, out info))
				{
					return null;
				}
				return info.PlayerName;
			}
			case ChannelType.Room:
				return _rooms.Name(txtRecipient);
			default:
				throw Log.CreatePossibleBugException("Received a text message intended for an unknown recipient type", "521CB5B5-A45A-402E-95C8-CA99E8FFE4D9");
			}
		}
	}
}
