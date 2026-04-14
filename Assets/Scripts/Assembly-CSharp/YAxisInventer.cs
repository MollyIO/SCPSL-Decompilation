using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class YAxisInventer : MonoBehaviour
{
	public Toggle toggle;

	private void Start()
	{
		toggle.isOn = PlayerPrefs.GetInt("y_invert", 0) == 1;
		ChangeState(toggle.isOn);
	}

	public void ChangeState(bool b)
	{
		PlayerPrefs.SetInt("y_invert", b ? 1 : 0);
		MouseLook.invert = b;
	}
}
