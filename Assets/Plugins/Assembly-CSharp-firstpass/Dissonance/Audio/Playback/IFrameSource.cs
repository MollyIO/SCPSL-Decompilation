using System;
using NAudio.Wave;

namespace Dissonance.Audio.Playback
{
	internal interface IFrameSource
	{
		uint FrameSize { get; }

		[NotNull]
		WaveFormat WaveFormat { get; }

		void Prepare(SessionContext context);

		bool Read(ArraySegment<float> frame);

		void Reset();
	}
}
