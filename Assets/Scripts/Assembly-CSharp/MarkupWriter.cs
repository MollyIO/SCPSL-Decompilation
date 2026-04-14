using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarkupWriter : MonoBehaviour
{
	public delegate void OnCreateAction(MarkupElement objectCreated);

	public static MarkupWriter singleton;

	public GameObject sample;

	public List<string> errorLogs = new List<string>();

	private List<GameObject> spawnedElements = new List<GameObject>();

	public static event OnCreateAction OnCreateObject;

	private void Awake()
	{
		singleton = this;
	}

	private void ClearAll()
	{
		foreach (GameObject spawnedElement in spawnedElements)
		{
			Object.Destroy(spawnedElement);
		}
		spawnedElements.Clear();
	}

	public void ReadTag(string input)
	{
		string[] array = input.Split(';');
		foreach (string text in array)
		{
			if (!text.Contains(" "))
			{
				continue;
			}
			List<string> list = text.Split(' ').ToList();
			for (int j = 0; j < 10; j++)
			{
				list.Add("empty");
			}
			if (list[0].ToLower() == "clear")
			{
				ClearAll();
				continue;
			}
			float result;
			if (!float.TryParse(list[1], out result))
			{
				result = 0f;
			}
			float result2;
			if (!float.TryParse(list[2], out result2))
			{
				result2 = 0f;
			}
			float result3;
			if (!float.TryParse(list[3], out result3))
			{
				result3 = 100f;
			}
			float result4;
			if (!float.TryParse(list[4], out result4))
			{
				result4 = 100f;
			}
			float result5;
			if (!float.TryParse(list[5], out result5))
			{
				result5 = 0f;
			}
			MarkupElement component = Object.Instantiate(sample, MarkupCanvas.singleton.transform).GetComponent<MarkupElement>();
			spawnedElements.Add(component.gameObject);
			component.markupStyle.position = new Vector3(result, result2, 0f);
			component.markupStyle.size = new Vector2(result3, result4);
			component.markupStyle.rotation = result5;
			component.RefreshStyle(list[0]);
			if (MarkupWriter.OnCreateObject != null)
			{
				MarkupWriter.OnCreateObject(component);
			}
		}
	}
}
