using UnityEngine;
using UnityEngine.UI;

public class WarningAwawke : MonoBehaviour
{
	public Toggle toggle;

	private void Awake()
	{
		if (PlayerPrefs.GetString("warningToggle", "false") == "true")
		{
			base.gameObject.SetActive(false);
		}
	}

	public void Close()
	{
		if (toggle.isOn)
		{
			PlayerPrefs.SetString("warningToggle", "true");
		}
		base.gameObject.SetActive(false);
	}
}
