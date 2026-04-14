namespace Dissonance.Audio.Playback
{
	internal interface IJitterEstimator
	{
		float Jitter { get; }

		float Confidence { get; }
	}
}
