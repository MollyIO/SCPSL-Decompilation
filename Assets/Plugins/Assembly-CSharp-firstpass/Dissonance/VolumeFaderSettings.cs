using System;
using UnityEngine;

namespace Dissonance
{
	[Serializable]
	public class VolumeFaderSettings
	{
		[SerializeField]
		private float _volume;

		[SerializeField]
		private long _fadeInTicks;

		[SerializeField]
		private long _fadeOutTicks;

		public float Volume
		{
			get
			{
				return _volume;
			}
			set
			{
				_volume = value;
			}
		}

		public TimeSpan FadeIn
		{
			get
			{
				return new TimeSpan(_fadeInTicks);
			}
			set
			{
				_fadeInTicks = value.Ticks;
			}
		}

		public TimeSpan FadeOut
		{
			get
			{
				return new TimeSpan(_fadeOutTicks);
			}
			set
			{
				_fadeOutTicks = value.Ticks;
			}
		}
	}
}
