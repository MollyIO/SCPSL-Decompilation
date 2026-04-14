using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
	public class Example04SbaitsoClone : MonoBehaviour
	{
		private enum States
		{
			Intro = 0,
			NamePrompt = 1,
			WaitForName = 2,
			Outro = 3,
			Talking = 4
		}

		public Text _mTextWaiting;

		public GameObject _mPanelForText;

		public InputField _mPrefabInputField;

		private bool _mDoGetVoices;

		private bool _mWaitForOnEnd;

		private List<GameObject> _mTextLines = new List<GameObject>();

		private string _mName = string.Empty;

		private States _mState;

		private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin;

		private VoiceResult _mVoiceResult;

		private SpeechSynthesisUtterance _mSpeechSynthesisUtterance;

		private void CreateText(string msg)
		{
			GameObject gameObject = new GameObject("Text");
			gameObject.transform.SetParent(_mPanelForText.transform);
			Text text = gameObject.AddComponent<Text>();
			text.fontSize = 24;
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			text.text = msg;
			text.alignment = TextAnchor.MiddleLeft;
			gameObject.AddComponent<ContentSizeFitter>();
			_mTextLines.Add(text.gameObject);
			if (_mTextLines.Count > 5)
			{
				Object.Destroy(_mTextLines[0]);
				_mTextLines.RemoveAt(0);
			}
		}

		private IEnumerator CreateNameInputField()
		{
			yield return new WaitForSeconds(1f);
			GameObject go = Object.Instantiate(_mPrefabInputField.gameObject);
			InputField inputField = go.GetComponent<InputField>();
			go.transform.SetParent(_mPanelForText.transform);
			EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
			while (string.IsNullOrEmpty(inputField.text) || !Input.GetKeyUp(KeyCode.Return))
			{
				yield return null;
			}
			_mName = inputField.text;
			Object.Destroy(go);
			_mState = States.Outro;
			CreateText(_mName);
		}

		private IEnumerator CreateTalkInputField()
		{
			yield return new WaitForSeconds(0.1f);
			GameObject go = Object.Instantiate(_mPrefabInputField.gameObject);
			InputField inputField = go.GetComponent<InputField>();
			go.transform.SetParent(_mPanelForText.transform);
			EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
			while (string.IsNullOrEmpty(inputField.text) || !Input.GetKeyUp(KeyCode.Return))
			{
				yield return null;
			}
			string text = inputField.text;
			Object.Destroy(go);
			CreateText(text);
			string response = AISbaitso.GetResponse(text);
			CreateTextAndSpeak(response);
			StartCoroutine(CreateTalkInputField());
		}

		private void CreateTextAndSpeak(string msg)
		{
			_mWaitForOnEnd = true;
			CreateText(msg);
			Speak(msg);
		}

		private IEnumerator Start()
		{
			_mSpeechSynthesisPlugin = ProxySpeechSynthesisPlugin.GetInstance();
			if (_mSpeechSynthesisPlugin == null)
			{
				Debug.LogError("Proxy Speech Synthesis Plugin is not set!");
				yield break;
			}
			if (null == _mPanelForText)
			{
				Debug.LogError("Panel for text not set!");
				yield break;
			}
			if (null == _mPrefabInputField)
			{
				Debug.LogError("Prefab Input Field not set!");
				yield break;
			}
			while (!_mSpeechSynthesisPlugin.IsAvailable())
			{
				yield return null;
			}
			_mSpeechSynthesisPlugin.AddListenerSynthesisOnEnd(HandleSynthesisOnEnd);
			SpeechSynthesisUtils.SetActive(false, _mTextWaiting);
			StartCoroutine(GetVoices());
			_mSpeechSynthesisPlugin.CreateSpeechSynthesisUtterance(delegate(SpeechSynthesisUtterance utterance)
			{
				_mSpeechSynthesisUtterance = utterance;
			});
			while (_mSpeechSynthesisUtterance == null || _mVoiceResult == null)
			{
				Debug.Log("Waiting for proxy");
				yield return null;
			}
			while (true)
			{
				if (_mWaitForOnEnd)
				{
					yield return null;
					continue;
				}
				switch (_mState)
				{
				case States.Intro:
					CreateTextAndSpeak("Dr. Sbaitso, by Creative Labs.");
					_mState = States.NamePrompt;
					break;
				case States.NamePrompt:
					CreateTextAndSpeak("Please enter your name...");
					_mState = States.WaitForName;
					StartCoroutine(CreateNameInputField());
					break;
				case States.Outro:
					CreateTextAndSpeak(string.Format("Hello {0}, my name is Dr. Sbaitso.", _mName));
					while (_mWaitForOnEnd)
					{
						yield return null;
					}
					CreateTextAndSpeak("I am here to help you.");
					while (_mWaitForOnEnd)
					{
						yield return null;
					}
					CreateTextAndSpeak("Say whatever is in your mind freely.");
					while (_mWaitForOnEnd)
					{
						yield return null;
					}
					CreateTextAndSpeak("Our conversation will be kept in strict confidence.");
					while (_mWaitForOnEnd)
					{
						yield return null;
					}
					CreateTextAndSpeak("Memory contents will be wiped off after you leave.");
					while (_mWaitForOnEnd)
					{
						yield return null;
					}
					CreateTextAndSpeak("So, tell me about your problems.");
					while (_mWaitForOnEnd)
					{
						yield return null;
					}
					_mState = States.Talking;
					StartCoroutine(CreateTalkInputField());
					break;
				}
				yield return new WaitForFixedUpdate();
			}
		}

		private void HandleSynthesisOnEnd(SpeechSynthesisEvent speechSynthesisEvent)
		{
			if (speechSynthesisEvent != null)
			{
				_mWaitForOnEnd = false;
			}
		}

		private IEnumerator GetVoices()
		{
			yield return new WaitForSeconds(0.25f);
			_mSpeechSynthesisPlugin.GetVoices(delegate(VoiceResult voiceResult)
			{
				if (voiceResult == null)
				{
					_mDoGetVoices = true;
				}
				else
				{
					_mVoiceResult = voiceResult;
				}
			});
		}

		private void FixedUpdate()
		{
			if (_mDoGetVoices)
			{
				_mDoGetVoices = false;
				StartCoroutine(GetVoices());
			}
		}

		private void Speak(string text)
		{
			if (text == null)
			{
				Debug.LogError("Text is not set!");
				return;
			}
			if (_mSpeechSynthesisUtterance == null)
			{
				Debug.LogError("Utterance is not set!");
				return;
			}
			_mSpeechSynthesisPlugin.Cancel();
			_mSpeechSynthesisPlugin.SetText(_mSpeechSynthesisUtterance, text);
			_mSpeechSynthesisPlugin.Speak(_mSpeechSynthesisUtterance);
		}
	}
}
