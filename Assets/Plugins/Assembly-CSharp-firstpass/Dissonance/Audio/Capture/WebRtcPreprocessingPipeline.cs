using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Dissonance.Config;
using Dissonance.Threading;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Capture
{
	internal class WebRtcPreprocessingPipeline : BasePreprocessingPipeline
	{
		internal sealed class WebRtcPreprocessor : IDisposable
		{
			public enum SampleRates
			{
				SampleRate8KHz = 8000,
				SampleRate16KHz = 16000,
				SampleRate32KHz = 32000,
				SampleRate48KHz = 48000
			}

			private enum ProcessorErrors
			{
				Ok = 0,
				Unspecified = -1,
				CreationFailed = -2,
				UnsupportedComponent = -3,
				UnsupportedFunction = -4,
				NullPointer = -5,
				BadParameter = -6,
				BadSampleRate = -7,
				BadDataLength = -8,
				BadNumberChannels = -9,
				FileError = -10,
				StreamParameterNotSet = -11,
				NotEnabled = -12
			}

			internal enum FilterState
			{
				FilterNotRunning = 0,
				FilterNoInstance = 1,
				FilterNoSamplesSubmitted = 2,
				FilterOk = 3
			}

			private readonly LockedValue<IntPtr> _handle;

			private readonly List<PropertyChangedEventHandler> _subscribed = new List<PropertyChangedEventHandler>();

			private readonly bool _useMobileAec;

			private NoiseSuppressionLevels _nsLevel;

			private AecSuppressionLevels _aecLevel;

			private AecmRoutingMode _aecmLevel;

			private NoiseSuppressionLevels NoiseSuppressionLevel
			{
				get
				{
					return _nsLevel;
				}
				set
				{
					using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
					{
						_nsLevel = value;
						if (unlocker.Value != IntPtr.Zero)
						{
							Dissonance_ConfigureNoiseSuppression(unlocker.Value, _nsLevel);
						}
					}
				}
			}

			private AecSuppressionLevels AecSuppressionLevel
			{
				get
				{
					return _aecLevel;
				}
				set
				{
					using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
					{
						_aecLevel = value;
						if (!_useMobileAec && unlocker.Value != IntPtr.Zero)
						{
							Dissonance_ConfigureAecSuppression(unlocker.Value, _aecLevel, AecmRoutingMode.Disabled);
						}
					}
				}
			}

			private AecmRoutingMode AecmSuppressionLevel
			{
				get
				{
					return _aecmLevel;
				}
				set
				{
					using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
					{
						_aecmLevel = value;
						if (_useMobileAec && unlocker.Value != IntPtr.Zero)
						{
							Dissonance_ConfigureAecSuppression(unlocker.Value, AecSuppressionLevels.Disabled, _aecmLevel);
						}
					}
				}
			}

			public WebRtcPreprocessor(bool useMobileAec)
			{
				_useMobileAec = useMobileAec;
				_nsLevel = VoiceSettings.Instance.DenoiseAmount;
				_aecLevel = VoiceSettings.Instance.AecSuppressionAmount;
				_aecmLevel = VoiceSettings.Instance.AecmRoutingMode;
				_handle = new LockedValue<IntPtr>(IntPtr.Zero);
			}

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern IntPtr Dissonance_CreatePreprocessor(NoiseSuppressionLevels nsLevel, AecSuppressionLevels aecLevel, bool aecDelayAgnostic, bool aecExtended, bool aecRefined, AecmRoutingMode aecmRoutingMode, bool aecmComfortNoise);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern void Dissonance_DestroyPreprocessor(IntPtr handle);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern void Dissonance_ConfigureNoiseSuppression(IntPtr handle, NoiseSuppressionLevels nsLevel);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern void Dissonance_ConfigureAecSuppression(IntPtr handle, AecSuppressionLevels aecLevel, AecmRoutingMode aecmRouting);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern bool Dissonance_GetVadSpeechState(IntPtr handle);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern ProcessorErrors Dissonance_PreprocessCaptureFrame(IntPtr handle, int sampleRate, float[] input, float[] output, int streamDelay);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern bool Dissonance_PreprocessorExchangeInstance(IntPtr previous, IntPtr replacement);

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int Dissonance_GetFilterState();

			[DllImport("AudioPluginDissonance", CallingConvention = CallingConvention.Cdecl)]
			private static extern void Dissonance_GetAecMetrics(IntPtr floatBuffer, int bufferLength);

			public bool Process(SampleRates inputSampleRate, float[] input, float[] output, int estimatedStreamDelay)
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw Log.CreatePossibleBugException("Attempted  to access a null WebRtc Preprocessor encoder", "5C97EF6A-353B-4B96-871F-1073746B5708");
					}
					ProcessorErrors processorErrors = Dissonance_PreprocessCaptureFrame(unlocker.Value, (int)inputSampleRate, input, output, estimatedStreamDelay);
					if (processorErrors != ProcessorErrors.Ok)
					{
						throw Log.CreatePossibleBugException(string.Format("Preprocessor error: '{0}'", processorErrors), "0A89A5E7-F527-4856-BA01-5A19578C6D88");
					}
					return Dissonance_GetVadSpeechState(unlocker.Value);
				}
			}

			public void Reset()
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
				{
					if (unlocker.Value != IntPtr.Zero)
					{
						ClearFilterPreprocessor();
						Dissonance_DestroyPreprocessor(unlocker.Value);
						unlocker.Value = IntPtr.Zero;
					}
					unlocker.Value = CreatePreprocessor();
					SetFilterPreprocessor(unlocker.Value);
				}
			}

			private IntPtr CreatePreprocessor()
			{
				VoiceSettings instance = VoiceSettings.Instance;
				AecSuppressionLevels aecLevel = AecSuppressionLevel;
				AecmRoutingMode aecmRoutingMode = AecmSuppressionLevel;
				if (_useMobileAec)
				{
					aecLevel = AecSuppressionLevels.Disabled;
				}
				else
				{
					aecmRoutingMode = AecmRoutingMode.Disabled;
				}
				return Dissonance_CreatePreprocessor(NoiseSuppressionLevel, aecLevel, instance.AecDelayAgnostic, instance.AecExtendedFilter, instance.AecRefinedAdaptiveFilter, aecmRoutingMode, instance.AecmComfortNoise);
			}

			private void SetFilterPreprocessor(IntPtr preprocessor)
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw Log.CreatePossibleBugException("Attempted  to access a null WebRtc Preprocessor encoder", "3BA66D46-A7A6-41E8-BE38-52AFE5212ACD");
					}
					if (!Dissonance_PreprocessorExchangeInstance(IntPtr.Zero, unlocker.Value))
					{
						throw Log.CreatePossibleBugException("Cannot associate preprocessor with Playback filter - one already exists", "D5862DD2-B44E-4605-8D1C-29DD2C72A70C");
					}
					if (Dissonance_GetFilterState() == 0)
					{
					}
					Bind((VoiceSettings s) => s.DenoiseAmount, "DenoiseAmount", delegate(NoiseSuppressionLevels v)
					{
						NoiseSuppressionLevel = v;
					});
					Bind((VoiceSettings s) => s.AecSuppressionAmount, "AecSuppressionAmount", delegate(AecSuppressionLevels v)
					{
						AecSuppressionLevel = v;
					});
					Bind((VoiceSettings s) => s.AecmRoutingMode, "AecmRoutingMode", delegate(AecmRoutingMode v)
					{
						AecmSuppressionLevel = v;
					});
				}
			}

			private void Bind<T>(Func<VoiceSettings, T> getValue, string propertyName, Action<T> setValue)
			{
				VoiceSettings settings = VoiceSettings.Instance;
				PropertyChangedEventHandler propertyChangedEventHandler;
				settings.PropertyChanged += (propertyChangedEventHandler = delegate(object sender, PropertyChangedEventArgs args)
				{
					if (args.PropertyName == propertyName)
					{
						setValue(getValue(settings));
					}
				});
				_subscribed.Add(propertyChangedEventHandler);
				propertyChangedEventHandler(settings, new PropertyChangedEventArgs(propertyName));
			}

			private bool ClearFilterPreprocessor(bool throwOnError = true)
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw Log.CreatePossibleBugException("Attempted  to access a null WebRtc Preprocessor encoder", "2DBC7779-F1B9-45F2-9372-3268FD8D7EBA");
					}
					if (!Dissonance_PreprocessorExchangeInstance(unlocker.Value, IntPtr.Zero))
					{
						if (throwOnError)
						{
							throw Log.CreatePossibleBugException("Cannot clear preprocessor from Playback filter", "6323106A-04BD-4217-9ECA-6FD49BF04FF0");
						}
						Log.Error("Failed to clear preprocessor from playback filter", "CBC6D727-BE07-4073-AA5A-F750A0CC023D");
						return false;
					}
					VoiceSettings instance = VoiceSettings.Instance;
					for (int i = 0; i < _subscribed.Count; i++)
					{
						instance.PropertyChanged -= _subscribed[i];
					}
					_subscribed.Clear();
					return true;
				}
			}

			private void ReleaseUnmanagedResources()
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _handle.Lock())
				{
					if (unlocker.Value != IntPtr.Zero)
					{
						ClearFilterPreprocessor(false);
						Dissonance_DestroyPreprocessor(unlocker.Value);
						unlocker.Value = IntPtr.Zero;
					}
				}
			}

			public void Dispose()
			{
				ReleaseUnmanagedResources();
				GC.SuppressFinalize(this);
			}

			~WebRtcPreprocessor()
			{
				ReleaseUnmanagedResources();
			}
		}

		private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(WebRtcPreprocessingPipeline).Name);

		private bool _isVadDetectingSpeech;

		private readonly WebRtcPreprocessor _preprocessor;

		protected override bool VadIsSpeechDetected
		{
			get
			{
				return _isVadDetectingSpeech;
			}
		}

		public WebRtcPreprocessingPipeline([NotNull] WaveFormat inputFormat, bool mobilePlatform)
			: base(inputFormat, 480, 48000, 480, 48000)
		{
			_preprocessor = new WebRtcPreprocessor(mobilePlatform);
		}

		public override void Dispose()
		{
			base.Dispose();
			_preprocessor.Dispose();
		}

		protected override void ApplyReset()
		{
			_preprocessor.Reset();
			base.ApplyReset();
		}

		protected override void PreprocessAudioFrame(float[] frame)
		{
			AudioConfiguration configuration = AudioSettingsWatcher.Instance.Configuration;
			int preprocessorLatencyMs = base.PreprocessorLatencyMs;
			int num = (int)(1000f * ((float)configuration.dspBufferSize / (float)configuration.sampleRate));
			int estimatedStreamDelay = preprocessorLatencyMs + num;
			_isVadDetectingSpeech = _preprocessor.Process(WebRtcPreprocessor.SampleRates.SampleRate48KHz, frame, frame, estimatedStreamDelay);
			SendSamplesToSubscribers(frame);
		}

		internal static WebRtcPreprocessor.FilterState GetAecFilterState()
		{
			return (WebRtcPreprocessor.FilterState)WebRtcPreprocessor.Dissonance_GetFilterState();
		}
	}
}
