using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TranslationReader : MonoBehaviour
{
	[Serializable]
	public class TranslatedElement
	{
		public string fileName;

		public string[] values;
	}

	public static TranslationReader singleton;

	public static string path;

	public List<TranslatedElement> elements = new List<TranslatedElement>();

	private void Awake()
	{
		singleton = this;
	}

	private void Start()
	{
		NewInput.Load();
		CmdBinding.Load();
		Refresh();
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 1)
		{
			Refresh();
		}
	}

	public static string Get(string n, int v)
	{
		try
		{
			if (n.Contains("."))
			{
				n = n.Remove(46);
			}
		}
		catch
		{
		}
		try
		{
			foreach (TranslatedElement element in singleton.elements)
			{
				if (element.fileName == n)
				{
					return element.values[v].Replace("\\n", Environment.NewLine);
				}
			}
			return "NO_TRANSLATION";
		}
		catch
		{
			return "TRANSLATION_ERROR";
		}
	}

	private void Refresh()
	{
		if (CustomNetworkManager.isPrivateBeta)
		{
			path = "PrivateBeta/Translations/" + PlayerPrefs.GetString("translation_path", "English (default)");
		}
		else
		{
			path = "Translations/" + PlayerPrefs.GetString("translation_path", "English (default)");
		}
		if (!Directory.Exists(path))
		{
			path = "Translations/English (default)";
		}
		singleton.elements.Clear();
		string[] files = Directory.GetFiles(path);
		string[] array = files;
		foreach (string text in array)
		{
			string text2 = text.Replace("\\", "/");
			try
			{
				StreamReader streamReader = new StreamReader(text2);
				string text3 = streamReader.ReadToEnd();
				streamReader.Close();
				string text4 = text2.Remove(0, text2.LastIndexOf("/") + 1);
				TranslatedElement translatedElement = new TranslatedElement();
				translatedElement.fileName = text4.Remove(text4.IndexOf('.'));
				translatedElement.values = text3.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
				TranslatedElement item = translatedElement;
				singleton.elements.Add(item);
			}
			catch
			{
			}
		}
		SimpleMenu.LoadCorrectScene();
	}
}
