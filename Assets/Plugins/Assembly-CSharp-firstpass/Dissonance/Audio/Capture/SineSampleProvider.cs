using System;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal class SineSampleProvider : ISampleProvider
	{
		private readonly WaveFormat _format;

		private readonly float _frequency;

		private readonly double _step;

		private const double TwoPi = Math.PI * 2.0;

		private double _index;

		public float Frequency
		{
			get
			{
				return _frequency;
			}
		}

		public WaveFormat WaveFormat
		{
			get
			{
				return _format;
			}
		}

		public SineSampleProvider(WaveFormat format, float frequency)
		{
			_format = format;
			_frequency = frequency;
			_step = Math.PI * 2.0 * (double)_frequency / (double)_format.SampleRate;
		}

		public int Read(float[] buffer, int offset, int count)
		{
			for (int i = offset; i < count; i++)
			{
				buffer[i] = (float)Math.Sin(_index) * 0.95f;
				_index = (_index + _step) % (Math.PI * 2.0);
			}
			return count;
		}
	}
}
