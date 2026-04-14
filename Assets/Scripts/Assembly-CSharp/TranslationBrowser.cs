using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TranslationBrowser : MonoBehaviour
{
	public Text instancePrefab;

	public Transform parent;

	private List<GameObject> spawns = new List<GameObject>();

	private void OnEnable()
	{
		string[] directories = Directory.GetDirectories("Translations");
		foreach (GameObject spawn in spawns)
		{
			Object.Destroy(spawn);
		}
		string[] array = directories;
		foreach (string text in array)
		{
			Text text2 = Object.Instantiate(instancePrefab, parent);
			text2.transform.localScale = Vector3.one;
			text2.text = text.Remove(0, text.IndexOf("\\") + 1);
			spawns.Add(text2.gameObject);
		}
	}
}
