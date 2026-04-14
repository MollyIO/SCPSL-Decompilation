namespace Dissonance.Audio.Playback
{
	public interface IPriorityManager
	{
		ChannelPriority TopPriority { get; }
	}
}
