using System;

namespace Dissonance.Audio.Codecs
{
	internal interface IVoiceEncoder : IDisposable
	{
		float PacketLoss { set; }

		int FrameSize { get; }

		int SampleRate { get; }

		ArraySegment<byte> Encode(ArraySegment<float> samples, ArraySegment<byte> array);

		void Reset();
	}
}
