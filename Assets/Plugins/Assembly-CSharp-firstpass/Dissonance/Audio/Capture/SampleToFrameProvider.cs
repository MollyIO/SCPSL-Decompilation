using System;
using Dissonance.Extensions;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal class SampleToFrameProvider : IFrameProvider
	{
		private readonly ISampleProvider _source;

		private readonly uint _frameSize;

		private int _samplesInFrame;

		private readonly float[] _frame;

		public WaveFormat WaveFormat
		{
			get
			{
				return _source.WaveFormat;
			}
		}

		public uint FrameSize
		{
			get
			{
				return _frameSize;
			}
		}

		public SampleToFrameProvider(ISampleProvider source, uint frameSize)
		{
			_source = source;
			_frameSize = frameSize;
			_frame = new float[frameSize];
		}

		public bool Read(ArraySegment<float> outBuffer)
		{
			if (outBuffer.Count < _frameSize)
			{
				throw new ArgumentException(string.Format("Supplied buffer is smaller than frame size. {0} < {1}", outBuffer.Count, _frameSize), "outBuffer");
			}
			_samplesInFrame += _source.Read(_frame, _samplesInFrame, checked((int)(_frameSize - _samplesInFrame)));
			if (_samplesInFrame == _frameSize)
			{
				outBuffer.CopyFrom(_frame);
				_samplesInFrame = 0;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_samplesInFrame = 0;
		}
	}
}
