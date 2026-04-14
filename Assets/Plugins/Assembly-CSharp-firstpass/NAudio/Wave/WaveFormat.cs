using System;

namespace NAudio.Wave
{
	public sealed class WaveFormat
	{
		private readonly int _channels;

		private readonly int _sampleRate;

		public int Channels
		{
			get
			{
				return _channels;
			}
		}

		public int SampleRate
		{
			get
			{
				return _sampleRate;
			}
		}

		public WaveFormat(int channels, int sampleRate)
		{
			if (channels > 64)
			{
				throw new ArgumentOutOfRangeException("channels", "More than 64 channels");
			}
			_channels = channels;
			_sampleRate = sampleRate;
		}

		public bool Equals(WaveFormat other)
		{
			return other.Channels == Channels && other.SampleRate == SampleRate;
		}

		public override int GetHashCode()
		{
			int num = 1022251;
			num += _channels;
			num *= 16777619;
			num += _sampleRate;
			return num * 16777619;
		}

		public override string ToString()
		{
			return string.Format("(Channels:{0}, Rate:{1})", Channels, SampleRate);
		}
	}
}
