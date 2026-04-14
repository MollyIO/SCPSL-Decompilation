using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class TextBasedRemoteAdmin : MonoBehaviour
	{
		private readonly List<string> _logs = new List<string>();

		public static TextBasedRemoteAdmin singleton;

		public TextMeshProUGUI consoleWindow;

		public InputField commandField;

		private UIController _ui;

		private void Start()
		{
			FirstPersonController.usingRemoteAdmin = false;
			_ui = GetComponent<UIController>();
			_logs.Add("[SYSTEM] Text Based Remote Admin system started at " + DateTime.Now.ToLongTimeString());
			RefreshConsole();
		}

		private void Awake()
		{
			singleton = this;
		}

		public static void AddLog(string log)
		{
			singleton._logs.Add(log);
			singleton.RefreshConsole();
		}

		private void RefreshConsole()
		{
			string text = string.Empty;
			foreach (string log in _logs)
			{
				text = text + log + "\n\n";
			}
			consoleWindow.text = text;
		}

		private void Update()
		{
			if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && _ui.loggedIn && _ui.opened && !string.IsNullOrEmpty(commandField.text))
			{
				SendCommand();
			}
		}

		public void SendCommand()
		{
			if (!string.IsNullOrEmpty(commandField.text))
			{
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery(commandField.text);
				commandField.text = string.Empty;
			}
			commandField.ActivateInputField();
		}
	}
}
