using UnityEngine;

public class BottomPickerItem : MonoBehaviour
{
	private string key;

	private int id;

	public void SetupButton(string k, int i)
	{
		key = k;
		id = i;
	}

	public void Submit()
	{
		PlayerPrefs.SetInt(key, id);
	}
}
