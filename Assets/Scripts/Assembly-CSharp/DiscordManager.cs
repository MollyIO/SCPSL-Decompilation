using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordManager : MonoBehaviour
{
	public static DiscordManager singleton;

	public DiscordRpc.RichPresencePrefab[] classPresets;

	public DiscordRpc.RichPresencePrefab menuPreset;

	public DiscordRpc.RichPresencePrefab waitingPreset;

	private CustomNetworkManager nm;

	private void Start()
	{
		singleton = this;
		nm = GetComponent<CustomNetworkManager>();
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	public void ChangePreset(int classID)
	{
		if (classID < 0)
		{
			DiscordController.presence = ((classID != -1) ? DiscordRpc.FromPrefab(menuPreset) : DiscordRpc.FromPrefab(waitingPreset));
		}
		else
		{
			try
			{
				DiscordController.presence.state = classPresets[classID].state;
				DiscordController.presence.largeImageKey = classPresets[classID].largeImageKey;
				DiscordController.presence.smallImageKey = classPresets[classID].smallImageKey;
			}
			catch
			{
			}
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long startTimestamp = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		DiscordController.presence.startTimestamp = startTimestamp;
		string text = ((classID == -2 || !nm.networkAddress.Contains(".")) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(nm.networkAddress + ":" + nm.networkPort + ":" + CustomNetworkManager.CompatibleVersions[0])));
		DiscordController.presence.joinSecret = text;
		DiscordController.presence.partyId = "LOBBY#" + text;
		if (string.IsNullOrEmpty(text))
		{
			DiscordController.presence.partySize = 0;
			DiscordController.presence.partyMax = 0;
		}
		DiscordRpc.UpdatePresence(DiscordController.presence);
	}

	public static void ChangeLobbyStatus(int cur, int max)
	{
		DiscordController.presence.partySize = cur;
		DiscordController.presence.partyMax = max;
		DiscordRpc.UpdatePresence(DiscordController.presence);
	}

	public static void PrintMessage(string msg)
	{
		Debug.Log(msg);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			if (Input.GetKeyDown(KeyCode.Y))
			{
				DiscordController.RequestRespondYes();
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				DiscordController.RequestRespondNo();
			}
		}
	}

	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 3 || scene.buildIndex == 4)
		{
			DiscordController.presence.partySize = 0;
			DiscordController.presence.partyMax = 0;
			ChangePreset(-2);
		}
		if (scene.name == "Facility")
		{
			ChangePreset(-1);
		}
	}
}
