using System;
using Dissonance.Audio.Capture;

namespace Dissonance
{
	public sealed class ChannelProperties
	{
		private readonly IChannelPriorityProvider _defaultPriority;

		private float _amplitudeMultiplier;

		public ushort Id { get; internal set; }

		public bool Positional { get; set; }

		public ChannelPriority Priority { get; set; }

		internal ChannelPriority TransmitPriority
		{
			get
			{
				if (Priority == ChannelPriority.None)
				{
					return _defaultPriority.DefaultChannelPriority;
				}
				return Priority;
			}
		}

		internal float AmplitudeMultiplier
		{
			get
			{
				return _amplitudeMultiplier;
			}
			set
			{
				_amplitudeMultiplier = Math.Min(2f, Math.Max(0f, value));
			}
		}

		internal ChannelProperties(IChannelPriorityProvider defaultPriority)
		{
			_defaultPriority = defaultPriority;
		}
	}
}
