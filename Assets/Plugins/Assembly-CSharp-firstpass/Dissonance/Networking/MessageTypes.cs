namespace Dissonance.Networking
{
	internal enum MessageTypes : byte
	{
		ClientState = 1,
		VoiceData = 2,
		TextData = 3,
		HandshakeRequest = 4,
		HandshakeResponse = 5,
		ErrorWrongSession = 6,
		ServerRelayReliable = 7,
		ServerRelayUnreliable = 8,
		DeltaChannelState = 9,
		RemoveClient = 10,
		HandshakeP2P = 11
	}
}
