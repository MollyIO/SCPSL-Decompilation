using System;

namespace Dissonance.Audio.Codecs.Opus
{
	internal class OpusEncoder : IVoiceEncoder, IDisposable
	{
		private readonly OpusNative.OpusEncoder _encoder;

		private readonly int _frameSize;

		public int SampleRate
		{
			get
			{
				return 48000;
			}
		}

		public float PacketLoss
		{
			set
			{
				_encoder.PacketLoss = value;
			}
		}

		public int FrameSize
		{
			get
			{
				return _frameSize;
			}
		}

		public OpusEncoder(AudioQuality quality, FrameSize frameSize, bool fec = true)
		{
			_encoder = new OpusNative.OpusEncoder(SampleRate, 1)
			{
				EnableForwardErrorCorrection = fec,
				Bitrate = GetTargetBitrate(quality)
			};
			_frameSize = GetFrameSize(frameSize);
		}

		private static int GetTargetBitrate(AudioQuality quality)
		{
			switch (quality)
			{
			case AudioQuality.Low:
				return 10000;
			case AudioQuality.Medium:
				return 17000;
			case AudioQuality.High:
				return 24000;
			default:
				throw new ArgumentOutOfRangeException("quality", quality, null);
			}
		}

		private int GetFrameSize(FrameSize size)
		{
			switch (size)
			{
			case Dissonance.FrameSize.Small:
				return _encoder.PermittedFrameSizes[3];
			case Dissonance.FrameSize.Medium:
				return _encoder.PermittedFrameSizes[4];
			case Dissonance.FrameSize.Large:
				return _encoder.PermittedFrameSizes[5];
			default:
				throw new ArgumentOutOfRangeException("size", size, null);
			}
		}

		public ArraySegment<byte> Encode(ArraySegment<float> samples, ArraySegment<byte> encodedBuffer)
		{
			int count = _encoder.EncodeFloats(samples, encodedBuffer);
			return new ArraySegment<byte>(encodedBuffer.Array, encodedBuffer.Offset, count);
		}

		public void Reset()
		{
			_encoder.Reset();
		}

		public void Dispose()
		{
			_encoder.Dispose();
		}
	}
}
