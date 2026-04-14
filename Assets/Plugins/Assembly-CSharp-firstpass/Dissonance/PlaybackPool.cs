using System;
using Dissonance.Audio.Playback;
using Dissonance.Datastructures;
using UnityEngine;

namespace Dissonance
{
	internal class PlaybackPool
	{
		private readonly Pool<VoicePlayback> _pool;

		[NotNull]
		private readonly IPriorityManager _priority;

		[NotNull]
		private readonly IVolumeProvider _volume;

		private GameObject _prefab;

		private Transform _parent;

		public PlaybackPool([NotNull] IPriorityManager priority, [NotNull] IVolumeProvider volume)
		{
			if (priority == null)
			{
				throw new ArgumentNullException("priority");
			}
			if (volume == null)
			{
				throw new ArgumentNullException("volume");
			}
			_priority = priority;
			_volume = volume;
			_pool = new Pool<VoicePlayback>(6, CreatePlayback);
		}

		public void Start([NotNull] GameObject playbackPrefab, [NotNull] Transform transform)
		{
			if (playbackPrefab == null)
			{
				throw new ArgumentNullException("playbackPrefab");
			}
			if (transform == null)
			{
				throw new ArgumentNullException("transform");
			}
			_prefab = playbackPrefab;
			_parent = transform;
		}

		[NotNull]
		private VoicePlayback CreatePlayback()
		{
			_prefab.gameObject.SetActive(false);
			GameObject gameObject = UnityEngine.Object.Instantiate(_prefab.gameObject);
			gameObject.transform.parent = _parent;
			AudioSource audioSource = gameObject.GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.rolloffMode = AudioRolloffMode.Linear;
				audioSource.bypassReverbZones = true;
			}
			audioSource.loop = true;
			audioSource.pitch = 1f;
			audioSource.clip = null;
			audioSource.playOnAwake = false;
			audioSource.ignoreListenerPause = true;
			audioSource.spatialBlend = 1f;
			audioSource.Stop();
			SamplePlaybackComponent component = gameObject.GetComponent<SamplePlaybackComponent>();
			if (component == null)
			{
				gameObject.AddComponent<SamplePlaybackComponent>();
			}
			VoicePlayback component2 = gameObject.GetComponent<VoicePlayback>();
			component2.PriorityManager = _priority;
			component2.VolumeProvider = _volume;
			return component2;
		}

		[NotNull]
		public VoicePlayback Get([NotNull] string playerId)
		{
			if (playerId == null)
			{
				throw new ArgumentNullException("playerId");
			}
			VoicePlayback voicePlayback = _pool.Get();
			voicePlayback.gameObject.name = string.Format("Player {0} voice comms", playerId);
			voicePlayback.PlayerName = playerId;
			return voicePlayback;
		}

		public void Put([NotNull] VoicePlayback playback)
		{
			if (playback == null)
			{
				throw new ArgumentNullException("playback");
			}
			playback.gameObject.SetActive(false);
			playback.gameObject.name = "Spare voice comms";
			playback.PlayerName = null;
			_pool.Put(playback);
		}
	}
}
