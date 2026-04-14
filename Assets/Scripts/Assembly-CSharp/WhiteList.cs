using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class WhiteList : MonoBehaviour
{
	public static List<string> SteamIDs;

	private void Start()
	{
		ReloadWhitelist();
	}

	public static void ReloadWhitelist()
	{
		string path = FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "SteamIDWhitelist.txt";
		if (!Directory.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs)))
		{
			Directory.CreateDirectory(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs));
		}
		if (!File.Exists(path))
		{
			File.Create(path).Close();
		}
		string[] source = FileManager.ReadAllLines(path);
		SteamIDs = source.Where((string id) => !string.IsNullOrEmpty(id)).ToList();
	}

	public static bool IsWhitelisted(string steamId)
	{
		return SteamIDs.Contains(steamId) || !ConfigFile.ServerConfig.GetBool("enable_whitelist") || !ConfigFile.ServerConfig.GetBool("online_mode", true);
	}
}
