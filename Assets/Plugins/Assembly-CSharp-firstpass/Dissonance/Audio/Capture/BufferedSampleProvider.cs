using System;
using Dissonance.Datastructures;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal class BufferedSampleProvider : ISampleProvider
	{
		private readonly WaveFormat _format;

		private readonly TransferBuffer<float> _samples;

		public int Count
		{
			get
			{
				return _samples.EstimatedUnreadCount;
			}
		}

		public int Capacity
		{
			get
			{
				return _samples.Capacity;
			}
		}

		public WaveFormat WaveFormat
		{
			get
			{
				return _format;
			}
		}

		public BufferedSampleProvider(WaveFormat format, int bufferSize)
		{
			_format = format;
			_samples = new TransferBuffer<float>(bufferSize);
		}

		public int Read(float[] buffer, int offset, int count)
		{
			if (!_samples.Read(new ArraySegment<float>(buffer, offset, count)))
			{
				return 0;
			}
			return count;
		}

		public int Write(ArraySegment<float> data)
		{
			if (data.Array == null)
			{
				throw new ArgumentNullException("data");
			}
			return _samples.WriteSome(data);
		}

		public void Reset()
		{
			_samples.Clear();
		}
	}
}
