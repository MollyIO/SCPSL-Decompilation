using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class YamlConfig
{
	private readonly IEnumerable<string> _rolevars = new string[4] { "color", "badge", "cover", "hidden" };

	private bool _afteradding;

	public string Path;

	public string[] RawData;

	private string[] _rawDataUnfiltered;

	public YamlConfig()
	{
		RawData = new string[0];
	}

	public YamlConfig(string path)
	{
		Path = path;
		LoadConfigFile(path);
	}

	private static string[] Filter(IEnumerable<string> lines)
	{
		return lines.Where((string line) => !string.IsNullOrEmpty(line) && !line.StartsWith("#") && (line.StartsWith(" - ") || line.Contains(':'))).ToArray();
	}

	public void LoadConfigFile(string path)
	{
		Path = path;
		if (!ServerStatic.DisableConfigValidation)
		{
			RemoveInvalid();
		}
		if (!ServerStatic.DisableConfigValidation && Path.EndsWith("config_gameplay.txt") && !_afteradding && FileManager.FileExists("MiscData" + System.IO.Path.DirectorySeparatorChar + "gameconfig_template.txt"))
		{
			AddMissingTemplateKeys("MiscData" + System.IO.Path.DirectorySeparatorChar + "gameconfig_template.txt");
		}
		else if (!ServerStatic.DisableConfigValidation && Path.EndsWith("config_remoteadmin.txt") && !_afteradding)
		{
			AddMissingRoleVars();
			AddMissingPerms();
		}
		_rawDataUnfiltered = FileManager.ReadAllLines(path);
		RawData = Filter(_rawDataUnfiltered);
		if (GetString("port_queue", string.Empty) != string.Empty)
		{
			ServerConsole.AddLog("Port queue has a invalid format!");
		}
		if (ServerStatic.IsDedicated && Path.EndsWith("config_remoteadmin.txt"))
		{
			Application.targetFrameRate = GetInt("server_tickrate", 60);
		}
	}

	private void AddMissingPerms()
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			string[] perms = GetStringList("Permissions", Path).ToArray();
			string[] names = Enum.GetNames(typeof(PlayerPermissions));
			if (perms.Length != names.Length)
			{
				List<string> collection = (from permtype in names
					let inconfig = perms.Any((string perm) => perm.StartsWith(permtype))
					where !inconfig
					select " - " + permtype + ": []").ToList();
				List<string> list = FileManager.ReadAllLines(Path).ToList();
				for (int num = 0; num < list.Count; num++)
				{
					if (list[num] == "Permissions:")
					{
						list.InsertRange(num + 1, collection);
					}
				}
				FileManager.WriteToFile(list.ToArray(), Path);
				_afteradding = true;
			}
		});
		thread.Start();
		thread.Join();
	}

	private void AddMissingRoleVars()
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			string time = TimeBehaviour.FormatTime("yyyy/MM/dd HH:mm:ss");
			string[] array = GetStringList("Roles", Path).ToArray();
			List<string> list = new List<string>();
			string[] array2 = array;
			foreach (string role in array2)
			{
				list.AddRange(from rolevar in _rolevars
					where !string.Join("\r\n", FileManager.ReadAllLines(Path)).Contains(role + "_" + rolevar + ":")
					select role + "_" + rolevar + ": default");
			}
			if (list.Count > 0)
			{
				Write(list, ref time);
			}
		});
		thread.Start();
		thread.Join();
	}

	private void AddMissingTemplateKeys(string templatepath)
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			string time = TimeBehaviour.FormatTime("yyyy/MM/dd HH:mm:ss");
			string[] array = FileManager.ReadAllLines(templatepath);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].StartsWith("#") && !array[i].StartsWith(" -") && array[i].Contains(":") && ((i + 1 < array.Length && array[i + 1].StartsWith(" -")) || array[i].EndsWith("[]")))
				{
					list.Add(array[i]);
				}
				else if (!array[i].StartsWith("#") && array[i].Contains(":") && !array[i].StartsWith(" -"))
				{
					list2.Add(array[i].Substring(0, array[i].IndexOf(':') + 1));
				}
			}
			foreach (string item in list2)
			{
				if (!string.Join("\r\n", FileManager.ReadAllLines(Path)).Contains(item))
				{
					list3.Add(item + " default");
				}
			}
			Write(list3, ref time);
			foreach (string item2 in list)
			{
				if (!string.Join("\r\n", FileManager.ReadAllLines(Path)).Contains(item2))
				{
					bool flag = false;
					List<string> list4 = new List<string> { "#LIST", item2 };
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (text.StartsWith(item2) && text.EndsWith("[]"))
						{
							list4.Clear();
							list4.AddRange(new string[2] { "#LIST - [] equals to empty", text });
							break;
						}
						if (text.StartsWith(item2))
						{
							flag = true;
						}
						else if (flag)
						{
							if (text.StartsWith(" - "))
							{
								list4.Add(text);
							}
							else if (!text.StartsWith("#"))
							{
								break;
							}
						}
					}
					Write(list4, ref time);
				}
			}
			_afteradding = true;
		});
		thread.Start();
		thread.Join();
	}

	private void Write(IEnumerable<string> text, ref string time)
	{
		string[] array = text.ToArray();
		if (array.Length > 0)
		{
			Write(string.Join("\r\n", array), ref time);
		}
	}

	private void Write(string text, ref string time)
	{
		using (StreamWriter streamWriter = File.AppendText(Path))
		{
			streamWriter.Write("\r\n\r\n#ADDED BY CONFIG VALIDATOR - " + time + " Game version: " + CustomNetworkManager.CompatibleVersions[0] + "\r\n" + text);
		}
	}

	private void RemoveInvalid()
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			string[] array = FileManager.ReadAllLines(Path);
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].StartsWith("#") && !array[i].StartsWith(" -") && !array[i].Contains(":") && !string.IsNullOrEmpty(array[i].Replace(" ", string.Empty)))
				{
					flag = true;
					array[i] = "#INVALID - " + array[i];
				}
			}
			if (flag)
			{
				FileManager.WriteToFile(array, Path);
			}
		});
		thread.Start();
		thread.Join();
	}

	public bool Reload()
	{
		if (string.IsNullOrEmpty(Path))
		{
			return false;
		}
		LoadConfigFile(Path);
		return true;
	}

	private string GetRawString(string key, string def)
	{
		string[] rawData = RawData;
		foreach (string text in rawData)
		{
			if (text.StartsWith(key + ": "))
			{
				return (!(text.Substring(key.Length + 2) == "default")) ? text.Substring(key.Length + 2) : def;
			}
		}
		return def;
	}

	public string GetString(string key, string def = "")
	{
		return GetRawString(key, def);
	}

	public int GetInt(string key, int def = 0)
	{
		int result;
		if (int.TryParse(GetRawString(key, def.ToString()), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
		{
			return result;
		}
		ServerConsole.AddLog(key + " has invalid value, " + GetRawString(key, def.ToString()) + " is not a valid integer!");
		CommentInvalid(key, "INT");
		return def;
	}

	public float GetFloat(string key, float def = 0f)
	{
		float result;
		if (float.TryParse(GetRawString(key, def.ToString(CultureInfo.InvariantCulture)).Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
		{
			return result;
		}
		ServerConsole.AddLog(key + " has invalid value, " + GetRawString(key, def.ToString(CultureInfo.InvariantCulture)) + " is not a valid float!");
		CommentInvalid(key, "FLOAT");
		return def;
	}

	public bool GetBool(string key, bool def = false)
	{
		bool result;
		if (bool.TryParse(GetRawString(key, def.ToString()).ToLower(), out result))
		{
			return result;
		}
		ServerConsole.AddLog(key + " has invalid value, " + GetRawString(key, def.ToString()) + " is not a valid bool!");
		CommentInvalid(key, "BOOL");
		return def;
	}

	private void CommentInvalid(string key, string type)
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			for (int i = 0; i < _rawDataUnfiltered.Length; i++)
			{
				if (_rawDataUnfiltered[i].StartsWith(key + ": "))
				{
					_rawDataUnfiltered[i] = "#INVALID " + type + " - " + _rawDataUnfiltered[i];
				}
			}
			if (!ServerStatic.DisableConfigValidation)
			{
				FileManager.WriteToFile(_rawDataUnfiltered, Path);
			}
		});
		thread.Start();
		thread.Join();
	}

	public void SetString(string key, string value = null)
	{
		Reload();
		int num = 0;
		List<string> list = null;
		for (int i = 0; i < _rawDataUnfiltered.Length; i++)
		{
			string text = _rawDataUnfiltered[i];
			if (text.StartsWith(key + ": "))
			{
				if (value == null)
				{
					list = _rawDataUnfiltered.ToList();
					list.RemoveAt(i);
					num = 2;
				}
				else
				{
					_rawDataUnfiltered[i] = key + ": " + value;
					num = 1;
				}
				break;
			}
		}
		switch (num)
		{
		case 0:
			list = _rawDataUnfiltered.ToList();
			list.Insert(list.Count, key + ": " + value);
			FileManager.WriteToFile(list.ToArray(), Path);
			break;
		case 1:
			FileManager.WriteToFile(_rawDataUnfiltered, Path);
			break;
		case 2:
			if (list != null)
			{
				FileManager.WriteToFile(list.ToArray(), Path);
			}
			break;
		}
		Reload();
	}

	public List<string> GetStringList(string key)
	{
		bool flag = false;
		List<string> list = new List<string>();
		string[] rawData = RawData;
		foreach (string text in rawData)
		{
			if (text.StartsWith(key) && text.EndsWith("[]"))
			{
				break;
			}
			if (text.StartsWith(key + ":"))
			{
				flag = true;
			}
			else if (flag)
			{
				if (text.StartsWith(" - "))
				{
					list.Add(text.Substring(3));
				}
				else if (!text.StartsWith("#"))
				{
					break;
				}
			}
		}
		return list;
	}

	private static List<string> GetStringList(string key, string path)
	{
		bool flag = false;
		List<string> list = new List<string>();
		string[] array = FileManager.ReadAllLines(path);
		foreach (string text in array)
		{
			if (text.StartsWith(key) && text.EndsWith("[]"))
			{
				break;
			}
			if (text.StartsWith(key + ":"))
			{
				flag = true;
			}
			else if (flag)
			{
				if (text.StartsWith(" - "))
				{
					list.Add(text.Substring(3));
				}
				else if (!text.StartsWith("#"))
				{
					break;
				}
			}
		}
		return list;
	}

	public void SetStringListItem(string key, string value, string newValue)
	{
		Reload();
		bool flag = false;
		int num = 0;
		List<string> list = null;
		for (int i = 0; i < _rawDataUnfiltered.Length; i++)
		{
			string text = _rawDataUnfiltered[i];
			if (text.StartsWith(key + ":"))
			{
				flag = true;
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				if (value != null && text == " - " + value)
				{
					if (newValue == null)
					{
						list = _rawDataUnfiltered.ToList();
						list.RemoveAt(i);
						num = 2;
					}
					else
					{
						_rawDataUnfiltered[i] = " - " + newValue;
						num = 1;
					}
					break;
				}
				if (!text.StartsWith(" - ") && !text.StartsWith("#"))
				{
					if (value != null)
					{
						list = _rawDataUnfiltered.ToList();
						list.Insert(i, " - " + newValue);
						num = 2;
					}
					break;
				}
			}
		}
		switch (num)
		{
		case 1:
			FileManager.WriteToFile(_rawDataUnfiltered, Path);
			break;
		case 2:
			if (list != null)
			{
				FileManager.WriteToFile(list.ToArray(), Path);
			}
			break;
		}
		Reload();
	}

	public List<int> GetIntList(string key)
	{
		List<string> stringList = GetStringList(key);
		return ((IEnumerable<string>)stringList).Select((Func<string, int>)Convert.ToInt32).ToList();
	}

	public Dictionary<string, string> GetStringDictionary(string key)
	{
		List<string> stringList = GetStringList(key);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string item in stringList)
		{
			int num = item.IndexOf(": ", StringComparison.Ordinal);
			string text = item.Substring(0, num);
			if (!dictionary.ContainsKey(text))
			{
				dictionary.Add(text, item.Substring(num + 2));
				continue;
			}
			ServerConsole.AddLog("Ignoring duplicated subkey " + text + " in dictionary " + key + " in the config file.");
		}
		return dictionary;
	}

	public void SetStringDictionaryItem(string key, string subkey, string value)
	{
		Reload();
		bool flag = false;
		int num = 0;
		List<string> list = null;
		for (int i = 0; i < _rawDataUnfiltered.Length; i++)
		{
			string text = _rawDataUnfiltered[i];
			if (text.StartsWith(key + ":"))
			{
				flag = true;
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				if (text.StartsWith(" - " + subkey + ": "))
				{
					if (value == null)
					{
						list = _rawDataUnfiltered.ToList();
						list.RemoveAt(i);
						num = 2;
					}
					else
					{
						_rawDataUnfiltered[i] = " - " + subkey + ": " + value;
						num = 1;
					}
					break;
				}
				if (!text.StartsWith(" - ") && !text.StartsWith("#"))
				{
					if (value != null)
					{
						list = _rawDataUnfiltered.ToList();
						list.Insert(i, " - " + subkey + ": " + value);
						num = 2;
					}
					break;
				}
			}
		}
		switch (num)
		{
		case 0:
			list = _rawDataUnfiltered.ToList();
			list.Insert(list.Count, " - " + subkey + ": " + value);
			FileManager.WriteToFile(list.ToArray(), Path);
			break;
		case 1:
			FileManager.WriteToFile(_rawDataUnfiltered, Path);
			break;
		case 2:
			if (list != null)
			{
				FileManager.WriteToFile(list.ToArray(), Path);
			}
			break;
		}
		Reload();
	}

	public static string[] ParseCommaSeparatedString(string data)
	{
		if (!data.StartsWith("[") || !data.EndsWith("]"))
		{
			return null;
		}
		data = data.Substring(1, data.Length - 2);
		return data.Split(new string[1] { ", " }, StringSplitOptions.None);
	}
}
