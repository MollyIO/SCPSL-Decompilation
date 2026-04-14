using UnityEngine;

public class SteamServerManager : MonoBehaviour
{
	public static SteamServerManager Instance;

	private bool _gsInitialized;

	private void Start()
	{
		Instance = this;
	}
}
