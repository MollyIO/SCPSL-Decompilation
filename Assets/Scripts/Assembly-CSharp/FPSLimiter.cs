using UnityEngine;
using UnityEngine.UI;

public class FPSLimiter : MonoBehaviour
{
	public GameObject Warning;

	private void OnEnable()
	{
		if (ServerStatic.IsDedicated)
		{
			return;
		}
		Warning.SetActive(QualitySettings.vSyncCount != 0);
		int num = PlayerPrefs.GetInt("MaxFramerate", 969);
		Application.targetFrameRate = ((num != 969) ? num : (-1));
		if (Application.targetFrameRate == -1)
		{
			base.gameObject.GetComponent<Dropdown>().value = 0;
			return;
		}
		bool flag = false;
		for (int i = 1; i < base.gameObject.GetComponent<Dropdown>().options.Count; i++)
		{
			int result;
			if (!flag && int.TryParse(base.gameObject.GetComponent<Dropdown>().options[i].text, out result) && result == Application.targetFrameRate)
			{
				base.gameObject.GetComponent<Dropdown>().value = i;
				flag = true;
			}
		}
		if (!flag)
		{
			base.gameObject.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(Application.targetFrameRate.ToString()));
			base.gameObject.GetComponent<Dropdown>().RefreshShownValue();
			base.gameObject.GetComponent<Dropdown>().value = base.gameObject.GetComponent<Dropdown>().options.Count - 1;
		}
	}

	public void OnValueChange()
	{
		ChangeLimit(base.gameObject.GetComponent<Dropdown>().options[base.gameObject.GetComponent<Dropdown>().value].text);
	}

	private static void ChangeLimit(string limit)
	{
		int result;
		if (int.TryParse(limit, out result))
		{
			Application.targetFrameRate = Mathf.Clamp(result, 15, 999);
			PlayerPrefs.SetInt("MaxFramerate", Mathf.Clamp(result, 15, 999));
		}
		else
		{
			Application.targetFrameRate = -1;
			PlayerPrefs.SetInt("MaxFramerate", 969);
		}
	}
}
