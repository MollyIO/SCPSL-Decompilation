using NAudio.Dsp;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal class Resampler : ISampleProvider
	{
		private readonly WaveFormat _format;

		[CanBeNull]
		private readonly WdlResampler _resampler;

		private readonly ISampleProvider _source;

		public WaveFormat WaveFormat
		{
			get
			{
				return _format;
			}
		}

		public Resampler([NotNull] ISampleProvider source, int newSampleRate)
		{
			_source = source;
			_format = new WaveFormat(source.WaveFormat.Channels, newSampleRate);
			if (source.WaveFormat.SampleRate != newSampleRate)
			{
				_resampler = new WdlResampler();
				_resampler.SetMode(true, 2, false);
				_resampler.SetFilterParms();
				_resampler.SetFeedMode(false);
				_resampler.SetRates(source.WaveFormat.SampleRate, newSampleRate);
			}
		}

		public int Read(float[] buffer, int offset, int count)
		{
			if (_resampler == null)
			{
				return _source.Read(buffer, offset, count);
			}
			if (count == 0)
			{
				return 0;
			}
			int channels = _source.WaveFormat.Channels;
			int num = count / channels;
			float[] inbuffer;
			int inbufferOffset;
			int num2 = _resampler.ResamplePrepare(num, channels, out inbuffer, out inbufferOffset);
			int num3 = _source.Read(inbuffer, inbufferOffset, num2 * channels) / channels;
			if (num3 == 0)
			{
				return 0;
			}
			int num4 = _resampler.ResampleOut(buffer, offset, num3, num, channels);
			return num4 * channels;
		}

		public void Reset()
		{
			if (_resampler != null)
			{
				_resampler.Reset();
			}
		}
	}
}
