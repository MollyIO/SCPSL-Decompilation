using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Audio;
using Dissonance.Audio.Capture;
using Dissonance.Audio.Playback;
using Dissonance.Config;
using Dissonance.Networking;
using Dissonance.VAD;
using UnityEngine;

namespace Dissonance
{
	[HelpURL("https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Dissonance-Comms/")]
	public sealed class DissonanceComms : MonoBehaviour, IPriorityManager, IAccessTokenCollection, IChannelPriorityProvider, IVolumeProvider
	{
		private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(DissonanceComms).Name);

		private bool _started;

		private readonly Rooms _rooms = new Rooms();

		private readonly PlayerChannels _playerChannels;

		private readonly RoomChannels _roomChannels;

		private readonly TextChat _text;

		private readonly OpenChannelVolumeDuck _autoChannelDuck;

		private readonly PlayerTrackerManager _playerTrackers;

		private readonly PlaybackPool _playbackPool;

		private readonly PlayerCollection _players = new PlayerCollection();

		private readonly CodecSettingsLoader _codecSettingsLoader = new CodecSettingsLoader();

		private readonly PriorityManager _playbackPriorityManager;

		private readonly CapturePipelineManager _capture;

		private ICommsNetwork _net;

		private string _localPlayerName;

		[SerializeField]
		[UsedImplicitly]
		private bool _isMuted;

		[SerializeField]
		[UsedImplicitly]
		private bool _isDeafened;

		[SerializeField]
		[UsedImplicitly]
		private float _oneMinusBaseRemoteVoiceVolume;

		[SerializeField]
		[UsedImplicitly]
		private VoicePlayback _playbackPrefab;

		[SerializeField]
		[UsedImplicitly]
		private GameObject _playbackPrefab2;

		[SerializeField]
		[UsedImplicitly]
		private string _micName;

		[SerializeField]
		[UsedImplicitly]
		private ChannelPriority _playerPriority;

		[SerializeField]
		[UsedImplicitly]
		private TokenSet _tokens = new TokenSet();

		public static readonly SemanticVersion Version = new SemanticVersion(6, 2, 6);

		ChannelPriority IPriorityManager.TopPriority
		{
			get
			{
				return _playbackPriorityManager.TopPriority;
			}
		}

		ChannelPriority IChannelPriorityProvider.DefaultChannelPriority
		{
			get
			{
				return _playerPriority;
			}
			set
			{
				_playerPriority = value;
			}
		}

		float IVolumeProvider.TargetVolume
		{
			get
			{
				return (!_isDeafened) ? (RemoteVoiceVolume * _autoChannelDuck.TargetVolume) : 0f;
			}
		}

		internal bool IsStarted
		{
			get
			{
				return _started;
			}
		}

		internal float PacketLoss
		{
			get
			{
				return _capture.PacketLoss;
			}
		}

		public string LocalPlayerName
		{
			get
			{
				return _localPlayerName;
			}
			set
			{
				if (!(_localPlayerName == value))
				{
					if (_started)
					{
						throw Log.CreateUserErrorException("Cannot set player name when the component has been started", "directly setting the 'LocalPlayerName' property too late", "https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Dissonance-Comms", "58973EDF-42B5-4FF1-BE01-FFF28300A97E");
					}
					_localPlayerName = value;
					Action<string> action = this.LocalPlayerNameChanged;
					if (action != null)
					{
						action(value);
					}
				}
			}
		}

		public bool IsNetworkInitialized
		{
			get
			{
				return _net != null && _net.Status == ConnectionStatus.Connected;
			}
		}

		[NotNull]
		public Rooms Rooms
		{
			get
			{
				return _rooms;
			}
		}

		[NotNull]
		public PlayerChannels PlayerChannels
		{
			get
			{
				return _playerChannels;
			}
		}

		[NotNull]
		public RoomChannels RoomChannels
		{
			get
			{
				return _roomChannels;
			}
		}

		[NotNull]
		public TextChat Text
		{
			get
			{
				return _text;
			}
		}

		[NotNull]
		public ReadOnlyCollection<VoicePlayerState> Players
		{
			get
			{
				return _players.Readonly;
			}
		}

		public ChannelPriority TopPrioritySpeaker
		{
			get
			{
				return _playbackPriorityManager.TopPriority;
			}
		}

		[NotNull]
		public IEnumerable<string> Tokens
		{
			get
			{
				return _tokens;
			}
		}

		public ChannelPriority PlayerPriority
		{
			get
			{
				return _playerPriority;
			}
			set
			{
				_playerPriority = value;
			}
		}

		[CanBeNull]
		public string MicrophoneName
		{
			get
			{
				return _micName;
			}
			set
			{
				if (!(_micName == value))
				{
					_capture.MicrophoneName = value;
					_micName = value;
				}
			}
		}

		[CanBeNull]
		public IMicrophoneCapture MicrophoneCapture
		{
			get
			{
				return _capture.Microphone;
			}
		}

		public GameObject PlaybackPrefab
		{
			get
			{
				return _playbackPrefab2;
			}
			set
			{
				if (_started)
				{
					throw Log.CreateUserErrorException("Cannot set playback prefab when the component has been started", "directly setting the 'PlaybackPrefab' property too late", "https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Dissonance-Comms", "A0796DA8-A0BC-49E4-A1B3-F0AA0F51BAA0");
				}
				if (value != null && value.GetComponent<IVoicePlayback>() == null)
				{
					throw Log.CreateUserErrorException("Cannot set playback prefab to a prefab without an implemented of IVoicePlayback", "Setting the 'PlaybackPrefab' property to an incorrect prefab", "https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Dissonance-Comms", "543EB2C1-8911-405B-8BEA-5DBC185DF0C3");
				}
				_playbackPrefab2 = value;
			}
		}

		public bool IsMuted
		{
			get
			{
				return _isMuted;
			}
			set
			{
				if (_isMuted != value)
				{
					_isMuted = value;
				}
			}
		}

		public bool IsDeafened
		{
			get
			{
				return _isDeafened;
			}
			set
			{
				if (_isDeafened != value)
				{
					_isDeafened = value;
				}
			}
		}

		public float RemoteVoiceVolume
		{
			get
			{
				return Mathf.Clamp01(1f - _oneMinusBaseRemoteVoiceVolume);
			}
			set
			{
				if (value < 0f)
				{
					throw new ArgumentOutOfRangeException("value", "Value must be greater than or equal to zero");
				}
				if (value > 1f)
				{
					throw new ArgumentOutOfRangeException("value", "Value must be less than or equal to one");
				}
				_oneMinusBaseRemoteVoiceVolume = 1f - value;
			}
		}

		public event Action<VoicePlayerState> OnPlayerJoinedSession;

		public event Action<VoicePlayerState> OnPlayerLeftSession;

		public event Action<VoicePlayerState> OnPlayerStartedSpeaking;

		public event Action<VoicePlayerState> OnPlayerStoppedSpeaking;

		public event Action<VoicePlayerState, string> OnPlayerEnteredRoom;

		public event Action<VoicePlayerState, string> OnPlayerExitedRoom;

		public event Action<string> LocalPlayerNameChanged;

		public event Action<string> TokenAdded
		{
			add
			{
				_tokens.TokenAdded += value;
			}
			remove
			{
				_tokens.TokenAdded -= value;
			}
		}

		public event Action<string> TokenRemoved
		{
			add
			{
				_tokens.TokenRemoved += value;
			}
			remove
			{
				_tokens.TokenRemoved -= value;
			}
		}

		public DissonanceComms()
		{
			_playbackPool = new PlaybackPool(this, this);
			_playerChannels = new PlayerChannels(this);
			_roomChannels = new RoomChannels(this);
			_text = new TextChat(() => _net);
			_autoChannelDuck = new OpenChannelVolumeDuck(_roomChannels, _playerChannels);
			_playerTrackers = new PlayerTrackerManager(_players);
			_playbackPriorityManager = new PriorityManager(_players);
			_capture = new CapturePipelineManager(_codecSettingsLoader, _roomChannels, _playerChannels, Players);
		}

		[UsedImplicitly]
		private void Start()
		{
			ChatRoomSettings.Preload();
			DebugSettings.Preload();
			VoiceSettings.Preload();
			Logs.WriteMultithreadedLogs();
			ICommsNetwork component = base.gameObject.GetComponent<ICommsNetwork>();
			if (component == null)
			{
				throw new Exception("Cannot find a voice network component. Please attach a voice network component appropriate to your network system to the DissonanceVoiceComms' entity.");
			}
			if (!Application.isMobilePlatform && !Application.runInBackground)
			{
				Log.Error(Log.UserErrorMessage("Run In Background is not set", "The 'Run In Background' toggle on the player settings has not been checked", "https://dissonance.readthedocs.io/en/latest/Basics/Getting-Started/#3-run-in-background", "98D123BB-CF4F-4B41-8555-41CD01108DA7"));
			}
			if (PlaybackPrefab == null)
			{
				if (_playbackPrefab != null)
				{
					PlaybackPrefab = _playbackPrefab.gameObject;
				}
				else
				{
					Log.Info("Loading default playback prefab");
					PlaybackPrefab = Resources.Load<GameObject>("PlaybackPrefab");
					if (PlaybackPrefab == null)
					{
						throw Log.CreateUserErrorException("Failed to load PlaybackPrefab from resources - Dissonance voice will be disabled", "Incorrect installation of Dissonance", "https://dissonance.readthedocs.io/en/latest/Basics/Getting-Started/", "F542DAE5-AB78-4ADE-8FF0-4573233505AB");
					}
				}
			}
			component.PlayerJoined += Net_PlayerJoined;
			component.PlayerLeft += Net_PlayerLeft;
			component.PlayerEnteredRoom += Net_PlayerRoomEvent;
			component.PlayerExitedRoom += Net_PlayerRoomEvent;
			component.VoicePacketReceived += Net_VoicePacketReceived;
			component.PlayerStartedSpeaking += Net_PlayerStartedSpeaking;
			component.PlayerStoppedSpeaking += Net_PlayerStoppedSpeaking;
			component.TextPacketReceived += _text.OnMessageReceived;
			if (string.IsNullOrEmpty(LocalPlayerName))
			{
				string localPlayerName = Guid.NewGuid().ToString();
				LocalPlayerName = localPlayerName;
			}
			_started = true;
			_playbackPool.Start(PlaybackPrefab, base.transform);
			_codecSettingsLoader.Start();
			_players.Start(LocalPlayerName, _capture, Rooms, RoomChannels, PlayerChannels, _capture);
			component.Initialize(LocalPlayerName, Rooms, PlayerChannels, RoomChannels, _codecSettingsLoader.Config);
			_net = component;
			_capture.MicrophoneName = _micName;
			_capture.Start(_net, GetOrAddMicrophone());
			Log.Info("Starting Dissonance Voice Comms\n- Network: [{0}]\n- Quality Settings: [{1}]\n- Codec: [{2}]", _net, VoiceSettings.Instance, _codecSettingsLoader);
		}

		private IMicrophoneCapture GetOrAddMicrophone()
		{
			IMicrophoneCapture microphoneCapture = GetComponent<IMicrophoneCapture>();
			if (microphoneCapture == null)
			{
				microphoneCapture = base.gameObject.AddComponent<BasicMicrophoneCapture>();
			}
			return microphoneCapture;
		}

		[UsedImplicitly]
		private void OnEnable()
		{
		}

		[UsedImplicitly]
		private void OnDisable()
		{
		}

		private void OnEditorPlaymodeChanged()
		{
		}

		public void ResetMicrophoneCapture()
		{
			if (_capture != null)
			{
				_capture.ForceReset();
			}
		}

		private void Net_PlayerStoppedSpeaking([NotNull] string player)
		{
			VoicePlayerState state;
			if (player == null)
			{
				Log.Warn(Log.PossibleBugMessage("Received a player-stopped-speaking event for a null player ID", "5A424BF0-D384-4A63-B6E2-042A1F31A085"));
			}
			else if (_players.TryGet(player, out state))
			{
				state.InvokeOnStoppedSpeaking();
				if (this.OnPlayerStoppedSpeaking != null)
				{
					this.OnPlayerStoppedSpeaking(state);
				}
			}
		}

		private void Net_PlayerStartedSpeaking([NotNull] string player)
		{
			VoicePlayerState state;
			if (!Log.AssertAndLogError(player != null, "CA95E783-CA35-441B-9B8B-FAA0FA0B41E3", "Received a player-started-speaking event for a null player ID") && _players.TryGet(player, out state))
			{
				state.InvokeOnStartedSpeaking();
				if (this.OnPlayerStartedSpeaking != null)
				{
					this.OnPlayerStartedSpeaking(state);
				}
			}
		}

		private void Net_PlayerRoomEvent(RoomEvent evt)
		{
			VoicePlayerState state;
			if (Log.AssertAndLogError(evt.PlayerName != null, "221D74AD-4F5B-4215-9ADE-CC4D5B455327", "Received a remote room event with a null player name") || !_players.TryGet(evt.PlayerName, out state))
			{
				return;
			}
			if (evt.Joined)
			{
				state.InvokeOnEnteredRoom(evt);
				if (this.OnPlayerEnteredRoom != null)
				{
					this.OnPlayerEnteredRoom(state, evt.Room);
				}
			}
			else
			{
				state.InvokeOnExitedRoom(evt);
				if (this.OnPlayerExitedRoom != null)
				{
					this.OnPlayerExitedRoom(state, evt.Room);
				}
			}
		}

		private void Net_VoicePacketReceived(VoicePacket packet)
		{
			VoicePlayerState state;
			if (!Log.AssertAndLogError(packet.SenderPlayerId != null, "C0FE4E98-3CC9-466E-AA39-51F0B6D22D09", "Discarding a voice packet with a null player ID") && _players.TryGet(packet.SenderPlayerId, out state) && state.PlaybackInternal != null)
			{
				state.PlaybackInternal.ReceiveAudioPacket(packet);
			}
		}

		private void Net_PlayerLeft([NotNull] string playerId)
		{
			if (Log.AssertAndLogError(playerId != null, "37A2506B-6489-4679-BD72-1C53D69797B1", "Received a player-left event for a null player ID"))
			{
				return;
			}
			VoicePlayerState voicePlayerState = _players.Remove(playerId);
			if (voicePlayerState != null)
			{
				IVoicePlayback playback = voicePlayerState.Playback;
				if (playback != null)
				{
					_playbackPool.Put((VoicePlayback)playback);
				}
				_playerTrackers.RemovePlayer(voicePlayerState);
				voicePlayerState.InvokeOnLeftSession();
				if (this.OnPlayerLeftSession != null)
				{
					this.OnPlayerLeftSession(voicePlayerState);
				}
			}
		}

		private void Net_PlayerJoined([NotNull] string playerId, CodecSettings codecSettings)
		{
			if (!Log.AssertAndLogError(playerId != null, "86074592-4BAD-4DF5-9B2C-1DF42A68FAF8", "Received a player-joined event for a null player ID") && !(playerId == LocalPlayerName))
			{
				VoicePlayback voicePlayback = _playbackPool.Get(playerId);
				voicePlayback.CodecSettings = codecSettings;
				RemoteVoicePlayerState remoteVoicePlayerState = new RemoteVoicePlayerState(voicePlayback);
				_players.Add(remoteVoicePlayerState);
				_playerTrackers.AddPlayer(remoteVoicePlayerState);
				voicePlayback.gameObject.SetActive(true);
				if (this.OnPlayerJoinedSession != null)
				{
					this.OnPlayerJoinedSession(remoteVoicePlayerState);
				}
			}
		}

		[CanBeNull]
		public VoicePlayerState FindPlayer([NotNull] string playerId)
		{
			if (playerId == null)
			{
				throw new ArgumentNullException("playerId");
			}
			VoicePlayerState state;
			if (_players.TryGet(playerId, out state))
			{
				return state;
			}
			return null;
		}

		[UsedImplicitly]
		private void Update()
		{
			Logs.WriteMultithreadedLogs();
			_playbackPriorityManager.Update();
			_players.Update();
			_capture.Update(IsMuted, Time.deltaTime);
			_autoChannelDuck.Update(IsMuted);
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			_capture.Destroy();
		}

		public void SubcribeToVoiceActivation([NotNull] IVoiceActivationListener listener)
		{
			_capture.Subscribe(listener);
		}

		public void UnsubscribeFromVoiceActivation([NotNull] IVoiceActivationListener listener)
		{
			_capture.Unsubscribe(listener);
		}

		public void TrackPlayerPosition([NotNull] IDissonancePlayer player)
		{
			_playerTrackers.AddTracker(player);
		}

		public void StopTracking([NotNull] IDissonancePlayer player)
		{
			_playerTrackers.RemoveTracker(player);
		}

		public bool AddToken(string token)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token", "Cannot add a null token");
			}
			return _tokens.AddToken(token);
		}

		public bool RemoveToken(string token)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token", "Cannot remove a null token");
			}
			return _tokens.RemoveToken(token);
		}

		public bool ContainsToken(string token)
		{
			if (token == null)
			{
				throw new ArgumentNullException("token", "Cannot search for a null token");
			}
			return _tokens.ContainsToken(token);
		}

		public bool HasAnyToken([NotNull] TokenSet tokens)
		{
			if (tokens == null)
			{
				throw new ArgumentNullException("tokens", "Cannot intersect with a null set");
			}
			return _tokens.IntersectsWith(tokens);
		}
	}
}
