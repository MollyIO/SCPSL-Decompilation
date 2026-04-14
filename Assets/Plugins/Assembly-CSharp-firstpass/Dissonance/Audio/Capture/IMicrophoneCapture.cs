using System;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	public interface IMicrophoneCapture
	{
		bool IsRecording { get; }

		TimeSpan Latency { get; }

		[CanBeNull]
		WaveFormat StartCapture([CanBeNull] string name);

		void StopCapture();

		void Subscribe([NotNull] IMicrophoneSubscriber listener);

		bool Unsubscribe([NotNull] IMicrophoneSubscriber listener);

		bool UpdateSubscribers();
	}
}
