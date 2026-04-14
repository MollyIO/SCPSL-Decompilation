using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSlider : MonoBehaviour
{
	public AudioMixer master;

	public Slider slider;

	public Text optionalValueText;

	public string keyName = "Volume";

	private void Awake()
	{
		keyName += "-new";
	}

	private void Start()
	{
		slider.value = PlayerPrefs.GetFloat(keyName, 1f);
		OnValueChanged(slider.value);
	}

	public void OnValueChanged(float vol)
	{
		if (optionalValueText != null)
		{
			optionalValueText.text = Mathf.RoundToInt(vol * 100f) + " %";
		}
		PlayerPrefs.SetFloat(keyName, vol);
		float value = ((vol == 0f) ? (-144f) : (20f * Mathf.Log10(vol)));
		master.SetFloat(keyName, value);
	}
}
