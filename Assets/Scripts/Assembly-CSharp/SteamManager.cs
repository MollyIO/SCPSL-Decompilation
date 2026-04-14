using System;
using System.Linq;
using Facepunch.Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
	public const uint SteamAppId = 700330u;

	private static Auth.Ticket _ticket;

	private static string _state = string.Empty;

	public static Client Client { get; private set; }

	public static bool Running { get; private set; }

	public static ulong SteamId64 { get; private set; }

	public static void SetAchievement(string key)
	{
		if (Running)
		{
			Client.Update();
			Client.Achievements.Find(key).Trigger();
		}
	}

	public static bool CheckAchievement(string key)
	{
		if (!Running)
		{
			return false;
		}
		Client.Update();
		return Client.Achievements.Find(key).State;
	}

	public static bool IndicateAchievementProgress(string name, uint curProgress, uint maxProgress)
	{
		return Client.Achievements.IndicateAchievementProgress(name, curProgress, maxProgress);
	}

	public static void SetStat(string key, int value)
	{
		Client.Stats.Set(key, value);
	}

	public static int GetStat(string key)
	{
		return Client.Stats.GetInt(key);
	}

	public static string GetPersonaName(ulong steamid = 0uL)
	{
		return Client.Friends.GetName((steamid != 0) ? steamid : SteamId64);
	}

	public static Auth.Ticket GetAuthSessionTicket()
	{
		if (!Running)
		{
			return null;
		}
		if (_ticket == null)
		{
			_ticket = Client.Auth.GetAuthSessionTicket();
		}
		return _ticket;
	}

	public static void CancelTicket()
	{
		if (_ticket != null)
		{
			_ticket.Cancel();
			_ticket = null;
		}
	}

	public static void OpenProfile(ulong steamid = 0uL)
	{
		Client.Overlay.OpenProfile((steamid != 0) ? steamid : SteamId64);
	}

	public static void StartClient()
	{
		if (Running)
		{
			Debug.Log("Only one Steam Client can be initalized at the same time!");
			return;
		}
		Client = new Client(700330u, out _state);
		if (!Client.IsValid)
		{
			Debug.LogError(_state);
			StopClient();
		}
		else
		{
			Client.Inventory.EnableItemProperties = false;
			SteamId64 = Client.SteamId;
			Running = true;
		}
	}

	public static void StopClient()
	{
		Client client = Client;
		if (client != null)
		{
			client.Dispose();
		}
		Client = null;
		Running = false;
	}

	public static string GetApiState()
	{
		return Running ? "Loaded" : ((!string.IsNullOrEmpty(_state)) ? _state : "Not Loaded");
	}

	public static void RestartSteam()
	{
		StopClient();
		StartClient();
	}

	public static void RefreshToken()
	{
		GetAuthSessionTicket();
		CancelTicket();
		Debug.Log("Refreshed auth token!");
	}

	private void Awake()
	{
		if (!ServerStatic.IsDedicated && !Environment.GetCommandLineArgs().Contains("-nographics") && !GetComponent<ServerStatic>().Simulate)
		{
			Config.ForUnity(Application.platform.ToString());
			UnityEngine.Object.DontDestroyOnLoad(this);
			StartClient();
			RefreshToken();
		}
	}

	private void Update()
	{
		if (Running)
		{
			Client.Update();
		}
	}

	private void OnApplicationQuit()
	{
		CancelTicket();
		StopClient();
	}

	private void OnDestroy()
	{
		CancelTicket();
		StopClient();
	}
}
