namespace Dissonance.Audio.Playback
{
	public interface IVoicePlayback
	{
		string PlayerName { get; }

		bool IsActive { get; }

		bool IsSpeaking { get; }

		float Amplitude { get; }

		float? PacketLoss { get; }

		float Jitter { get; }

		ChannelPriority Priority { get; }
	}
}
