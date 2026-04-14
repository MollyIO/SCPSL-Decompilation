using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class NewInput
{
	[Serializable]
	public class Bind
	{
		public string axis;

		public KeyCode key;
	}

	public static List<Bind> bindings = new List<Bind>();

	public static string defaultBinding = "Shoot:323;Zoom:324;Jump:32;Interact:101;Inventory:9;Reload:114;Run:304;Voice Chat:113;Sneak:99;Move Forward:119;Move Backward:115;Move Left:97;Move Right:100;Player List:110;Remote Admin:109;Toggle flashlight:102;939's speech:118";

	public static KeyCode GetKey(string axis)
	{
		Bind bind = bindings.FirstOrDefault((Bind bind2) => bind2.axis == axis);
		if (bind != null)
		{
			return bind.key;
		}
		Debug.LogError("Key axis '" + axis + "' does not exist.");
		return KeyCode.None;
	}

	public static void ChangeKey(string axis, KeyCode code)
	{
		foreach (Bind binding in bindings)
		{
			if (binding.axis != axis)
			{
				continue;
			}
			binding.key = code;
			Save();
			return;
		}
		Debug.LogError("Key axis '" + axis + "' does not exist.");
	}

	public static void Save()
	{
		string text = string.Empty;
		for (int i = 0; i < bindings.Count; i++)
		{
			string text2 = text;
			text = text2 + bindings[i].axis + ":" + (int)bindings[i].key;
			if (i != bindings.Count - 1)
			{
				text += ";";
			}
		}
		StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/keybinding.txt");
		streamWriter.WriteLine(text);
		streamWriter.Close();
	}

	public static void CheckForNewBindings()
	{
		List<Bind> list = (from item in defaultBinding.Split(';')
			select new Bind
			{
				axis = item.Split(':')[0],
				key = (KeyCode)int.Parse(item.Split(':')[1])
			}).ToList();
		foreach (Bind item in list)
		{
			bool flag = false;
			foreach (Bind binding in bindings)
			{
				if (binding.axis == item.axis)
				{
					flag = true;
				}
			}
			if (flag)
			{
				continue;
			}
			bindings.Add(item);
			CheckForNewBindings();
			break;
		}
	}

	public static void Load()
	{
		try
		{
			bindings.Clear();
			if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/keybinding.txt"))
			{
				Revent();
			}
			StreamReader streamReader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/keybinding.txt");
			string text = streamReader.ReadToEnd();
			streamReader.Close();
			if (!text.Contains(";"))
			{
				Revent();
				text = defaultBinding;
			}
			string[] array = text.Split(';');
			foreach (string text2 in array)
			{
				bindings.Add(new Bind
				{
					axis = text2.Split(':')[0],
					key = (KeyCode)int.Parse(text2.Split(':')[1])
				});
			}
			CheckForNewBindings();
		}
		catch
		{
			Revent();
		}
	}

	public static void Revent()
	{
		Debug.Log("Reventing!");
		StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/keybinding.txt");
		streamWriter.WriteLine(defaultBinding);
		streamWriter.Close();
		Load();
	}
}
