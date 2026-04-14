namespace Dissonance.Audio.Playback
{
	public struct PlaybackOptions
	{
		private readonly bool _isPositional;

		private readonly float _amplitudeMultiplier;

		private readonly ChannelPriority _priority;

		public bool IsPositional
		{
			get
			{
				return _isPositional;
			}
		}

		public float AmplitudeMultiplier
		{
			get
			{
				return _amplitudeMultiplier;
			}
		}

		public ChannelPriority Priority
		{
			get
			{
				return _priority;
			}
		}

		public PlaybackOptions(bool isPositional, float amplitudeMultiplier, ChannelPriority priority)
		{
			_isPositional = isPositional;
			_amplitudeMultiplier = amplitudeMultiplier;
			_priority = priority;
		}
	}
}
