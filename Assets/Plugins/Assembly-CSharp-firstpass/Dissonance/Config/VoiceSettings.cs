using System;
using System.ComponentModel;
using System.IO;
using Dissonance.Audio.Capture;
using UnityEngine;

namespace Dissonance.Config
{
	public sealed class VoiceSettings : ScriptableObject, INotifyPropertyChanged
	{
		private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(VoiceSettings).Name);

		private const string PersistName_Quality = "Dissonance_Audio_Quality";

		private const string PersistName_FrameSize = "Dissonance_Audio_FrameSize";

		private const string PersistName_Fec = "Dissonance_Audio_DisableFEC";

		private const string PersistName_DenoiseAmount = "Dissonance_Audio_Denoise_Amount";

		private const string PersistName_PttDuckAmount = "Dissonance_Audio_Duck_Amount";

		private const string PersistName_AecSuppressionAmount = "Dissonance_Audio_Aec_Suppression_Amount";

		private const string PersistName_AecDelayAgnostic = "Dissonance_Audio_Aec_Delay_Agnostic";

		private const string PersistName_AecExtendedFilter = "Dissonance_Audio_Aec_Extended_Filter";

		private const string PersistName_AecRefinedAdaptiveFilter = "Dissonance_Audio_Aec_Refined_Adaptive_Filter";

		private const string PersistName_AecmRoutingMode = "Dissonance_Audio_Aecm_Routing_Mode";

		private const string PersistName_AecmComfortNoise = "Dissonance_Audio_Aecm_Comfort_Noise";

		private const string SettingsFileResourceName = "VoiceSettings";

		public static readonly string SettingsFilePath = Path.Combine("Assets/Plugins/Dissonance/Resources", "VoiceSettings.asset");

		[SerializeField]
		private AudioQuality _quality;

		[SerializeField]
		private FrameSize _frameSize;

		[SerializeField]
		private int _forwardErrorCorrection;

		[SerializeField]
		private int _denoiseAmount;

		[SerializeField]
		private int _aecAmount;

		[SerializeField]
		private int _aecDelayAgnostic;

		[SerializeField]
		private int _aecExtendedFilter;

		[SerializeField]
		private int _aecRefinedAdaptiveFilter;

		[SerializeField]
		private int _aecmRoutingMode;

		[SerializeField]
		private int _aecmComfortNoise;

		[SerializeField]
		private float _voiceDuckLevel;

		private static VoiceSettings _instance;

		public AudioQuality Quality
		{
			get
			{
				return _quality;
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Quality", ref _quality, value, delegate(string key, AudioQuality q)
				{
					PlayerPrefs.SetInt(key, (int)q);
				}, Log);
				OnPropertyChanged("Quality");
			}
		}

		public FrameSize FrameSize
		{
			get
			{
				return _frameSize;
			}
			set
			{
				Preferences.Set("Dissonance_Audio_FrameSize", ref _frameSize, value, delegate(string key, FrameSize f)
				{
					PlayerPrefs.SetInt(key, (int)f);
				}, Log);
				OnPropertyChanged("FrameSize");
			}
		}

		public bool ForwardErrorCorrection
		{
			get
			{
				return Convert.ToBoolean(_forwardErrorCorrection);
			}
			set
			{
				Preferences.Set("Dissonance_Audio_DisableFEC", ref _forwardErrorCorrection, Convert.ToInt32(value), PlayerPrefs.SetInt, Log);
				OnPropertyChanged("ForwardErrorCorrection");
			}
		}

		public NoiseSuppressionLevels DenoiseAmount
		{
			get
			{
				return (NoiseSuppressionLevels)_denoiseAmount;
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Denoise_Amount", ref _denoiseAmount, (int)value, PlayerPrefs.SetInt, Log);
				OnPropertyChanged("DenoiseAmount");
			}
		}

		public AecSuppressionLevels AecSuppressionAmount
		{
			get
			{
				return (AecSuppressionLevels)_aecAmount;
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Aec_Suppression_Amount", ref _aecAmount, (int)value, PlayerPrefs.SetInt, Log);
				OnPropertyChanged("AecSuppressionAmount");
			}
		}

		public bool AecDelayAgnostic
		{
			get
			{
				return Convert.ToBoolean(_aecDelayAgnostic);
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Aec_Delay_Agnostic", ref _aecDelayAgnostic, Convert.ToInt32(value), PlayerPrefs.SetInt, Log);
				OnPropertyChanged("AecDelayAgnostic");
			}
		}

		public bool AecExtendedFilter
		{
			get
			{
				return Convert.ToBoolean(_aecExtendedFilter);
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Aec_Extended_Filter", ref _aecExtendedFilter, Convert.ToInt32(value), PlayerPrefs.SetInt, Log);
				OnPropertyChanged("AecExtendedFilter");
			}
		}

		public bool AecRefinedAdaptiveFilter
		{
			get
			{
				return Convert.ToBoolean(_aecRefinedAdaptiveFilter);
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Aec_Refined_Adaptive_Filter", ref _aecRefinedAdaptiveFilter, Convert.ToInt32(value), PlayerPrefs.SetInt, Log);
				OnPropertyChanged("AecRefinedAdaptiveFilter");
			}
		}

		public AecmRoutingMode AecmRoutingMode
		{
			get
			{
				return (AecmRoutingMode)_aecmRoutingMode;
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Aecm_Routing_Mode", ref _aecmRoutingMode, (int)value, PlayerPrefs.SetInt, Log);
				OnPropertyChanged("AecmRoutingMode");
			}
		}

		public bool AecmComfortNoise
		{
			get
			{
				return Convert.ToBoolean(_aecmComfortNoise);
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Aecm_Comfort_Noise", ref _aecmComfortNoise, Convert.ToInt32(value), PlayerPrefs.SetInt, Log);
				OnPropertyChanged("AecmComfortNoise");
			}
		}

		public float VoiceDuckLevel
		{
			get
			{
				return _voiceDuckLevel;
			}
			set
			{
				Preferences.Set("Dissonance_Audio_Duck_Amount", ref _voiceDuckLevel, value, PlayerPrefs.SetFloat, Log);
				OnPropertyChanged("VoiceDuckLevel");
			}
		}

		[NotNull]
		public static VoiceSettings Instance
		{
			get
			{
				return _instance ?? (_instance = Load());
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public VoiceSettings()
		{
			_quality = AudioQuality.Medium;
			_frameSize = FrameSize.Medium;
			_forwardErrorCorrection = Convert.ToInt32(true);
			_denoiseAmount = 2;
			_aecAmount = -1;
			_aecDelayAgnostic = Convert.ToInt32(true);
			_aecExtendedFilter = Convert.ToInt32(true);
			_aecRefinedAdaptiveFilter = Convert.ToInt32(true);
			_aecmRoutingMode = -1;
			_aecmComfortNoise = Convert.ToInt32(true);
			_voiceDuckLevel = 0.8f;
		}

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChangedEventHandler = this.PropertyChanged;
			if (propertyChangedEventHandler != null)
			{
				propertyChangedEventHandler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public static void Preload()
		{
			if (_instance == null)
			{
				_instance = Load();
			}
		}

		[NotNull]
		private static VoiceSettings Load()
		{
			VoiceSettings voiceSettings = Resources.Load<VoiceSettings>("VoiceSettings") ?? ScriptableObject.CreateInstance<VoiceSettings>();
			Preferences.Get("Dissonance_Audio_Quality", ref voiceSettings._quality, (string s, AudioQuality q) => (AudioQuality)PlayerPrefs.GetInt(s, (int)q), Log);
			Preferences.Get("Dissonance_Audio_FrameSize", ref voiceSettings._frameSize, (string s, FrameSize f) => (FrameSize)PlayerPrefs.GetInt(s, (int)f), Log);
			Preferences.Get("Dissonance_Audio_DisableFEC", ref voiceSettings._forwardErrorCorrection, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Denoise_Amount", ref voiceSettings._denoiseAmount, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Aec_Suppression_Amount", ref voiceSettings._aecAmount, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Aec_Delay_Agnostic", ref voiceSettings._aecDelayAgnostic, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Aec_Extended_Filter", ref voiceSettings._aecExtendedFilter, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Aec_Refined_Adaptive_Filter", ref voiceSettings._aecRefinedAdaptiveFilter, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Aecm_Routing_Mode", ref voiceSettings._aecmRoutingMode, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Aecm_Comfort_Noise", ref voiceSettings._aecmRoutingMode, PlayerPrefs.GetInt, Log);
			Preferences.Get("Dissonance_Audio_Duck_Amount", ref voiceSettings._voiceDuckLevel, PlayerPrefs.GetFloat, Log);
			return voiceSettings;
		}

		public override string ToString()
		{
			return string.Format("Quality: {0}, FrameSize: {1}, FEC: {2}, DenoiseAmount: {3}, VoiceDuckLevel: {4}", Quality, FrameSize, ForwardErrorCorrection, DenoiseAmount, VoiceDuckLevel);
		}
	}
}
