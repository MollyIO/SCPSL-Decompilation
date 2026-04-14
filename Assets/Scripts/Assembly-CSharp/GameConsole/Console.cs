using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cryptography;
using MEC;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

namespace GameConsole
{
	public class Console : MonoBehaviour
	{
		[Serializable]
		public class CommandHint
		{
			public string name;

			public string shortDesc;

			[Multiline]
			public string fullDesc;
		}

		[Serializable]
		public class Value
		{
			public string key;

			public string value;

			public Value(string k, string v)
			{
				key = k;
				value = v;
			}
		}

		[Serializable]
		public class Log
		{
			public string text;

			public Color32 color;

			public bool nospace;

			public Log(string t, Color32 c, bool b)
			{
				text = t;
				color = c;
				nospace = b;
			}
		}

		public static AsymmetricKeyParameter Publickey;

		public CommandHint[] hints;

		public static Console singleton;

		public Text txt;

		public InputField cmdField;

		public GameObject console;

		public static bool DisableSLML;

		public static bool DisableRemoteSLML;

		public static bool RequestDNT;

		public static bool HideBadge;

		public static string[] StartupArgs;

		internal static AsymmetricCipherKeyPair SessionKeys;

		internal static byte[] EcdheSignature;

		private readonly List<Log> _logs = new List<Log>();

		private readonly List<Value> _values = new List<Value>();

		private int scrollup;

		private int previous_scrlup;

		private string loadedLevel;

		private string _content;

		private string _response = string.Empty;

		private bool alwaysRefreshing;

		private bool _change;

		[CompilerGenerated]
		private static Dictionary<string, int> _003C_003Ef__switch_0024map1;

		private void Start()
		{
			AddLog("Hi there! Initializing console...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			AddLog("Done! Type 'help' to print the list of available commands.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			Timing.RunCoroutine(_RefreshPublicKey(), Segment.FixedUpdate);
			Timing.RunCoroutine(_RefreshCentralServers(), Segment.FixedUpdate);
			AddLog("Generatig session keys...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			SessionKeys = ECDSA.GenerateKeys();
			AddLog("Session keys generated (ECDSA)!", Color.green);
			StartupArgs = Environment.GetCommandLineArgs();
			if (StartupArgs.Any((string arg) => arg.ToLower() == "-noslml"))
			{
				DisableSLML = true;
				DisableRemoteSLML = true;
			}
			if (StartupArgs.Any((string arg) => arg.ToLower() == "-norslml" || arg.ToLower() == "-noremote"))
			{
				DisableRemoteSLML = true;
			}
			if (DisableSLML)
			{
				AddLog("SLML disabled by the startup argument.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			else if (DisableRemoteSLML)
			{
				AddLog("Remote SLML disabled by the startup argument.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			if (StartupArgs.Any((string arg) => arg.ToLower() == "-dnt"))
			{
				RequestDNT = true;
				AddLog("\"Do not track\" request will be sent to all servers you are joining - enabled by startup argument.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			if (StartupArgs.Any((string arg) => arg.ToLower() == "-hidetag"))
			{
				HideBadge = true;
				AddLog("Your global badge will be automatically hidden.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
		}

		private void Update()
		{
			if (_change)
			{
				txt.text = _content;
				_change = false;
			}
		}

		private void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				ProceedButton();
			}
			else if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				ToggleConsole();
			}
			else if (Input.GetKey(KeyCode.Escape) && console.activeSelf)
			{
				ToggleConsole();
			}
			scrollup += Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel") * 10f);
			scrollup = ((_logs.Count > 0) ? Mathf.Clamp(scrollup, 0, _logs.Count - 1) : 0);
			if (previous_scrlup != scrollup)
			{
				previous_scrlup = scrollup;
				RefreshConsoleScreen();
			}
			Scene activeScene = SceneManager.GetActiveScene();
			if (activeScene.name != loadedLevel)
			{
				loadedLevel = activeScene.name;
				AddLog("Scene Manager: Loaded scene '" + activeScene.name + "' [" + activeScene.path + "]", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				RefreshConsoleScreen();
			}
			if (alwaysRefreshing)
			{
				RefreshConsoleScreen();
			}
		}

		public List<Log> GetAllLogs()
		{
			return _logs;
		}

		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			if (singleton == null)
			{
				singleton = this;
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
			}
		}

		private void RefreshConsoleScreen()
		{
			if (txt == null)
			{
				return;
			}
			while (true)
			{
				_content = string.Empty;
				if (_logs.Count == 0)
				{
					break;
				}
				for (int i = 0; i < _logs.Count - scrollup; i++)
				{
					string text = ((!_logs[i].nospace) ? "\n\n" : "\n") + "<color=" + ColorToHex(_logs[i].color) + ">" + _logs[i].text + "</color>";
					if (text.Contains("@#{["))
					{
						string text2 = text.Remove(text.IndexOf("@#{[", StringComparison.Ordinal));
						string text3 = text.Remove(0, text.IndexOf("@#{[", StringComparison.Ordinal) + 4);
						text3 = text3.Remove(text3.Length - 12);
						foreach (Value value in _values)
						{
							if (value.key == text3)
							{
								text = text2 + value.value + "</color>";
							}
						}
					}
					_content += text;
				}
				if (_content.Length > 15000)
				{
					_logs.RemoveAt(0);
					continue;
				}
				break;
			}
			_change = true;
		}

		public void AddLog(string text, Color32 c, bool nospace = false)
		{
			if (ServerStatic.IsDedicated)
			{
				ServerConsole.AddLog(text);
				return;
			}
			_response = _response + text + Environment.NewLine;
			if (!nospace)
			{
				_response += Environment.NewLine;
			}
			DateTimeOffset now = DateTimeOffset.Now;
			text = "<color=#808080><size=18>[" + TimeBehaviour.FormatTime("HH:mm:ss", now) + "<size=16>." + TimeBehaviour.FormatTime("fff", now) + "</size>]</size></color> " + text;
			scrollup = 0;
			_logs.Add(new Log(text, c, nospace));
			RefreshConsoleScreen();
		}

		private string ColorToHex(Color32 color)
		{
			string text = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return "#" + text;
		}

		public static GameObject FindConnectedRoot(NetworkConnection conn)
		{
			try
			{
				foreach (PlayerController playerController in conn.playerControllers)
				{
					if (playerController.gameObject.tag == "Player")
					{
						return playerController.gameObject;
					}
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		public string TypeCommand(string cmd)
		{
			if (cmd.StartsWith(".") && cmd.Length > 1)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array2 = array;
				foreach (GameObject gameObject in array2)
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Sending command to server: " + cmd.Substring(1), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						gameObject.GetComponent<GameConsoleTransmission>().SendToServer(cmd.Substring(1));
					}
				}
				return string.Empty;
			}
			if (cmd.StartsWith("/") && cmd.Length > 1)
			{
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array4 = array3;
				foreach (GameObject gameObject2 in array4)
				{
					if (gameObject2.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Sending remote admin request to server: " + cmd.Substring(1), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						gameObject2.GetComponent<QueryProcessor>().ProcessQuery(cmd.Substring(1));
						return string.Empty;
					}
				}
				return string.Empty;
			}
			if (cmd.ToUpper().StartsWith("CMDBIND"))
			{
				string[] array5 = cmd.Split(' ');
				if (array5.Length < 3)
				{
					if (array5.Length <= 1)
					{
						string text = "Command Binding List\n";
						foreach (CmdBinding.Bind binding in CmdBinding.bindings)
						{
							string text2 = text;
							text = string.Concat(text2, binding.key, "(", (int)binding.key, "):", binding.command, "\n");
						}
						AddLog(text, Color.green);
					}
					else
					{
						AddLog("Syntax: \"cmdbind <key/keycode> <command>\"", Color.red);
					}
					return _response;
				}
				array5[1] = array5[1].ToUpper();
				int result = -1;
				int.TryParse(array5[1], out result);
				if (result == 0 && Enum.IsDefined(typeof(KeyCode), array5[1]))
				{
					result = (int)Enum.Parse(typeof(KeyCode), array5[1], true);
				}
				if (result <= 0 || result > 509)
				{
					AddLog("Invalid key code: " + result, Color.red);
					return _response;
				}
				string text3 = array5[2];
				if (array5.Length > 3)
				{
					for (int k = 3; k < array5.Length; k++)
					{
						text3 = text3 + " " + array5[k];
					}
				}
				CmdBinding.KeyBind((KeyCode)result, text3);
				CmdBinding.Save();
				AddLog(string.Concat("Command [", text3, "] has been bound to [", (KeyCode)result, "]!"), Color.green);
				return _response;
			}
			_response = string.Empty;
			string[] array6 = cmd.Split(' ');
			cmd = array6[0].ToUpper();
			if (cmd != null)
			{
				if (_003C_003Ef__switch_0024map1 == null)
				{
					Dictionary<string, int> dictionary = new Dictionary<string, int>(62);
					dictionary.Add("HELLO", 0);
					dictionary.Add("LENNY", 1);
					dictionary.Add("CONTACT", 2);
					dictionary.Add("SRVCFG", 3);
					dictionary.Add("GROUPS", 4);
					dictionary.Add("ADMINME", 5);
					dictionary.Add("OVERRIDE", 5);
					dictionary.Add("HIDETAG", 6);
					dictionary.Add("HTAG", 6);
					dictionary.Add("HT", 6);
					dictionary.Add("ID", 7);
					dictionary.Add("MYID", 7);
					dictionary.Add("SHOWTAG", 8);
					dictionary.Add("TAG", 8);
					dictionary.Add("STAG", 8);
					dictionary.Add("ST", 8);
					dictionary.Add("BCCLEAR", 9);
					dictionary.Add("CLEARBC", 9);
					dictionary.Add("ALERTCLEAR", 9);
					dictionary.Add("CLEARALERT", 9);
					dictionary.Add("GLOBALTAG", 10);
					dictionary.Add("GTAG", 10);
					dictionary.Add("GTG", 10);
					dictionary.Add("GT", 10);
					dictionary.Add("GLOBALBAN", 11);
					dictionary.Add("GBAN", 11);
					dictionary.Add("SUPERBAN", 11);
					dictionary.Add("CONFIRM", 12);
					dictionary.Add("KEY", 13);
					dictionary.Add("OVERWATCH", 14);
					dictionary.Add("OVR", 14);
					dictionary.Add("OW", 14);
					dictionary.Add("GIVE", 15);
					dictionary.Add("RANGE", 16);
					dictionary.Add("MOUSESENS", 17);
					dictionary.Add("ROUNDRESTART", 18);
					dictionary.Add("ITEMLIST", 19);
					dictionary.Add("BAN", 20);
					dictionary.Add("CLS", 21);
					dictionary.Add("CLEAR", 21);
					dictionary.Add("QUIT", 22);
					dictionary.Add("EXIT", 22);
					dictionary.Add("REPORT", 23);
					dictionary.Add("HELP", 24);
					dictionary.Add("REFRESHFIX", 25);
					dictionary.Add("COLOR", 26);
					dictionary.Add("COLORS", 26);
					dictionary.Add("VALUE", 27);
					dictionary.Add("SEED", 28);
					dictionary.Add("SLML", 29);
					dictionary.Add("CENTRAL", 30);
					dictionary.Add("CS", 30);
					dictionary.Add("CSRV", 30);
					dictionary.Add("CONNECT", 31);
					dictionary.Add("DISCONNECT", 32);
					dictionary.Add("DC", 32);
					dictionary.Add("SHOWRIDS", 33);
					dictionary.Add("CLASSLIST", 34);
					dictionary.Add("WARHEAD", 35);
					dictionary.Add("CONFIG", 36);
					dictionary.Add("KEYBIND", 37);
					dictionary.Add("SYNCCMD", 38);
					_003C_003Ef__switch_0024map1 = dictionary;
				}
				int value;
				if (_003C_003Ef__switch_0024map1.TryGetValue(cmd, out value))
				{
					switch (value)
					{
					case 0:
						break;
					case 1:
						goto IL_07ec;
					case 2:
						goto IL_0816;
					case 3:
						goto IL_0883;
					case 4:
						goto IL_08f0;
					case 5:
						goto IL_095d;
					case 6:
						goto IL_09e8;
					case 7:
						goto IL_0a55;
					case 8:
						goto IL_0acc;
					case 9:
						goto IL_0b3a;
					case 10:
						goto IL_0bab;
					case 11:
						goto IL_0c19;
					case 12:
						goto IL_0d3f;
					case 13:
						goto IL_0d96;
					case 14:
						goto IL_0e39;
					case 15:
						goto IL_0f98;
					case 16:
						goto IL_11c6;
					case 17:
						goto IL_1245;
					case 18:
						goto IL_1314;
					case 19:
						goto IL_13bd;
					case 20:
						goto IL_15d6;
					case 21:
						goto IL_1849;
					case 22:
						goto IL_185f;
					case 23:
						goto IL_18b0;
					case 24:
						goto IL_19bc;
					case 25:
						goto IL_1b45;
					case 26:
						goto IL_1b95;
					case 27:
						goto IL_1da9;
					case 28:
						goto IL_1eb9;
					case 29:
						goto IL_1f1f;
					case 30:
						goto IL_1f8a;
					case 31:
						goto IL_227e;
					case 32:
						goto IL_22c3;
					case 33:
						goto IL_2301;
					case 34:
						goto IL_23d9;
					case 35:
						goto IL_25f2;
					case 36:
						goto IL_288f;
					case 37:
						goto IL_2a43;
					case 38:
						goto IL_2beb;
					default:
						goto IL_2c87;
					}
					AddLog("Hello World!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					goto IL_2cb8;
				}
			}
			goto IL_2c87;
			IL_19bc:
			if (array6.Length > 1)
			{
				string text4 = array6[1].ToUpper();
				CommandHint[] array7 = hints;
				foreach (CommandHint commandHint in array7)
				{
					if (!(commandHint.name != text4))
					{
						AddLog(commandHint.name + " - " + commandHint.fullDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
						RefreshConsoleScreen();
						return _response;
					}
				}
				AddLog("Help for command '" + text4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				RefreshConsoleScreen();
				return _response;
			}
			AddLog("List of available commands:\n", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			CommandHint[] array8 = hints;
			foreach (CommandHint commandHint2 in array8)
			{
				AddLog(commandHint2.name + " - " + commandHint2.shortDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
			}
			AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			RefreshConsoleScreen();
			goto IL_2cb8;
			IL_18b0:
			if (SceneManager.GetActiveScene().name.Contains("Facility"))
			{
				if (array6.Length < 2)
				{
					AddLog("Syntax: \"report <playerid> <reason>\"", Color.red);
					return _response;
				}
				if (array6.Length >= 3)
				{
					string text5 = array6[2];
					if (array6.Length > 3)
					{
						for (int n = 3; n < array6.Length; n++)
						{
							text5 = text5 + " " + array6[n];
						}
					}
					array6[2] = text5;
				}
				int result2 = -1;
				if (!int.TryParse(array6[1], out result2))
				{
					return _response;
				}
				GameObject[] array9 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array10 = array9;
				foreach (GameObject gameObject3 in array10)
				{
					if (gameObject3.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						gameObject3.GetComponent<CheaterReport>().Report(result2, array6[2]);
						break;
					}
				}
			}
			goto IL_2cb8;
			IL_15d6:
			if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				return _response;
			}
			if (array6.Length < 3)
			{
				AddLog("Syntax: BAN [player kick / ip] [minutes]", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				foreach (NetworkConnection connection in NetworkServer.connections)
				{
					string text6 = string.Empty;
					GameObject gameObject4 = FindConnectedRoot(connection);
					if (gameObject4 != null)
					{
						text6 = gameObject4.GetComponent<NicknameSync>().myNick;
					}
					if (text6 == string.Empty)
					{
						AddLog("Player :: " + connection.address, new Color32(160, 128, 128, byte.MaxValue), true);
					}
					else
					{
						AddLog("Player :: " + text6 + " :: " + connection.address, new Color32(128, 160, 128, byte.MaxValue), true);
					}
				}
			}
			else
			{
				int result3 = 0;
				if (int.TryParse(array6[2], out result3))
				{
					bool flag = false;
					foreach (NetworkConnection connection2 in NetworkServer.connections)
					{
						GameObject gameObject5 = FindConnectedRoot(connection2);
						if (connection2.address.ToUpper().Contains(array6[1].ToUpper()) || (!(gameObject5 == null) && gameObject5.GetComponent<NicknameSync>().myNick.ToUpper().Contains(array6[1].ToUpper())))
						{
							flag = true;
							PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(gameObject5, result3, string.Empty, "Administrator");
							AddLog("Player banned.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						}
					}
					if (!flag)
					{
						AddLog("Player not found.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Parse error: [minutes] - has to be an integer.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				}
			}
			goto IL_2cb8;
			IL_1da9:
			if (array6.Length < 2)
			{
				AddLog("The second argument cannot be <i>null</i>!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			else
			{
				bool flag2 = false;
				string text7 = array6[1].ToUpper();
				foreach (Value value2 in _values)
				{
					if (!(value2.key != text7))
					{
						flag2 = true;
						AddLog("The value of " + text7 + " is: @#{[" + text7 + "}]#@", new Color32(50, 70, 100, byte.MaxValue));
					}
				}
				if (!flag2)
				{
					AddLog("Key " + text7 + " not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
			}
			goto IL_2cb8;
			IL_2cb8:
			return _response;
			IL_288f:
			if (array6.Length < 2)
			{
				TypeCommand("HELP CONFIG");
			}
			else
			{
				switch (array6[1].ToUpper())
				{
				case "RELOAD":
				case "R":
				case "RLD":
					ConfigFile.ReloadGameConfig();
					ServerStatic.RolesConfig = new YamlConfig(ServerStatic.RolesConfigPath);
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig);
					AddLog("Configuration file <b>successfully reloaded</b>. New settings will be applied on <b>your</b> server in <b>next</b> round.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					break;
				case "PATH":
					AddLog("Configuration file path: <i>" + ConfigFile.ConfigPath + "</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					AddLog("<i>No visible drive letter means the root game directory.</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					break;
				case "VALUE":
					if (array6.Length < 3)
					{
						AddLog("Please enter key name in the third argument. (CONFIG VALUE <i>KEYNAME</i>)", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					}
					else
					{
						AddLog("The value of <i>'" + array6[2].ToUpper() + "'</i> is: " + ConfigFile.ServerConfig.GetString(array6[2].ToUpper(), "<color=ff0>DENIED: Entered key does not exists</color>"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
					break;
				}
			}
			goto IL_2cb8;
			IL_2301:
			GameObject[] array11 = GameObject.FindGameObjectsWithTag("RoomID");
			GameObject[] array12 = array11;
			foreach (GameObject gameObject6 in array12)
			{
				gameObject6.GetComponentsInChildren<MeshRenderer>()[0].enabled = !gameObject6.GetComponentsInChildren<MeshRenderer>()[0].enabled;
				gameObject6.GetComponentsInChildren<MeshRenderer>()[1].enabled = !gameObject6.GetComponentsInChildren<MeshRenderer>()[1].enabled;
			}
			if (array11.Length > 0)
			{
				AddLog("Show RIDS: " + array11[0].GetComponentInChildren<MeshRenderer>().enabled, new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			else
			{
				AddLog("There are no RIDS!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			goto IL_2cb8;
			IL_0d3f:
			GameObject[] array13 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array14 = array13;
			foreach (GameObject gameObject7 in array14)
			{
				if (gameObject7.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					gameObject7.GetComponent<QueryProcessor>().ConfirmGlobalBanning();
				}
			}
			goto IL_2cb8;
			IL_2c87:
			AddLog("Command " + cmd + " does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			goto IL_2cb8;
			IL_0d96:
			GameObject[] array15 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array16 = array15;
			foreach (GameObject gameObject8 in array16)
			{
				if (gameObject8.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					if (gameObject8.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey == null)
					{
						AddLog("Encryption key: (null) - session not encrypted (probably due to online mode disabled).", Color.grey);
					}
					else
					{
						AddLog("Encryption key (KEEP SECRET!): " + BitConverter.ToString(gameObject8.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey), Color.grey);
					}
				}
			}
			goto IL_2cb8;
			IL_07ec:
			AddLog("<size=450>( \u0361° \u035cʖ \u0361°)</size>\n\n", new Color32(byte.MaxValue, 180, 180, byte.MaxValue));
			goto IL_2cb8;
			IL_0816:
			GameObject[] array17 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array18 = array17;
			foreach (GameObject gameObject9 in array18)
			{
				if (gameObject9.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Requesting server-owner's contact email...", Color.yellow);
					gameObject9.GetComponent<CharacterClassManager>().CallCmdRequestContactEmail();
				}
			}
			goto IL_2cb8;
			IL_11c6:
			if (SceneManager.GetActiveScene().name.Contains("Menu"))
			{
				CustomNetworkManager customNetworkManager = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
				customNetworkManager.onlineScene = "Shooting Range";
				customNetworkManager.CreateMatch();
				AddLog("Loading training range...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			else
			{
				AddLog("This command can only be run in the main menu", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
			}
			goto IL_2cb8;
			IL_0b3a:
			GameObject[] array19 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array20 = array19;
			foreach (GameObject gameObject10 in array20)
			{
				if (gameObject10.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					Broadcast.Messages.Clear();
					BroadcastAssigner.MessageDisplayed = false;
					AddLog("All broadcasts locally cleared.", Color.green);
				}
			}
			goto IL_2cb8;
			IL_227e:
			if (array6.Length != 2)
			{
				AddLog("Syntax: \"connect IP\" or \"connect IP:Port\"", Color.red);
				return _response;
			}
			MainMenuScript mainMenuScript = UnityEngine.Object.FindObjectOfType<MainMenuScript>();
			mainMenuScript.SetIP(array6[1]);
			mainMenuScript.Connect();
			goto IL_2cb8;
			IL_1b95:
			bool flag3 = array6.Length > 1 && array6[1].ToUpper() == "LIST";
			bool flag4 = (array6.Length > 1 && array6[1].ToUpper() == "ALL") || (array6.Length > 2 && array6[2].ToUpper() == "ALL");
			GameObject[] array21 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array22 = array21;
			foreach (GameObject gameObject11 in array22)
			{
				ServerRoles component = gameObject11.GetComponent<ServerRoles>();
				if (!component.isLocalPlayer)
				{
					continue;
				}
				AddLog("Available colors:", Color.gray);
				string text8 = string.Empty;
				ServerRoles.NamedColor[] namedColors = component.NamedColors;
				foreach (ServerRoles.NamedColor namedColor in namedColors)
				{
					if (!namedColor.Restricted || flag4)
					{
						if (flag3)
						{
							AddLog("<color=#" + namedColor.ColorHex + ">" + namedColor.Name + ((!namedColor.Restricted) ? string.Empty : "*") + " - #" + namedColor.ColorHex + "</color>", Color.white);
						}
						else
						{
							string text2 = text8;
							text8 = text2 + "<color=#" + namedColor.ColorHex + ">" + namedColor.Name + ((!namedColor.Restricted) ? string.Empty : "*") + "</color>    ";
						}
					}
				}
				if (!flag3)
				{
					AddLog(text8, Color.white);
				}
			}
			goto IL_2cb8;
			IL_0883:
			GameObject[] array23 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array24 = array23;
			foreach (GameObject gameObject12 in array24)
			{
				if (gameObject12.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Requesting server config...", Color.yellow);
					gameObject12.GetComponent<CharacterClassManager>().CallCmdRequestServerConfig();
				}
			}
			goto IL_2cb8;
			IL_185f:
			_logs.Clear();
			RefreshConsoleScreen();
			AddLog("<size=50>GOODBYE!</size>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			RefreshConsoleScreen();
			Invoke("QuitGame", 1f);
			goto IL_2cb8;
			IL_1314:
			bool flag5 = false;
			GameObject[] array25 = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject gameObject13 in array25)
			{
				PlayerStats component2 = gameObject13.GetComponent<PlayerStats>();
				if (component2.isLocalPlayer && component2.isServer)
				{
					flag5 = true;
					AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					component2.Roundrestart();
				}
			}
			if (!flag5)
			{
				AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			goto IL_2cb8;
			IL_1f1f:
			DisableSLML = !DisableSLML;
			AddLog("SLML has been turned " + ((!DisableSLML) ? "ON" : "OFF") + " for current session.", Color.green);
			if (DisableSLML)
			{
				AddLog("To disable SLML by default, use \"-noslml\" startup argument.", Color.green);
			}
			goto IL_2cb8;
			IL_22c3:
			AddLog("Disconnecting...", Color.gray);
			if (NetworkServer.active)
			{
				NetworkManager.singleton.StopHost();
			}
			else
			{
				NetworkManager.singleton.StopClient();
			}
			goto IL_2cb8;
			IL_08f0:
			GameObject[] array26 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array27 = array26;
			foreach (GameObject gameObject14 in array27)
			{
				if (gameObject14.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Requesting server groups...", Color.yellow);
					gameObject14.GetComponent<CharacterClassManager>().CallCmdRequestServerGroups();
				}
			}
			goto IL_2cb8;
			IL_1b45:
			alwaysRefreshing = !alwaysRefreshing;
			AddLog("Console log refresh mode: " + ((!alwaysRefreshing) ? "OPTIMIZED" : "FIXED"), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			goto IL_2cb8;
			IL_0bab:
			GameObject[] array28 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array29 = array28;
			foreach (GameObject gameObject15 in array29)
			{
				if (gameObject15.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Requesting your global tag...", Color.yellow);
					gameObject15.GetComponent<CharacterClassManager>().CallCmdRequestShowTag(true);
				}
			}
			goto IL_2cb8;
			IL_0f98:
			if (!(from player in GameObject.FindGameObjectsWithTag("Player")
				select player.GetComponent<PlayerStats>()).Any((PlayerStats nid) => nid.isLocalPlayer && nid.isServer))
			{
				AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			else
			{
				int result4 = 0;
				if (array6.Length >= 2 && int.TryParse(array6[1], out result4))
				{
					string text9 = "offline";
					GameObject[] array30 = GameObject.FindGameObjectsWithTag("Player");
					GameObject[] array31 = array30;
					foreach (GameObject gameObject16 in array31)
					{
						if (!gameObject16.GetComponent<NetworkIdentity>().isLocalPlayer)
						{
							continue;
						}
						text9 = "online";
						Inventory component3 = gameObject16.GetComponent<Inventory>();
						if (!(component3 == null))
						{
							if (component3.availableItems.Length > result4)
							{
								component3.AddNewItem(result4);
								text9 = "none";
							}
							else
							{
								AddLog("Failed to add ITEM#" + result4.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
						}
					}
					if (text9 == "offline" || text9 == "online")
					{
						AddLog((!(text9 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else
					{
						AddLog("ITEM#" + result4.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
			}
			goto IL_2cb8;
			IL_1f8a:
			string text10 = CentralServer.Servers.Aggregate((string text13, string adding) => text13 = text13 + ", " + adding);
			AddLog("Use \"" + array6[0].ToUpper() + " -r\" to change to different central server.", Color.gray);
			AddLog("Use \"" + array6[0].ToUpper() + " -t\" to change to TEST central server.", Color.gray);
			AddLog("Use \"" + array6[0].ToUpper() + " -s CentralServerNameHere\" to change to specified central server.", Color.gray);
			if (array6.Length > 1)
			{
				switch (array6[1].ToUpper())
				{
				case "-R":
					CentralServer.ChangeCentralServer(false);
					AddLog("--- Central server changed ---", Color.green);
					break;
				case "-T":
					CentralServer.SelectedServer = "TEST";
					CentralServer.StandardUrl = "https://test.scpslgame.com/";
					CentralServer.TestServer = true;
					AddLog("--- Central server changed to TEST SERVER ---", Color.green);
					break;
				default:
					if ((array6[1].ToUpper() == "-S" || array6[1].ToUpper() == "-FS") && array6.Length == 3)
					{
						if (!CentralServer.Servers.Contains(array6[2].ToUpper()) && array6[1].ToUpper() != "-FS")
						{
							AddLog("Server " + array6[2].ToUpper() + " is not on the list. Use " + array6[0].ToUpper() + " -fs " + array6[2].ToUpper() + " to force the change.", Color.red);
							return _response;
						}
						CentralServer.SelectedServer = array6[2].ToUpper();
						CentralServer.StandardUrl = "https://" + array6[2].ToUpper() + ".scpslgame.com/";
						CentralServer.TestServer = false;
						AddLog("--- Central server changed to " + array6[2].ToUpper() + " ---", Color.green);
					}
					break;
				}
			}
			AddLog("Master central server: " + CentralServer.MasterUrl, Color.green);
			AddLog("Selected central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.green);
			AddLog("All central servers: " + text10, Color.green);
			goto IL_2cb8;
			IL_095d:
			GameObject gameObject17 = GameObject.Find("Host");
			if (gameObject17 != null && gameObject17.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				ServerRoles component4 = gameObject17.GetComponent<ServerRoles>();
				component4.RemoteAdmin = true;
				component4.OverwatchPermitted = true;
				component4.Permissions = ServerStatic.PermissionsHandler.FullPerm;
				component4.CallTargetOpenRemoteAdmin(component4.connectionToClient, false);
				AddLog("Remote admin enabled for you.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				return string.Empty;
			}
			goto IL_2cb8;
			IL_23d9:
			string text11 = "offline";
			GameObject[] array32 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array33 = array32;
			foreach (GameObject gameObject18 in array33)
			{
				int result5 = 1;
				if (array6.Length >= 2 && !int.TryParse(array6[1], out result5))
				{
					AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					return _response;
				}
				if (!gameObject18.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					continue;
				}
				text11 = "online";
				CharacterClassManager component5 = gameObject18.GetComponent<CharacterClassManager>();
				if (component5 == null)
				{
					continue;
				}
				text11 = "none";
				if (result5 < 1)
				{
					AddLog("Page '" + result5 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					RefreshConsoleScreen();
					return _response;
				}
				Class[] klasy = component5.klasy;
				for (int num15 = 10 * (result5 - 1); num15 < 10 * result5; num15++)
				{
					if (10 * (result5 - 1) > klasy.Length)
					{
						AddLog("Page '" + result5 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						break;
					}
					if (num15 >= klasy.Length)
					{
						break;
					}
					AddLog("CLASS#" + num15.ToString("000") + " : " + klasy[num15].fullName, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
			}
			if (text11 != "none")
			{
				AddLog((!(text11 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			goto IL_2cb8;
			IL_1849:
			_logs.Clear();
			RefreshConsoleScreen();
			goto IL_2cb8;
			IL_09e8:
			GameObject[] array34 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array35 = array34;
			foreach (GameObject gameObject19 in array35)
			{
				if (gameObject19.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Hidding your tag...", Color.yellow);
					gameObject19.GetComponent<CharacterClassManager>().CallCmdRequestHideTag();
				}
			}
			goto IL_2cb8;
			IL_2a43:
			if (array6.Length < 3)
			{
				AddLog("Syntax: \"keybind <key/keycode> <axis>\"", Color.red);
				return _response;
			}
			foreach (NewInput.Bind binding2 in NewInput.bindings)
			{
				if (!binding2.axis.ToUpper().Contains(array6[2].ToUpper()))
				{
					continue;
				}
				int result6 = -1;
				int.TryParse(array6[1], out result6);
				if (result6 == 0 && Enum.IsDefined(typeof(KeyCode), array6[1].ToUpper()))
				{
					result6 = (int)Enum.Parse(typeof(KeyCode), array6[1].ToUpper(), true);
				}
				if (result6 < 0 || result6 > 509)
				{
					AddLog("Invalid key code: " + result6, Color.red);
					return _response;
				}
				binding2.key = (KeyCode)result6;
				NewInput.Save();
				AddLog(string.Concat(binding2.axis, " has been bound to [", binding2.key, "]!"), Color.green);
				return _response;
			}
			AddLog("Key axis '" + array6[2].ToUpper() + "' does not exist.", Color.red);
			goto IL_2cb8;
			IL_1eb9:
			GameObject gameObject20 = GameObject.Find("Host");
			AddLog("Map seed is: <b>" + ((!(gameObject20 == null)) ? gameObject20.GetComponent<RandomSeedSync>().seed.ToString() : "NONE") + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			goto IL_2cb8;
			IL_1245:
			float result7 = 0f;
			if (array6.Length >= 2 && float.TryParse(array6[1], out result7))
			{
				PlayerPrefs.SetFloat("Sens", result7);
				Sensitivity.sens = result7;
				SensitivitySlider[] array36 = UnityEngine.Object.FindObjectsOfType<SensitivitySlider>();
				foreach (SensitivitySlider sensitivitySlider in array36)
				{
					sensitivitySlider.ChangeViaConsole(result7);
				}
				AddLog("New sensitivity saved! (" + Sensitivity.sens + ")", new Color32(80, 150, 80, byte.MaxValue));
			}
			else
			{
				AddLog("The current sensitivity is: " + Sensitivity.sens, new Color32(80, 150, 80, byte.MaxValue));
			}
			goto IL_2cb8;
			IL_0e39:
			GameObject[] array37 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array38 = array37;
			foreach (GameObject gameObject21 in array38)
			{
				if (gameObject21.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					if (array6.Length == 1)
					{
						gameObject21.GetComponent<ServerRoles>().CallCmdToggleOverwatch();
					}
					else if (array6[1] == "1" || array6[1].ToLower() == "true" || array6[1].ToLower() == "enable" || array6[1].ToLower() == "on")
					{
						gameObject21.GetComponent<ServerRoles>().CallCmdSetOverwatchStatus(true);
					}
					else if (array6[1] == "0" || array6[1].ToLower() == "false" || array6[1].ToLower() == "disable" || array6[1].ToLower() == "off")
					{
						gameObject21.GetComponent<ServerRoles>().CallCmdSetOverwatchStatus(false);
					}
					else
					{
						AddLog("Unknown status: " + array6[1], Color.red);
					}
				}
			}
			goto IL_2cb8;
			IL_0a55:
			GameObject[] array39 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array40 = array39;
			foreach (GameObject gameObject22 in array40)
			{
				if (gameObject22.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Your Player ID on the current server: " + gameObject22.GetComponent<QueryProcessor>().PlayerId, Color.green);
				}
			}
			goto IL_2cb8;
			IL_13bd:
			string text12 = "offline";
			GameObject[] array41 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array42 = array41;
			foreach (GameObject gameObject23 in array42)
			{
				int result8 = 1;
				if (array6.Length >= 2 && !int.TryParse(array6[1], out result8))
				{
					AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					return _response;
				}
				if (!gameObject23.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					continue;
				}
				text12 = "online";
				Inventory component6 = gameObject23.GetComponent<Inventory>();
				if (component6 == null)
				{
					continue;
				}
				text12 = "none";
				if (result8 < 1)
				{
					AddLog("Page '" + result8 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					RefreshConsoleScreen();
					return _response;
				}
				Item[] availableItems = component6.availableItems;
				for (int num21 = 10 * (result8 - 1); num21 < 10 * result8; num21++)
				{
					if (10 * (result8 - 1) > availableItems.Length)
					{
						AddLog("Page '" + result8 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						break;
					}
					if (num21 >= availableItems.Length)
					{
						break;
					}
					AddLog("ITEM#" + num21.ToString("000") + " : " + availableItems[num21].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
			}
			if (text12 != "none")
			{
				AddLog((!(text12 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			goto IL_2cb8;
			IL_0c19:
			if (array6.Length < 3 || (array6[1].ToLower() != "nick" && array6[1].ToLower() != "id"))
			{
				AddLog("Syntax: globalban <selector: \"nick\" OR \"id\"> <player to ban>", Color.red);
			}
			else if (!File.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "StaffAPI.txt"))
			{
				AddLog("Staff API token not found on your computer!", Color.red);
			}
			else
			{
				GameObject[] array43 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array44 = array43;
				foreach (GameObject gameObject24 in array44)
				{
					if (gameObject24.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting your global ban...", Color.yellow);
						gameObject24.GetComponent<QueryProcessor>().RequestGlobalBan(array6[2].ToUpper(), (!(array6[1].ToLower() == "id")) ? 1 : 0);
					}
				}
			}
			goto IL_2cb8;
			IL_2beb:
			if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/internal/SyncCmd"))
			{
				File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/internal/SyncCmd");
				AddLog("SyncServerCommandBinding has been disabled.", Color.green);
			}
			else
			{
				StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/internal/SyncCmd");
				streamWriter.Close();
				AddLog("SyncServerCommandBinding has been enabled.", Color.green);
				AddLog("[WARNING] Your command binding might be messed up, and your key might be logged by the server you join.", Color.yellow);
			}
			goto IL_2cb8;
			IL_25f2:
			AlphaWarheadController host = AlphaWarheadController.host;
			if (array6.Length == 1)
			{
				AddLog("Synax: warhead (status|detonate|cancel|enable|disable)", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
			}
			else
			{
				switch (array6[1].ToLower())
				{
				case "status":
					if (host.detonated || host.timeToDetonation == 0f)
					{
						AddLog("Warhead has been detonated.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else if (host.inProgress)
					{
						AddLog("Detonation is in progress.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else if (!AlphaWarheadOutsitePanel.nukeside.enabled)
					{
						AddLog("Warhead is disabled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else if (host.timeToDetonation > AlphaWarheadController.host.RealDetonationTime())
					{
						AddLog("Warhead is restarting.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else
					{
						AddLog("Warhead is ready to detonation.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					break;
				case "detonate":
					AlphaWarheadController.host.StartDetonation();
					AddLog("Detonation sequence started.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				case "cancel":
					AlphaWarheadController.host.CancelDetonation(null);
					AddLog("Detonation has been canceled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				case "enable":
					AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
					AddLog("Warhead has been enabled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				case "disable":
					AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
					AddLog("Warhead has been disabled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				default:
					AddLog("WARHEAD: Unknown subcommand.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				}
			}
			goto IL_2cb8;
			IL_0acc:
			GameObject[] array45 = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array46 = array45;
			foreach (GameObject gameObject25 in array46)
			{
				if (gameObject25.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Requesting your local tag...", Color.yellow);
					gameObject25.GetComponent<CharacterClassManager>().CallCmdRequestShowTag(false);
				}
			}
			goto IL_2cb8;
		}

		public void ProceedButton()
		{
			if (cmdField.text != string.Empty)
			{
				TypeCommand(cmdField.text);
			}
			cmdField.text = string.Empty;
			EventSystem.current.SetSelectedGameObject(cmdField.gameObject);
		}

		public void ToggleConsole()
		{
			CursorManager.singleton.consoleOpen = !console.activeSelf;
			cmdField.text = string.Empty;
			console.SetActive(!console.activeSelf);
			if (PlayerManager.singleton != null)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array2 = array;
				foreach (GameObject gameObject in array2)
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						FirstPersonController component = gameObject.GetComponent<FirstPersonController>();
						if (component != null)
						{
							component.usingConsole = console.activeSelf;
						}
					}
				}
			}
			if (console.activeSelf)
			{
				EventSystem.current.SetSelectedGameObject(cmdField.gameObject);
			}
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
			if (!string.IsNullOrEmpty(cache))
			{
				Publickey = ECDSA.PublicKeyFromString(cache);
				cacheHash = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Publickey)));
				AddLog("Loaded central server public key from cache.\nSHA256 of public key: " + cacheHash, Color.gray);
			}
			while (!CentralServer.ServerSelected)
			{
				yield return Timing.WaitForSeconds(1f);
			}
			using (WWW www = new WWW(CentralServer.StandardUrl + "publickey.php"))
			{
				yield return Timing.WaitUntilDone(www);
				try
				{
					if (!string.IsNullOrEmpty(www.error))
					{
						AddLog("Can't refresh central server public key - " + www.error, Color.red);
						yield break;
					}
					Publickey = ECDSA.PublicKeyFromString(www.text);
					ServerConsole.Publickey = Publickey;
					string text = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Publickey)));
					AddLog("Downloaded public key from central server.\nSHA256 of public key: " + text, Color.green);
					if (text != cacheHash)
					{
						CentralServerKeyCache.SaveCache(www.text);
					}
					else
					{
						AddLog("SHA256 of cached key matches, no need to update cache.", Color.grey);
					}
				}
				catch
				{
					AddLog("Can't refresh central server public key!", Color.red);
				}
			}
		}

		private void QuitGame()
		{
			Application.Quit();
		}
	}
}
