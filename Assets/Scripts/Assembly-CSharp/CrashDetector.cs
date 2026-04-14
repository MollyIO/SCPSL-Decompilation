using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class CrashDetector : MonoBehaviour
{
	public GameObject root;

	public static CrashDetector singleton;

	private void Awake()
	{
		singleton = this;
	}

	public static bool Show()
	{
		if (SystemInfo.graphicsDeviceName.ToUpper().Contains("INTEL") && PlayerPrefs.GetInt("intel_warning") != 1)
		{
			PlayerPrefs.SetInt("intel_warning", 1);
			singleton.RunCoroutine(singleton._IShow());
			return true;
		}
		return false;
	}

	public void RunCoroutine(IEnumerator<float> coroutine)
	{
		Timing.RunCoroutine(coroutine, Segment.FixedUpdate);
	}

	public IEnumerator<float> _IShow()
	{
		root.SetActive(true);
		Button button = root.GetComponentInChildren<Button>();
		Text text = button.GetComponent<Text>();
		button.interactable = false;
		for (int i = 15; i >= 1; i--)
		{
			text.text = "OKAY (" + i + ")";
			yield return Timing.WaitForSeconds(1f);
		}
		text.text = "OKAY";
		button.interactable = true;
	}
}
