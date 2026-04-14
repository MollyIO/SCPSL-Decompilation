using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameConsole;
using UnityEngine;

public class MuteHandler : MonoBehaviour
{
	private static string _path;

	private static List<string> mutes;

	private static object _fileLock;

	private void Start()
	{
		_fileLock = new object();
		if (ServerStatic.IsDedicated)
		{
			ServerConsole.AddLog("Loading saved mutes...");
		}
		else
		{
			GameConsole.Console.singleton.AddLog("Loading saved mutes...", Color.gray);
		}
		_path = FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "mutes.txt";
		mutes = new List<string>();
		try
		{
			if (!Directory.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs)))
			{
				Directory.CreateDirectory(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs));
			}
			if (!File.Exists(_path))
			{
				File.Create(_path).Close();
			}
			else
			{
				mutes = FileManager.ReadAllLines(_path).ToList();
			}
		}
		catch
		{
			ServerConsole.AddLog("Can't create mute file!");
		}
	}

	public static bool QueryPersistantMute(string steamId)
	{
		if (mutes == null)
		{
			if (ServerStatic.IsDedicated)
			{
				ServerConsole.AddLog("MuteHandler module not loaded. Can't query mute for player " + steamId + "!");
			}
			else
			{
				GameConsole.Console.singleton.AddLog("MuteHandler module not loaded. Can't query mute for player  " + steamId + "!", Color.red);
			}
			return false;
		}
		if (string.IsNullOrEmpty(steamId))
		{
			return false;
		}
		steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
		return mutes.Any((string item) => item == steamId);
	}

	public static void IssuePersistantMute(string steamId)
	{
		if (mutes == null)
		{
			if (ServerStatic.IsDedicated)
			{
				ServerConsole.AddLog("MuteHandler module not loaded. Can't save mute for player " + steamId + "!");
			}
			else
			{
				GameConsole.Console.singleton.AddLog("MuteHandler module not loaded. Can't save mute for player  " + steamId + "!", Color.red);
			}
		}
		else
		{
			if (string.IsNullOrEmpty(steamId))
			{
				return;
			}
			steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			if (mutes.Any((string item) => item == steamId))
			{
				return;
			}
			lock (_fileLock)
			{
				mutes.Add(steamId);
				FileManager.AppendFile(steamId, _path);
				if (ServerStatic.IsDedicated)
				{
					ServerConsole.AddLog("Mute for player " + steamId + " saved.");
				}
				else
				{
					GameConsole.Console.singleton.AddLog("Mute for player " + steamId + " saved.", Color.gray);
				}
			}
		}
	}

	public static void RevokePersistantMute(string steamId)
	{
		if (mutes == null)
		{
			if (ServerStatic.IsDedicated)
			{
				ServerConsole.AddLog("MuteHandler module not loaded. Can't save unmute for player " + steamId + "!");
			}
			else
			{
				GameConsole.Console.singleton.AddLog("MuteHandler module not loaded. Can't save unmute for player  " + steamId + "!", Color.red);
			}
		}
		else
		{
			if (string.IsNullOrEmpty(steamId))
			{
				return;
			}
			steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			if (!mutes.Any((string item) => item == steamId))
			{
				return;
			}
			lock (_fileLock)
			{
				mutes.Remove(steamId);
				string[] data = (from l in FileManager.ReadAllLines(_path)
					where l != steamId
					select l).ToArray();
				FileManager.WriteToFile(data, _path, true);
				if (ServerStatic.IsDedicated)
				{
					ServerConsole.AddLog("Mute for player " + steamId + " removed.");
				}
				else
				{
					GameConsole.Console.singleton.AddLog("Mute for player " + steamId + " removed.", Color.gray);
				}
			}
		}
	}
}
