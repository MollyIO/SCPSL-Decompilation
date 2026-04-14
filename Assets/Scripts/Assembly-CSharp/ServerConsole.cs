using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Cryptography;
using GameConsole;
using MEC;
using Org.BouncyCastle.Crypto;
using UnityEngine;
using UnityEngine.Networking;

public class ServerConsole : MonoBehaviour, IDisposable
{
	public static ServerConsole singleton;

	public static int LogId;

	public static int Cycle;

	public static int Port;

	private static bool _disposing;

	public static Process ConsoleId;

	public static string Session;

	public static string Password;

	public static string Ip;

	public static AsymmetricKeyParameter Publickey;

	private static bool _accepted = true;

	public static bool Update;

	public static bool FriendlyFire = false;

	public static bool WhiteListEnabled = false;

	private static readonly Queue<string> PrompterQueue = new Queue<string>();

	private bool _errorSent;

	public Thread CheckProcessThread;

	public Thread QueueThread;

	public void Dispose()
	{
		_disposing = true;
		if (Directory.Exists("SCPSL_Data/Dedicated/" + Session))
		{
			Directory.Delete("SCPSL_Data/Dedicated/" + Session, true);
		}
	}

	private void Start()
	{
		if (!ServerStatic.IsDedicated)
		{
			return;
		}
		LogId = 0;
		_accepted = true;
		if (string.IsNullOrEmpty(Session))
		{
			Session = "default";
		}
		if (Directory.Exists("SCPSL_Data/Dedicated/" + Session) && Environment.GetCommandLineArgs().Contains("-nodedicateddelete"))
		{
			string[] files = Directory.GetFiles("SCPSL_Data/Dedicated/" + Session);
			foreach (string path in files)
			{
				File.Delete(path);
			}
		}
		else if (Directory.Exists("SCPSL_Data/Dedicated/" + Session))
		{
			Directory.Delete("SCPSL_Data/Dedicated/" + Session, true);
		}
		Directory.CreateDirectory("SCPSL_Data/Dedicated/" + Session);
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
		fileSystemWatcher.Path = "SCPSL_Data/Dedicated/" + Session;
		fileSystemWatcher.NotifyFilter = NotifyFilters.FileName;
		FileSystemWatcher fileSystemWatcher2 = fileSystemWatcher;
		fileSystemWatcher2.Created += delegate(object sender, FileSystemEventArgs args)
		{
			if (args.Name.Contains("cs") && args.Name.Contains("mapi"))
			{
				new Thread((ThreadStart)delegate
				{
					ReadLog(args.FullPath);
				}).Start();
			}
		};
		fileSystemWatcher2.EnableRaisingEvents = true;
		QueueThread = new Thread(Prompt)
		{
			Priority = System.Threading.ThreadPriority.Lowest,
			IsBackground = true,
			Name = "Dedicated server console output"
		};
		QueueThread.Start();
		if (ServerStatic.ProcessIdPassed)
		{
			CheckProcessThread = new Thread(CheckProcess)
			{
				Priority = System.Threading.ThreadPriority.Lowest,
				IsBackground = true,
				Name = "Dedicated server console running check"
			};
			CheckProcessThread.Start();
		}
	}

	private void Awake()
	{
		singleton = this;
	}

	private static void ReadLog(string path)
	{
		try
		{
			if (!File.Exists(path))
			{
				return;
			}
			string text = path.Remove(0, path.IndexOf("cs", StringComparison.Ordinal));
			string text2 = string.Empty;
			string text3 = string.Empty;
			try
			{
				text3 = "Error while reading the file: " + text;
				string text4;
				using (StreamReader streamReader = new StreamReader("SCPSL_Data/Dedicated/" + Session + "/" + text))
				{
					text4 = streamReader.ReadToEnd();
					text3 = "Error while dedecting 'terminator end-of-message' signal.";
					if (text4.Contains("terminator"))
					{
						text4 = text4.Remove(text4.LastIndexOf("terminator", StringComparison.Ordinal));
					}
					text3 = "Error while sending message.";
					text2 = EnterCommand(text4);
					try
					{
						text3 = "Error while closing the file: " + text + " :: " + text4;
					}
					catch
					{
						text3 = "Error while closing the file.";
					}
				}
				try
				{
					text3 = "Error while deleting the file: " + text + " :: " + text4;
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					text3 = "Error while deleting the file.";
				}
				File.Delete("SCPSL_Data/Dedicated/" + Session + "/" + text);
			}
			catch
			{
				UnityEngine.Debug.LogError("Error in server console: " + text3);
			}
			if (!string.IsNullOrEmpty(text2))
			{
				AddLog(text2);
			}
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
	}

	private void CheckProcess()
	{
		while (!_disposing)
		{
			Thread.Sleep(4000);
			if (ConsoleId == null || ConsoleId.HasExited)
			{
				DisposeStatic();
				TerminateProcess();
			}
		}
	}

	private void Prompt()
	{
		while (!_disposing)
		{
			if (PrompterQueue.Count == 0 || !_accepted)
			{
				Thread.Sleep(25);
				continue;
			}
			string text = PrompterQueue.Dequeue();
			if (!_errorSent || !text.Contains("Could not update the session - Server is not verified."))
			{
				_errorSent = true;
				StreamWriter streamWriter = new StreamWriter("SCPSL_Data/Dedicated/" + Session + "/sl" + LogId + ".mapi");
				LogId++;
				streamWriter.WriteLine(text);
				streamWriter.Close();
			}
		}
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void OnApplicationQuit()
	{
		Dispose();
	}

	public static void DisposeStatic()
	{
		singleton.Dispose();
	}

	public static void AddLog(string q)
	{
		if (ServerStatic.IsDedicated)
		{
			PrompterQueue.Enqueue(q);
		}
		else
		{
			GameConsole.Console.singleton.AddLog(q, Color.grey);
		}
	}

	public static string GetClientInfo(NetworkConnection conn)
	{
		GameObject gameObject = GameConsole.Console.FindConnectedRoot(conn);
		return gameObject.GetComponent<NicknameSync>().myNick + " ( " + gameObject.GetComponent<CharacterClassManager>().SteamId + " | " + conn.address + " )";
	}

	public static string GetClientInfo(GameObject gameObject)
	{
		return gameObject.GetComponent<NicknameSync>().myNick + " ( " + gameObject.GetComponent<CharacterClassManager>().SteamId + " | " + gameObject.GetComponent<NetworkBehaviour>().connectionToClient.address + " )";
	}

	public static void Disconnect(GameObject player, string message)
	{
		if (player == null)
		{
			return;
		}
		NetworkBehaviour component = player.GetComponent<NetworkBehaviour>();
		if (!(component == null) && component.connectionToClient.isConnected)
		{
			CharacterClassManager component2 = player.GetComponent<CharacterClassManager>();
			if (component2 == null)
			{
				component.connectionToClient.Disconnect();
				component.connectionToClient.Dispose();
			}
			else
			{
				component2.DisconnectClient(component.connectionToClient, message);
			}
		}
	}

	public static void Disconnect(NetworkConnection conn, string message)
	{
		GameObject gameObject = GameConsole.Console.FindConnectedRoot(conn);
		if (gameObject == null)
		{
			conn.Disconnect();
			conn.Dispose();
		}
		else
		{
			Disconnect(gameObject, message);
		}
	}

	private static void ColorText(string text)
	{
		UnityEngine.Debug.Log(string.Format("<color={0}>{1}</color>", GetColor(text), text), null);
	}

	private static string GetColor(string text)
	{
		int num = 9;
		if (text.Contains("LOGTYPE"))
		{
			try
			{
				string text2 = text.Remove(0, text.IndexOf("LOGTYPE", StringComparison.Ordinal) + 7);
				num = int.Parse((!text2.Contains("-")) ? text2 : text2.Remove(0, 1));
			}
			catch
			{
				num = 9;
			}
		}
		string empty = string.Empty;
		switch (num)
		{
		case 0:
			return "#000";
		case 1:
			return "#183487";
		case 2:
			return "#0b7011";
		case 3:
			return "#0a706c";
		case 4:
			return "#700a0a";
		case 5:
			return "#5b0a40";
		case 6:
			return "#aaa800";
		case 7:
			return "#afafaf";
		case 8:
			return "#5b5b5b";
		case 9:
			return "#0055ff";
		case 10:
			return "#10ce1a";
		case 11:
			return "#0fc7ce";
		case 12:
			return "#ce0e0e";
		case 13:
			return "#c70dce";
		case 14:
			return "#ffff07";
		case 15:
			return "#e0e0e0";
		default:
			return "#fff";
		}
	}

	internal static string EnterCommand(string cmds)
	{
		string result = "Command accepted.";
		string[] array = cmds.ToUpper().Split(' ');
		if (array.Length <= 0)
		{
			return result;
		}
		switch (array[0])
		{
		case "FORCESTART":
		{
			bool flag = false;
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				if (component != null && component.isLocalPlayer && component.isServer && !component.roundStarted)
				{
					component.ForceRoundStart();
					flag = true;
				}
			}
			result = ((!flag) ? "Failed to force start.LOGTYPE14" : "Forced round start.");
			break;
		}
		case "CONFIG":
			if (File.Exists(ConfigFile.ConfigPath))
			{
				Application.OpenURL(ConfigFile.ConfigPath);
			}
			else
			{
				result = "Config file not found!";
			}
			break;
		default:
			result = GameConsole.Console.singleton.TypeCommand(cmds);
			break;
		}
		return result;
	}

	public void RunServer()
	{
		Timing.RunCoroutine(_RefreshSession(), Segment.Update);
	}

	public void RunRefreshPublicKey()
	{
		Timing.RunCoroutine(_RefreshPublicKey(), Segment.Update);
	}

	public void RunRefreshCentralServers()
	{
		Timing.RunCoroutine(_RefreshCentralServers(), Segment.Update);
	}

	private IEnumerator<float> _RefreshCentralServers()
	{
		while (this != null)
		{
			yield return Timing.WaitForSeconds(900f);
			new Thread((ThreadStart)delegate
			{
				CentralServer.RefreshServerList(true);
			}).Start();
		}
	}

	private IEnumerator<float> _RefreshPublicKey()
	{
		string cache = CentralServerKeyCache.ReadCache();
		string cacheHash = string.Empty;
		string lastHash = string.Empty;
		if (!string.IsNullOrEmpty(cache))
		{
			Publickey = ECDSA.PublicKeyFromString(cache);
			cacheHash = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Publickey)));
			AddLog("Loaded central server public key from cache.\nSHA256 of public key: " + cacheHash);
		}
		AddLog("Downloading public key from central server...");
		while (this != null)
		{
			using (WWW www = new WWW(form: new WWWForm(), url: CentralServer.StandardUrl + "publickey.php"))
			{
				yield return Timing.WaitUntilDone(www);
				try
				{
					bool flag = false;
					if (!string.IsNullOrEmpty(www.error))
					{
						AddLog("Can't refresh central server public key - " + www.error);
						flag = true;
					}
					if (!flag)
					{
						Publickey = ECDSA.PublicKeyFromString(www.text);
						string text = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Publickey)));
						if (text != lastHash)
						{
							lastHash = text;
							AddLog("Downloaded public key from central server.\nSHA256 of public key: " + text);
							if (text != cacheHash)
							{
								CentralServerKeyCache.SaveCache(www.text);
							}
							else
							{
								AddLog("SHA256 of cached key matches, no need to update cache.");
							}
						}
						else
						{
							AddLog("Refreshed public key of central server - key hash not changed.");
						}
					}
				}
				catch (Exception ex)
				{
					AddLog("Can't refresh central server public key - " + ex.Message);
				}
			}
			yield return Timing.WaitForSeconds(360f);
		}
	}

	private IEnumerator<float> _RefreshPublicKeyOnce()
	{
		using (WWW www = new WWW(form: new WWWForm(), url: CentralServer.StandardUrl + "publickey.php"))
		{
			yield return Timing.WaitUntilDone(www);
			try
			{
				if (!string.IsNullOrEmpty(www.error))
				{
					AddLog("Can't refresh central server public key - " + www.error);
					yield break;
				}
				Publickey = ECDSA.PublicKeyFromString(www.text);
				string text = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Publickey)));
				AddLog("Downloaded public key from central server.\nSHA256 of public key: " + text);
				CentralServerKeyCache.SaveCache(www.text);
			}
			catch (Exception ex)
			{
				AddLog("Can't refresh central server public key - " + ex.Message);
			}
		}
	}

	private IEnumerator<float> _RefreshSession()
	{
		CustomNetworkManager cnm = GetComponent<CustomNetworkManager>();
		string masterServer = CentralServer.MasterUrl + "authenticator.php";
		while (this != null)
		{
			float timeBefore = Time.realtimeSinceStartup;
			Cycle++;
			if (string.IsNullOrEmpty(Password) && Cycle < 15)
			{
				if (Cycle == 5 || Cycle == 12)
				{
					RefreshToken();
				}
			}
			else
			{
				WWWForm form = new WWWForm();
				form.AddField("ip", Ip);
				if (!string.IsNullOrEmpty(Password))
				{
					form.AddField("passcode", Password);
				}
				int plys = 0;
				try
				{
					plys = GameObject.FindGameObjectsWithTag("Player").Length - 1;
				}
				catch
				{
				}
				form.AddField("players", plys + "/" + cnm.ReservedMaxPlayers);
				form.AddField("port", cnm.networkPort);
				form.AddField("version", 2);
				if (Update || Cycle == 10)
				{
					Update = false;
					string value = ConfigFile.ServerConfig.GetString("server_name", "Unnamed server") + ":[:BREAK:]:" + ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT") + ":[:BREAK:]:" + CustomNetworkManager.CompatibleVersions[0];
					form.AddField("update", 1);
					form.AddField("info", value);
					form.AddField("privateBeta", CustomNetworkManager.isPrivateBeta.ToString());
					form.AddField("staffRA", ServerStatic.PermissionsHandler.StaffAccess.ToString());
					form.AddField("friendlyFire", FriendlyFire.ToString());
					form.AddField("modded", CustomNetworkManager.Modded.ToString());
					form.AddField("whitelist", WhiteListEnabled.ToString());
				}
				using (WWW www = new WWW(masterServer, form))
				{
					yield return Timing.WaitUntilDone(www);
					if (!string.IsNullOrEmpty(www.error) || www.text != "YES")
					{
						if (!string.IsNullOrEmpty(www.error))
						{
							AddLog("Could not update data on server list - " + www.error + www.text + "LOGTYPE-8");
						}
						else
						{
							if (www.text.StartsWith("New code generated:"))
							{
								ServerStatic.PermissionsHandler.SetServerAsVerified();
								string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
								try
								{
									File.Delete(path);
								}
								catch
								{
									AddLog("New password could not be saved.LOGTYPE-8");
								}
								try
								{
									StreamWriter streamWriter = new StreamWriter(path);
									string text = www.text.Remove(0, www.text.IndexOf(":", StringComparison.Ordinal)).Remove(www.text.IndexOf(":", StringComparison.Ordinal));
									while (text.Contains(":"))
									{
										text = text.Replace(":", string.Empty);
									}
									streamWriter.WriteLine(text);
									streamWriter.Close();
									AddLog("New password saved.LOGTYPE-8");
									Update = true;
								}
								catch
								{
									AddLog("New password could not be saved.LOGTYPE-8");
								}
							}
							else if (www.text.Contains(":Restart:"))
							{
								AddLog("Server restart requested by central server.LOGTYPE-8");
								Application.Quit();
							}
							else if (www.text.Contains(":RoundRestart:"))
							{
								AddLog("Round restart requested by central server.LOGTYPE-8");
								GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
								foreach (GameObject gameObject in array)
								{
									PlayerStats component = gameObject.GetComponent<PlayerStats>();
									if (component.isLocalPlayer && component.isServer)
									{
										component.Roundrestart();
									}
								}
							}
							else if (www.text.Contains(":UpdateData:"))
							{
								Update = true;
							}
							else if (www.text.Contains(":RefreshKey:"))
							{
								AddLog("Public key refresh requested by central server.LOGTYPE-8");
								Timing.RunCoroutine(_RefreshPublicKeyOnce(), Segment.Update);
							}
							else if (www.text.Contains(":Message - "))
							{
								string text2 = www.text.Substring(www.text.IndexOf(":Message - ", StringComparison.Ordinal) + 11);
								text2 = text2.Substring(0, text2.IndexOf(":::", StringComparison.Ordinal));
								AddLog("[MESSAGE FROM CENTRAL SERVER] " + text2 + " LOGTYPE-3");
							}
							else if (!www.text.Contains("Server is not verified"))
							{
								AddLog("Could not update data on server list - " + www.error + www.text + "LOGTYPE-8");
							}
							RefreshToken();
						}
					}
				}
			}
			if (Cycle >= 15)
			{
				Cycle = 0;
			}
			yield return Timing.WaitForSeconds(5f - (Time.realtimeSinceStartup - timeBefore));
		}
	}

	public void RefreshToken()
	{
		string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
		if (File.Exists(path))
		{
			StreamReader streamReader = new StreamReader(path);
			string text = streamReader.ReadToEnd();
			if (string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(text))
			{
				AddLog("Verification token loaded! Server probably will be listed on public list.");
			}
			if (Password != text)
			{
				AddLog("Verification token reloaded.");
				Update = true;
			}
			Password = text;
			ServerStatic.PermissionsHandler.SetServerAsVerified();
			streamReader.Close();
		}
	}

	private static void TerminateProcess()
	{
		ServerStatic.IsDedicated = false;
		Application.Quit();
	}
}
