using System;
using System.IO;
using System.Linq;
using GameConsole;
using UnityEngine;

public class CentralServerKeyCache
{
	public const string CacheLocation = "internal/KeyCache";

	public const string InternalDir = "internal/";

	public static string ReadCache()
	{
		try
		{
			string path = FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/KeyCache";
			if (!File.Exists(path))
			{
				ServerConsole.AddLog("Central server public key not found in cache.");
				return null;
			}
			string[] source = FileManager.ReadAllLines(path);
			string result = source.Aggregate(string.Empty, (string current, string line) => current + line + "\n");
			try
			{
				return result;
			}
			catch (Exception ex)
			{
				if (ServerStatic.IsDedicated)
				{
					ServerConsole.AddLog("Can't load central server public key from cache - " + ex.Message);
				}
				else
				{
					GameConsole.Console.singleton.AddLog("Can't load central server public key from cache - " + ex.Message, Color.magenta);
				}
				return null;
			}
		}
		catch (Exception ex2)
		{
			ServerConsole.AddLog("Can't read public key cache - " + ex2.Message);
			return null;
		}
	}

	public static void SaveCache(string key)
	{
		try
		{
			string path = FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/KeyCache";
			if (!Directory.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/"))
			{
				Directory.CreateDirectory(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/");
			}
			if (File.Exists(path))
			{
				if (key == ReadCache())
				{
					ServerConsole.AddLog("Key cache is up to date.");
					return;
				}
				File.Delete(path);
			}
			ServerConsole.AddLog("Updating key cache...");
			FileManager.WriteStringToFile(key, path);
			ServerConsole.AddLog("Key cache updated.");
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Can't write public key cache - " + ex.Message);
		}
	}
}
