using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Dissonance.Integrations.UNet_HLAPI;
using GameConsole;
using MEC;
using Mono.Nat;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
	[Serializable]
	public class DisconnectLog
	{
		[Serializable]
		public class LogButton
		{
			public ConnInfoButton[] actions;
		}

		[Multiline]
		public string msg_en;

		public LogButton button;

		public bool autoHideOnSceneLoad;
	}

	public GameObject popup;

	public GameObject createpop;

	public RectTransform contSize;

	public Text content;

	private GameConsole.Console _console;

	private static QueryServer _queryserver;

	private List<INatDevice> _mappedDevices;

	public DisconnectLog[] logs;

	private int _curLogId;

	private int _queryPort;

	private int _expectedGameFilesVersion;

	public bool reconnect;

	private bool _queryEnabled;

	private bool _configLoaded;

	private bool _activated;

	public string disconnectMessage = string.Empty;

	public static string Ip = string.Empty;

	public static string ConnectionIp;

	[Space(30f)]
	public int GameFilesVersion;

	public static string[] CompatibleVersions;

	public static readonly bool isPrivateBeta;

	public static readonly bool isStreamingAllowed = true;

	public static bool Modded;

	private int reservedSlots;

	public int ReservedMaxPlayers
	{
		[CompilerGenerated]
		get
		{
			return base.maxConnections - reservedSlots;
		}
	}

	private void SetCompatibleVersions()
	{
		CompatibleVersions = new string[1] { "8.0.1 (Revision III)" };
		_expectedGameFilesVersion = 3;
	}

	private void Update()
	{
		if (popup.activeSelf && Input.GetKey(KeyCode.Escape))
		{
			ClickButton();
		}
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		SteamManager.CancelTicket();
		ShowLog((int)conn.lastError, string.Empty, string.Empty);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		ShowLog(errorCode, string.Empty, string.Empty);
	}

	public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient(client);
		StartCoroutine(_ConnectToServer(client));
	}

	private IEnumerator<float> _ConnectToServer(NetworkClient client)
	{
		while (_curLogId == 13)
		{
			if (client.isConnected)
			{
				ShowLog(17, string.Empty, string.Empty);
			}
			yield return 0f;
		}
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		if (BanHandler.QueryBan(null, conn.address).Value != null)
		{
			ServerConsole.AddLog("Player tried to connect from banned IP address " + conn.address + ".");
			ServerConsole.Disconnect(conn, "You are banned from this server.");
		}
		else
		{
			ServerConsole.AddLog("Player joined from IP address " + conn.address + ".");
		}
		if (base.numPlayers > ReservedMaxPlayers || ConfigFile.ServerConfig.GetBool("reserved_slots_simulate_full"))
		{
			string text = ReservedSlot.TrimIPAddress(conn.address);
			if (!ReservedSlot.ContainsIP(text))
			{
				ServerConsole.Disconnect(conn, "Reserved Slots - Server is Full.");
			}
			else
			{
				ServerConsole.AddLog("RESERVED_SLOTS # Player joined dedicated slot (" + text + ")");
			}
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		HlapiServer.OnServerDisconnect(conn);
		conn.Disconnect();
		conn.Dispose();
	}

	public static void PlayerDisconnect(NetworkConnection conn)
	{
		HlapiServer.OnServerDisconnect(conn);
	}

	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (!_activated && scene.name.ToLower().Contains("menu"))
		{
			_activated = true;
			base.networkAddress = "none";
			StartClient();
			base.networkAddress = "localhost";
			StopClient();
		}
		if (reconnect)
		{
			ShowLog(14, string.Empty, string.Empty);
			Invoke("Reconnect", 3f);
		}
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		base.OnClientSceneChanged(conn);
		if (!reconnect && logs[_curLogId].autoHideOnSceneLoad)
		{
			popup.SetActive(false);
		}
	}

	private void Reconnect()
	{
		if (reconnect)
		{
			reconnect = false;
			StartClient();
		}
	}

	public void StopReconnecting()
	{
		reconnect = false;
	}

	public void ShowLog(int id, string your = "", string server = "")
	{
		_curLogId = id;
		popup.SetActive(true);
		content.text = TranslationReader.Get("Connection_Errors", id);
		if (your != string.Empty && server != string.Empty)
		{
			content.text = content.text.Replace("[your]", your).Replace("[server]", server);
		}
		if (!string.IsNullOrEmpty(disconnectMessage))
		{
			string[] array = content.text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			if (array.Length > 0)
			{
				content.text = array[0] + Environment.NewLine + disconnectMessage;
			}
			disconnectMessage = string.Empty;
		}
		content.rectTransform.sizeDelta = Vector3.zero;
	}

	public void ClickButton()
	{
		ConnInfoButton[] actions = logs[_curLogId].button.actions;
		foreach (ConnInfoButton connInfoButton in actions)
		{
			connInfoButton.UseButton();
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
	}

	public void LoadConfigs()
	{
		if (!_configLoaded)
		{
			_configLoaded = true;
			SetCompatibleVersions();
			if (File.Exists("hoster_policy.txt"))
			{
				ConfigFile.HosterPolicy = new YamlConfig("hoster_policy.txt");
			}
			else if (File.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "hoster_policy.txt"))
			{
				ConfigFile.HosterPolicy = new YamlConfig(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "hoster_policy.txt");
			}
			else
			{
				ConfigFile.HosterPolicy = new YamlConfig();
			}
			if (!ServerStatic.IsDedicated)
			{
				ServerConsole.AddLog("Loading config...");
				ConfigFile.ServerConfig = ConfigFile.ReloadGameConfig(FileManager.GetAppFolder() + "config_gameplay.txt");
				ServerConsole.AddLog("Config file loaded!");
			}
		}
	}

	private void Start()
	{
		LoadConfigs();
		if (ServerStatic.IsDedicated)
		{
			return;
		}
		_console = GameConsole.Console.singleton;
		if (!SteamManager.Running)
		{
			_console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue));
		}
		else
		{
			if (Directory.Exists("SCPSL_Data\\Managed"))
			{
				if (!File.Exists("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
				{
					CreateVersionFile(false);
				}
				else
				{
					string[] array = FileManager.ReadAllLines("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
					if (array.Length < 1 || !Misc.Base64Decode(array[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Contains(";"))
					{
						CreateVersionFile(false);
					}
					else
					{
						string[] array2 = Misc.Base64Decode(array[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Split(';');
						if (array2.Length != 3 || array2[0] != CompatibleVersions[0])
						{
							CreateVersionFile(false);
						}
						else if (array2[2] != SteamManager.SteamId64.ToString())
						{
							try
							{
								string plainText = array2[0] + ";" + array2[1] + ";" + SteamManager.SteamId64;
								File.Delete("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
								File.Create("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
								FileManager.AppendFile("UI Build GUID: " + GUIDSplit(Misc.Base64Encode(plainText)), "SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
							}
							catch (Exception)
							{
								GameConsole.Console.singleton.AddLog("IO startup error 2.1", Color.red);
							}
						}
					}
				}
			}
			if (Directory.Exists("PrivateBeta") && Directory.Exists("PrivateBeta\\SCPSL_Data\\Managed"))
			{
				if (!File.Exists("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
				{
					CreateVersionFile(true);
				}
				else
				{
					string[] array3 = FileManager.ReadAllLines("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
					if (array3.Length < 1 || !Misc.Base64Decode(array3[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Contains(";"))
					{
						CreateVersionFile(true);
					}
					else
					{
						string[] array4 = Misc.Base64Decode(array3[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Split(';');
						if (array4.Length != 3 || array4[0] != CompatibleVersions[0])
						{
							CreateVersionFile(true);
						}
						else if (array4[2] != SteamManager.SteamId64.ToString())
						{
							try
							{
								string plainText2 = array4[0] + ";" + array4[1] + ";" + SteamManager.SteamId64;
								File.Delete("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
								File.Create("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
								FileManager.AppendFile("UI Build GUID: " + GUIDSplit(Misc.Base64Encode(plainText2)), "PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
							}
							catch (Exception)
							{
								GameConsole.Console.singleton.AddLog("IO startup error 2.2", Color.red);
							}
						}
					}
				}
			}
		}
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
		base.connectionConfig.MaxSentMessageQueueSize = 300;
	}

	private string GUIDSplit(string GUID)
	{
		string text = string.Empty;
		while (GUID.Length > 5)
		{
			text += GUID.Substring(0, 5);
			text += "-";
			GUID = GUID.Substring(5);
		}
		return text + GUID;
	}

	private void CreateVersionFile(bool privbeta)
	{
		if (!privbeta)
		{
			try
			{
				if (File.Exists("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
				{
					File.Delete("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
				}
				File.Create("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
				FileManager.AppendFile("UI Build GUID: " + GUIDSplit(Misc.Base64Encode(CompatibleVersions[0] + ";" + SteamManager.SteamId64 + ";-")), "SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
				return;
			}
			catch
			{
				GameConsole.Console.singleton.AddLog("IO startup error 1.1", Color.red);
				return;
			}
		}
		if (!Directory.Exists("PrivateBeta"))
		{
			return;
		}
		try
		{
			if (File.Exists("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
			{
				File.Delete("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
			}
			File.Create("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
			FileManager.AppendFile("UI Build GUID: " + GUIDSplit(Misc.Base64Encode(CompatibleVersions[0] + ";" + SteamManager.SteamId64 + ";-")), "PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
		}
		catch
		{
			GameConsole.Console.singleton.AddLog("IO startup error 1.2", Color.red);
		}
	}

	public void CreateMatch()
	{
		LoadConfigs();
		ShowLog(13, string.Empty, string.Empty);
		createpop.SetActive(false);
		NetworkServer.Reset();
		base.networkPort = GetFreePort();
		reservedSlots = ConfigFile.ServerConfig.GetInt("reserved_slots", ReservedSlot.GetSlots().Length);
		reservedSlots = Mathf.Max(reservedSlots, 0);
		base.maxConnections = ConfigFile.ServerConfig.GetInt("max_players", 20) + reservedSlots;
		int num = ConfigFile.HosterPolicy.GetInt("players_limit", -1);
		if (num > 0 && base.maxConnections > num)
		{
			base.maxConnections = num;
			ServerConsole.AddLog("You have exceeded players limit set by your hosting provider. Max players value set to " + num);
		}
		ServerConsole.Port = base.networkPort;
		ServerConsole.AddLog("Config file loaded: " + ConfigFile.ConfigPath);
		_queryEnabled = ConfigFile.ServerConfig.GetBool("enable_query");
		if (ConfigFile.ServerConfig.GetBool("forward_ports", true))
		{
			UpnpStart();
		}
		string text = FileManager.GetAppFolder() + "config_remoteadmin.txt";
		if (!File.Exists(text))
		{
			File.Copy("MiscData/remoteadmin_template.txt", text);
		}
		ServerStatic.RolesConfigPath = text;
		ServerStatic.RolesConfig = new YamlConfig(text);
		ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig);
		ServerConsole.FriendlyFire = ConfigFile.ServerConfig.GetBool("friendly_fire");
		ServerConsole.WhiteListEnabled = ConfigFile.ServerConfig.GetBool("enable_whitelist");
		Timing.RunCoroutine(_CreateLobby());
		if (!ServerStatic.IsDedicated)
		{
			NonsteamHost();
		}
	}

	private IEnumerator<float> _CreateLobby()
	{
		if (GameFilesVersion != _expectedGameFilesVersion)
		{
			ServerConsole.AddLog("This source code file is made for different version of the game!");
			ServerConsole.AddLog("Please validate game files integrity using steam!");
			ServerConsole.AddLog("Aborting server startup.");
			yield break;
		}
		ServerConsole.AddLog("Game version: " + CompatibleVersions[0]);
		if (isPrivateBeta)
		{
			ServerConsole.AddLog("PRIVATE BETA VERSION - DO NOT SHARE");
		}
		yield return 0f;
		ServerConsole.AddLog((!ConfigFile.ServerConfig.GetBool("online_mode", true)) ? "Online mode is DISABLED - SERVER CANNOT VALIDATE STEAM ID OF CONNECTING PLAYERS!!! Features like SteamID admin authentication won't work." : "Online mode is ENABLED.");
		UnityEngine.Object.FindObjectOfType<ServerConsole>().RunRefreshPublicKey();
		UnityEngine.Object.FindObjectOfType<ServerConsole>().RunRefreshCentralServers();
		if (_queryEnabled)
		{
			_queryPort = base.networkPort + ConfigFile.ServerConfig.GetInt("query_port_shift");
			ServerConsole.AddLog("Query port will be enabled on port " + _queryPort + " TCP.");
			_queryserver = new QueryServer(_queryPort, ConfigFile.ServerConfig.GetBool("query_use_IPv6", true));
			_queryserver.StartServer();
		}
		else
		{
			ServerConsole.AddLog("Query port disabled in config!");
		}
		ServerConsole.AddLog("Starting server...");
		if (ConfigFile.HosterPolicy.GetString("server_ip", "none") != "none")
		{
			Ip = ConfigFile.HosterPolicy.GetString("server_ip", "none");
			ServerConsole.AddLog("Server IP set to " + Ip + " by your hosting provider.");
		}
		else if (ConfigFile.ServerConfig.GetBool("online_mode", true) && ServerStatic.IsDedicated)
		{
			if (ConfigFile.ServerConfig.GetString("server_ip", "auto") != "auto")
			{
				Ip = ConfigFile.ServerConfig.GetString("server_ip", "auto");
				ServerConsole.AddLog("Custom config detected. Your game-server IP will be " + Ip);
			}
			else
			{
				ServerConsole.AddLog("Obtaining your external IP address...");
				using (WWW www = new WWW(CentralServer.StandardUrl + "ip.php"))
				{
					yield return Timing.WaitUntilDone(www);
					if (!string.IsNullOrEmpty(www.error))
					{
						ServerConsole.AddLog("Error: connection to " + CentralServer.StandardUrl + " failed. Website returned: " + www.error + " | Aborting startup... LOGTYPE-8");
						yield break;
					}
					Ip = ((!www.text.EndsWith(".")) ? www.text : www.text.Remove(www.text.Length - 1));
					ServerConsole.AddLog("Done, your game-server IP will be " + Ip);
				}
			}
		}
		else
		{
			Ip = "127.0.0.1";
		}
		ServerConsole.Ip = Ip;
		ServerConsole.AddLog("Initializing game server...");
		if (!ServerStatic.IsDedicated)
		{
			yield break;
		}
		if (ConfigFile.HosterPolicy.GetString("bind_ip", "none") != "none")
		{
			if (ConfigFile.HosterPolicy.GetString("bind_ip", "ANY").ToUpper() == "ANY")
			{
				ServerConsole.AddLog("Server starting at all IP addresses and port " + base.networkPort + " - set by your hosting provider.");
				base.serverBindToIP = false;
				StartHost();
			}
			else
			{
				ServerConsole.AddLog("Server starting at IP " + ConfigFile.HosterPolicy.GetString("bind_ip", "ANY") + " and port " + base.networkPort + " - set by your hosting provider.");
				base.serverBindAddress = ConfigFile.HosterPolicy.GetString("bind_ip", "ANY");
				base.serverBindToIP = true;
				StartHost();
			}
		}
		else if (ConfigFile.ServerConfig.GetString("bind_ip", "ANY").ToUpper() == "ANY")
		{
			ServerConsole.AddLog("Server starting at all IP addresses and port " + base.networkPort);
			base.serverBindToIP = false;
			StartHost();
		}
		else
		{
			ServerConsole.AddLog("Server starting at IP " + ConfigFile.ServerConfig.GetString("bind_ip", "ANY") + " and port " + base.networkPort);
			base.serverBindAddress = ConfigFile.ServerConfig.GetString("bind_ip", "ANY");
			base.serverBindToIP = true;
			StartHost();
		}
		while (SceneManager.GetActiveScene().name != "Facility")
		{
			yield return 0f;
		}
		ServerConsole.AddLog("Level loaded. Creating match...");
		if (!ConfigFile.ServerConfig.GetBool("online_mode", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to online_mode turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		if (!ConfigFile.ServerConfig.GetBool("use_vac", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to use_vac turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		if (!ConfigFile.ServerConfig.GetBool("global_bans_cheating", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to global_bans_cheating turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		if (ConfigFile.ServerConfig.GetBool("disable_global_badges"))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to disable_global_badges turned on in server configuration (this is servermod function - if you are not using servermod, you can safely remove this config value, it won't change anything).LOGTYPE-8");
			yield break;
		}
		if (ConfigFile.ServerConfig.GetBool("hide_global_badges"))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to hide_global_badges turned on in server configuration. You can still disable specific badges instead of using this command. (this is servermod function - if you are not using servermod, you can safely remove this config value, it won't change anything).LOGTYPE-8");
			yield break;
		}
		if (ConfigFile.ServerConfig.GetBool("disable_ban_bypass"))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to disable_ban_bypass turned on in server configuration. (this is servermod function - if you are not using servermod, you can safely remove this config value, it won't change anything).LOGTYPE-8");
			yield break;
		}
		if (ConfigFile.ServerConfig.GetBool("hide_from_public_list"))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to hide_from_public_list enabled in server configuration.LOGTYPE-8");
			yield break;
		}
		if (ConfigFile.ServerConfig.GetBool("hide_patreon_badges_by_default") || ConfigFile.ServerConfig.GetBool("block_gtag_patreon_badges") || ConfigFile.ServerConfig.GetBool("block_gtag_banteam_badges") || ConfigFile.ServerConfig.GetBool("block_gtag_management_badges"))
		{
			ServerConsole.AddLog("If your server is verified (put in the official server list) some badge settings enabled in your config will be ignored. If your server isn't on the public list - ignore this message.LOGTYPE-8");
		}
		string info = ConfigFile.ServerConfig.GetString("server_name", "Unnamed server") + ":[:BREAK:]:" + ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT") + ":[:BREAK:]:" + CompatibleVersions[0];
		WWWForm form = new WWWForm();
		form.AddField("update", 1);
		form.AddField("ip", Ip);
		form.AddField("info", info);
		form.AddField("port", base.networkPort);
		form.AddField("players", 0);
		form.AddField("privateBeta", isPrivateBeta.ToString());
		form.AddField("staffRA", ServerStatic.PermissionsHandler.StaffAccess.ToString());
		form.AddField("friendlyFire", ServerConsole.FriendlyFire.ToString());
		form.AddField("modded", Modded.ToString());
		form.AddField("whitelist", ServerConsole.WhiteListEnabled.ToString());
		form.AddField("startup", 1);
		string pth = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
		if (File.Exists(pth))
		{
			StreamReader streamReader = new StreamReader(pth);
			string text = streamReader.ReadToEnd();
			form.AddField("passcode", text);
			form.AddField("version", 2);
			ServerConsole.Password = text;
			streamReader.Close();
		}
		else
		{
			form.AddField("passcode", string.Empty);
		}
		using (WWW www2 = new WWW(CentralServer.MasterUrl + "authenticator.php", form))
		{
			yield return Timing.WaitUntilDone(www2);
			if (string.IsNullOrEmpty(www2.error))
			{
				if (www2.text.Contains("YES"))
				{
					if (www2.text.StartsWith("New code generated:"))
					{
						try
						{
							StreamWriter streamWriter = new StreamWriter(pth);
							string text2 = www2.text.Remove(0, www2.text.IndexOf(":")).Remove(www2.text.IndexOf(":"));
							while (text2.Contains(":"))
							{
								text2 = text2.Replace(":", string.Empty);
							}
							streamWriter.WriteLine(text2);
							streamWriter.Close();
							ServerConsole.AddLog("New password saved.LOGTYPE-8");
							UnityEngine.Object.FindObjectOfType<ServerConsole>().RefreshToken();
						}
						catch
						{
							ServerConsole.AddLog("New password could not be saved.LOGTYPE-8");
						}
					}
					ServerConsole.AddLog("The match is now on public list!LOGTYPE-8");
					ServerStatic.PermissionsHandler.SetServerAsVerified();
				}
				else
				{
					ServerConsole.AddLog("Your server won't be visible on the public server list - " + www2.text + " (" + Ip + ")LOGTYPE-8");
					if (string.IsNullOrEmpty(ConfigFile.ServerConfig.GetString("contact_email", string.Empty)))
					{
						ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC, please set up your email in configuration file (\"contact_email\" value) and restart the server. LOGTYPE-8");
					}
					else
					{
						ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC please email following information: LOGTYPE-8");
						ServerConsole.AddLog("- IP address of server (probably " + Ip + ") LOGTYPE-8");
						ServerConsole.AddLog("- is this static or dynamic IP address (most of home adresses are dynamic) LOGTYPE-8");
						ServerConsole.AddLog("PLEASE READ rules for verified servers first: https://scpslgame.com/Verified_server_rules.pdf LOGTYPE-8");
						ServerConsole.AddLog("send us that information to: server.verification@scpslgame.com LOGTYPE-8");
						ServerConsole.AddLog("email must be sent from email address set as \"contact_email\" in your config file (current value: " + ConfigFile.ServerConfig.GetString("contact_email", string.Empty) + "). LOGTYPE-8");
					}
				}
			}
			else
			{
				ServerConsole.AddLog("Could not create the match - " + www2.error + "LOGTYPE-8");
			}
		}
		UnityEngine.Object.FindObjectOfType<ServerConsole>().RunServer();
	}

	private void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = 20;
		int num = ConfigFile.HosterPolicy.GetInt("players_limit", -1);
		if (num > 0 && base.maxConnections > num)
		{
			base.maxConnections = num;
		}
		StartHostWithPort();
	}

	public void StartHostWithPort()
	{
		if (ConfigFile.ServerConfig.GetString("bind_ip", "ANY").ToUpper() == "ANY")
		{
			ServerConsole.AddLog("Server starting at all IP addresses and port " + base.networkPort);
			base.serverBindToIP = false;
			StartHost();
			return;
		}
		ServerConsole.AddLog("Server starting at IP " + ConfigFile.ServerConfig.GetString("bind_ip", "ANY") + " and  port " + base.networkPort);
		base.serverBindAddress = ConfigFile.ServerConfig.GetString("bind_ip", "ANY");
		base.serverBindToIP = true;
		StartHost();
	}

	public int GetFreePort()
	{
		ServerConsole.AddLog("Loading config...");
		ConfigFile.ServerConfig = ConfigFile.ReloadGameConfig(FileManager.GetAppFolder() + "config_gameplay.txt");
		string q = string.Empty;
		try
		{
			q = "Failed to split ports.";
			int[] array = ConfigFile.ServerConfig.GetIntList("port_queue").ToArray();
			if (array.Length == 0)
			{
				array = new int[8] { 7777, 7778, 7779, 7780, 7781, 7782, 7783, 7784 };
			}
			string text = string.Join(", ", new List<int>(array).ConvertAll((int i) => i.ToString()).ToArray());
			if (array.Length == 0)
			{
				q = "Failed to detect ports.";
				throw new Exception();
			}
			ServerConsole.AddLog("Port queue loaded: " + text);
			int[] array2 = array;
			foreach (int num2 in array2)
			{
				ServerConsole.AddLog("Trying to init port: " + num2 + "...");
				if (NetworkServer.Listen(num2))
				{
					NetworkServer.Reset();
					ServerConsole.AddLog("Done!LOGTYPE-10");
					return num2;
				}
				ServerConsole.AddLog("...failed.LOGTYPE-6");
			}
		}
		catch
		{
			ServerConsole.AddLog(q);
		}
		return 7777;
	}

	private void UpnpStart()
	{
		if (_mappedDevices == null)
		{
			ServerConsole.AddLog("Automatic port forwarding using UPnP enabled!LOGTYPE-9");
			_mappedDevices = new List<INatDevice>();
		}
		NatUtility.DeviceFound += DeviceFound;
		NatUtility.DeviceLost += DeviceLost;
		NatUtility.StartDiscovery();
	}

	private void UpnpStop()
	{
		NatUtility.StopDiscovery();
		foreach (INatDevice mappedDevice in _mappedDevices)
		{
			try
			{
				mappedDevice.DeletePortMap(new Mapping(Protocol.Udp, base.networkPort, base.networkPort));
				if (_mappedDevices.Contains(mappedDevice))
				{
					_mappedDevices.Remove(mappedDevice);
				}
				ServerConsole.AddLog(string.Concat("Removed forwarding rule on port ", base.networkPort, " from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-10"));
			}
			catch
			{
				ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on port ", base.networkPort, " UDP from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-12"));
			}
			if (_queryEnabled)
			{
				try
				{
					mappedDevice.DeletePortMap(new Mapping(Protocol.Tcp, _queryPort, _queryPort));
					ServerConsole.AddLog(string.Concat("Removed forwarding rule on query port ", _queryPort, " from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-10"));
				}
				catch
				{
					ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on query port ", _queryPort, " UDP from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-12"));
				}
			}
		}
	}

	private void DeviceFound(object sender, DeviceEventArgs args)
	{
		INatDevice device = args.Device;
		try
		{
			device = args.Device;
			_mappedDevices.Add(device);
			device.CreatePortMap(new Mapping(Protocol.Udp, base.networkPort, base.networkPort));
			ServerConsole.AddLog(string.Concat("Forwarded port ", base.networkPort, " UDP (game port) from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog(string.Concat("Can't forward port ", base.networkPort, " UDP from ", device.GetExternalIP(), " to this device. Error: ", ex.Message, "LOGTYPE-12"));
		}
		if (!_queryEnabled)
		{
			return;
		}
		try
		{
			if (_queryEnabled)
			{
				device.CreatePortMap(new Mapping(Protocol.Tcp, _queryPort, _queryPort));
				ServerConsole.AddLog(string.Concat("Forwarded port ", _queryPort, " TCP (query port) from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
			}
		}
		catch (Exception ex2)
		{
			ServerConsole.AddLog(string.Concat("Can't forward query port ", _queryPort, " TCP from ", device.GetExternalIP(), " to this device. Error: ", ex2.Message, "LOGTYPE-12"));
		}
	}

	private void DeviceLost(object sender, DeviceEventArgs args)
	{
		INatDevice device = args.Device;
		try
		{
			device.DeletePortMap(new Mapping(Protocol.Udp, base.networkPort, base.networkPort));
			if (_mappedDevices.Contains(device))
			{
				_mappedDevices.Remove(device);
			}
			ServerConsole.AddLog(string.Concat("Removed forwarding rule on port ", base.networkPort, " from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
		}
		catch
		{
			ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on port ", base.networkPort, " UDP from ", device.GetExternalIP(), " to this device.LOGTYPE-12"));
		}
		if (!_queryEnabled)
		{
			return;
		}
		try
		{
			device.DeletePortMap(new Mapping(Protocol.Tcp, _queryPort, _queryPort));
			ServerConsole.AddLog(string.Concat("Removed forwarding rule on query port ", _queryPort, " from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
		}
		catch
		{
			ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on query port ", _queryPort, " UDP from ", device.GetExternalIP(), " to this device.LOGTYPE-12"));
		}
	}
}
