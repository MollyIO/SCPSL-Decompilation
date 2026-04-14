using UnityEngine;
using UnityEngine.UI;

public class TutorialUnlocker : MonoBehaviour
{
	public Button[] buttons;

	private void Start()
	{
		for (int i = 0; i < Mathf.Clamp(PlayerPrefs.GetInt("TutorialProgress", 1), 1, buttons.Length); i++)
		{
			buttons[i].interactable = true;
		}
	}
}
