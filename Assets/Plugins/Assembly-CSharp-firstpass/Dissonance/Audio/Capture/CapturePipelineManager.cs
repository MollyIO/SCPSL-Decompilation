using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Networking;
using Dissonance.VAD;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal class CapturePipelineManager : IAmplitudeProvider, ILossEstimator
	{
		private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(CapturePipelineManager).Name);

		private bool _isMobilePlatform;

		private readonly CodecSettingsLoader _codecSettingsLoader;

		private readonly RoomChannels _roomChannels;

		private readonly PlayerChannels _playerChannels;

		private readonly PacketLossMonitor _receivingPacketLossMonitor;

		[CanBeNull]
		private ICommsNetwork _network;

		private IMicrophoneCapture _microphone;

		private IPreprocessingPipeline _preprocessor;

		private EncoderPipeline _encoder;

		private bool _netModeRequiresPipeline;

		private bool _encoderSubscribed;

		private FrameSkipDetector _skipDetector = new FrameSkipDetector(TimeSpan.FromMilliseconds(150.0), TimeSpan.FromMilliseconds(350.0), TimeSpan.FromMilliseconds(10000.0), TimeSpan.FromMilliseconds(250.0));

		private readonly List<IVoiceActivationListener> _activationListeners = new List<IVoiceActivationListener>();

		private string _micName;

		private bool _pendingResetRequest;

		[CanBeNull]
		public IMicrophoneCapture Microphone
		{
			get
			{
				return _microphone;
			}
		}

		public string MicrophoneName
		{
			get
			{
				return _micName;
			}
			set
			{
				if (!(_micName == value) && (!string.IsNullOrEmpty(_micName) || !string.IsNullOrEmpty(value)))
				{
					if (_microphone != null && _microphone.IsRecording)
					{
						Log.Info("Changing microphone device from '{0}' to '{1}'", _micName, value);
					}
					_micName = value;
					RestartTransmissionPipeline("Microphone name changed");
				}
			}
		}

		public float PacketLoss
		{
			get
			{
				return _receivingPacketLossMonitor.PacketLoss;
			}
		}

		public float Amplitude
		{
			get
			{
				return (_preprocessor != null) ? _preprocessor.Amplitude : 0f;
			}
		}

		public CapturePipelineManager([NotNull] CodecSettingsLoader codecSettingsLoader, [NotNull] RoomChannels roomChannels, [NotNull] PlayerChannels playerChannels, [NotNull] ReadOnlyCollection<VoicePlayerState> players)
		{
			if (codecSettingsLoader == null)
			{
				throw new ArgumentNullException("codecSettingsLoader");
			}
			if (roomChannels == null)
			{
				throw new ArgumentNullException("roomChannels");
			}
			if (playerChannels == null)
			{
				throw new ArgumentNullException("playerChannels");
			}
			if (players == null)
			{
				throw new ArgumentNullException("players");
			}
			_codecSettingsLoader = codecSettingsLoader;
			_roomChannels = roomChannels;
			_playerChannels = playerChannels;
			_receivingPacketLossMonitor = new PacketLossMonitor(players);
		}

		public void Start([NotNull] ICommsNetwork network, [NotNull] IMicrophoneCapture microphone)
		{
			if (network == null)
			{
				throw new ArgumentNullException("network");
			}
			if (microphone == null)
			{
				throw new ArgumentNullException("microphone");
			}
			_microphone = microphone;
			_network = network;
			AudioSettingsWatcher.Instance.Start();
			Net_ModeChanged(network.Mode);
			network.ModeChanged += Net_ModeChanged;
			_isMobilePlatform = IsMobilePlatform();
		}

		private static bool IsMobilePlatform()
		{
			return false;
		}

		public void Destroy()
		{
			if (!object.ReferenceEquals(_network, null))
			{
				_network.ModeChanged -= Net_ModeChanged;
			}
			StopTransmissionPipeline();
		}

		private void Net_ModeChanged(NetworkMode mode)
		{
			_netModeRequiresPipeline = mode.IsClientEnabled();
		}

		public void Update(bool muted, float deltaTime)
		{
			_receivingPacketLossMonitor.Update();
			if (!_netModeRequiresPipeline)
			{
				StopTransmissionPipeline();
				return;
			}
			bool flag = _skipDetector.IsFrameSkip(deltaTime);
			bool flag2 = _microphone.IsRecording && _microphone.UpdateSubscribers();
			bool flag3 = _netModeRequiresPipeline && _encoder == null;
			if (flag || flag2 || _pendingResetRequest || flag3)
			{
				string text = (flag ? "Detected a frame skip, forcing capture pipeline reset" : (flag3 ? "Network mode changed" : ((!_pendingResetRequest) ? "Microphone requested a pipeline reset" : "Applying external reset request")));
				if (flag)
				{
					if (Log.IsWarn)
					{
						text = string.Format("Detected a frame skip, forcing capture pipeline reset (Delta Time:{0})", deltaTime);
					}
					Log.Warn(text);
				}
				RestartTransmissionPipeline(text);
			}
			if (_encoder == null)
			{
				return;
			}
			if (_encoder.Stopped && _encoderSubscribed)
			{
				_preprocessor.Unsubscribe(_encoder);
				_encoder.Reset();
				_encoderSubscribed = false;
			}
			bool flag4 = (!_encoder.Stopping || _encoder.Stopped) && !muted && _roomChannels.Count + _playerChannels.Count > 0;
			if (flag4 != _encoderSubscribed)
			{
				if (flag4)
				{
					_encoder.Reset();
					_preprocessor.Subscribe(_encoder);
					_encoderSubscribed = true;
				}
				else if (!_encoder.Stopping)
				{
					_encoder.Stop();
				}
			}
			if (_encoder != null)
			{
				_encoder.TransmissionPacketLoss = _receivingPacketLossMonitor.PacketLoss;
			}
		}

		private void StopTransmissionPipeline()
		{
			if (_microphone != null && _microphone.IsRecording)
			{
				_microphone.StopCapture();
			}
			if (_preprocessor != null)
			{
				if (_microphone != null)
				{
					_microphone.Unsubscribe(_preprocessor);
				}
				if (_encoder != null)
				{
					_preprocessor.Unsubscribe(_encoder);
				}
				_preprocessor.Dispose();
				_preprocessor = null;
			}
			if (_encoder != null)
			{
				_encoder.Dispose();
				_encoder = null;
			}
			_encoderSubscribed = false;
		}

		private void RestartTransmissionPipeline(string reason)
		{
			StopTransmissionPipeline();
			_pendingResetRequest = false;
			if (_network == null || !_network.Mode.IsClientEnabled())
			{
				return;
			}
			WaveFormat waveFormat = _microphone.StartCapture(_micName);
			if (waveFormat != null)
			{
				_roomChannels.Refresh();
				_playerChannels.Refresh();
				_preprocessor = CreatePreprocessor(waveFormat);
				_preprocessor.UpstreamLatency = _microphone.Latency;
				_preprocessor.Start();
				_microphone.Subscribe(_preprocessor);
				for (int i = 0; i < _activationListeners.Count; i++)
				{
					_preprocessor.Subscribe(_activationListeners[i]);
				}
				Log.AssertAndThrowPossibleBug(_network != null, "5F33336B-15B5-4A85-9B54-54352C74768E", "Network object is unexpectedly null");
				_encoder = new EncoderPipeline(_preprocessor.OutputFormat, _codecSettingsLoader.CreateEncoder(), _network);
			}
			else
			{
				Log.Warn("Failed to start microphone capture; local voice transmission will be disabled.");
			}
		}

		[NotNull]
		protected virtual IPreprocessingPipeline CreatePreprocessor([NotNull] WaveFormat format)
		{
			return new WebRtcPreprocessingPipeline(format, _isMobilePlatform);
		}

		public void Subscribe([NotNull] IVoiceActivationListener listener)
		{
			if (listener == null)
			{
				throw new ArgumentNullException("listener", "Cannot subscribe with a null listener");
			}
			_activationListeners.Add(listener);
			if (_preprocessor != null)
			{
				_preprocessor.Subscribe(listener);
			}
		}

		public void Unsubscribe([NotNull] IVoiceActivationListener listener)
		{
			if (listener == null)
			{
				throw new ArgumentNullException("listener", "Cannot unsubscribe with a null listener");
			}
			_activationListeners.Remove(listener);
			if (_preprocessor != null)
			{
				_preprocessor.Unsubscribe(listener);
			}
		}

		public void ForceReset()
		{
			Log.Warn("Forcing capture pipeline reset");
			_pendingResetRequest = true;
		}
	}
}
