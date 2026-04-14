using UnityEngine;

namespace Dissonance.Audio
{
	internal struct Fader
	{
		private float _fadeTime;

		private float _elapsedTime;

		public float Volume { get; private set; }

		public float EndVolume { get; private set; }

		public float StartVolume { get; private set; }

		public void Update(float dt)
		{
			_elapsedTime += dt;
			Volume = CalculateVolume();
		}

		private float CalculateVolume()
		{
			if (_fadeTime <= 0f || _elapsedTime >= _fadeTime)
			{
				return EndVolume;
			}
			float t = _elapsedTime / _fadeTime;
			return Mathf.Lerp(StartVolume, EndVolume, t);
		}

		public void FadeTo(float target, float duration)
		{
			_fadeTime = duration;
			_elapsedTime = 0f;
			StartVolume = Volume;
			EndVolume = target;
		}
	}
}
