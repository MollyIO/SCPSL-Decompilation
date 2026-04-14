using System;

namespace Dissonance.Networking
{
	public interface ICommsNetwork
	{
		ConnectionStatus Status { get; }

		NetworkMode Mode { get; }

		event Action<NetworkMode> ModeChanged;

		event Action<string, CodecSettings> PlayerJoined;

		event Action<string> PlayerLeft;

		event Action<VoicePacket> VoicePacketReceived;

		event Action<TextMessage> TextPacketReceived;

		event Action<string> PlayerStartedSpeaking;

		event Action<string> PlayerStoppedSpeaking;

		event Action<RoomEvent> PlayerEnteredRoom;

		event Action<RoomEvent> PlayerExitedRoom;

		void Initialize(string playerName, Rooms rooms, PlayerChannels playerChannels, RoomChannels roomChannels, CodecSettings codecSettings);

		void SendVoice(ArraySegment<byte> data);

		void SendText(string data, ChannelType recipientType, string recipientId);
	}
}
