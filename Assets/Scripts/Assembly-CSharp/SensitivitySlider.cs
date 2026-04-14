using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class SensitivitySlider : MonoBehaviour
{
	public Slider slider;

	private void Start()
	{
		if (PlayerPrefs.GetFloat("Sens", 1f) > slider.maxValue)
		{
			slider.maxValue = PlayerPrefs.GetFloat("Sens", 1f);
		}
		OnValueChanged(PlayerPrefs.GetFloat("Sens", 1f));
		slider.value = PlayerPrefs.GetFloat("Sens", 1f);
	}

	public void OnValueChanged(float vol)
	{
		PlayerPrefs.SetFloat("Sens", vol);
		Sensitivity.sens = slider.value;
	}

	public void ChangeViaConsole(float x)
	{
		if (slider.maxValue < x)
		{
			slider.maxValue = x;
		}
		slider.value = x;
	}
}
