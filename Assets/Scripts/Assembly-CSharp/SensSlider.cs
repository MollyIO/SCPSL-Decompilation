using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SensSlider : MonoBehaviour
{
	public AudioMixer master;

	public Slider slider;

	public Text optionalValueText;

	private void Start()
	{
		OnValueChanged(PlayerPrefs.GetInt("Volume", 0));
		slider.value = PlayerPrefs.GetInt("Volume", 0);
		master.SetFloat("volume", PlayerPrefs.GetInt("Volume", 0));
		optionalValueText.text = PlayerPrefs.GetInt("Volume", 0) + " dB";
	}

	public void OnValueChanged(float vol)
	{
		master.SetFloat("volume", vol);
		PlayerPrefs.SetInt("Volume", (int)vol);
		if (optionalValueText != null)
		{
			optionalValueText.text = (int)vol/*cast due to .constrained prefix*/ + " dB";
		}
	}
}
