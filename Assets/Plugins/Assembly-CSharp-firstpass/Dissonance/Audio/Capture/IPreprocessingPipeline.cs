using System;
using Dissonance.VAD;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal interface IPreprocessingPipeline : IDisposable, IMicrophoneSubscriber
	{
		WaveFormat OutputFormat { get; }

		float Amplitude { get; }

		TimeSpan UpstreamLatency { set; }

		int OutputFrameSize { get; }

		void Start();

		void Subscribe(IMicrophoneSubscriber listener);

		bool Unsubscribe(IMicrophoneSubscriber listener);

		void Subscribe(IVoiceActivationListener listener);

		bool Unsubscribe(IVoiceActivationListener listener);
	}
}
