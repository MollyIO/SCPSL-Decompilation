namespace Dissonance.Networking
{
	public struct TextMessage
	{
		public readonly string Sender;

		public readonly ChannelType RecipientType;

		public readonly string Recipient;

		public readonly string Message;

		public TextMessage(string sender, ChannelType recipientType, string recipient, string message)
		{
			Sender = sender;
			RecipientType = recipientType;
			Recipient = recipient;
			Message = message;
		}
	}
}
