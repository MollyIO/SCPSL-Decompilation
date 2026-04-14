using System;
using UnityEngine;
using UnityEngine.UI;

public class Embargo : MonoBehaviour
{
	private Text txt;

	public bool showEmbargo;

	private void Start()
	{
		txt = GetComponent<Text>();
		InvokeRepeating("ChangePosition", 3f, 3f);
	}

	private void ChangePosition()
	{
		GetComponent<RectTransform>().localPosition = new Vector3(UnityEngine.Random.Range(-500, 500), UnityEngine.Random.Range(-250, 280), 0f);
	}

	private void Update()
	{
		if (showEmbargo)
		{
			DateTime now = DateTime.Now;
			txt.text = "<size=30><color=#a11>EMBARGO</color></size>\n\n" + now.Day + "." + now.Month + "." + now.Year + " " + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + "\n" + SystemInfo.operatingSystem + "\n" + SystemInfo.deviceName + "\n<size=18><color=#a11>DO NOT SHARE</color></size>";
		}
		else
		{
			txt.text = string.Empty;
		}
	}
}
