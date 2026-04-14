using System;
using NAudio.Dsp;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
	internal class Resampler : ISampleSource
	{
		private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof(Resampler).Name);

		private readonly ISampleSource _source;

		private volatile WaveFormat _outputFormat;

		private WdlResampler _resampler;

		public WaveFormat WaveFormat
		{
			get
			{
				return _outputFormat;
			}
		}

		public Resampler(ISampleSource source)
		{
			_source = source;
			AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
			OnAudioConfigurationChanged(false);
		}

		public void Prepare(SessionContext context)
		{
			_source.Prepare(context);
		}

		public bool Read(ArraySegment<float> samples)
		{
			WaveFormat waveFormat = _source.WaveFormat;
			WaveFormat outputFormat = _outputFormat;
			if (outputFormat.SampleRate == waveFormat.SampleRate)
			{
				return _source.Read(samples);
			}
			if (_resampler == null || outputFormat.SampleRate != (int)_resampler.OutputSampleRate)
			{
				_resampler = new WdlResampler();
				_resampler.SetMode(true, 2, false);
				_resampler.SetFilterParms();
				_resampler.SetFeedMode(false);
				_resampler.SetRates(waveFormat.SampleRate, outputFormat.SampleRate);
			}
			int channels = waveFormat.Channels;
			int num = samples.Count / channels;
			float[] inbuffer;
			int inbufferOffset;
			int num2 = _resampler.ResamplePrepare(num, channels, out inbuffer, out inbufferOffset);
			ArraySegment<float> samples2 = new ArraySegment<float>(inbuffer, inbufferOffset, num2 * channels);
			bool result = _source.Read(samples2);
			_resampler.ResampleOut(samples.Array, samples.Offset, num2, num, channels);
			return result;
		}

		public void Reset()
		{
			if (_resampler != null)
			{
				_resampler.Reset();
			}
			_source.Reset();
		}

		private void OnAudioConfigurationChanged(bool deviceWasChanged)
		{
			_outputFormat = new WaveFormat(_source.WaveFormat.Channels, AudioSettings.outputSampleRate);
		}
	}
}
