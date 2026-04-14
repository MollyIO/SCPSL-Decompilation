using UnityEngine;

public class VeryHighPerformance : MonoBehaviour
{
	private void Start()
	{
		if (PlayerPrefs.GetInt("gfxsets_hp", 0) == 0 || ServerStatic.IsDedicated)
		{
			LocalCurrentRoomEffects.isVhigh = false;
			return;
		}
		Light[] array = Object.FindObjectsOfType<Light>();
		Light[] array2 = array;
		foreach (Light light in array2)
		{
			Object.Destroy(light.transform.gameObject);
		}
		LocalCurrentRoomEffects.isVhigh = true;
	}
}
