using System;
using Dissonance.Audio.Playback;

namespace Dissonance
{
	public struct RemoteChannel
	{
		private readonly string _target;

		private readonly ChannelType _type;

		private readonly PlaybackOptions _options;

		public ChannelType Type
		{
			get
			{
				return _type;
			}
		}

		public PlaybackOptions Options
		{
			get
			{
				return _options;
			}
		}

		public string TargetName
		{
			get
			{
				return _target;
			}
		}

		internal RemoteChannel([NotNull] string targetName, ChannelType type, PlaybackOptions options)
		{
			if (targetName == null)
			{
				throw new ArgumentNullException("targetName");
			}
			_target = targetName;
			_type = type;
			_options = options;
		}
	}
}
