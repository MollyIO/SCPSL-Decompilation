using System;

namespace UnityWebGLSpeechSynthesis
{
	public interface ISpeechSynthesisPlugin
	{
		void AddListenerSynthesisOnEnd(BaseSpeechSynthesisPlugin.DelegateHandleSynthesisOnEnd listener);

		void Cancel();

		void CreateSpeechSynthesisUtterance(Action<SpeechSynthesisUtterance> callback);

		void GetVoices(Action<VoiceResult> callback);

		bool IsAvailable();

		void ManagementCloseBrowserTab();

		void ManagementCloseProxy();

		void ManagementLaunchProxy();

		void ManagementOpenBrowserTab();

		void ManagementSetProxyPort(int port);

		void RemoveListenerSynthesisOnEnd(BaseSpeechSynthesisPlugin.DelegateHandleSynthesisOnEnd listener);

		void SetPitch(SpeechSynthesisUtterance utterance, float pitch);

		void SetRate(SpeechSynthesisUtterance utterance, float rate);

		void SetText(SpeechSynthesisUtterance utterance, string text);

		void SetVoice(SpeechSynthesisUtterance utterance, Voice voice);

		void Speak(SpeechSynthesisUtterance speechSynthesisUtterance);
	}
}
