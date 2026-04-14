using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugLogReader : MonoBehaviour
{
	private static readonly List<GameObject> Lines = new List<GameObject>();

	private static string[] _linesstring;

	public GameObject Parent;

	public GameObject Prefab;

	public ScrollRect Scroll;

	public static bool SuccesfullyInitialized()
	{
		string path = ((SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows) ? (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/unity3d/Hubert Moszka/SCP Secret Laboratory/Player.log") : (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + "/Hubert Moszka/SCPSL/output_log.txt"));
		if (File.Exists(path))
		{
			try
			{
				_linesstring = (from line in File.ReadAllLines(path)
					where !string.IsNullOrEmpty(line.Trim())
					select line).ToArray();
				for (int num = 0; num < _linesstring.Length; num++)
				{
					_linesstring[num] = _linesstring[num].Trim();
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		foreach (GameObject line in Lines)
		{
			UnityEngine.Object.Destroy(line);
		}
		Lines.Clear();
		string[] linesstring = _linesstring;
		foreach (string text in linesstring)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Prefab, Parent.transform);
			gameObject.GetComponent<Text>().text = text;
			Lines.Add(gameObject);
		}
		StartCoroutine(ScrollToDown(Scroll));
	}

	private static IEnumerator ScrollToDown(ScrollRect scrollRect)
	{
		yield return new WaitForEndOfFrame();
		scrollRect.gameObject.SetActive(true);
		scrollRect.verticalNormalizedPosition = 0f;
	}
}
