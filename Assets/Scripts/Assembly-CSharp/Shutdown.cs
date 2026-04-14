using UnityEngine;

public class Shutdown : MonoBehaviour
{
	private static bool _quitting;

	public static void SafeQuit()
	{
		if (!_quitting)
		{
			_quitting = true;
			SteamManager.StopClient();
			Application.Quit();
		}
	}

	private void OnApplicationQuit()
	{
		if (!_quitting)
		{
			_quitting = true;
			SteamManager.StopClient();
		}
	}
}
