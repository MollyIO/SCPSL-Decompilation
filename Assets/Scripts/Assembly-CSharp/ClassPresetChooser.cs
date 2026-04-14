using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassPresetChooser : MonoBehaviour
{
	[Serializable]
	public class PickerPreset
	{
		public string classID;

		public Texture icon;

		public int health;

		public float wSpeed;

		public float rSpeed;

		public float stamina;

		public Texture[] startingItems;

		public string en_additionalInfo;

		public string pl_additionalInfo;
	}

	public GameObject bottomMenuItem;

	public Transform bottomMenuHolder;

	public PickerPreset[] presets;

	private string curKey;

	private List<PickerPreset> curPresets = new List<PickerPreset>();

	public Slider health;

	public Slider wSpeed;

	public Slider rSpeed;

	public RawImage[] startItems;

	public RawImage avatar;

	public TextMeshProUGUI addInfo;

	public void RefreshBottomItems(string key)
	{
		curKey = key;
		int num = 0;
		PickerPreset[] array = presets;
		foreach (PickerPreset pickerPreset in array)
		{
			if (pickerPreset.classID == key)
			{
				num++;
				curPresets.Add(pickerPreset);
			}
		}
		foreach (Transform item in bottomMenuHolder)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		for (int j = 0; j < num; j++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(bottomMenuItem, bottomMenuHolder);
			gameObject.GetComponent<BottomPickerItem>().SetupButton(key, j);
			gameObject.GetComponentInChildren<Text>().text = "ABCDEFGHIJKL"[j].ToString();
		}
	}

	private void Update()
	{
		if (curPresets.Count <= 0)
		{
			return;
		}
		PickerPreset pickerPreset = curPresets[PlayerPrefs.GetInt(curKey, 0)];
		health.value = pickerPreset.health;
		wSpeed.value = pickerPreset.wSpeed;
		rSpeed.value = pickerPreset.rSpeed;
		avatar.texture = pickerPreset.icon;
		for (int i = 0; i < startItems.Length; i++)
		{
			if (i >= pickerPreset.startingItems.Length)
			{
				startItems[i].color = Color.clear;
				continue;
			}
			startItems[i].color = Color.white;
			startItems[i].texture = pickerPreset.startingItems[i];
		}
	}
}
