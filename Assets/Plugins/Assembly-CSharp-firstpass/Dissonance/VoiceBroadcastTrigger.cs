using System;
using Dissonance.Audio;
using Dissonance.VAD;
using UnityEngine;

namespace Dissonance
{
	[HelpURL("https://placeholder-software.co.uk/dissonance/docs/Reference/Components/Voice-Broadcast-Trigger/")]
	public class VoiceBroadcastTrigger : BaseCommsTrigger, IVoiceActivationListener
	{
		private PlayerChannel? _playerChannel;

		private RoomChannel? _roomChannel;

		private bool _isVadSpeaking;

		private CommActivationMode? _previousMode;

		private IDissonancePlayer _self;

		private Fader _activationFader = default(Fader);

		[SerializeField]
		private VolumeFaderSettings _activationFaderSettings = new VolumeFaderSettings
		{
			Volume = 1f,
			FadeIn = TimeSpan.Zero,
			FadeOut = TimeSpan.FromSeconds(0.15000000596046448)
		};

		private Fader _triggerFader = default(Fader);

		[SerializeField]
		private VolumeFaderSettings _triggerFaderSettings = new VolumeFaderSettings
		{
			Volume = 1f,
			FadeIn = TimeSpan.FromSeconds(0.75),
			FadeOut = TimeSpan.FromSeconds(1.149999976158142)
		};

		[SerializeField]
		private bool _broadcastPosition = true;

		[SerializeField]
		private CommTriggerTarget _channelType;

		[SerializeField]
		private string _inputName;

		[SerializeField]
		private CommActivationMode _mode = CommActivationMode.VoiceActivation;

		[SerializeField]
		private string _playerId;

		[SerializeField]
		private bool _useTrigger;

		[SerializeField]
		private string _roomName;

		[SerializeField]
		private ChannelPriority _priority = ChannelPriority.None;

		public static bool is939Speaking;

		[NotNull]
		public VolumeFaderSettings ActivationFader
		{
			get
			{
				return _activationFaderSettings;
			}
		}

		[NotNull]
		public VolumeFaderSettings ColliderTriggerFader
		{
			get
			{
				return _triggerFaderSettings;
			}
		}

		public float CurrentFaderVolume
		{
			get
			{
				return _activationFader.Volume * ((!UseColliderTrigger) ? 1f : _triggerFader.Volume);
			}
		}

		public bool BroadcastPosition
		{
			get
			{
				return _broadcastPosition;
			}
			set
			{
				if (_broadcastPosition != value)
				{
					_broadcastPosition = value;
					if (_playerChannel.HasValue)
					{
						PlayerChannel value2 = _playerChannel.Value;
						value2.Positional = value;
					}
					if (_roomChannel.HasValue)
					{
						RoomChannel value3 = _roomChannel.Value;
						value3.Positional = value;
					}
				}
			}
		}

		public CommTriggerTarget ChannelType
		{
			get
			{
				return _channelType;
			}
			set
			{
				if (_channelType != value)
				{
					_channelType = value;
					CloseChannel();
				}
			}
		}

		public string InputName
		{
			get
			{
				return _inputName;
			}
			set
			{
				_inputName = value;
			}
		}

		public CommActivationMode Mode
		{
			get
			{
				return _mode;
			}
			set
			{
				_mode = value;
			}
		}

		public string PlayerId
		{
			get
			{
				return _playerId;
			}
			set
			{
				if (_playerId != value)
				{
					_playerId = value;
					if (_channelType == CommTriggerTarget.Player)
					{
						CloseChannel();
					}
				}
			}
		}

		public override bool UseColliderTrigger
		{
			get
			{
				return _useTrigger;
			}
			set
			{
				_useTrigger = value;
			}
		}

		public string RoomName
		{
			get
			{
				return _roomName;
			}
			set
			{
				if (_roomName != value)
				{
					_roomName = value;
					if (_channelType == CommTriggerTarget.Room)
					{
						CloseChannel();
					}
				}
			}
		}

		public ChannelPriority Priority
		{
			get
			{
				return _priority;
			}
			set
			{
				if (_priority != value)
				{
					_priority = value;
					if (_playerChannel.HasValue)
					{
						PlayerChannel value2 = _playerChannel.Value;
						value2.Priority = value;
					}
					if (_roomChannel.HasValue)
					{
						RoomChannel value3 = _roomChannel.Value;
						value3.Priority = value;
					}
				}
			}
		}

		public bool IsTransmitting
		{
			get
			{
				PlayerChannel? playerChannel = _playerChannel;
				int result;
				if (!playerChannel.HasValue)
				{
					RoomChannel? roomChannel = _roomChannel;
					result = (roomChannel.HasValue ? 1 : 0);
				}
				else
				{
					result = 1;
				}
				return (byte)result != 0;
			}
		}

		public override bool CanTrigger
		{
			get
			{
				if (base.Comms == null || !base.Comms.IsStarted)
				{
					return false;
				}
				if (_channelType == CommTriggerTarget.Self && _self == null)
				{
					return false;
				}
				if (_channelType == CommTriggerTarget.Self && _self != null && _self.Type == NetworkPlayerType.Local)
				{
					return false;
				}
				if (_channelType == CommTriggerTarget.Player && base.Comms.LocalPlayerName == _playerId)
				{
					return false;
				}
				return true;
			}
		}

		protected override void Start()
		{
			base.Start();
			_self = GetComponent<IDissonancePlayer>();
		}

		protected override void OnDisable()
		{
			CloseChannel();
			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			CloseChannel();
			if (base.Comms != null)
			{
				base.Comms.UnsubscribeFromVoiceActivation(this);
			}
			base.OnDestroy();
		}

		protected override void Update()
		{
			base.Update();
			if (!CheckVoiceComm())
			{
				return;
			}
			CommActivationMode? previousMode = _previousMode;
			if (previousMode.GetValueOrDefault() != Mode || !previousMode.HasValue)
			{
				SwitchMode();
			}
			_triggerFader.Update(Time.deltaTime);
			_activationFader.Update(Time.deltaTime);
			SetChannelVolume(CurrentFaderVolume);
			bool isTransmitting = IsTransmitting;
			bool flag = ShouldActivate(IsUserActivated());
			if (isTransmitting != flag)
			{
				if (isTransmitting)
				{
					if (Math.Abs(_activationFader.EndVolume) > float.Epsilon)
					{
						_activationFader.FadeTo(0f, (float)_activationFaderSettings.FadeOut.TotalSeconds);
					}
					if (CurrentFaderVolume <= float.Epsilon)
					{
						CloseChannel();
					}
				}
				else
				{
					OpenChannel();
				}
			}
			else if (isTransmitting && Math.Abs(_activationFader.EndVolume - _activationFaderSettings.Volume) > float.Epsilon)
			{
				_activationFader.FadeTo(1f, (float)_activationFaderSettings.FadeIn.TotalSeconds);
			}
		}

		protected override void ColliderTriggerChanged()
		{
			base.ColliderTriggerChanged();
			if (base.IsColliderTriggered)
			{
				_triggerFader.FadeTo(_triggerFaderSettings.Volume, (float)_triggerFaderSettings.FadeIn.TotalSeconds);
			}
			else
			{
				_triggerFader.FadeTo(0f, (float)_triggerFaderSettings.FadeOut.TotalSeconds);
			}
		}

		private void SwitchMode()
		{
			if (CheckVoiceComm())
			{
				CloseChannel();
				CommActivationMode? previousMode = _previousMode;
				if (previousMode == CommActivationMode.VoiceActivation && previousMode.HasValue && Mode != CommActivationMode.VoiceActivation)
				{
					base.Comms.UnsubscribeFromVoiceActivation(this);
					_isVadSpeaking = false;
				}
				if (Mode == CommActivationMode.VoiceActivation)
				{
					base.Comms.SubcribeToVoiceActivation(this);
				}
				_previousMode = Mode;
			}
		}

		private bool ShouldActivate(bool intent)
		{
			if (!intent)
			{
				return false;
			}
			if (!CanTrigger)
			{
				if (_channelType == CommTriggerTarget.Self && _self == null)
				{
					Log.Error("Attempting to broadcast to 'Self' but no sibling IDissonancePlayer component found");
				}
				return false;
			}
			if (!base.TokenActivationState)
			{
				return false;
			}
			if (UseColliderTrigger && !base.IsColliderTriggered)
			{
				return false;
			}
			return true;
		}

		private bool IsUserActivated()
		{
			switch (Mode)
			{
			case CommActivationMode.VoiceActivation:
				return _isVadSpeaking;
			case CommActivationMode.PushToTalk:
				return is939Speaking || Input.GetKey(NewInput.GetKey(InputName));
			case CommActivationMode.None:
				return false;
			default:
				Log.Error("Unknown Activation Mode '{0}'", Mode);
				return false;
			}
		}

		private void SetChannelVolume(float value)
		{
			if (_playerChannel.HasValue)
			{
				PlayerChannel value2 = _playerChannel.Value;
				value2.Volume = value;
			}
			if (_roomChannel.HasValue)
			{
				RoomChannel value3 = _roomChannel.Value;
				value3.Volume = value;
			}
		}

		private void OpenChannel()
		{
			if (!CheckVoiceComm())
			{
				return;
			}
			if (ChannelType == CommTriggerTarget.Room)
			{
				_roomChannel = base.Comms.RoomChannels.Open(RoomName, _broadcastPosition, _priority, CurrentFaderVolume);
			}
			else if (ChannelType == CommTriggerTarget.Player)
			{
				if (PlayerId != null)
				{
					_playerChannel = base.Comms.PlayerChannels.Open(PlayerId, _broadcastPosition, _priority, CurrentFaderVolume);
				}
				else
				{
					Log.Warn("Attempting to transmit to a null player ID");
				}
			}
			else if (ChannelType == CommTriggerTarget.Self)
			{
				if (_self == null)
				{
					Log.Warn("Attempting to transmit to a null player object");
				}
				else if (_self.PlayerId != null)
				{
					_playerChannel = base.Comms.PlayerChannels.Open(_self.PlayerId, _broadcastPosition, _priority);
				}
			}
		}

		private void CloseChannel()
		{
			RoomChannel? roomChannel = _roomChannel;
			if (roomChannel.HasValue)
			{
				_roomChannel.Value.Dispose();
				_roomChannel = null;
			}
			PlayerChannel? playerChannel = _playerChannel;
			if (playerChannel.HasValue)
			{
				_playerChannel.Value.Dispose();
				_playerChannel = null;
			}
		}

		void IVoiceActivationListener.VoiceActivationStart()
		{
			_isVadSpeaking = true;
		}

		void IVoiceActivationListener.VoiceActivationStop()
		{
			_isVadSpeaking = false;
		}
	}
}
