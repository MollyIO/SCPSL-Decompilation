using System;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	public interface IMicrophoneSubscriber
	{
		void ReceiveMicrophoneData(ArraySegment<float> buffer, WaveFormat format);

		void Reset();
	}
}
