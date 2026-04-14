using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class SubmenuSelector : MonoBehaviour
	{
		[Serializable]
		public class SubMenu
		{
			public Button button;

			public int argumentsCount;

			public string commandTemplate;

			public GameObject panel;

			public TextMeshProUGUI optionalDisplay;

			public Button submitButton;
		}

		public Color c_selected;

		public Color c_deselected;

		public SubMenu[] menus;

		private string[] arguments;

		private int currentMenu;

		public static SubmenuSelector singleton;

		private void Start()
		{
			menus[0].panel.SetActive(true);
			SelectMenu(0);
			SubMenu[] array = menus;
			foreach (SubMenu subMenu in array)
			{
				subMenu.button.interactable = true;
			}
		}

		private void Awake()
		{
			singleton = this;
		}

		public void SetProperty(int field, string value)
		{
			arguments[field - 1] = value;
			if (menus[currentMenu].submitButton == null)
			{
				return;
			}
			menus[currentMenu].submitButton.interactable = true;
			string[] array = arguments;
			foreach (string value2 in array)
			{
				if (string.IsNullOrEmpty(value2) && arguments.Length > 0)
				{
					menus[currentMenu].submitButton.interactable = false;
				}
			}
		}

		public void Confirm()
		{
			if (menus[currentMenu].optionalDisplay != null)
			{
				menus[currentMenu].optionalDisplay.text = string.Empty;
			}
			string text = menus[currentMenu].commandTemplate;
			List<string> list = new List<string>();
			string text2 = string.Empty;
			foreach (PlayerRecord record in PlayerRecord.records)
			{
				if (record.isSelected)
				{
					text2 = text2 + record.playerId + ".";
				}
			}
			list.Add(text2);
			list.AddRange(arguments);
			if (text.Contains("{0}"))
			{
				try
				{
					text = string.Format(text, list.ToArray());
				}
				catch
				{
					Debug.Log(text + ":" + list.Count);
				}
			}
			PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery(text);
		}

		public void ConfirmCustom(string command)
		{
			if (menus[currentMenu].optionalDisplay != null)
			{
				menus[currentMenu].optionalDisplay.text = string.Empty;
			}
			List<string> list = new List<string>();
			string text = string.Empty;
			foreach (PlayerRecord record in PlayerRecord.records)
			{
				if (record.isSelected)
				{
					text = text + record.playerId + ".";
				}
			}
			list.Add(text);
			list.AddRange(arguments);
			if (command.Contains("{0}"))
			{
				try
				{
					command = string.Format(command, list.ToArray());
				}
				catch
				{
					Debug.Log(command + ":" + list.Count);
				}
			}
			PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery(command);
		}

		public void PlayerInfoQuery(string operation)
		{
			List<PlayerRecord> records = PlayerRecord.records;
			PlayerRecord playerRecord = records.FirstOrDefault((PlayerRecord player) => player.isSelected);
			if (!(playerRecord == null))
			{
				string playerId = playerRecord.playerId;
				switch (operation)
				{
				case "short-info":
					PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("REQUEST_DATA SHORT-PLAYER " + playerId);
					break;
				case "info":
					PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("REQUEST_DATA PLAYER " + playerId);
					break;
				case "auth":
					PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("REQUEST_DATA AUTH " + playerId);
					break;
				}
			}
		}

		public void AdminToolsConfirm(string operation)
		{
			string text = string.Empty;
			if (operation == "GoTo")
			{
				PlayerRecord playerRecord = PlayerRecord.records.FirstOrDefault((PlayerRecord pl) => pl.isSelected);
				if (playerRecord == null)
				{
					return;
				}
				text = playerRecord.playerId;
			}
			else
			{
				foreach (PlayerRecord record in PlayerRecord.records)
				{
					if (record.isSelected)
					{
						text = text + record.playerId + ".";
					}
				}
			}
			switch (operation)
			{
			case "OverwatchEnable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("overwatch " + text + " 1");
				break;
			case "OverwatchDisable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("overwatch " + text + " 0");
				break;
			case "BypassEnable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("bypass " + text + " 1");
				break;
			case "BypassDisable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("bypass " + text + " 0");
				break;
			case "GodEnable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("god " + text + " 1");
				break;
			case "GodDisable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("god " + text + " 0");
				break;
			case "Heal":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("heal " + text);
				break;
			case "Bring":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("bring " + text);
				break;
			case "GoTo":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("goto " + text);
				break;
			case "Lockdown":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("lockdown");
				break;
			case "Open":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("open " + DoorPrinter.SelectedDoors);
				break;
			case "Close":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("close " + DoorPrinter.SelectedDoors);
				break;
			case "Lock":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("lock " + DoorPrinter.SelectedDoors);
				break;
			case "Unlock":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("unlock " + DoorPrinter.SelectedDoors);
				break;
			case "Destroy":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("destroy " + DoorPrinter.SelectedDoors);
				break;
			case "DoorTp":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("doortp " + text + " " + DoorPrinter.SelectedDoors);
				break;
			case "Mute":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("mute " + text);
				break;
			case "Unmute":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("unmute " + text);
				break;
			case "Imute":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("imute " + text);
				break;
			case "Iunmute":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("iunmute " + text);
				break;
			}
		}

		public void RunCommand(string command)
		{
			PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery(command);
		}

		public void SetConfigConfirm()
		{
			Dropdown componentInChildren = menus[currentMenu].panel.GetComponentInChildren<Dropdown>();
			RunCommand("setconfig " + componentInChildren.options[componentInChildren.value].text + " " + menus[currentMenu].panel.GetComponentInChildren<InputField>().text);
		}

		public void SelectMenu(Button b)
		{
			for (int i = 0; i < menus.Length; i++)
			{
				bool flag = menus[i].button == b;
				menus[i].button.GetComponent<Text>().color = ((!flag) ? c_deselected : c_selected);
				menus[i].panel.SetActive(flag);
				if (flag)
				{
					SelectMenu(i);
				}
			}
		}

		public void SelectMenu(int i)
		{
			currentMenu = i;
			arguments = new string[menus[i].argumentsCount];
		}
	}
}
