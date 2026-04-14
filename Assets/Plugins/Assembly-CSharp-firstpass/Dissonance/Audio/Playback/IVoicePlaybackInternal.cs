using Dissonance.Networking;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
	internal interface IVoicePlaybackInternal : IRemoteChannelProvider, IVoicePlayback
	{
		bool IsMuted { get; set; }

		bool AllowPositionalPlayback { get; set; }

		bool IsApplyingAudioSpatialization { get; }

		float PlaybackVolume { get; set; }

		void Reset();

		void StartPlayback();

		void StopPlayback();

		void SetTransform(Vector3 position, Quaternion rotation);

		void ReceiveAudioPacket(VoicePacket packet);
	}
}
