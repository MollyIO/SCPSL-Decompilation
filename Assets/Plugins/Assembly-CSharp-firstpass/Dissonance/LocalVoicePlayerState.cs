using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Audio.Capture;
using Dissonance.Audio.Playback;
using Dissonance.Networking;

namespace Dissonance
{
	internal class LocalVoicePlayerState : VoicePlayerState
	{
		private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(LocalVoicePlayerState).Name);

		[NotNull]
		private readonly IAmplitudeProvider _micAmplitude;

		[NotNull]
		private readonly Rooms _rooms;

		[NotNull]
		private readonly RoomChannels _roomChannels;

		[NotNull]
		private readonly PlayerChannels _playerChannels;

		[NotNull]
		private readonly ILossEstimator _loss;

		public override bool IsConnected
		{
			get
			{
				return true;
			}
		}

		internal override IVoicePlaybackInternal PlaybackInternal
		{
			get
			{
				return null;
			}
		}

		public override bool IsLocallyMuted
		{
			get
			{
				return true;
			}
			set
			{
				if (!value)
				{
					Log.Error(Log.UserErrorMessage("Attempted to Locally UnMute the local player", "Setting `IsLocallyMuted = false` on the local player", "https://dissonance.readthedocs.io/en/latest/Reference/Other/VoicePlayerState/", "BEF78918-1805-4D59-A071-74E7B38D13C8"));
				}
			}
		}

		public override ReadOnlyCollection<string> Rooms
		{
			get
			{
				return _rooms.Memberships;
			}
		}

		public override IDissonancePlayer Tracker { get; internal set; }

		public override float Amplitude
		{
			get
			{
				return _micAmplitude.Amplitude;
			}
		}

		public override ChannelPriority? SpeakerPriority
		{
			get
			{
				return null;
			}
		}

		public override float Volume
		{
			get
			{
				return 1f;
			}
			set
			{
				Log.Error(Log.UserErrorMessage("Attempted to set playback volume of local player", "Setting `Volume = value` on the local player", "https://dissonance.readthedocs.io/en/latest/Reference/Other/VoicePlayerState/", "9822EFB8-1A4A-4F54-9A32-5F183AE8D4DE"));
			}
		}

		public override bool IsSpeaking
		{
			get
			{
				return _roomChannels.Count > 0 || _playerChannels.Count > 0;
			}
		}

		public override float? PacketLoss
		{
			get
			{
				return _loss.PacketLoss;
			}
		}

		public LocalVoicePlayerState(string name, [NotNull] IAmplitudeProvider micAmplitude, [NotNull] Rooms rooms, [NotNull] RoomChannels roomChannels, [NotNull] PlayerChannels playerChannels, [NotNull] ILossEstimator loss)
			: base(name)
		{
			_rooms = rooms;
			_micAmplitude = micAmplitude;
			_roomChannels = roomChannels;
			_playerChannels = playerChannels;
			_loss = loss;
			rooms.JoinedRoom += OnLocallyEnteredRoom;
			rooms.LeftRoom += OnLocallyExitedRoom;
			roomChannels.OpenedChannel += OnChannelOpened;
			roomChannels.ClosedChannel += OnChannelClosed;
			playerChannels.OpenedChannel += OnChannelOpened;
			playerChannels.ClosedChannel += OnChannelClosed;
		}

		private void OnChannelOpened(string channel, ChannelProperties properties)
		{
			int num = _playerChannels.Count + _roomChannels.Count;
			if (num == 1)
			{
				InvokeOnStartedSpeaking();
			}
		}

		private void OnChannelClosed(string channel, ChannelProperties properties)
		{
			if (_playerChannels.Count + _roomChannels.Count == 0)
			{
				InvokeOnStoppedSpeaking();
			}
		}

		private void OnLocallyEnteredRoom([NotNull] string room)
		{
			InvokeOnEnteredRoom(new RoomEvent(base.Name, room, true, Rooms));
		}

		private void OnLocallyExitedRoom([NotNull] string room)
		{
			InvokeOnExitedRoom(new RoomEvent(base.Name, room, false, Rooms));
		}

		public override void GetSpeakingChannels(List<RemoteChannel> channels)
		{
			foreach (KeyValuePair<ushort, RoomChannel> roomChannel in _roomChannels)
			{
				channels.Add(CreateRemoteChannel(roomChannel.Value, ChannelType.Room));
			}
			foreach (KeyValuePair<ushort, PlayerChannel> playerChannel in _playerChannels)
			{
				channels.Add(CreateRemoteChannel(playerChannel.Value, ChannelType.Player));
			}
		}

		private static RemoteChannel CreateRemoteChannel<T>([NotNull] T item, ChannelType type) where T : IChannel<string>
		{
			return new RemoteChannel(item.TargetId, type, new PlaybackOptions(item.Properties.Positional, item.Properties.AmplitudeMultiplier, item.Properties.TransmitPriority));
		}

		internal override void Update()
		{
		}
	}
}
