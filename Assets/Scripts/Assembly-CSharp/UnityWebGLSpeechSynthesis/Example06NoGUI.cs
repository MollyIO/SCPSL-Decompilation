using System.Collections;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
	public class Example06NoGUI : MonoBehaviour
	{
		private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin;

		private VoiceResult _mVoiceResult;

		private SpeechSynthesisUtterance _mSpeechSynthesisUtterance;

		private bool _mUtteranceSet;

		private bool _mVoicesSet;

		private bool _mGetVoices;

		private string _mTextToSpeak = string.Empty;

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
			_mTextToSpeak = "Hello! Text to speech is great! Thumbs up!";
			_mSpeechSynthesisPlugin.AddListenerSynthesisOnEnd(HandleSynthesisOnEnd);
			StartCoroutine(GetVoices());
			_mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance(delegate(SpeechSynthesisUtterance utterance)
			{
				_mSpeechSynthesisUtterance = utterance;
				_mUtteranceSet = true;
				OnSpeechAPILoaded();
			});
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
					_mVoicesSet = true;
					OnSpeechAPILoaded();
				}
			});
		}

		private void Speak()
		{
			if (_mSpeechSynthesisUtterance == null)
			{
				Debug.LogError("Utterance is not set!");
				return;
			}
			_mSpeechSynthesisPlugin.Cancel();
			_mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, _mTextToSpeak);
			_mSpeechSynthesisPlugin.Speak(_mSpeechSynthesisUtterance);
		}

		private void FixedUpdate()
		{
			if (_mGetVoices)
			{
				_mGetVoices = false;
				StartCoroutine(GetVoices());
			}
		}

		private void OnSpeechAPILoaded()
		{
			if (!_mVoicesSet || !_mUtteranceSet || _mSpeechSynthesisUtterance == null)
			{
				return;
			}
			if (((_mVoiceResult != null) ? _mVoiceResult.voices : null) != null && _mVoiceResult.voices.Length > 0)
			{
				int num = Random.Range(0, _mVoiceResult.voices.Length);
				Voice voice = _mVoiceResult.voices[num];
				if (voice != null)
				{
					_mSpeechSynthesisPlugin.SetVoice(_mSpeechSynthesisUtterance, voice);
				}
			}
			float rate = Random.Range(0.1f, 2f);
			_mSpeechSynthesisPlugin.SetRate(_mSpeechSynthesisUtterance, rate);
			float pitch = Random.Range(0.1f, 2f);
			_mSpeechSynthesisPlugin.SetPitch(_mSpeechSynthesisUtterance, pitch);
			Speak();
		}

		private void HandleSynthesisOnEnd(SpeechSynthesisEvent speechSynthesisEvent)
		{
			OnSpeechAPILoaded();
		}
	}
}
