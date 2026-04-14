using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
	public GameObject checkmark;

	public bool isOn;

	private void OnEnable()
	{
		isOn = PlayerPrefs.GetInt("SavedFullscreen", 1) != 0;
		checkmark.SetActive(isOn);
	}

	public void Click()
	{
		isOn = !isOn;
		checkmark.SetActive(isOn);
		PlayerPrefs.SetInt("SavedFullscreen", isOn ? 1 : 0);
		ResolutionManager.ChangeFullscreen(isOn);
	}
}
