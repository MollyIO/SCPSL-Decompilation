using TMPro;
using UnityEngine;

public class SignBlink : MonoBehaviour
{
	public bool verticalText;

	private string startText;

	private const string alphabet = "QWERTYUIOPASDFGHJKLZXCVBNM01234567890!@#$%^&*()-_=+[]{}/<>";

	public void Play(int duration)
	{
		if (startText == string.Empty)
		{
			startText = GetComponent<TextMeshProUGUI>().text;
		}
		else
		{
			GetComponent<TextMeshProUGUI>().text = startText;
		}
	}
}
