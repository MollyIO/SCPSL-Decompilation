using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AntiFaker;
using GameConsole;
using MEC;
using RemoteAdmin;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class CharacterClassManager : NetworkBehaviour
{
	public Class[] klasy;

	private static Class[] _staticClasses;

	public List<Team> classTeamQueue = new List<Team>();

	public GameObject unfocusedCamera;

	private GameObject _plyCam;

	private CentralAuthInterface _centralAuthInt;

	private static GameObject _host;

	public float ciPercentage;

	public float aliveTime;

	public long DeathTime;

	public int forceClass = -1;

	private int _seed;

	private int _prevId = -1;

	public bool OnlineMode;

	public bool GodMode;

	private bool _wasAnytimeAlive;

	private bool _commandtokensent;

	private bool _enableSyncServerCmdBinding;

	public string AuthToken;

	public bool SpawnProtected;

	public float ProtectedTime;

	public bool EnableSP;

	public float SProtectedDuration;

	public List<int> SProtectedTeam;

	public int EscapeStartTime;

	public bool CheatReported;

	[SerializeField]
	public AudioClip bell;

	[SerializeField]
	public AudioClip bell_dead;

	[HideInInspector]
	public GameObject myModel;

	[HideInInspector]
	public GameObject charCamera;

	[SyncVar(hook = "SetMuted")]
	public bool Muted;

	[SyncVar]
	public bool IntercomMuted;

	private Scp049PlayerScript _scp049;

	private Scp079PlayerScript _scp079;

	private Scp049_2PlayerScript _scp0492;

	private Scp106PlayerScript _scp106;

	private Scp173PlayerScript _scp173;

	private Scp096PlayerScript _scp096;

	private Scp939PlayerScript _scp939;

	private LureSubjectContainer _lureSpj;

	[SyncVar(hook = "SetClassID")]
	public int curClass;

	[SyncVar(hook = "SyncDeathPos")]
	public Vector3 deathPosition;

	[SyncVar(hook = "SetUnit")]
	public int ntfUnit;

	[SyncVar(hook = "SetRoundStart")]
	public bool roundStarted;

	[SyncVar(hook = "SetVerification")]
	public bool IsVerified;

	[SyncVar(hook = "SetSteamId")]
	public string SteamId;

	private static int kRpcRpcPlaceBlood;

	private static int kTargetRpcTargetChangeCmdBinding;

	private static int kCmdCmdSendToken;

	private static int kCmdCmdRequestContactEmail;

	private static int kCmdCmdRequestServerConfig;

	private static int kCmdCmdRequestServerGroups;

	private static int kCmdCmdRequestHideTag;

	private static int kCmdCmdRequestShowTag;

	private static int kCmdCmdSuicide;

	private static int kTargetRpcTargetSetDisconnectError;

	private static int kCmdCmdConfirmDisconnect;

	private static int kCmdCmdRegisterEscape;

	private static int kCmdCmdRequestDeathScreen;

	private static int kTargetRpcTargetDeathScreen;

	public bool NetworkMuted
	{
		get
		{
			return Muted;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetMuted(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.Muted, 1u);
		}
	}

	public bool NetworkIntercomMuted
	{
		get
		{
			return IntercomMuted;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref IntercomMuted, 2u);
		}
	}

	public int NetworkcurClass
	{
		get
		{
			return curClass;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetClassID(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.curClass, 4u);
		}
	}

	public Vector3 NetworkdeathPosition
	{
		get
		{
			return deathPosition;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SyncDeathPos(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.deathPosition, 8u);
		}
	}

	public int NetworkntfUnit
	{
		get
		{
			return ntfUnit;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetUnit(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.ntfUnit, 16u);
		}
	}

	public bool NetworkroundStarted
	{
		get
		{
			return roundStarted;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetRoundStart(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.roundStarted, 32u);
		}
	}

	public bool NetworkIsVerified
	{
		get
		{
			return IsVerified;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetVerification(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.IsVerified, 64u);
		}
	}

	public string NetworkSteamId
	{
		get
		{
			return SteamId;
		}
		[param: In]
		set
		{
			CharacterClassManager characterClassManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetSteamId(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref characterClassManager.SteamId, 128u);
		}
	}

	private void Start()
	{
		OnlineMode = ConfigFile.ServerConfig.GetBool("online_mode", true);
		_centralAuthInt = new CentralAuthInterface(this, base.isServer);
		_lureSpj = UnityEngine.Object.FindObjectOfType<LureSubjectContainer>();
		_scp049 = GetComponent<Scp049PlayerScript>();
		_scp079 = GetComponent<Scp079PlayerScript>();
		_scp0492 = GetComponent<Scp049_2PlayerScript>();
		_scp106 = GetComponent<Scp106PlayerScript>();
		_scp173 = GetComponent<Scp173PlayerScript>();
		_scp096 = GetComponent<Scp096PlayerScript>();
		_scp939 = GetComponent<Scp939PlayerScript>();
		forceClass = ConfigFile.ServerConfig.GetInt("server_forced_class", -1);
		ciPercentage = ConfigFile.ServerConfig.GetInt("ci_on_start_percent", 10);
		_enableSyncServerCmdBinding = ConfigFile.ServerConfig.GetBool("enable_sync_command_binding");
		EnableSP = !ConfigFile.ServerConfig.GetBool("spawn_protect_disable", true);
		SProtectedDuration = ConfigFile.ServerConfig.GetInt("spawn_protect_time", 30);
		SProtectedTeam = new List<int>(ConfigFile.ServerConfig.GetIntList("spawn_protect_team"));
		if (SProtectedTeam.Count <= 0)
		{
			SProtectedTeam = new List<int> { 1, 2 };
		}
		StartCoroutine(Init());
		string text = ConfigFile.ServerConfig.GetString("team_respawn_queue", "401431403144144");
		classTeamQueue.Clear();
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			int result = 4;
			if (!int.TryParse(c.ToString(), out result))
			{
				result = 4;
			}
			classTeamQueue.Add((Team)result);
		}
		while (classTeamQueue.Count < NetworkManager.singleton.maxConnections)
		{
			classTeamQueue.Add(Team.CDP);
		}
		if (!base.isLocalPlayer && TutorialManager.status)
		{
			ApplyProperties();
		}
		if (base.isLocalPlayer)
		{
			for (int j = 0; j < klasy.Length; j++)
			{
				if (klasy[j].team != Team.SCP)
				{
					klasy[j].fullName = TranslationReader.Get("Class_Names", j);
					klasy[j].description = TranslationReader.Get("Class_Descriptions", j);
				}
			}
			_staticClasses = klasy;
			if (SteamManager.Running && !ServerStatic.IsDedicated)
			{
				CentralAuth.singleton.GenerateToken(_centralAuthInt);
				return;
			}
			if (!ServerStatic.IsDedicated)
			{
				GameConsole.Console.singleton.AddLog("Steam not initialized - sending empty auth token.\nIf server is using online mode, you will probably get kicked.", Color.red);
			}
			CallCmdSendToken(string.Empty);
		}
		else if (_staticClasses == null || _staticClasses.Length == 0)
		{
			for (int k = 0; k < klasy.Length; k++)
			{
				klasy[k].description = TranslationReader.Get("Class_Descriptions", k);
				if (klasy[k].team != Team.SCP)
				{
					klasy[k].fullName = TranslationReader.Get("Class_Names", k);
				}
			}
		}
		else
		{
			klasy = _staticClasses;
		}
	}

	private void Update()
	{
		if (curClass == 2)
		{
			aliveTime = 0f;
		}
		else
		{
			aliveTime += Time.deltaTime;
		}
		if (base.isLocalPlayer)
		{
			if (ServerStatic.IsDedicated)
			{
				CursorManager.singleton.isServerOnly = true;
			}
			if (base.isServer)
			{
				AllowContain();
			}
		}
		if (_prevId != curClass)
		{
			RefreshPlyModel();
			_prevId = curClass;
		}
		if (base.name == "Host")
		{
			Radio.roundStarted = roundStarted;
		}
	}

	private void FixedUpdate()
	{
		if (base.isServer && SpawnProtected && (Time.time - ProtectedTime > SProtectedDuration || curClass < 0 || !SProtectedTeam.Contains((int)klasy[curClass].team)))
		{
			SpawnProtected = false;
			GodMode = false;
		}
	}

	private void SetVerification(bool b)
	{
		NetworkIsVerified = b;
	}

	public void SetUnit(int unit)
	{
		NetworkntfUnit = unit;
	}

	public void SyncDeathPos(Vector3 v)
	{
		NetworkdeathPosition = v;
	}

	public void SetSteamId(string i)
	{
		NetworkSteamId = i;
		if ((!base.isServer || !ServerStatic.IsDedicated) && MuteHandler.QueryPersistantMute(i))
		{
			PlayerList.Instance instance = PlayerList.instances.FirstOrDefault((PlayerList.Instance inst) => inst.owner == base.gameObject);
			if (instance != null)
			{
				GameConsole.Console.singleton.AddLog("Muting player " + i + " - previously muted by user.", Color.gray);
				instance.text.GetComponent<PlayerListElement>().Mute(true);
				instance.text.GetComponentInChildren<Toggle>().isOn = true;
			}
		}
	}

	public void SetMuted(bool i)
	{
		NetworkMuted = i;
		if (base.isServer && ServerStatic.IsDedicated)
		{
			return;
		}
		try
		{
			PlayerList.Instance instance = PlayerList.instances.FirstOrDefault((PlayerList.Instance inst) => inst.owner == base.gameObject);
			if (instance != null)
			{
				instance.text.GetComponent<PlayerListElement>().Mute(i);
				instance.text.GetComponentInChildren<Toggle>().isOn = i;
			}
		}
		catch
		{
		}
	}

	[ServerCallback]
	public void AllowContain()
	{
		if (!NetworkServer.active || !NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (Vector3.Distance(gameObject.transform.position, _lureSpj.transform.position) < 1.97f)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				PlayerStats component2 = gameObject.GetComponent<PlayerStats>();
				if (component.klasy[component.curClass].team != Team.SCP && component.curClass != 2 && !component.GodMode)
				{
					component2.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, 0), gameObject);
					_lureSpj.SetState(true);
				}
			}
		}
	}

	[ClientRpc]
	public void RpcPlaceBlood(Vector3 pos, int type, float f)
	{
		GetComponent<BloodDrawer>().PlaceUnderneath(pos, type, f);
	}

	public void SyncServerCmdBinding()
	{
		if (!base.isServer || !_enableSyncServerCmdBinding)
		{
			return;
		}
		foreach (CmdBinding.Bind binding in CmdBinding.bindings)
		{
			if (binding.command.StartsWith(".") || binding.command.StartsWith("/"))
			{
				CallTargetChangeCmdBinding(base.connectionToClient, binding.key, binding.command);
			}
		}
	}

	[TargetRpc]
	public void TargetChangeCmdBinding(NetworkConnection connection, KeyCode code, string cmd)
	{
		CmdBinding.ChangeKeybinding(code, cmd);
	}

	public void TargetConsolePrint(NetworkConnection connection, string text, string color)
	{
		GetComponent<GameConsoleTransmission>().SendToClient(connection, text, color);
	}

	public bool IsHuman()
	{
		return curClass >= 0 && klasy[curClass].team != Team.SCP && klasy[curClass].team != Team.RIP;
	}

	public bool IsTargetForSCPs()
	{
		return curClass >= 0 && klasy[curClass].team != Team.SCP && klasy[curClass].team != Team.RIP && klasy[curClass].team != Team.CHI;
	}

	private IEnumerator Init()
	{
		if (NetworkServer.active)
		{
			if (ConfigFile.ServerConfig.GetBool("online_mode", true) && !base.isLocalPlayer)
			{
				float timeout = 0f;
				while (timeout < 12f)
				{
					timeout += Timing.DeltaTime;
					yield return 0f;
					if (!string.IsNullOrEmpty(SteamId))
					{
						NetworkIsVerified = true;
						yield break;
					}
					if (timeout < 12f)
					{
						continue;
					}
					ServerConsole.Disconnect(base.connectionToClient, "Your client has failed to authenticate in time.");
					yield break;
				}
			}
			else
			{
				NetworkIsVerified = true;
			}
		}
		while (_host == null)
		{
			_host = GameObject.Find("Host");
			yield return 0f;
		}
		if (base.isLocalPlayer)
		{
			while (_seed == 0)
			{
				_seed = _host.GetComponent<RandomSeedSync>().seed;
			}
			if (NetworkServer.active)
			{
				if (ServerStatic.IsDedicated)
				{
					ServerConsole.AddLog("Waiting for players..");
				}
				CursorManager.singleton.roundStarted = true;
				if (NonFacilityCompatibility.currentSceneSettings.roundAutostart)
				{
					ForceRoundStart();
				}
				else
				{
					RoundStart.singleton.ShowButton();
					int timeLeft = 20;
					int mostPlayersSoFar = 1;
					while (RoundStart.singleton.info != "started")
					{
						if (mostPlayersSoFar > 1)
						{
							timeLeft--;
						}
						int count = PlayerManager.singleton.players.Length;
						if (count > mostPlayersSoFar)
						{
							mostPlayersSoFar = count;
							if (mostPlayersSoFar >= ((CustomNetworkManager)NetworkManager.singleton).ReservedMaxPlayers)
							{
								timeLeft = 0;
							}
							else if (timeLeft % 5 > 0)
							{
								timeLeft = timeLeft / 5 * 5 + 5;
							}
						}
						if (timeLeft > 0)
						{
							RoundStart.singleton.Networkinfo = timeLeft.ToString();
						}
						else
						{
							ForceRoundStart();
						}
						yield return new WaitForSeconds(1.2f);
					}
				}
				CursorManager.singleton.roundStarted = false;
				CmdStartRound();
				SetRoundStart(true);
				SetRandomRoles();
			}
			while (!_host.GetComponent<CharacterClassManager>().roundStarted)
			{
				yield return 0f;
			}
			yield return new WaitForSeconds(2f);
			while (curClass < 0)
			{
				CallCmdSuicide(default(PlayerStats.HitInfo));
				yield return new WaitForSeconds(1f);
			}
		}
		if (!base.isLocalPlayer)
		{
			yield break;
		}
		int iteration = 0;
		while (true)
		{
			GameObject[] plys = PlayerManager.singleton.players;
			if (iteration >= plys.Length)
			{
				yield return new WaitForSeconds(3f);
				iteration = 0;
			}
			try
			{
				plys[iteration].GetComponent<CharacterClassManager>().InitSCPs();
			}
			catch
			{
			}
			iteration++;
			yield return 0f;
		}
	}

	[Command(channel = 2)]
	public void CmdSendToken(string token)
	{
		if (ConfigFile.ServerConfig.GetBool("online_mode", true))
		{
			if (_commandtokensent)
			{
				if (!base.isLocalPlayer)
				{
					ServerConsole.Disconnect(base.connectionToClient, "Your client sent second authentication token.");
				}
			}
			else if (string.IsNullOrEmpty(token) || _commandtokensent)
			{
				if (!base.isLocalPlayer)
				{
					ServerConsole.Disconnect(base.connectionToClient, "Your client sent an empty authentication token. Make sure you are running the game by steam.");
				}
				else
				{
					SetVerification(true);
				}
			}
			else if (!base.isLocalPlayer)
			{
				CentralAuth.singleton.StartValidateToken(_centralAuthInt, token);
				AuthToken = token;
			}
			else
			{
				SetVerification(true);
				CentralAuth.singleton.StartValidateToken(_centralAuthInt, token);
				AuthToken = token;
			}
		}
		_commandtokensent = true;
	}

	[Command(channel = 2)]
	public void CmdRequestContactEmail()
	{
		if (GetComponent<ServerRoles>().RemoteAdmin || GetComponent<ServerRoles>().Staff)
		{
			TargetConsolePrint(base.connectionToClient, "Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email", string.Empty), "green");
		}
		else
		{
			TargetConsolePrint(base.connectionToClient, "You don't have permissions to execute this command.", "red");
		}
	}

	[Command(channel = 2)]
	public void CmdRequestServerConfig()
	{
		YamlConfig serverConfig = ConfigFile.ServerConfig;
		if (GetComponent<ServerRoles>().Staff || (GetComponent<ServerRoles>().RemoteAdmin && ServerStatic.PermissionsHandler.IsPermitted(GetComponent<ServerRoles>().Permissions, new PlayerPermissions[5]
		{
			PlayerPermissions.BanningUpToDay,
			PlayerPermissions.LongTermBanning,
			PlayerPermissions.PermissionsManagement,
			PlayerPermissions.SetGroup,
			PlayerPermissions.ForceclassWithoutRestrictions
		})))
		{
			TargetConsolePrint(base.connectionToClient, "Extended server configuration:\nServer name: " + serverConfig.GetString("server_name", string.Empty) + "\nServer IP: " + serverConfig.GetString("server_ip", string.Empty) + "\nCurrent Server IP: " + CustomNetworkManager.Ip + "\nServer pastebin ID: " + serverConfig.GetString("serverinfo_pastebin_id", string.Empty) + "\nServer max players: " + serverConfig.GetInt("max_players") + "\nOnline mode: " + serverConfig.GetBool("online_mode") + "\nRA password authentication: " + GetComponent<QueryProcessor>().OverridePasswordEnabled + "\nNoclip protection enabled: " + GetComponent<AntiFakeCommands>().NoclipProtection + "\nIP banning: " + serverConfig.GetBool("ip_banning") + "\nWhitelist: " + serverConfig.GetBool("enable_whitelist") + "\nQuery status: " + serverConfig.GetBool("enable_query") + " with port shift " + serverConfig.GetInt("query_port_shift") + "\nFriendly fire: " + ServerConsole.FriendlyFire + "\nMap seed: " + serverConfig.GetInt("map_seed"), "green");
		}
		else
		{
			TargetConsolePrint(base.connectionToClient, "Basic server configuration:\nServer name: " + serverConfig.GetString("server_name", string.Empty) + "\nServer IP: " + serverConfig.GetString("server_ip", string.Empty) + "\nServer pastebin ID: " + serverConfig.GetString("serverinfo_pastebin_id", string.Empty) + "\nServer max players: " + serverConfig.GetInt("max_players") + "\nRA password authentication: " + GetComponent<QueryProcessor>().OverridePasswordEnabled + "\nOnline mode: " + serverConfig.GetBool("online_mode") + "\nWhitelist: " + serverConfig.GetBool("enable_whitelist") + "\nFriendly fire: " + ServerConsole.FriendlyFire + "\nMap seed: " + serverConfig.GetInt("map_seed"), "green");
		}
	}

	[Command(channel = 2)]
	public void CmdRequestServerGroups()
	{
		string text = "Groups defined on this server:";
		Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
		ServerRoles.NamedColor[] namedColors = GetComponent<ServerRoles>().NamedColors;
		foreach (KeyValuePair<string, UserGroup> permentry in allGroups)
		{
			try
			{
				if (namedColors != null)
				{
					string text2 = text;
					object[] obj = new object[11]
					{
						text2,
						"\n",
						permentry.Key,
						" (",
						permentry.Value.Permissions,
						") - <color=#",
						null,
						null,
						null,
						null,
						null
					};
					ServerRoles.NamedColor namedColor = namedColors.FirstOrDefault((ServerRoles.NamedColor x) => x.Name == permentry.Value.BadgeColor);
					obj[6] = ((namedColor != null) ? namedColor.ColorHex : null);
					obj[7] = ">";
					obj[8] = permentry.Value.BadgeText;
					obj[9] = "</color> in color ";
					obj[10] = permentry.Value.BadgeColor;
					text = string.Concat(obj);
				}
			}
			catch
			{
				string text2 = text;
				text = text2 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
			{
				text += " B1";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
			{
				text += " B2";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
			{
				text += " B3";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
			{
				text += " FSE";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
			{
				text += " FSP";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
			{
				text += " FWR";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
			{
				text += " GIV";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
			{
				text += " EWA";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
			{
				text += " ERE";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
			{
				text += " ERO";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
			{
				text += " SGR";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GameplayData))
			{
				text += " GMD";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Overwatch))
			{
				text += " OVR";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
			{
				text += " FCM";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayersManagement))
			{
				text += " PLM";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PermissionsManagement))
			{
				text += " PRM";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConsoleCommands))
			{
				text += " SCC";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenBadges))
			{
				text += " VHB";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConfigs))
			{
				text += " CFG";
			}
		}
		TargetConsolePrint(base.connectionToClient, "Defined groups on server " + text, "grey");
	}

	[Command(channel = 2)]
	public void CmdRequestHideTag()
	{
		ServerRoles component = GetComponent<ServerRoles>();
		component.HiddenBadge = component.MyText;
		component.SetBadgeUpdate(string.Empty);
		component.SetText(string.Empty);
		component.SetColor("default");
		component.GlobalSet = false;
		component.RefreshHiddenTag();
		TargetConsolePrint(base.connectionToClient, "Badge hidden.", "green");
	}

	[Command(channel = 2)]
	public void CmdRequestShowTag(bool global)
	{
		ServerRoles component = GetComponent<ServerRoles>();
		if (global)
		{
			if (string.IsNullOrEmpty(component.PrevBadge))
			{
				TargetConsolePrint(base.connectionToClient, "You don't have global tag.", "magenta");
				return;
			}
			if ((string.IsNullOrEmpty(component.MyText) || !component.RemoteAdmin) && (((component.GlobalBadgeType == 3 || component.GlobalBadgeType == 4) && ConfigFile.ServerConfig.GetBool("block_gtag_banteam_badges") && !ServerStatic.PermissionsHandler.IsVerified) || (component.GlobalBadgeType == 1 && ConfigFile.ServerConfig.GetBool("block_gtag_staff_badges")) || (component.GlobalBadgeType == 2 && ConfigFile.ServerConfig.GetBool("block_gtag_management_badges") && !ServerStatic.PermissionsHandler.IsVerified) || (component.GlobalBadgeType == 0 && ConfigFile.ServerConfig.GetBool("block_gtag_patreon_badges") && !ServerStatic.PermissionsHandler.IsVerified)))
			{
				TargetConsolePrint(base.connectionToClient, "You can't show this type of global badge on this server. Try joining server with global badges allowed.", "red");
				return;
			}
			component.SetBadgeUpdate(component.PrevBadge);
			component.GlobalSet = true;
			component.HiddenBadge = string.Empty;
			component.CallRpcResetFixed();
			TargetConsolePrint(base.connectionToClient, "Global tag refreshed.", "green");
		}
		else
		{
			component.SetBadgeUpdate(string.Empty);
			component.HiddenBadge = string.Empty;
			component.CallRpcResetFixed();
			component.RefreshPermissions(true);
			TargetConsolePrint(base.connectionToClient, "Local tag refreshed.", "green");
		}
	}

	[Command]
	public void CmdSuicide(PlayerStats.HitInfo hitInfo)
	{
		hitInfo.amount = ((hitInfo.amount != 0f) ? hitInfo.amount : 999799f);
		GetComponent<PlayerStats>().HurtPlayer(hitInfo, base.gameObject);
	}

	public void ForceRoundStart()
	{
		if (NetworkServer.active)
		{
			ServerLogs.AddLog(ServerLogs.Modules.Logger, "Round has been started.", ServerLogs.ServerLogType.GameEvent);
			ServerConsole.AddLog("New round has been started.");
			RoundStart.singleton.Networkinfo = "started";
		}
	}

	[TargetRpc(channel = 2)]
	private void TargetSetDisconnectError(NetworkConnection conn, string message)
	{
		((CustomNetworkManager)NetworkManager.singleton).disconnectMessage = message;
		CallCmdConfirmDisconnect();
	}

	[Command(channel = 2)]
	private void CmdConfirmDisconnect()
	{
		if (base.connectionToClient != null && base.connectionToClient.isConnected)
		{
			base.connectionToClient.Disconnect();
			base.connectionToClient.Dispose();
		}
	}

	public void DisconnectClient(NetworkConnection conn, string message)
	{
		CallTargetSetDisconnectError(conn, message);
		Timing.RunCoroutine(_DisconnectAfterTimeout(conn), Segment.Update);
	}

	private IEnumerator<float> _DisconnectAfterTimeout(NetworkConnection conn)
	{
		yield return Timing.WaitForSeconds(3f);
		if (conn != null && conn.isConnected)
		{
			conn.Disconnect();
			conn.Dispose();
		}
	}

	public void InitSCPs()
	{
		if (curClass != -1 && !TutorialManager.status)
		{
			Class c = klasy[curClass];
			_scp049.Init(curClass, c);
			_scp0492.Init(curClass, c);
			_scp079.Init(curClass, c);
			_scp106.Init(curClass, c);
			_scp173.Init(curClass, c);
			_scp096.Init(curClass, c);
			_scp939.Init(curClass, c);
		}
	}

	public void RegisterEscape()
	{
		CallCmdRegisterEscape();
	}

	[Command(channel = 2)]
	private void CmdRegisterEscape()
	{
		if (!(Vector3.Distance(base.transform.position, GetComponent<Escape>().worldPosition) < (float)(GetComponent<Escape>().radius * 2)))
		{
			return;
		}
		CharacterClassManager component = GetComponent<CharacterClassManager>();
		bool flag = PlayerManager.singleton.players.Any(delegate(GameObject p)
		{
			object obj;
			if ((object)p == null)
			{
				obj = null;
			}
			else
			{
				Handcuffs component2 = p.GetComponent<Handcuffs>();
				obj = (((object)component2 != null) ? component2.cuffTarget : null);
			}
			return (UnityEngine.Object)obj == base.gameObject;
		});
		bool flag2 = ConfigFile.ServerConfig.GetBool("cuffed_escapee_change_team");
		switch (klasy[component.curClass].team)
		{
		case Team.CDP:
			if (flag2 && flag)
			{
				SetPlayersClass(13, base.gameObject);
				RoundSummary.escaped_scientists++;
			}
			else
			{
				SetPlayersClass(8, base.gameObject);
				RoundSummary.escaped_ds++;
			}
			break;
		case Team.RSC:
			if (flag2 && flag)
			{
				SetPlayersClass(8, base.gameObject);
				RoundSummary.escaped_ds++;
			}
			else
			{
				SetPlayersClass(4, base.gameObject);
				RoundSummary.escaped_scientists++;
			}
			break;
		}
	}

	public bool IsScpButNotZombie()
	{
		return curClass >= 0 && curClass != 10 && klasy[curClass].team == Team.SCP;
	}

	public void ApplyProperties(bool lite = false)
	{
		Class obj = klasy[curClass];
		InitSCPs();
		aliveTime = 0f;
		if (curClass != 2)
		{
			_wasAnytimeAlive = true;
		}
		switch (obj.team)
		{
		case Team.MTF:
			AchievementManager.Achieve("arescue");
			break;
		case Team.CHI:
			AchievementManager.Achieve("chaos");
			break;
		case Team.RSC:
		case Team.CDP:
			EscapeStartTime = (int)Time.realtimeSinceStartup;
			break;
		}
		Inventory component = GetComponent<Inventory>();
		PlyMovementSync component2 = GetComponent<PlyMovementSync>();
		try
		{
			GetComponent<FootstepSync>().SetLoundness(obj.team, obj.fullName.Contains("939"));
		}
		catch
		{
		}
		PlayerManager.localPlayer.GetComponent<SpectatorManager>().RefreshList();
		if (base.isLocalPlayer)
		{
			GetComponent<FirstPersonController>().isSCP = obj.team == Team.SCP;
			DiscordManager.singleton.ChangePreset(curClass);
			GetComponent<Radio>().UpdateClass();
			GetComponent<Handcuffs>().CallCmdTarget(null);
			GetComponent<WeaponManager>().flashlightEnabled = true;
			GetComponent<Searching>().Init((obj.team == Team.SCP) | (obj.team == Team.RIP));
		}
		if (obj.team == Team.RIP)
		{
			if (base.isServer)
			{
				component2.SetPosition(new Vector3(0f, 2048f, 0f));
				component2.SetRotation(0f);
			}
			if (base.isLocalPlayer)
			{
				component.NetworkcurItem = -1;
				GetComponent<FirstPersonController>().enabled = false;
				if (curClass != 2 || Radio.roundStarted)
				{
					if (_wasAnytimeAlive)
					{
						CallCmdRequestDeathScreen();
					}
					else
					{
						UnityEngine.Object.FindObjectOfType<StartScreen>().PlayAnimation(curClass);
					}
					GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(bell_dead);
				}
				GetComponent<PlayerStats>().maxHP = obj.maxHP;
				unfocusedCamera.GetComponent<Camera>().enabled = false;
				unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = false;
			}
		}
		else
		{
			if (NetworkServer.active && !lite)
			{
				GameObject gameObject = null;
				Vector3 constantRespawnPoint = NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
				if (constantRespawnPoint != Vector3.zero)
				{
					component2.SetPosition(constantRespawnPoint);
				}
				else
				{
					gameObject = UnityEngine.Object.FindObjectOfType<SpawnpointManager>().GetRandomPosition(curClass);
					if (gameObject != null)
					{
						component2.SetPosition(gameObject.transform.position);
						component2.SetRotation(gameObject.transform.rotation.eulerAngles.y);
					}
					else
					{
						component2.SetPosition(deathPosition);
					}
				}
				if (!SpawnProtected && EnableSP && SProtectedTeam.Contains((int)obj.team))
				{
					GodMode = true;
					SpawnProtected = true;
					ProtectedTime = Time.time;
				}
			}
			if (base.isLocalPlayer)
			{
				GetComponent<Scp106PlayerScript>().SetDoors();
				component.NetworkcurItem = -1;
				if (curClass != 7)
				{
					UnityEngine.Object.FindObjectOfType<StartScreen>().PlayAnimation(curClass);
					if (!GetComponent<HorrorSoundController>().horrorSoundSource.isPlaying)
					{
						GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(bell);
					}
				}
				Invoke("EnableFPC", 0.2f);
				GetComponent<Radio>().ResetPreset();
				GetComponent<Radio>().Invoke("ResetPreset", 2f);
				FirstPersonController component3 = GetComponent<FirstPersonController>();
				PlayerStats component4 = GetComponent<PlayerStats>();
				unfocusedCamera.GetComponent<Camera>().enabled = true;
				unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
				component3.m_WalkSpeed = obj.walkSpeed;
				component3.m_RunSpeed = obj.runSpeed;
				component3.m_UseHeadBob = obj.useHeadBob;
				component3.m_JumpSpeed = obj.jumpSpeed;
				int num = (component4.maxHP = obj.maxHP);
				UnityEngine.Object.FindObjectOfType<UserMainInterface>().lerpedHP = num;
				SkyboxFollower.iAm939 = obj.fullName.Contains("939");
			}
			else
			{
				GetComponent<PlayerStats>().maxHP = obj.maxHP;
			}
		}
		_scp049.iAm049 = curClass == 5;
		_scp0492.iAm049_2 = curClass == 10;
		_scp096.iAm096 = curClass == 9;
		_scp106.iAm106 = curClass == 3;
		_scp173.iAm173 = curClass == 0;
		_scp939.iAm939 = curClass >= 0 && curClass < klasy.Length && klasy[curClass].fullName.Contains("939");
		if (base.isLocalPlayer)
		{
			UnityEngine.Object.FindObjectOfType<InventoryDisplay>().isSCP = (curClass == 2) | (obj.team == Team.SCP);
			UnityEngine.Object.FindObjectOfType<InterfaceColorAdjuster>().ChangeColor((curClass != 7) ? obj.classColor : Color.clear);
		}
		QueryProcessor.StaticRefreshPlayerList();
		RefreshPlyModel();
	}

	private void EnableFPC()
	{
		GetComponent<FirstPersonController>().enabled = true;
	}

	public void RefreshPlyModel(int classID = -1)
	{
		GetComponent<AnimationController>().OnChangeClass();
		if (myModel != null)
		{
			UnityEngine.Object.Destroy(myModel);
		}
		Class obj = klasy[(classID >= 0) ? classID : curClass];
		if (obj.team != Team.RIP)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.model_player);
			gameObject.transform.SetParent(base.gameObject.transform);
			gameObject.transform.localPosition = obj.model_offset.position;
			gameObject.transform.localRotation = Quaternion.Euler(obj.model_offset.rotation);
			gameObject.transform.localScale = obj.model_offset.scale;
			myModel = gameObject;
			if (myModel.GetComponent<Animator>() != null)
			{
				GetComponent<AnimationController>().animator = myModel.GetComponent<Animator>();
			}
			if (base.isLocalPlayer)
			{
				if (myModel.GetComponent<Renderer>() != null)
				{
					myModel.GetComponent<Renderer>().enabled = false;
				}
				Renderer[] componentsInChildren = myModel.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = false;
				}
				Collider[] componentsInChildren2 = myModel.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren2)
				{
					if (collider.name != "LookingTarget")
					{
						collider.enabled = false;
					}
				}
			}
		}
		GetComponent<CapsuleCollider>().enabled = obj.team != Team.RIP;
		if (myModel != null)
		{
			GetComponent<WeaponManager>().hitboxes = myModel.GetComponentsInChildren<HitboxIdentity>(true);
		}
	}

	public void SetClassID(int id)
	{
		SetClassIDAdv(id, false);
	}

	public void SetClassIDAdv(int id, bool lite)
	{
		if ((!IsVerified && id != 2) || (id == 14 && ServerStatic.IsDedicated && !ConfigFile.ServerConfig.GetBool("allow_playing_as_tutorial", true)))
		{
			return;
		}
		if (GetComponent<ServerRoles>().OverwatchEnabled && id != 2)
		{
			if (curClass == 2)
			{
				return;
			}
			id = 2;
		}
		DeathTime = ((id != 2) ? 0 : DateTime.UtcNow.Ticks);
		NetworkcurClass = id;
		bool flag = id == 2;
		if (NetworkServer.active)
		{
			if (flag)
			{
				GameObject[] players = PlayerManager.singleton.players;
				foreach (GameObject gameObject in players)
				{
					PlayerStats component = gameObject.GetComponent<PlayerStats>();
					component.CallTargetSyncHp(base.connectionToClient, component.health);
				}
			}
			else
			{
				GameObject[] players2 = PlayerManager.singleton.players;
				foreach (GameObject gameObject2 in players2)
				{
					if (gameObject2.GetComponent<CharacterClassManager>() != this)
					{
						gameObject2.GetComponent<PlayerStats>().CallTargetSyncHp(base.connectionToClient, -1);
					}
				}
			}
			GetComponent<PlayerStats>().MakeHpDirty();
		}
		if (!flag || base.isLocalPlayer)
		{
			aliveTime = 0f;
			ApplyProperties(lite);
		}
	}

	public void InstantiateRagdoll(int id)
	{
		if (id >= 0)
		{
			Class obj = klasy[curClass];
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.model_ragdoll);
			gameObject.transform.position = base.transform.position + obj.ragdoll_offset.position;
			gameObject.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles + obj.ragdoll_offset.rotation);
			gameObject.transform.localScale = obj.ragdoll_offset.scale;
		}
	}

	public void SetRandomRoles()
	{
		MTFRespawn component = GetComponent<MTFRespawn>();
		int forcedClass = NonFacilityCompatibility.currentSceneSettings.forcedClass;
		if (base.isLocalPlayer && base.isServer)
		{
			GameObject[] array = GetShuffledPlayerList().ToArray();
			RoundSummary component2 = GetComponent<RoundSummary>();
			bool flag = (float)UnityEngine.Random.Range(0, 100) < ciPercentage;
			RoundSummary.SumInfo_ClassList startClassList = default(RoundSummary.SumInfo_ClassList);
			bool flag2 = false;
			int num = 0;
			float[] array2 = new float[4] { 0f, 0.4f, 0.6f, 0.5f };
			for (int i = 0; i < array.Length; i++)
			{
				int num2 = ((forceClass >= 0) ? forceClass : Find_Random_ID_Using_Defined_Team(classTeamQueue[i]));
				if (forcedClass >= 0)
				{
					num2 = forcedClass;
				}
				switch (klasy[num2].team)
				{
				case Team.CDP:
					startClassList.class_ds++;
					break;
				case Team.CHI:
					startClassList.chaos_insurgents++;
					break;
				case Team.MTF:
					startClassList.mtf_and_guards++;
					break;
				case Team.RSC:
					startClassList.scientists++;
					break;
				case Team.SCP:
					startClassList.scps_except_zombies++;
					break;
				}
				if (klasy[num2].team == Team.SCP && !flag2)
				{
					if (array2[Mathf.Clamp(num, 0, array2.Length)] > UnityEngine.Random.value)
					{
						flag2 = true;
						num2 = 7;
					}
					num++;
				}
				if (TutorialManager.status)
				{
					SetPlayersClass(14, base.gameObject);
					continue;
				}
				SetPlayersClass(num2, array[i]);
				ServerLogs.AddLog(ServerLogs.Modules.ClassChange, array[i].GetComponent<NicknameSync>().myNick + " (" + array[i].GetComponent<CharacterClassManager>().SteamId + ") spawned as " + klasy[num2].fullName.Replace("\n", string.Empty) + ".", ServerLogs.ServerLogType.GameEvent);
			}
			UnityEngine.Object.FindObjectOfType<PlayerList>().NetworkRoundStartTime = (int)Time.realtimeSinceStartup;
			startClassList.time = (int)Time.realtimeSinceStartup;
			startClassList.warhead_kills = -1;
			UnityEngine.Object.FindObjectOfType<RoundSummary>().SetStartClassList(startClassList);
			if (ConfigFile.ServerConfig.GetBool("smart_class_picker", true))
			{
				RunSmartClassPicker();
			}
		}
		if (NetworkServer.active)
		{
			Timing.RunCoroutine(MakeSureToSetHP(), Segment.Update);
		}
	}

	private List<GameObject> GetShuffledPlayerList()
	{
		List<GameObject> list = new List<GameObject>(PlayerManager.singleton.players);
		if (ConfigFile.ServerConfig.GetBool("use_crypto_rng"))
		{
			Misc.ShuffleListSecure(list);
		}
		else
		{
			Misc.ShuffleList(list);
		}
		return list;
	}

	[Command]
	private void CmdRequestDeathScreen()
	{
		CallTargetDeathScreen(base.connectionToClient, GetComponent<PlayerStats>().lastHitInfo);
	}

	[TargetRpc]
	private void TargetDeathScreen(NetworkConnection conn, PlayerStats.HitInfo hitinfo)
	{
		UnityEngine.Object.FindObjectOfType<YouWereKilled>().Play(hitinfo);
	}

	private void RunSmartClassPicker()
	{
		string text = "Before Starting";
		try
		{
			text = "Setting Initial Value";
			if (ConfigFile.smBalancedPicker == null)
			{
				ConfigFile.smBalancedPicker = new Dictionary<string, int[]>();
			}
			text = "Valid Players List Error";
			List<GameObject> shuffledPlayerList = GetShuffledPlayerList();
			text = "Copying Balanced Picker List";
			Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(ConfigFile.smBalancedPicker);
			text = "Clearing Balanced Picker List";
			ConfigFile.smBalancedPicker.Clear();
			text = "Re-building Balanced Picker List";
			foreach (GameObject item in shuffledPlayerList)
			{
				if (item == null)
				{
					continue;
				}
				NetworkConnection component = item.GetComponent<NetworkConnection>();
				CharacterClassManager component2 = item.GetComponent<CharacterClassManager>();
				text = "Getting Player ID";
				if (component == null && component2 == null)
				{
					shuffledPlayerList.Remove(item);
					break;
				}
				if (GetComponent<ServerRoles>().DoNotTrack)
				{
					shuffledPlayerList.Remove(item);
					continue;
				}
				string text2 = ((component == null) ? string.Empty : component.address);
				string text3 = ((!(component2 != null)) ? string.Empty : component2.SteamId);
				string text4 = text2 + text3;
				text = "Setting up Player \"" + text4 + "\"";
				if (!dictionary.ContainsKey(text4))
				{
					text = "Adding Player \"" + text4 + "\" to smBalancedPicker";
					int[] array = new int[klasy.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = ConfigFile.ServerConfig.GetInt("smart_cp_starting_weight", 6);
					}
					ConfigFile.smBalancedPicker.Add(text4, array);
				}
				else
				{
					text = "Updating Player \"" + text4 + "\" in smBalancedPicker";
					int[] value;
					if (dictionary.TryGetValue(text4, out value))
					{
						ConfigFile.smBalancedPicker.Add(text4, value);
					}
				}
			}
			text = "Clearing Copied Balanced Picker List";
			dictionary.Clear();
			List<int> list = new List<int>();
			text = "Getting Available Roles";
			if (shuffledPlayerList.Contains(null))
			{
				shuffledPlayerList.Remove(null);
			}
			foreach (GameObject item2 in shuffledPlayerList)
			{
				if (!(item2 == null))
				{
					CharacterClassManager component3 = item2.GetComponent<CharacterClassManager>();
					if (component3 != null)
					{
						list.Add(component3.curClass);
					}
					else
					{
						shuffledPlayerList.Remove(item2);
					}
				}
			}
			List<GameObject> list2 = new List<GameObject>();
			text = "Setting Roles";
			foreach (GameObject item3 in shuffledPlayerList)
			{
				if (!(item3 == null))
				{
					NetworkConnection component4 = item3.GetComponent<NetworkConnection>();
					CharacterClassManager component5 = item3.GetComponent<CharacterClassManager>();
					if (component4 == null && component5 == null)
					{
						shuffledPlayerList.Remove(item3);
						break;
					}
					string text5 = ((component4 == null) ? string.Empty : component4.address);
					string text6 = ((!(component5 != null)) ? string.Empty : component5.SteamId);
					string text7 = text5 + text6;
					text = "Setting Player \"" + text7 + "\"'s Class";
					int mostLikelyClass = GetMostLikelyClass(text7, list);
					if (mostLikelyClass != -1)
					{
						SetPlayersClass(mostLikelyClass, item3);
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, item3.GetComponent<NicknameSync>().myNick + " (" + item3.GetComponent<CharacterClassManager>().SteamId + ") class set to " + klasy[mostLikelyClass].fullName.Replace("\n", string.Empty) + " by Smart Class Picker.", ServerLogs.ServerLogType.GameEvent);
						list.Remove(mostLikelyClass);
					}
					else
					{
						list2.Add(item3);
					}
				}
			}
			text = "Reversing Additional Classes List";
			list.Reverse();
			text = "Setting Unknown Players Classes";
			foreach (GameObject item4 in list2)
			{
				if (!(item4 == null))
				{
					if (list.Count > 0)
					{
						int num = list[0];
						SetPlayersClass(num, item4);
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, item4.GetComponent<NicknameSync>().myNick + " (" + item4.GetComponent<CharacterClassManager>().SteamId + ") class set to " + klasy[num].fullName.Replace("\n", string.Empty) + " by Smart Class Picker.", ServerLogs.ServerLogType.GameEvent);
						list.Remove(num);
					}
					else
					{
						SetPlayersClass(2, item4);
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, item4.GetComponent<NicknameSync>().myNick + " (" + item4.GetComponent<CharacterClassManager>().SteamId + ") class set to SPECTATOR by Smart Class Picker.", ServerLogs.ServerLogType.GameEvent);
					}
				}
			}
			text = "Clearing Unknown Players List";
			list2.Clear();
			text = "Clearing Available Classes List";
			list.Clear();
		}
		catch (Exception ex)
		{
			GameConsole.Console.singleton.AddLog("Smart Class Picker Failed: " + text + ", " + ex.Message, new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
		}
	}

	private int GetMostLikelyClass(string playerUuid, List<int> availableClasses)
	{
		int[] value = null;
		int num = -1;
		if (availableClasses.Count <= 0 || !ConfigFile.smBalancedPicker.TryGetValue(playerUuid, out value) || value == null || value.Length != klasy.Length)
		{
			return num;
		}
		if (!ContainsPossibleClass(value, availableClasses))
		{
			return num;
		}
		int num2 = 0;
		int[] array = (int[])value.Clone();
		for (int i = 0; i < array.Length; i++)
		{
			num2 = (array[i] = num2 + array[i]);
		}
		while (!availableClasses.Contains(num))
		{
			int num3 = UnityEngine.Random.Range(0, num2);
			for (int j = 0; j < array.Length; j++)
			{
				if (num3 < array[j])
				{
					num = j;
					break;
				}
			}
		}
		if (num < 0 || num >= klasy.Length)
		{
			return -1;
		}
		UpdateClassChances(num, value);
		return num;
	}

	private bool ContainsPossibleClass(int[] classChances, List<int> availableClasses)
	{
		foreach (int availableClass in availableClasses)
		{
			if (availableClass < 0 || availableClass >= classChances.Length || classChances[availableClass] <= 0)
			{
				continue;
			}
			return true;
		}
		return false;
	}

	private void UpdateClassChances(int classChoice, int[] classChances)
	{
		int num = ConfigFile.ServerConfig.GetInt("smart_cp_weight_min", 1);
		num = ((num < 0) ? 1 : num);
		int num2 = ConfigFile.ServerConfig.GetInt("smart_cp_weight_max", 11);
		num2 = ((num2 >= num) ? num2 : (num + 10));
		for (int i = 0; i < classChances.Length; i++)
		{
			bool flag = false;
			bool flag2 = false;
			if (ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_decrease"), -99) != -99 && klasy[i].team == klasy[classChoice].team)
			{
				classChances[i] -= ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_decrease"));
				flag2 = true;
			}
			else if (ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_increase"), -99) != -99 && klasy[i].team != klasy[classChoice].team)
			{
				classChances[i] += ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_increase"));
				flag = true;
			}
			if (ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_decrease", -99) != -99 && i == classChoice && !flag)
			{
				classChances[i] -= ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_decrease", 3);
			}
			else if (ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_increase", -99) != -99 && i != classChoice && !flag2)
			{
				classChances[i] += ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_increase", 1);
			}
			else if (!flag && !flag2)
			{
				if (klasy[classChoice].team == Team.MTF && klasy[classChoice].team == klasy[i].team)
				{
					classChances[i] -= 2;
					if (i == classChoice)
					{
						classChances[i] -= 2;
					}
				}
				else if (klasy[classChoice].team == Team.CDP && klasy[classChoice].team == klasy[i].team)
				{
					classChances[i] -= 3;
				}
				else if (klasy[classChoice].team == Team.SCP && klasy[classChoice].team == klasy[i].team)
				{
					classChances[i] -= 2;
					if (i == classChoice)
					{
						classChances[i]--;
					}
				}
				else if (i == classChoice)
				{
					classChances[i] -= 2;
				}
				else
				{
					classChances[i]++;
				}
			}
			classChances[i] = Mathf.Clamp(classChances[i], num, num2);
		}
	}

	private void SetRoundStart(bool b)
	{
		NetworkroundStarted = b;
	}

	[ServerCallback]
	private void CmdStartRound()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (!TutorialManager.status)
		{
			try
			{
				Door componentInChildren = GameObject.Find("MeshDoor173").GetComponentInChildren<Door>();
				componentInChildren.ForceCooldown(25f);
				UnityEngine.Object.FindObjectOfType<ChopperAutostart>().SetState(false);
			}
			catch
			{
			}
		}
		SetRoundStart(true);
	}

	[ServerCallback]
	public void SetPlayersClass(int classid, GameObject ply, bool lite = false)
	{
		if (!NetworkServer.active || !ply.GetComponent<CharacterClassManager>().IsVerified)
		{
			return;
		}
		ply.GetComponent<CharacterClassManager>().SetClassIDAdv(classid, lite);
		ply.GetComponent<PlayerStats>().SetHPAmount(klasy[classid].maxHP);
		if (lite)
		{
			return;
		}
		foreach (Handcuffs item in from p in PlayerManager.singleton.players
			where p.GetComponent<Handcuffs>() != null
			select ((object)p != null) ? p.GetComponent<Handcuffs>() : null)
		{
			if ((((object)item != null) ? item.cuffTarget : null) == ply)
			{
				item.NetworkcuffTarget = null;
			}
		}
		Inventory component = ply.GetComponent<Inventory>();
		ply.GetComponent<AmmoBox>().SetAmmoAmount();
		component.items.Clear();
		int[] startItems = klasy[Mathf.Clamp(classid, 0, klasy.Length - 1)].startItems;
		foreach (int id in startItems)
		{
			component.AddNewItem(id);
		}
	}

	private IEnumerator<float> MakeSureToSetHP()
	{
		for (int i = 0; i < 7; i++)
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				PlayerStats component2 = gameObject.GetComponent<PlayerStats>();
				if (component2.health <= klasy[component.curClass].maxHP)
				{
					component2.SetHPAmount(klasy[component.curClass].maxHP);
				}
			}
			yield return Timing.WaitForSeconds(1f);
		}
	}

	private int Find_Random_ID_Using_Defined_Team(Team team)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < klasy.Length; i++)
		{
			if (klasy[i].team == team && !klasy[i].banClass)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return 1;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		if (klasy[list[index]].team == Team.SCP)
		{
			klasy[list[index]].banClass = true;
		}
		return list[index];
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSendToken(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendToken called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdSendToken(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdRequestContactEmail(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestContactEmail called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestContactEmail();
		}
	}

	protected static void InvokeCmdCmdRequestServerConfig(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestServerConfig called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestServerConfig();
		}
	}

	protected static void InvokeCmdCmdRequestServerGroups(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestServerGroups called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestServerGroups();
		}
	}

	protected static void InvokeCmdCmdRequestHideTag(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHideTag called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestHideTag();
		}
	}

	protected static void InvokeCmdCmdRequestShowTag(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestShowTag called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestShowTag(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSuicide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSuicide called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdSuicide(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	protected static void InvokeCmdCmdConfirmDisconnect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmDisconnect called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdConfirmDisconnect();
		}
	}

	protected static void InvokeCmdCmdRegisterEscape(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRegisterEscape called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRegisterEscape();
		}
	}

	protected static void InvokeCmdCmdRequestDeathScreen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestDeathScreen called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestDeathScreen();
		}
	}

	public void CallCmdSendToken(string token)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendToken called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendToken(token);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendToken);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(token);
		SendCommandInternal(networkWriter, 2, "CmdSendToken");
	}

	public void CallCmdRequestContactEmail()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestContactEmail called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestContactEmail();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestContactEmail);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestContactEmail");
	}

	public void CallCmdRequestServerConfig()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestServerConfig called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestServerConfig();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestServerConfig);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestServerConfig");
	}

	public void CallCmdRequestServerGroups()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestServerGroups called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestServerGroups();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestServerGroups);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestServerGroups");
	}

	public void CallCmdRequestHideTag()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestHideTag called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestHideTag();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestHideTag);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestHideTag");
	}

	public void CallCmdRequestShowTag(bool global)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestShowTag called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestShowTag(global);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestShowTag);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(global);
		SendCommandInternal(networkWriter, 2, "CmdRequestShowTag");
	}

	public void CallCmdSuicide(PlayerStats.HitInfo hitInfo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSuicide called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSuicide(hitInfo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSuicide);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, hitInfo);
		SendCommandInternal(networkWriter, 0, "CmdSuicide");
	}

	public void CallCmdConfirmDisconnect()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdConfirmDisconnect called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdConfirmDisconnect();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdConfirmDisconnect);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdConfirmDisconnect");
	}

	public void CallCmdRegisterEscape()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRegisterEscape called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRegisterEscape();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRegisterEscape);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRegisterEscape");
	}

	public void CallCmdRequestDeathScreen()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestDeathScreen called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestDeathScreen();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestDeathScreen);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdRequestDeathScreen");
	}

	protected static void InvokeRpcRpcPlaceBlood(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceBlood called on server.");
		}
		else
		{
			((CharacterClassManager)obj).RpcPlaceBlood(reader.ReadVector3(), (int)reader.ReadPackedUInt32(), reader.ReadSingle());
		}
	}

	protected static void InvokeRpcTargetChangeCmdBinding(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetChangeCmdBinding called on server.");
		}
		else
		{
			((CharacterClassManager)obj).TargetChangeCmdBinding(ClientScene.readyConnection, (KeyCode)reader.ReadInt32(), reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetSetDisconnectError(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetDisconnectError called on server.");
		}
		else
		{
			((CharacterClassManager)obj).TargetSetDisconnectError(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetDeathScreen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetDeathScreen called on server.");
		}
		else
		{
			((CharacterClassManager)obj).TargetDeathScreen(ClientScene.readyConnection, GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	public void CallRpcPlaceBlood(Vector3 pos, int type, float f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaceBlood called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaceBlood);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(pos);
		networkWriter.WritePackedUInt32((uint)type);
		networkWriter.Write(f);
		SendRPCInternal(networkWriter, 0, "RpcPlaceBlood");
	}

	public void CallTargetChangeCmdBinding(NetworkConnection connection, KeyCode code, string cmd)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetChangeCmdBinding called on client.");
			return;
		}
       if (connection.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetChangeCmdBinding called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetChangeCmdBinding);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)code);
		networkWriter.Write(cmd);
		SendTargetRPCInternal(connection, networkWriter, 0, "TargetChangeCmdBinding");
	}

	public void CallTargetSetDisconnectError(NetworkConnection conn, string message)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetDisconnectError called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSetDisconnectError called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetDisconnectError);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(message);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetSetDisconnectError");
	}

	public void CallTargetDeathScreen(NetworkConnection conn, PlayerStats.HitInfo hitinfo)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetDeathScreen called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetDeathScreen called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetDeathScreen);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, hitinfo);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetDeathScreen");
	}

	static CharacterClassManager()
	{
		kCmdCmdSendToken = 970325235;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdSendToken, InvokeCmdCmdSendToken);
		kCmdCmdRequestContactEmail = 2054299309;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestContactEmail, InvokeCmdCmdRequestContactEmail);
		kCmdCmdRequestServerConfig = -2046741578;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestServerConfig, InvokeCmdCmdRequestServerConfig);
		kCmdCmdRequestServerGroups = -1929409976;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestServerGroups, InvokeCmdCmdRequestServerGroups);
		kCmdCmdRequestHideTag = -1886885625;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestHideTag, InvokeCmdCmdRequestHideTag);
		kCmdCmdRequestShowTag = -732213908;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestShowTag, InvokeCmdCmdRequestShowTag);
		kCmdCmdSuicide = -1051695024;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdSuicide, InvokeCmdCmdSuicide);
		kCmdCmdConfirmDisconnect = -1987348706;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdConfirmDisconnect, InvokeCmdCmdConfirmDisconnect);
		kCmdCmdRegisterEscape = -1826587486;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRegisterEscape, InvokeCmdCmdRegisterEscape);
		kCmdCmdRequestDeathScreen = -1840245105;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestDeathScreen, InvokeCmdCmdRequestDeathScreen);
		kRpcRpcPlaceBlood = 1372291111;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kRpcRpcPlaceBlood, InvokeRpcRpcPlaceBlood);
		kTargetRpcTargetChangeCmdBinding = -2104928626;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kTargetRpcTargetChangeCmdBinding, InvokeRpcTargetChangeCmdBinding);
		kTargetRpcTargetSetDisconnectError = -2047672291;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kTargetRpcTargetSetDisconnectError, InvokeRpcTargetSetDisconnectError);
		kTargetRpcTargetDeathScreen = -520196787;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kTargetRpcTargetDeathScreen, InvokeRpcTargetDeathScreen);
		NetworkCRC.RegisterBehaviour("CharacterClassManager", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(Muted);
			writer.Write(IntercomMuted);
			writer.WritePackedUInt32((uint)curClass);
			writer.Write(deathPosition);
			writer.WritePackedUInt32((uint)ntfUnit);
			writer.Write(roundStarted);
			writer.Write(IsVerified);
			writer.Write(SteamId);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(Muted);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(IntercomMuted);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)curClass);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(deathPosition);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)ntfUnit);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(roundStarted);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(IsVerified);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(SteamId);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			Muted = reader.ReadBoolean();
			IntercomMuted = reader.ReadBoolean();
			curClass = (int)reader.ReadPackedUInt32();
			deathPosition = reader.ReadVector3();
			ntfUnit = (int)reader.ReadPackedUInt32();
			roundStarted = reader.ReadBoolean();
			IsVerified = reader.ReadBoolean();
			SteamId = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetMuted(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			IntercomMuted = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			SetClassID((int)reader.ReadPackedUInt32());
		}
		if ((num & 8) != 0)
		{
			SyncDeathPos(reader.ReadVector3());
		}
		if ((num & 0x10) != 0)
		{
			SetUnit((int)reader.ReadPackedUInt32());
		}
		if ((num & 0x20) != 0)
		{
			SetRoundStart(reader.ReadBoolean());
		}
		if ((num & 0x40) != 0)
		{
			SetVerification(reader.ReadBoolean());
		}
		if ((num & 0x80) != 0)
		{
			SetSteamId(reader.ReadString());
		}
	}
}
