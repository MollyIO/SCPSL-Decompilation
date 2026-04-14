using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerPrefsSl : MonoBehaviour
{
	public enum DataTypes
	{
		Float = 0,
		Int = 1,
		String = 2,
		Bool = 3
	}

	private const string Floatheader = "float_";

	private const string Intheader = "int_";

	private const string Stringheader = "string_";

	private const string Boolheader = "bool_";

	private const string Stringienumerableheader = "stringlist_";

	private const char Ienumerablejoinstring = ';';

	private static string[] _registry;

	private static string _path = "registry.txt";

	private void Awake()
	{
		_path = FileManager.GetAppFolder() + "registry.txt";
		Refresh();
	}

	private static string ToDoubleBase64(string text)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(text))));
	}

	private static string FromDoubleBase64(string base64)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(base64))));
	}

	private static void Refresh()
	{
		if (!File.Exists(_path))
		{
			File.Create(_path).Close();
		}
		_registry = File.ReadAllLines(_path);
	}

	private static string[] RemoveElement(string element, IEnumerable<string> myArray)
	{
		return myArray.Where((string w) => !w.StartsWith(element)).ToArray();
	}

	private static void WriteString(string key, string value)
	{
		bool flag = false;
		for (int i = 0; i < _registry.Length; i++)
		{
			if (_registry[i].StartsWith(key + ":"))
			{
				_registry[i] = key + ":" + value;
				flag = true;
			}
		}
		if (!flag)
		{
			using (StreamWriter streamWriter = File.AppendText(_path))
			{
				streamWriter.WriteLine(key + ":" + value);
				return;
			}
		}
		File.WriteAllLines(_path, _registry);
	}

	private static string GetValue(string key, out bool success)
	{
		Refresh();
		string[] registry = _registry;
		foreach (string text in registry)
		{
			if (text.StartsWith(key + ":"))
			{
				success = true;
				return text.Replace(key + ":", string.Empty);
			}
		}
		success = false;
		return string.Empty;
	}

	public static void SetFloat(string key, float value)
	{
		WriteString("float_" + key, value.ToString(CultureInfo.InvariantCulture));
	}

	public static void SetInt(string key, int value)
	{
		WriteString("int_" + key, value.ToString());
	}

	public static void SetString(string key, string value)
	{
		WriteString("string_" + key, ToDoubleBase64(value));
	}

	public static void SetBool(string key, bool value)
	{
		WriteString("bool_" + key, (!value) ? "false" : "true");
	}

	public static void SetStringArray(string key, IEnumerable<string> ienumerable)
	{
		WriteString("stringlist_" + key, string.Join(';'.ToString(), ienumerable.ToArray()));
	}

	public static void DeleteKey(string key, DataTypes type)
	{
		string element = key;
		switch (type)
		{
		case DataTypes.Bool:
			element = "bool_" + key;
			break;
		case DataTypes.Float:
			element = "float_" + key;
			break;
		case DataTypes.Int:
			element = "int_" + key;
			break;
		case DataTypes.String:
			element = "string_" + key;
			break;
		}
		File.WriteAllLines(_path, RemoveElement(element, _registry));
	}

	public static void DeleteKey(string key)
	{
		string[] myArray = RemoveElement("float_" + key, _registry);
		myArray = RemoveElement("int_" + key, myArray);
		myArray = RemoveElement("bool_" + key, myArray);
		myArray = RemoveElement("string_" + key, myArray);
		File.WriteAllLines(_path, myArray);
	}

	public static void DeleteAll()
	{
		File.WriteAllText(_path, string.Empty);
	}

	public static float GetFloat(string key, float defaultValue, bool forcedefault = false)
	{
		if (forcedefault)
		{
			return defaultValue;
		}
		bool success;
		string value = GetValue("float_" + key, out success);
		float result;
		return (!success) ? defaultValue : ((!float.TryParse(value, out result)) ? defaultValue : result);
	}

	public static int GetInt(string key, int defaultValue, bool forcedefault = false)
	{
		if (forcedefault)
		{
			return defaultValue;
		}
		bool success;
		string value = GetValue("int_" + key, out success);
		int result;
		return (!success) ? defaultValue : ((!int.TryParse(value, out result)) ? defaultValue : result);
	}

	public static string GetString(string key, string defaultValue, bool forcedefault = false)
	{
		if (forcedefault)
		{
			return defaultValue;
		}
		bool success;
		string value = GetValue("string_" + key, out success);
		return (!success) ? defaultValue : FromDoubleBase64(value);
	}

	public static bool GetBool(string key, bool defaultValue, bool forcedefault = false)
	{
		if (forcedefault)
		{
			return defaultValue;
		}
		bool success;
		string value = GetValue("bool_" + key, out success);
		bool result;
		return (!success) ? defaultValue : ((!bool.TryParse(value.ToLower(), out result)) ? defaultValue : result);
	}

	public static string[] GetStringArray(string key, IEnumerable<string> defaultValue, bool forcedefault = false)
	{
		if (forcedefault)
		{
			return defaultValue.ToArray();
		}
		bool success;
		string[] array = GetValue("bool_" + key, out success).Split(';');
		return (!success) ? defaultValue.ToArray() : array;
	}

	public static bool HasKey(string key, DataTypes type)
	{
		string hkey = key;
		switch (type)
		{
		case DataTypes.Bool:
			hkey = "bool_" + key;
			break;
		case DataTypes.Float:
			hkey = "float_" + key;
			break;
		case DataTypes.Int:
			hkey = "int_" + key;
			break;
		case DataTypes.String:
			hkey = "string_" + key;
			break;
		}
		return _registry.Any((string line) => line.StartsWith(hkey + ":"));
	}

	public static bool HasKey(string key)
	{
		return _registry.Any((string line) => line.StartsWith("float_" + key + ":") || line.StartsWith("int_" + key + ":") || line.StartsWith("bool_" + key + ":") || line.StartsWith("string_" + key + ":"));
	}

	public static bool HasKeyWithName(string key)
	{
		return _registry.Any((string line) => line.StartsWith(key + ":"));
	}
}
