using System;
using Dissonance.Extensions;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
	internal class FrameToSampleConverter : ISampleSource
	{
		private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof(FrameToSampleConverter).Name);

		private readonly IFrameSource _source;

		private readonly float[] _temp;

		private bool _upstreamComplete;

		private int _firstSample;

		private int _lastSample;

		public WaveFormat WaveFormat
		{
			get
			{
				return _source.WaveFormat;
			}
		}

		public FrameToSampleConverter([NotNull] IFrameSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			_source = source;
			_temp = new float[source.FrameSize * source.WaveFormat.Channels];
		}

		public void Prepare(SessionContext context)
		{
			_source.Prepare(context);
		}

		public bool Read(ArraySegment<float> samples)
		{
			int num = samples.Offset;
			int num2 = samples.Count;
			while (num2 > 0)
			{
				if (_firstSample < _lastSample)
				{
					int num3 = Math.Min(num2, _lastSample - _firstSample);
					Buffer.BlockCopy(_temp, _firstSample * 4, samples.Array, num * 4, num3 * 4);
					num += num3;
					num2 -= num3;
					_firstSample += num3;
					if (_upstreamComplete && _firstSample == _lastSample)
					{
						for (int i = num; i < samples.Offset + samples.Count; i++)
						{
							samples.Array[i] = 0f;
						}
						return true;
					}
				}
				if (num2 == 0)
				{
					break;
				}
				_firstSample = 0;
				_lastSample = _temp.Length;
				if (_upstreamComplete)
				{
					Log.Warn(Log.PossibleBugMessage("Attempting to read from a stream which has already finished", "C88903DE-17D4-4341-9AC6-28EB50BCFC8A"));
					samples.Clear();
					return true;
				}
				_upstreamComplete = _source.Read(new ArraySegment<float>(_temp));
			}
			return false;
		}

		public void Reset()
		{
			_firstSample = 0;
			_lastSample = 0;
			_upstreamComplete = false;
			_source.Reset();
		}
	}
}
