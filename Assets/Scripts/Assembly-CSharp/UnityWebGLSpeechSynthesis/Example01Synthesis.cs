using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
	public class Example01Synthesis : MonoBehaviour
	{
		public Text _mTextWarning;

		public Text _mTextSummary;

		public Text _mTextPitch;

		public Text _mTextRate;

		public Dropdown _mDropdownVoices;

		public InputField _mInputField;

		public Slider _mSliderPitch;

		public Slider _mSliderRate;

		public Button _mButtonSpeak;

		public Button _mButtonStop;

		private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin;

		private VoiceResult _mVoiceResult;

		private SpeechSynthesisUtterance _mSpeechSynthesisUtterance;

		private IEnumerator _mSetPitch;

		private IEnumerator _mSetRate;

		private bool _mUtteranceSet;

		private bool _mVoicesSet;

		private bool _mGetVoices;

		private void Awake()
		{
			SpeechSynthesisUtils.SetActive(false, _mTextSummary);
			SpeechSynthesisUtils.SetInteractable(false, _mButtonSpeak, _mButtonStop, _mDropdownVoices, _mSliderPitch, _mSliderRate, _mInputField);
		}

		private IEnumerator Start()
		{
			_mSpeechSynthesisPlugin = WebGLSpeechSynthesisPlugin.GetInstance();
			if (_mSpeechSynthesisPlugin == null)
			{
				Debug.LogError("WebGL Speech Synthesis Plugin is not set!");
				yield break;
			}
			while (!_mSpeechSynthesisPlugin.IsAvailable())
			{
				yield return null;
			}
			SpeechSynthesisUtils.SetActive(false, _mTextWarning);
			SpeechSynthesisUtils.SetActive(true, _mTextSummary);
			if ((bool)_mInputField)
			{
				_mInputField.text = "Hello! Text to speech is great! Thumbs up!";
			}
			StartCoroutine(GetVoices());
			_mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance(delegate(SpeechSynthesisUtterance utterance)
			{
				_mSpeechSynthesisUtterance = utterance;
				SpeechSynthesisUtils.SetInteractable(true, _mButtonSpeak, _mButtonStop, _mSliderPitch, _mSliderRate, _mInputField);
				_mUtteranceSet = true;
				SetIfReadyForDefaultVoice();
			});
			if ((bool)_mSliderPitch)
			{
				_mSliderPitch.onValueChanged.AddListener(delegate(float val)
				{
					_mSetPitch = SetPitch(Mathf.Lerp(0.1f, 2f, val));
				});
			}
			if ((bool)_mSliderRate)
			{
				_mSliderRate.onValueChanged.AddListener(delegate(float val)
				{
					_mSetRate = SetRate(Mathf.Lerp(0.1f, 2f, val));
				});
			}
			if ((bool)_mButtonSpeak)
			{
				_mButtonSpeak.onClick.AddListener(Speak);
			}
			if ((bool)_mButtonStop)
			{
				_mButtonStop.onClick.AddListener(delegate
				{
					_mSpeechSynthesisPlugin.Cancel();
				});
			}
		}

		private IEnumerator GetVoices()
		{
			yield return new WaitForSeconds(0.25f);
			_mSpeechSynthesisPlugin.GetVoices(delegate(VoiceResult voiceResult)
			{
				if (voiceResult == null)
				{
					_mGetVoices = true;
				}
				else
				{
					_mVoiceResult = voiceResult;
					SpeechSynthesisUtils.PopulateVoicesDropdown(_mDropdownVoices, _mVoiceResult);
					_mVoicesSet = true;
					SetIfReadyForDefaultVoice();
				}
			});
		}

		private void SetIfReadyForDefaultVoice()
		{
			if (!_mVoicesSet || !_mUtteranceSet)
			{
				return;
			}
			SpeechSynthesisUtils.RestoreVoice(_mDropdownVoices);
			SpeechSynthesisUtils.SetInteractable(true, _mDropdownVoices);
			if ((bool)_mDropdownVoices)
			{
				_mDropdownVoices.onValueChanged.AddListener(delegate
				{
					SpeechSynthesisUtils.HandleVoiceChangedDropdown(_mDropdownVoices, _mVoiceResult, _mSpeechSynthesisUtterance, _mSpeechSynthesisPlugin);
					Speak();
				});
			}
		}

		private void Speak()
		{
			if (null == _mInputField)
			{
				Debug.LogError("InputField is not set!");
				return;
			}
			if (_mSpeechSynthesisUtterance == null)
			{
				Debug.LogError("Utterance is not set!");
				return;
			}
			_mSpeechSynthesisPlugin.Cancel();
			_mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, _mInputField.text);
			_mSpeechSynthesisPlugin.Speak(_mSpeechSynthesisUtterance);
		}

		private IEnumerator SetPitch(float val)
		{
			if (_mSpeechSynthesisUtterance != null)
			{
				_mSpeechSynthesisPlugin.SetPitch(_mSpeechSynthesisUtterance, val);
				Speak();
			}
			yield break;
		}

		private IEnumerator SetRate(float val)
		{
			if (_mSpeechSynthesisUtterance != null)
			{
				_mSpeechSynthesisPlugin.SetRate(_mSpeechSynthesisUtterance, val);
				Speak();
			}
			yield break;
		}

		private void FixedUpdate()
		{
			if (_mSetPitch != null && !Input.GetMouseButton(0))
			{
				StartCoroutine(_mSetPitch);
				_mSetPitch = null;
			}
			if (_mSetRate != null && !Input.GetMouseButton(0))
			{
				StartCoroutine(_mSetRate);
				_mSetRate = null;
			}
			if (_mGetVoices)
			{
				_mGetVoices = false;
				StartCoroutine(GetVoices());
			}
		}
	}
}
