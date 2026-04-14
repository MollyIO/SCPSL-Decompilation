using System;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
	internal interface IDecoderPipeline
	{
		int BufferCount { get; }

		TimeSpan BufferTime { get; }

		float PacketLoss { get; }

		PlaybackOptions PlaybackOptions { get; }

		[NotNull]
		WaveFormat OutputFormat { get; }

		void Prepare(SessionContext context);

		bool Read(ArraySegment<float> samples);
	}
}
