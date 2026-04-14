using UnityEngine;

public class ToggleableLight : MonoBehaviour
{
	public GameObject[] allLights;

	public bool isAlarm;

	public void SetLights(bool b)
	{
		GameObject[] array = allLights;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive((!isAlarm) ? (!b) : b);
		}
	}
}
