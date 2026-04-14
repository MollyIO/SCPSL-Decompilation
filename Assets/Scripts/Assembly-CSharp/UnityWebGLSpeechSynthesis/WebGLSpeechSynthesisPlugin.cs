using System;
using System.Collections;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
	public class WebGLSpeechSynthesisPlugin : BaseSpeechSynthesisPlugin, ISpeechSynthesisPlugin
	{
		private static WebGLSpeechSynthesisPlugin _sInstance;

		public static WebGLSpeechSynthesisPlugin GetInstance()
		{
			return _sInstance;
		}

		private void Awake()
		{
			_sInstance = this;
		}

		private IEnumerator ProxyOnEnd()
		{
			yield break;
		}

		public bool IsAvailable()
		{
			return false;
		}

		public void CreateSpeechSynthesisUtterance(Action<SpeechSynthesisUtterance> callback)
		{
			callback(null);
		}

		public void Speak(SpeechSynthesisUtterance speechSynthesisUtterance)
		{
		}

		public void Cancel()
		{
		}

		public void GetVoices(Action<VoiceResult> callback)
		{
			callback(null);
		}

		public void SetPitch(SpeechSynthesisUtterance utterance, float pitch)
		{
			if (utterance == null)
			{
				Debug.LogError("Utterance not set!");
			}
		}

		public void SetRate(SpeechSynthesisUtterance utterance, float rate)
		{
			if (utterance == null)
			{
				Debug.LogError("Utterance not set!");
			}
		}

		public void SetText(SpeechSynthesisUtterance utterance, string text)
		{
			if (utterance == null)
			{
				Debug.LogError("Utterance not set!");
			}
		}

		public void SetVoice(SpeechSynthesisUtterance utterance, Voice voice)
		{
			if (utterance == null)
			{
				Debug.LogError("Utterance not set!");
			}
			else if (voice == null)
			{
				Debug.LogError("Voice not set!");
			}
		}

		public void ManagementCloseBrowserTab()
		{
		}

		public void ManagementCloseProxy()
		{
		}

		public void ManagementLaunchProxy()
		{
		}

		public void ManagementOpenBrowserTab()
		{
		}

		public void ManagementSetProxyPort(int port)
		{
		}
	}
}
