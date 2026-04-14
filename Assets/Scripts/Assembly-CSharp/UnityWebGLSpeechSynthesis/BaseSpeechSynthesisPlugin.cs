using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
	public class BaseSpeechSynthesisPlugin : MonoBehaviour
	{
		public delegate void DelegateHandleSynthesisOnEnd(SpeechSynthesisEvent speechSynthesisEvent);

		protected class IdEventArgs : EventArgs
		{
			public string _mId;
		}

		protected class SpeechSynthesisUtteranceEventArgs : IdEventArgs
		{
			public SpeechSynthesisUtterance _mUtterance;
		}

		protected class VoiceResultArgs : IdEventArgs
		{
			public VoiceResult _mVoiceResult;
		}

		protected static List<DelegateHandleSynthesisOnEnd> _sOnSynthesisOnEnd = new List<DelegateHandleSynthesisOnEnd>();

		protected EventHandler<SpeechSynthesisUtteranceEventArgs> _mOnCreateSpeechSynthesisUtterance;

		protected EventHandler<VoiceResultArgs> _mOnGetVoices;

		public void AddListenerSynthesisOnEnd(DelegateHandleSynthesisOnEnd listener)
		{
			if (!_sOnSynthesisOnEnd.Contains(listener))
			{
				_sOnSynthesisOnEnd.Add(listener);
			}
		}

		public void RemoveListenerSynthesisOnEnd(DelegateHandleSynthesisOnEnd listener)
		{
			if (_sOnSynthesisOnEnd.Contains(listener))
			{
				_sOnSynthesisOnEnd.Remove(listener);
			}
		}
	}
}
