using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Dissonance.Demo
{
	public class ChatInputController : MonoBehaviour
	{
		private bool _isInputtingText;

		private string _targetChannel;

		public DissonanceComms Comms;

		public string Team1Channel = "A";

		public string Team2Channel = "B";

		private InputField _input;

		private ChatLogController _log;

		public void Start()
		{
			Comms = Comms ?? Object.FindObjectOfType<DissonanceComms>();
			_input = GetComponentsInChildren<InputField>().Single((InputField a) => a.name == "ChatInput");
			_input.gameObject.SetActive(false);
			_input.onEndEdit.AddListener(OnInputEndEdit);
			_log = GetComponent<ChatLogController>();
		}

		private void OnInputEndEdit([CanBeNull] string message)
		{
			if (!string.IsNullOrEmpty(message))
			{
				if (Comms != null)
				{
					Comms.Text.Send(_targetChannel, message);
				}
				if (_log != null)
				{
					_log.AddMessage(string.Format("Me ({0}): {1}", _targetChannel, message), Color.gray);
				}
			}
			_input.text = string.Empty;
			_input.gameObject.SetActive(false);
			_isInputtingText = false;
			if (_log != null)
			{
				_log.ForceShow = false;
			}
		}

		public void Update()
		{
			if (!_isInputtingText)
			{
				bool key = Input.GetKey(KeyCode.Y);
				bool key2 = Input.GetKey(KeyCode.U);
				bool key3 = Input.GetKey(KeyCode.I);
				if (key)
				{
					ShowTextInput("Global");
				}
				else if (key2)
				{
					ShowTextInput(Team1Channel);
				}
				else if (key3)
				{
					ShowTextInput(Team2Channel);
				}
			}
		}

		private void ShowTextInput(string channel)
		{
			_isInputtingText = true;
			_targetChannel = channel;
			_input.gameObject.SetActive(true);
			_input.ActivateInputField();
			if (_log != null)
			{
				_log.ForceShow = true;
			}
		}
	}
}
