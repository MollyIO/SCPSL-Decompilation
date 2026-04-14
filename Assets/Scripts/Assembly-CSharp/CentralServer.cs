using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using GameConsole;
using UnityEngine;

public class CentralServer : MonoBehaviour
{
	public static object RefreshLock;

	private static string _serversPath;

	private static List<string> _workingServers;

	private static DateTime _lastReset;

	public static string MasterUrl { get; internal set; }

	public static string StandardUrl { get; internal set; }

	public static string SelectedServer { get; internal set; }

	public static bool TestServer { get; internal set; }

	public static bool ServerSelected { get; set; }

	internal static string[] Servers { get; private set; }

	public void Start()
	{
		if (File.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "testserver.txt"))
		{
			StandardUrl = "https://test.scpslgame.com/";
			MasterUrl = "https://test.scpslgame.com/";
			SelectedServer = "TEST";
			TestServer = true;
			ServerSelected = true;
			ServerConsole.AddLog("Using TEST central server: " + MasterUrl);
			return;
		}
		MasterUrl = "https://api.scpslgame.com/";
		StandardUrl = "https://api.scpslgame.com/";
		TestServer = false;
		_lastReset = DateTime.MinValue;
		Servers = new string[0];
		_workingServers = new List<string>();
		RefreshLock = new object();
		if (!Directory.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/"))
		{
			Directory.CreateDirectory(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/");
		}
		_serversPath = FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "internal/CentralServers";
		if (File.Exists(_serversPath))
		{
			Servers = FileManager.ReadAllLines(_serversPath);
			if (Servers.Any((string server) => !Regex.IsMatch(server, "^[a-zA-Z0-9]*$")))
			{
				GameConsole.Console.singleton.AddLog("Malformed server found on the list. Removing the list and redownloading it from api.scpslgame.com.", Color.yellow);
				Servers = new string[0];
				try
				{
					File.Delete(_serversPath);
				}
				catch (Exception ex)
				{
					GameConsole.Console.singleton.AddLog("Failed to delete malformed central server list.\nException: " + ex.Message, Color.red);
				}
				new Thread((ThreadStart)delegate
				{
					RefreshServerList(true);
				}).Start();
				return;
			}
			_workingServers = Servers.ToList();
			if (!ServerStatic.IsDedicated)
			{
				GameConsole.Console.singleton.AddLog("Cached central servers count: " + Servers.Length, Color.grey);
			}
			if (Servers.Length != 0)
			{
				System.Random random = new System.Random();
				SelectedServer = Servers[random.Next(Servers.Length)];
				StandardUrl = "https://" + SelectedServer.ToLower() + ".scpslgame.com/";
				if (ServerStatic.IsDedicated)
				{
					ServerConsole.AddLog("Selected central server: " + SelectedServer + " (" + StandardUrl + ")");
				}
				else
				{
					GameConsole.Console.singleton.AddLog("Selected central server: " + SelectedServer + " (" + StandardUrl + ")", Color.grey);
				}
			}
		}
		new Thread((ThreadStart)delegate
		{
			RefreshServerList(true);
		}).Start();
	}

	public static void RefreshServerList(bool planned = false)
	{
		lock (RefreshLock)
		{
			if (ServerSelected)
			{
				return;
			}
			if (_workingServers.Count == 0)
			{
				if (Servers.Length == 0)
				{
					StandardUrl = "https://api.scpslgame.com/";
					SelectedServer = "Primary API";
				}
				else
				{
					_workingServers = Servers.ToList();
					StandardUrl = "https://" + _workingServers[0] + ".scpslgame.com/";
					SelectedServer = _workingServers[0];
				}
			}
			int num = 1;
			while (num != 3)
			{
				num++;
				try
				{
					string text = HttpQuery.Get(StandardUrl + "servers.php");
					string[] array = text.Split(';');
					if (File.Exists(_serversPath))
					{
						File.Delete(_serversPath);
					}
					FileManager.WriteToFile(array, _serversPath);
					GameConsole.Console.singleton.AddLog("Updated list of central servers.", Color.green);
					GameConsole.Console.singleton.AddLog("Central servers count: " + array.Length, Color.cyan);
					Servers = array;
					if (planned && Servers.All((string srv) => srv != SelectedServer))
					{
						_workingServers = Servers.ToList();
						ChangeCentralServer(false);
					}
					ServerSelected = true;
					break;
				}
				catch (Exception ex)
				{
					GameConsole.Console.singleton.AddLog("Can't update central servers list!", Color.red);
					GameConsole.Console.singleton.AddLog("Error: " + ex.Message, Color.red);
					if (SelectedServer == "Primary API")
					{
						ServerSelected = true;
						break;
					}
					ChangeCentralServer(true);
				}
			}
		}
	}

	public static bool ChangeCentralServer(bool remove)
	{
		ServerSelected = false;
		TestServer = false;
		if (SelectedServer == "Primary API")
		{
			if (_lastReset >= DateTime.Now.AddMinutes(-2.0))
			{
				return false;
			}
			RefreshServerList();
			return true;
		}
		if (_workingServers.Count == 0)
		{
			GameConsole.Console.singleton.AddLog("All known central servers aren't working.", Color.yellow);
			_workingServers.Add("API");
			SelectedServer = "Primary API";
			StandardUrl = "https://api.scpslgame.com/";
			GameConsole.Console.singleton.AddLog("Changed central server: " + SelectedServer + " (" + StandardUrl + ")", Color.yellow);
			return true;
		}
		if (remove && _workingServers.Contains(SelectedServer))
		{
			_workingServers.Remove(SelectedServer);
		}
		if (_workingServers.Count == 0)
		{
			_workingServers.Add("API");
			SelectedServer = "Primary API";
			StandardUrl = "https://api.scpslgame.com/";
			GameConsole.Console.singleton.AddLog("Changed central server: " + SelectedServer + " (" + StandardUrl + ")", Color.yellow);
			return true;
		}
		System.Random random = new System.Random();
		SelectedServer = _workingServers[random.Next(0, _workingServers.Count)];
		StandardUrl = "https://" + SelectedServer.ToLower() + ".scpslgame.com/";
		GameConsole.Console.singleton.AddLog("Changed central server: " + SelectedServer + " (" + StandardUrl + ")", Color.yellow);
		return true;
	}
}
