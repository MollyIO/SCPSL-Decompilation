using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Audio.Playback;
using Dissonance.Networking;

namespace Dissonance
{
	public abstract class VoicePlayerState
	{
		private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(VoicePlayerState).Name);

		private readonly string _name;

		[NotNull]
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public abstract bool IsConnected { get; }

		public abstract bool IsSpeaking { get; }

		public abstract float Amplitude { get; }

		public abstract ChannelPriority? SpeakerPriority { get; }

		public abstract float Volume { get; set; }

		public abstract bool IsLocallyMuted { get; set; }

		[NotNull]
		public abstract ReadOnlyCollection<string> Rooms { get; }

		[CanBeNull]
		public IVoicePlayback Playback
		{
			get
			{
				return PlaybackInternal;
			}
		}

		[CanBeNull]
		internal abstract IVoicePlaybackInternal PlaybackInternal { get; }

		[CanBeNull]
		public abstract IDissonancePlayer Tracker { get; internal set; }

		[CanBeNull]
		public abstract float? PacketLoss { get; }

		public event Action<VoicePlayerState> OnStartedSpeaking;

		public event Action<VoicePlayerState> OnStoppedSpeaking;

		public event Action<VoicePlayerState, string> OnEnteredRoom;

		public event Action<VoicePlayerState, string> OnExitedRoom;

		public event Action<VoicePlayerState> OnLeftSession;

		internal VoicePlayerState(string name)
		{
			_name = name;
		}

		internal void InvokeOnStoppedSpeaking()
		{
			if (PlaybackInternal != null)
			{
				PlaybackInternal.StopPlayback();
			}
			Action<VoicePlayerState> action = this.OnStoppedSpeaking;
			if (action != null)
			{
				action(this);
			}
		}

		internal void InvokeOnStartedSpeaking()
		{
			if (PlaybackInternal != null)
			{
				PlaybackInternal.StartPlayback();
			}
			Action<VoicePlayerState> action = this.OnStartedSpeaking;
			if (action != null)
			{
				action(this);
			}
		}

		internal void InvokeOnLeftSession()
		{
			Action<VoicePlayerState> action = this.OnLeftSession;
			if (action != null)
			{
				action(this);
			}
		}

		internal virtual void InvokeOnEnteredRoom(RoomEvent evtData)
		{
			Log.AssertAndThrowPossibleBug(evtData.Joined, "FC760FE7-10D6-4572-B7D6-D33799D93FFD", "Passed leave event to join event handler");
			Action<VoicePlayerState, string> action = this.OnEnteredRoom;
			if (action != null)
			{
				action(this, evtData.Room);
			}
		}

		internal virtual void InvokeOnExitedRoom(RoomEvent evtData)
		{
			Log.AssertAndThrowPossibleBug(!evtData.Joined, "359A67D1-DE96-4181-B5FF-D4ED3B8C0DF0", "Passed join event to leave event handler");
			Action<VoicePlayerState, string> action = this.OnExitedRoom;
			if (action != null)
			{
				action(this, evtData.Room);
			}
		}

		public abstract void GetSpeakingChannels([NotNull] List<RemoteChannel> output);

		internal abstract void Update();
	}
}
