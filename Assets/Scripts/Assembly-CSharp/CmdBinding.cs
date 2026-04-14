using System;
using System.Collections.Generic;
using System.IO;
using GameConsole;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class CmdBinding : NetworkBehaviour
{
	public class Bind
	{
		public string command;

		public KeyCode key;
	}

	public static List<Bind> bindings = new List<Bind>();

	private static bool _allowSync;

	private void Start()
	{
		Load();
		_allowSync = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/internal/SyncCmd");
	}

	private void Update()
	{
        if (bindings.Count <= 0)
		{
			return;
		}
        if (GameConsole.Console.singleton == null || GameConsole.Console.singleton.console == null)
		{
			return;
		}
		if (GameConsole.Console.singleton.console.activeSelf)
		{
			return;
		}
		UIController singleton = UIController.singleton;
        if (singleton != null && (singleton.root_panel.activeSelf || singleton.root_login.activeSelf || singleton.root_tbra.activeSelf))
		{
			return;
		}
		if (PlayerList.singleton != null && PlayerList.singleton.reportForm != null && PlayerList.singleton.reportForm.activeSelf)
		{
			return;
		}
		foreach (Bind binding in bindings)
		{
			if (Input.GetKeyDown(binding.key))
			{
				GameConsole.Console.singleton.TypeCommand(binding.command);
			}
		}
	}

	public static void KeyBind(KeyCode code, string cmd)
	{
		foreach (Bind binding in bindings)
		{
			if (binding.key != code)
			{
				continue;
			}
			binding.command = cmd;
			Save();
			return;
		}
		bindings.Add(new Bind
		{
			command = cmd,
			key = code
		});
	}

	public static void Save()
	{
		string text = string.Empty;
		for (int i = 0; i < bindings.Count; i++)
		{
			string text2 = text;
			text = text2 + (int)bindings[i].key + ":" + bindings[i].command;
			if (i != bindings.Count - 1)
			{
				text += Environment.NewLine;
			}
		}
		StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt");
		streamWriter.WriteLine(text);
		streamWriter.Close();
	}

	public static void Load()
	{
		GameConsole.Console.singleton.AddLog("Loading cmd bindings...", Color.grey);
		try
		{
			bindings.Clear();
			if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt"))
			{
				Revent();
			}
			StreamReader streamReader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt");
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				if (!string.IsNullOrEmpty(text) && text.Contains(":"))
				{
					bindings.Add(new Bind
					{
						command = text.Split(':')[1],
						key = (KeyCode)int.Parse(text.Split(':')[0])
					});
				}
			}
			streamReader.Close();
		}
		catch (Exception ex)
		{
			Debug.Log("REVENT: " + ex.StackTrace + " - " + ex.Message);
			Revent();
		}
	}

	public static void Revent()
	{
		Debug.Log("Reventing!");
		StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt");
		streamWriter.Close();
	}

	public static void ChangeKeybinding(KeyCode code, string cmd)
	{
		if (cmd.StartsWith(".") || cmd.StartsWith("/"))
		{
			if (_allowSync)
			{
				GameConsole.Console.singleton.AddLog(string.Concat("[SYNC FROM SERVER] ", code, "(", (int)code, "):", cmd), Color.grey);
				KeyBind(code, cmd);
			}
			else
			{
				GameConsole.Console.singleton.AddLog(string.Concat("Server tries to sync keybinding: ", code, "(", (int)code, "):", cmd, " Type SYNCCMD and restart your game to accpet."), Color.grey);
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
