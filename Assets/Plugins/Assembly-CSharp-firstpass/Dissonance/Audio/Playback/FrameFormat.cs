using System;
using Dissonance.Audio.Codecs;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
	internal struct FrameFormat : IEquatable<FrameFormat>
	{
		public readonly Codec Codec;

		public readonly WaveFormat WaveFormat;

		public readonly uint FrameSize;

		public FrameFormat(Codec codec, WaveFormat waveFormat, uint frameSize)
		{
			Codec = codec;
			WaveFormat = waveFormat;
			FrameSize = frameSize;
		}

		public override int GetHashCode()
		{
			int num = 103577;
			num += (int)(Codec + 17);
			num *= 101117;
			num += WaveFormat.GetHashCode();
			num *= 101117;
			num += (int)FrameSize;
			return num * 101117;
		}

		public bool Equals(FrameFormat other)
		{
			if (Codec != other.Codec)
			{
				return false;
			}
			if (FrameSize != other.FrameSize)
			{
				return false;
			}
			if (!WaveFormat.Equals(other.WaveFormat))
			{
				return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is FrameFormat && Equals((FrameFormat)obj);
		}
	}
}
