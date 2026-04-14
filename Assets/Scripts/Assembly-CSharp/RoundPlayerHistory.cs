using System;
using System.Collections.Generic;
using UnityEngine;

public class RoundPlayerHistory : MonoBehaviour
{
	[Serializable]
	public class PlayerHistoryLog
	{
		public string Nickname;

		public int PlayerID;

		public string SteamID64;

		public int ConnectionStatus;

		public int LastAliveClass;

		public int CurrentClass;

		public DateTime ConnectionStart;

		public DateTime ConnectionStop;
	}

	public static RoundPlayerHistory singleton;

	public List<PlayerHistoryLog> historyLogs = new List<PlayerHistoryLog>();

	private void Awake()
	{
		singleton = this;
	}

	public PlayerHistoryLog GetData(int playerID)
	{
		foreach (PlayerHistoryLog historyLog in historyLogs)
		{
			if (historyLog.PlayerID == playerID)
			{
				return historyLog;
			}
		}
		return null;
	}

	public void SetData(int playerID, string _newNick, int _newPlyID, string _newSteamId, int _newConnectionStatus, int _newAliveClass, int _newCurrentClass, DateTime _newStartTime, DateTime _newStopTime)
	{
		int num = -1;
		if (playerID == -1)
		{
			historyLogs.Add(new PlayerHistoryLog
			{
				Nickname = "Player",
				PlayerID = 0,
				SteamID64 = string.Empty,
				ConnectionStatus = 0,
				LastAliveClass = -1,
				CurrentClass = -1,
				ConnectionStart = DateTime.Now,
				ConnectionStop = new DateTime(0, 0, 0)
			});
			num = historyLogs.Count - 1;
		}
		else
		{
			for (int i = 0; i < historyLogs.Count; i++)
			{
				if (historyLogs[i].PlayerID == playerID)
				{
					num = i;
				}
			}
		}
		if (num >= 0)
		{
			if (_newNick != string.Empty)
			{
				historyLogs[num].Nickname = _newNick;
			}
			if (_newPlyID != 0)
			{
				historyLogs[num].PlayerID = _newPlyID;
			}
			if (_newSteamId != string.Empty)
			{
				historyLogs[num].SteamID64 = _newSteamId;
			}
			if (_newConnectionStatus != 0)
			{
				historyLogs[num].ConnectionStatus = _newConnectionStatus;
			}
			if (_newAliveClass != 0)
			{
				historyLogs[num].LastAliveClass = _newAliveClass;
			}
			if (_newCurrentClass != 0)
			{
				historyLogs[num].CurrentClass = _newCurrentClass;
			}
			if (_newStartTime.Year != 0)
			{
				historyLogs[num].ConnectionStart = _newStartTime;
			}
			if (_newStopTime.Year != 0)
			{
				historyLogs[num].ConnectionStop = _newStopTime;
			}
		}
	}
}
