using System.Collections.Generic;
using Dissonance.Networking;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
	public class VoicePlayback : MonoBehaviour, IVoicePlaybackInternal, IVolumeProvider, IRemoteChannelProvider, IVoicePlayback
	{
		private static readonly Log Log = Logs.Create(LogCategory.Playback, "Voice Playback Component");

		private readonly SpeechSessionStream _sessions;

		private PlaybackOptions _cachedPlaybackOptions;

		private SamplePlaybackComponent _player;

		private CodecSettings _codecSettings;

		private FrameFormat _frameFormat;

		private float? _savedSpatialBlend;

		bool IVoicePlaybackInternal.AllowPositionalPlayback { get; set; }

		bool IVoicePlaybackInternal.IsMuted { get; set; }

		float IVoicePlaybackInternal.PlaybackVolume { get; set; }

		bool IVoicePlaybackInternal.IsApplyingAudioSpatialization
		{
			get
			{
				return IsApplyingAudioSpatialization;
			}
		}

		float? IVoicePlayback.PacketLoss
		{
			get
			{
				SpeechSession? session = _player.Session;
				if (!session.HasValue)
				{
					return null;
				}
				return session.Value.PacketLoss;
			}
		}

		float IVoicePlayback.Jitter
		{
			get
			{
				return ((IJitterEstimator)_sessions).Jitter;
			}
		}

		float IVolumeProvider.TargetVolume
		{
			get
			{
				if (((IVoicePlaybackInternal)this).IsMuted)
				{
					return 0f;
				}
				if (PriorityManager != null && PriorityManager.TopPriority > Priority)
				{
					return 0f;
				}
				IVolumeProvider volumeProvider = VolumeProvider;
				float num = ((volumeProvider != null) ? volumeProvider.TargetVolume : 1f);
				return ((IVoicePlaybackInternal)this).PlaybackVolume * num;
			}
		}

		public AudioSource AudioSource { get; private set; }

		public bool IsActive
		{
			get
			{
				return base.isActiveAndEnabled;
			}
		}

		public string PlayerName
		{
			get
			{
				return _sessions.PlayerName;
			}
			internal set
			{
				_sessions.PlayerName = value;
			}
		}

		public CodecSettings CodecSettings
		{
			get
			{
				return _codecSettings;
			}
			internal set
			{
				_codecSettings = value;
				if (_frameFormat.Codec != _codecSettings.Codec || _frameFormat.FrameSize != _codecSettings.FrameSize || _frameFormat.WaveFormat == null || _frameFormat.WaveFormat.SampleRate != _codecSettings.SampleRate)
				{
					_frameFormat = new FrameFormat(_codecSettings.Codec, new WaveFormat(1, _codecSettings.SampleRate), _codecSettings.FrameSize);
				}
			}
		}

		public bool IsSpeaking
		{
			get
			{
				return _player != null && _player.HasActiveSession;
			}
		}

		public float Amplitude
		{
			get
			{
				return (!(_player == null)) ? _player.ARV : 0f;
			}
		}

		public ChannelPriority Priority
		{
			get
			{
				if (_player == null)
				{
					return ChannelPriority.None;
				}
				if (!_player.Session.HasValue)
				{
					return ChannelPriority.None;
				}
				return _cachedPlaybackOptions.Priority;
			}
		}

		private bool IsApplyingAudioSpatialization { get; set; }

		internal IPriorityManager PriorityManager { get; set; }

		[CanBeNull]
		internal IVolumeProvider VolumeProvider { get; set; }

		public VoicePlayback()
		{
			_sessions = new SpeechSessionStream(this);
			((IVoicePlaybackInternal)this).PlaybackVolume = 1f;
		}

		public void Awake()
		{
			AudioSource = GetComponent<AudioSource>();
			_player = GetComponent<SamplePlaybackComponent>();
			((IVoicePlaybackInternal)this).Reset();
		}

		void IVoicePlaybackInternal.Reset()
		{
			((IVoicePlaybackInternal)this).IsMuted = false;
			((IVoicePlaybackInternal)this).PlaybackVolume = 1f;
		}

		public void OnEnable()
		{
			AudioSource.Stop();
			if (AudioSource.spatialize)
			{
				IsApplyingAudioSpatialization = false;
				AudioSource.clip = null;
				_player.MultiplyBySource = false;
			}
			else
			{
				IsApplyingAudioSpatialization = true;
				AudioSource.clip = AudioClip.Create("Flatline", 4096, 1, AudioSettings.outputSampleRate, true, delegate(float[] buf)
				{
					for (int i = 0; i < buf.Length; i++)
					{
						buf[i] = 1f;
					}
				});
				_player.MultiplyBySource = true;
			}
			AudioSource.Play();
		}

		public void OnDisable()
		{
			_sessions.StopSession(false);
		}

		public void Update()
		{
			if (!_player.HasActiveSession)
			{
				SpeechSession? speechSession = _sessions.TryDequeueSession();
				if (speechSession.HasValue)
				{
					_cachedPlaybackOptions = speechSession.Value.PlaybackOptions;
					_player.Play(speechSession.Value);
				}
			}
			else
			{
				AudioSource.pitch = _player.CorrectedPlaybackSpeed;
			}
			UpdatePositionalPlayback();
		}

		private void UpdatePositionalPlayback()
		{
			SpeechSession? session = _player.Session;
			if (!session.HasValue)
			{
				return;
			}
			_cachedPlaybackOptions = session.Value.PlaybackOptions;
			if (((IVoicePlaybackInternal)this).AllowPositionalPlayback && _cachedPlaybackOptions.IsPositional)
			{
				if (_savedSpatialBlend.HasValue)
				{
					AudioSource.spatialBlend = _savedSpatialBlend.Value;
					_savedSpatialBlend = null;
				}
			}
			else if (!_savedSpatialBlend.HasValue)
			{
				_savedSpatialBlend = AudioSource.spatialBlend;
				AudioSource.spatialBlend = 0f;
			}
		}

		void IVoicePlaybackInternal.SetTransform(Vector3 pos, Quaternion rot)
		{
			Transform transform = base.transform;
			transform.position = pos;
			transform.rotation = rot;
		}

		void IVoicePlaybackInternal.StartPlayback()
		{
			_sessions.StartSession(_frameFormat);
		}

		void IVoicePlaybackInternal.StopPlayback()
		{
			_sessions.StopSession();
		}

		void IVoicePlaybackInternal.ReceiveAudioPacket(VoicePacket packet)
		{
			_sessions.ReceiveFrame(packet);
		}

		void IRemoteChannelProvider.GetRemoteChannels(List<RemoteChannel> output)
		{
			output.Clear();
			if (!(_player == null))
			{
				SpeechSession? session = _player.Session;
				if (session.HasValue)
				{
					session.Value.Channels.GetRemoteChannels(output);
				}
			}
		}
	}
}
