using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cryptography;
using GameConsole;
using MEC;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class ServerRoles : NetworkBehaviour
{
	[Serializable]
	public class NamedColor
	{
		public string Name;

		public Gradient SpeakingColorIn;

		public Gradient SpeakingColorOut;

		public string ColorHex;

		public bool Restricted;
	}

	[Serializable]
	public enum AccessMode
	{
		LocalAccess = 1,
		GlobalAccess = 2,
		PasswordOverride = 3
	}

	public NamedColor CurrentColor;

	public NamedColor[] NamedColors;

	public Dictionary<string, string> FirstVerResult;

	internal AsymmetricKeyParameter PublicKey;

	public bool AuthroizeBadge;

	public bool BypassMode;

	public bool LocalRemoteAdmin;

	internal bool OverwatchPermitted;

	internal bool OverwatchEnabled;

	internal bool AmIInOverwatch;

	private bool _requested;

	private bool _badgeRequested;

	private bool _authRequested;

	internal string PrevBadge;

	private string _globalBadgeUnconfirmed;

	private string _prevColor;

	private string _prevText;

	private string _prevBadge;

	private string _badgeUserChallenge;

	private string _authChallenge;

	private string _badgeChallenge;

	private string _bgc;

	private string _bgt;

	[SyncVar(hook = "SetColor")]
	public string MyColor;

	[SyncVar(hook = "SetText")]
	public string MyText;

	[SyncVar(hook = "SetBadgeUpdate")]
	public string GlobalBadge;

	public bool GlobalSet;

	public bool BadgeCover;

	public string FixedBadge;

	public int GlobalBadgeType;

	public bool RemoteAdmin;

	public bool Staff;

	public bool BypassStaff;

	public bool RaEverywhere;

	public ulong Permissions;

	public string HiddenBadge;

	public bool DoNotTrack;

	public AccessMode RemoteAdminMode;

	private static int kTargetRpcTargetSetHiddenRole;

	private static int kRpcRpcResetFixed;

	private static int kCmdCmdRequestBadge;

	private static int kCmdCmdDoNotTrack;

	private static int kTargetRpcTargetSignServerChallenge;

	private static int kCmdCmdServerSignatureComplete;

	private static int kTargetRpcTargetOpenRemoteAdmin;

	private static int kTargetRpcTargetCloseRemoteAdmin;

	private static int kCmdCmdSetOverwatchStatus;

	private static int kCmdCmdToggleOverwatch;

	private static int kTargetRpcTargetSetOverwatch;

	public string NetworkMyColor
	{
		get
		{
			return MyColor;
		}
		[param: In]
		set
		{
			ServerRoles serverRoles = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetColor(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref serverRoles.MyColor, 1u);
		}
	}

	public string NetworkMyText
	{
		get
		{
			return MyText;
		}
		[param: In]
		set
		{
			ServerRoles serverRoles = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetText(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref serverRoles.MyText, 2u);
		}
	}

	public string NetworkGlobalBadge
	{
		get
		{
			return GlobalBadge;
		}
		[param: In]
		set
		{
			ServerRoles serverRoles = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetBadgeUpdate(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref serverRoles.GlobalBadge, 4u);
		}
	}

	public void Start()
	{
		if (base.isLocalPlayer && GameConsole.Console.RequestDNT)
		{
			GameConsole.Console.singleton.AddLog("Sending \"Do not track\" request to the game server...", Color.grey);
			CallCmdDoNotTrack();
		}
	}

	[TargetRpc(channel = 2)]
	public void TargetSetHiddenRole(NetworkConnection connection, string role)
	{
		if (!base.isServer)
		{
			if (string.IsNullOrEmpty(role))
			{
				GlobalSet = false;
				SetColor("default");
				SetText(string.Empty);
				FixedBadge = string.Empty;
				SetText(string.Empty);
			}
			else
			{
				GlobalSet = true;
				SetColor("silver");
				FixedBadge = role.Replace("[", string.Empty).Replace("]", string.Empty).Replace("<", string.Empty)
					.Replace(">", string.Empty) + " (hidden)";
				SetText(FixedBadge);
			}
		}
	}

	[ClientRpc(channel = 2)]
	public void RpcResetFixed()
	{
		FixedBadge = string.Empty;
	}

	[Command(channel = 2)]
	public void CmdRequestBadge(string token)
	{
		if (!_requested)
		{
			_requested = true;
			Timing.RunCoroutine(_RequestRoleFromServer(token), Segment.FixedUpdate);
		}
	}

	[Command(channel = 2)]
	public void CmdDoNotTrack()
	{
		SetDoNotTrack();
	}

	public void SetDoNotTrack()
	{
		if (!DoNotTrack)
		{
			DoNotTrack = true;
			if (!string.IsNullOrEmpty(GetComponent<NicknameSync>().myNick))
			{
				LogDNT();
			}
			if (!base.isLocalPlayer)
			{
				GetComponent<GameConsoleTransmission>().SendToClient(base.connectionToClient, "Your \"Do not track\" request has been received.", "green");
			}
		}
	}

	public void LogDNT()
	{
		ServerLogs.AddLog(ServerLogs.Modules.Networking, "Player with nickname " + GetComponent<NicknameSync>().myNick + ", SteamID " + GetComponent<CharacterClassManager>().SteamId + " connected from IP " + base.connectionToClient.address + " sent Do Not Track signal.", ServerLogs.ServerLogType.ConnectionUpdate);
	}

	[ServerCallback]
	public void RefreshPermissions(bool disp = false)
	{
		if (NetworkServer.active)
		{
			UserGroup userGroup = ServerStatic.PermissionsHandler.GetUserGroup(GetComponent<CharacterClassManager>().SteamId);
			if (userGroup != null)
			{
				SetGroup(userGroup, false, false, disp);
			}
		}
	}

	[ServerCallback]
	public void SetGroup(UserGroup group, bool ovr, bool byAdmin = false, bool disp = false)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (group == null)
		{
			if (!RaEverywhere || Permissions != ServerStatic.PermissionsHandler.FullPerm)
			{
				RemoteAdmin = false;
				Permissions = 0uL;
				RemoteAdminMode = AccessMode.LocalAccess;
				SetColor("default");
				SetText(string.Empty);
				BadgeCover = false;
				if (!string.IsNullOrEmpty(PrevBadge))
				{
					SetBadgeUpdate(PrevBadge);
				}
				CallTargetCloseRemoteAdmin(base.connectionToClient);
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your local permissions has been revoked by server administrator.", "red");
			}
			return;
		}
		GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, byAdmin ? "Updating your group on server (set by server administrator)..." : "Updating your group on server (local permissions)...", "cyan");
		BadgeCover = group.Cover;
		if (!OverwatchPermitted && ServerStatic.PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
		{
			OverwatchPermitted = true;
		}
		if (group.Permissions != 0 && Permissions != ServerStatic.PermissionsHandler.FullPerm && ServerStatic.PermissionsHandler.IsRaPermitted(group.Permissions))
		{
			RemoteAdmin = true;
			Permissions = group.Permissions;
			RemoteAdminMode = ((!ovr) ? AccessMode.LocalAccess : AccessMode.PasswordOverride);
			GetComponent<QueryProcessor>().PasswordTries = 0;
			CallTargetOpenRemoteAdmin(base.connectionToClient, ovr);
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, byAdmin ? "Your remote admin access has been granted (set by server administrator)." : "Your remote admin access has been granted (local permissions).", "cyan");
			if (ServerStatic.PermissionsHandler.IsPermitted(Permissions, PlayerPermissions.ViewHiddenBadges))
			{
				GameObject[] players = PlayerManager.singleton.players;
				foreach (GameObject gameObject in players)
				{
					ServerRoles component = gameObject.GetComponent<ServerRoles>();
					if (!string.IsNullOrEmpty(component.HiddenBadge))
					{
						component.CallTargetSetHiddenRole(base.connectionToClient, component.HiddenBadge);
					}
				}
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Hidden badges have been displayed for you (if there are any).", "gray");
			}
		}
		else if (!RaEverywhere && Permissions != ServerStatic.PermissionsHandler.FullPerm)
		{
			RemoteAdmin = false;
			Permissions = 0uL;
			RemoteAdminMode = AccessMode.LocalAccess;
			CallTargetCloseRemoteAdmin(base.connectionToClient);
		}
		ServerLogs.AddLog(ServerLogs.Modules.Permissions, "User with nickname " + GetComponent<NicknameSync>().myNick + " and SteamID " + GetComponent<CharacterClassManager>().SteamId + " has been assigned to group " + group.BadgeText + " (local permissions).", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
		if (group.BadgeColor == "none")
		{
			return;
		}
		if (group.HiddenByDefault && !disp)
		{
			BadgeCover = false;
			if (string.IsNullOrEmpty(MyText))
			{
				GlobalSet = false;
				NetworkMyText = string.Empty;
				NetworkMyColor = string.Empty;
				HiddenBadge = group.BadgeText;
				RefreshHiddenTag();
				CallTargetSetHiddenRole(base.connectionToClient, group.BadgeText);
				if (!byAdmin)
				{
					GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your role has been granted, but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "yellow");
				}
				else
				{
					GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your role has been granted to you (set by server administrator), but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "cyan");
				}
			}
			return;
		}
		GlobalSet = false;
		HiddenBadge = string.Empty;
		CallRpcResetFixed();
		NetworkMyText = group.BadgeText;
		NetworkMyColor = group.BadgeColor;
		if (!byAdmin)
		{
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your role \"" + group.BadgeText + "\" with color " + group.BadgeColor + " has been granted to you (local permissions).", "cyan");
		}
		else
		{
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your role \"" + group.BadgeText + "\" with color " + group.BadgeColor + " has been granted to you (set by server administrator).", "cyan");
		}
	}

	[ServerCallback]
	public void RefreshHiddenTag()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		PermissionsHandler handler = ServerStatic.PermissionsHandler;
		IEnumerable<GameObject> enumerable = PlayerManager.singleton.players.Where((GameObject player) => handler.IsPermitted(player.GetComponent<ServerRoles>().Permissions, PlayerPermissions.ViewHiddenBadges) || player.GetComponent<ServerRoles>().Staff);
		foreach (GameObject item in enumerable)
		{
			CallTargetSetHiddenRole(item.GetComponent<ServerRoles>().connectionToClient, HiddenBadge);
		}
	}

	private IEnumerator<float> _RequestRoleFromServer(string token)
	{
		Dictionary<string, string> dictionary = CentralAuth.ValidateBadgeRequest(token, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
		if (dictionary != null)
		{
			_globalBadgeUnconfirmed = token;
			StartServerChallenge(1);
		}
		yield break;
	}

	public string GetColoredRoleString(bool newLine = false)
	{
		if (string.IsNullOrEmpty(MyColor) || string.IsNullOrEmpty(MyText) || CurrentColor == null)
		{
			return string.Empty;
		}
		if ((CurrentColor.Restricted || MyText.Contains("[") || MyText.Contains("]") || MyText.Contains("<") || MyText.Contains(">")) && !AuthroizeBadge)
		{
			return string.Empty;
		}
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor != null)
		{
			return ((!newLine) ? string.Empty : "\n") + "<color=#" + namedColor.ColorHex + ">" + MyText + "</color>";
		}
		return string.Empty;
	}

	public string GetUncoloredRoleString()
	{
		if (string.IsNullOrEmpty(MyColor) || string.IsNullOrEmpty(MyText) || CurrentColor == null)
		{
			return string.Empty;
		}
		if ((CurrentColor.Restricted || MyText.Contains("[") || MyText.Contains("]") || MyText.Contains("<") || MyText.Contains(">")) && !AuthroizeBadge)
		{
			return string.Empty;
		}
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor != null)
		{
			return MyText;
		}
		return string.Empty;
	}

	public Color GetColor()
	{
		if (string.IsNullOrEmpty(MyColor) || MyColor == "default" || CurrentColor == null)
		{
			return Color.white;
		}
		if ((CurrentColor.Restricted || MyText.Contains("[") || MyText.Contains("]") || MyText.Contains("<") || MyText.Contains(">")) && !AuthroizeBadge)
		{
			return Color.white;
		}
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		return (namedColor != null) ? namedColor.SpeakingColorIn.Evaluate(1f) : Color.white;
	}

	public Gradient[] GetGradient()
	{
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		return (namedColor == null) ? null : new Gradient[2] { namedColor.SpeakingColorIn, namedColor.SpeakingColorOut };
	}

	private void Update()
	{
		if (CurrentColor == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(FixedBadge) && MyText != FixedBadge)
		{
			SetText(FixedBadge);
			SetColor("silver");
			return;
		}
		if (!string.IsNullOrEmpty(FixedBadge) && CurrentColor.Name != "silver")
		{
			SetColor("silver");
			return;
		}
		if (GlobalBadge != _prevBadge)
		{
			_prevBadge = GlobalBadge;
			if (string.IsNullOrEmpty(GlobalBadge))
			{
				_bgc = string.Empty;
				_bgt = string.Empty;
				AuthroizeBadge = false;
				_prevColor += ".";
				_prevText += ".";
				return;
			}
			GameConsole.Console.singleton.AddLog("Validating global badge of user " + GetComponent<NicknameSync>().myNick, Color.gray);
			Dictionary<string, string> dictionary = CentralAuth.ValidateBadgeRequest(GlobalBadge, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
			if (dictionary == null)
			{
				GameConsole.Console.singleton.AddLog("Validation of global badge of user " + GetComponent<NicknameSync>().myNick + " failed - invalid digital signature.", Color.red);
				_bgc = string.Empty;
				_bgt = string.Empty;
				AuthroizeBadge = false;
				_prevColor += ".";
				_prevText += ".";
				return;
			}
			GameConsole.Console.singleton.AddLog("Validation of global badge of user " + GetComponent<NicknameSync>().myNick + " complete - badge signed by central server " + dictionary["Issued by"] + ".", Color.grey);
			_bgc = dictionary["Badge color"];
			_bgt = dictionary["Badge text"];
			NetworkMyColor = dictionary["Badge color"];
			NetworkMyText = dictionary["Badge text"];
			AuthroizeBadge = true;
		}
		if (!(_prevColor == MyColor) || !(_prevText == MyText))
		{
			if (CurrentColor.Restricted && (MyText != _bgt || MyColor != _bgc))
			{
				GameConsole.Console.singleton.AddLog("TAG FAIL 1 - " + MyText + " - " + _bgt + " /-/ " + MyColor + " - " + _bgc, Color.gray);
				AuthroizeBadge = false;
				NetworkMyColor = string.Empty;
				NetworkMyText = string.Empty;
				_prevColor = string.Empty;
				_prevText = string.Empty;
				PlayerList.UpdatePlayerRole(base.gameObject);
			}
			else if ((MyText != _bgt && (MyText.Contains("[") || MyText.Contains("]"))) || MyText.Contains("<") || MyText.Contains(">"))
			{
				GameConsole.Console.singleton.AddLog("TAG FAIL 2 - " + MyText + " - " + _bgt + " /-/ " + MyColor + " - " + _bgc, Color.gray);
				AuthroizeBadge = false;
				NetworkMyColor = string.Empty;
				NetworkMyText = string.Empty;
				_prevColor = string.Empty;
				_prevText = string.Empty;
				PlayerList.UpdatePlayerRole(base.gameObject);
			}
			else
			{
				_prevColor = MyColor;
				_prevText = MyText;
				_prevBadge = GlobalBadge;
				PlayerList.UpdatePlayerRole(base.gameObject);
			}
		}
	}

	public void SetColor(string i)
	{
		NetworkMyColor = i;
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor == null && i != "default")
		{
			SetColor("default");
		}
		else
		{
			CurrentColor = namedColor;
		}
	}

	public void SetText(string i)
	{
		NetworkMyText = i;
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor != null)
		{
			CurrentColor = namedColor;
		}
	}

	public void SetBadgeUpdate(string i)
	{
		NetworkGlobalBadge = i;
	}

	[ServerCallback]
	public void StartServerChallenge(int selector)
	{
		if (NetworkServer.active && (selector != 0 || string.IsNullOrEmpty(_authChallenge)) && (selector != 1 || string.IsNullOrEmpty(_badgeChallenge)) && selector <= 1 && selector >= 0)
		{
			byte[] array;
			using (RandomNumberGenerator randomNumberGenerator = new RNGCryptoServiceProvider())
			{
				array = new byte[32];
				randomNumberGenerator.GetBytes(array);
			}
			string text = Convert.ToBase64String(array);
			if (selector == 0)
			{
				_authChallenge = "auth-" + text;
				CallTargetSignServerChallenge(base.connectionToClient, _authChallenge);
			}
			else
			{
				_badgeChallenge = "badge-server-" + text;
				CallTargetSignServerChallenge(base.connectionToClient, _badgeChallenge);
			}
		}
	}

	[TargetRpc(channel = 2)]
	public void TargetSignServerChallenge(NetworkConnection target, string challenge)
	{
		if (challenge.StartsWith("auth-"))
		{
			if (_authRequested)
			{
				return;
			}
			_authRequested = true;
		}
		else
		{
			if (!challenge.StartsWith("badge-server-") || _badgeRequested)
			{
				return;
			}
			_badgeRequested = true;
		}
		string response = ECDSA.Sign(challenge, GameConsole.Console.SessionKeys.Private);
		GameConsole.Console.singleton.AddLog("Signed " + challenge + " for server.", Color.cyan);
		CallCmdServerSignatureComplete(challenge, response, ECDSA.KeyToString(GameConsole.Console.SessionKeys.Public), GameConsole.Console.HideBadge);
	}

	[Command(channel = 2)]
	public void CmdServerSignatureComplete(string challenge, string response, string publickey, bool hide)
	{
		if (FirstVerResult == null)
		{
			FirstVerResult = CentralAuth.ValidateBadgeRequest(_globalBadgeUnconfirmed, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
		}
		if (FirstVerResult == null)
		{
			return;
		}
		if (FirstVerResult["Public key"] != Misc.Base64Encode(Sha.HashToString(Sha.Sha256(publickey))))
		{
			GameConsole.Console.singleton.AddLog("Rejected signature of challenge " + challenge + " due to public key hash mismatch.", Color.red);
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to public key mismatch.", "red");
			return;
		}
		if (PublicKey == null)
		{
			PublicKey = ECDSA.PublicKeyFromString(publickey);
		}
		if (!ECDSA.Verify(challenge, response, PublicKey))
		{
			GameConsole.Console.singleton.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.", Color.red);
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
		}
		else if (challenge.StartsWith("auth-") && challenge == _authChallenge)
		{
			GetComponent<CharacterClassManager>().NetworkSteamId = FirstVerResult["Steam ID"];
			GetComponent<NicknameSync>().UpdateNickname(Misc.Base64Decode(FirstVerResult["Nickname"]));
			if (DoNotTrack)
			{
				LogDNT();
			}
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Hi " + Misc.Base64Decode(FirstVerResult["Nickname"]) + "! Your challenge signature has been accepted.", "green");
			GetComponent<RemoteAdminCryptographicManager>().StartExchange();
			RefreshPermissions();
			_authChallenge = string.Empty;
		}
		else
		{
			if (!challenge.StartsWith("badge-server-") || !(challenge == _badgeChallenge))
			{
				return;
			}
			Dictionary<string, string> dictionary = CentralAuth.ValidateBadgeRequest(_globalBadgeUnconfirmed, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
			if (dictionary == null)
			{
				ServerConsole.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.");
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
				return;
			}
			PrevBadge = _globalBadgeUnconfirmed;
			if (dictionary["Remote admin"] == "YES" || dictionary["Management"] == "YES" || dictionary["Global banning"] == "YES")
			{
				Staff = true;
			}
			if (dictionary["Management"] == "YES" || dictionary["Global banning"] == "YES")
			{
				RaEverywhere = true;
			}
			if (dictionary["Overwatch mode"] == "YES")
			{
				OverwatchPermitted = true;
			}
			if (dictionary["Remote admin"] == "YES" && ServerStatic.PermissionsHandler.StaffAccess)
			{
				RemoteAdmin = true;
				Permissions = ServerStatic.PermissionsHandler.FullPerm;
				RemoteAdminMode = AccessMode.GlobalAccess;
				GetComponent<QueryProcessor>().PasswordTries = 0;
				CallTargetOpenRemoteAdmin(base.connectionToClient, false);
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - staff).", "cyan");
			}
			else if (dictionary["Management"] == "YES" && ServerStatic.PermissionsHandler.ManagersAccess)
			{
				RemoteAdmin = true;
				Permissions = ServerStatic.PermissionsHandler.FullPerm;
				RemoteAdminMode = AccessMode.GlobalAccess;
				GetComponent<QueryProcessor>().PasswordTries = 0;
				CallTargetOpenRemoteAdmin(base.connectionToClient, false);
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - management).", "cyan");
			}
			else if (dictionary["Global banning"] == "YES" && ServerStatic.PermissionsHandler.BanningTeamAccess)
			{
				RemoteAdmin = true;
				Permissions = ServerStatic.PermissionsHandler.FullPerm;
				RemoteAdminMode = AccessMode.GlobalAccess;
				GetComponent<QueryProcessor>().PasswordTries = 0;
				CallTargetOpenRemoteAdmin(base.connectionToClient, false);
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - banning team).", "cyan");
			}
			if (!BadgeCover || string.IsNullOrEmpty(MyText) || string.IsNullOrEmpty(MyColor))
			{
				if (dictionary["Badge type"] == "3")
				{
					hide = true;
				}
				else if (dictionary["Badge type"] == "4" && ConfigFile.ServerConfig.GetBool("hide_banteam_badges_by_default"))
				{
					hide = true;
				}
				else if (dictionary["Badge type"] == "1" && ConfigFile.ServerConfig.GetBool("hide_staff_badges_by_default"))
				{
					hide = true;
				}
				else if (dictionary["Badge type"] == "2" && ConfigFile.ServerConfig.GetBool("hide_management_badges_by_default"))
				{
					hide = true;
				}
				else if (dictionary["Badge type"] == "0" && ConfigFile.ServerConfig.GetBool("hide_patreon_badges_by_default") && !ServerStatic.PermissionsHandler.IsVerified)
				{
					hide = true;
				}
				int result = 0;
				GlobalSet = true;
				if (int.TryParse(dictionary["Badge type"], out result))
				{
					GlobalBadgeType = result;
				}
				if (hide)
				{
					HiddenBadge = dictionary["Badge text"];
					RefreshHiddenTag();
					GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your global badge has been granted, but it's hidden. Use \"gtag\" command in the game console to show your global badge.", "yellow");
				}
				else
				{
					HiddenBadge = string.Empty;
					CallRpcResetFixed();
					SetBadgeUpdate(_globalBadgeUnconfirmed);
					GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your global badge has been granted.", "cyan");
				}
			}
			else
			{
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Your global badge is covered by server badge. Use \"gtag\" command in the game console to show your global badge.", "yellow");
			}
			_badgeChallenge = string.Empty;
			_globalBadgeUnconfirmed = string.Empty;
			if (!Staff)
			{
				return;
			}
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				ServerRoles component = gameObject.GetComponent<ServerRoles>();
				if (!string.IsNullOrEmpty(component.HiddenBadge))
				{
					component.CallTargetSetHiddenRole(base.connectionToClient, component.HiddenBadge);
				}
			}
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Hidden badges have been displayed for you (if there are any).", "gray");
		}
	}

	[TargetRpc]
	internal void TargetOpenRemoteAdmin(NetworkConnection connection, bool password)
	{
		LocalRemoteAdmin = true;
		if (!base.isServer)
		{
			if (password && RemoteAdminMode != AccessMode.PasswordOverride)
			{
				RemoteAdminMode = AccessMode.PasswordOverride;
			}
			else if (!password && RemoteAdminMode == AccessMode.PasswordOverride)
			{
				RemoteAdminMode = AccessMode.LocalAccess;
			}
		}
		UnityEngine.Object.FindObjectOfType<UIController>().ActivateRemoteAdmin();
	}

	[TargetRpc]
	internal void TargetCloseRemoteAdmin(NetworkConnection connection)
	{
		LocalRemoteAdmin = false;
		UnityEngine.Object.FindObjectOfType<UIController>().DeactivateRemoteAdmin();
	}

	[Command(channel = 2)]
	public void CmdSetOverwatchStatus(bool status)
	{
		if (!OverwatchPermitted && status)
		{
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "You don't have permissions to enable overwatch mode!", "red");
		}
		else
		{
			SetOverwatchStatus(status);
		}
	}

	[Command(channel = 2)]
	public void CmdToggleOverwatch()
	{
		if (!OverwatchPermitted && !OverwatchEnabled)
		{
			GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "You don't have permissions to enable overwatch mode!", "red");
		}
		else
		{
			SetOverwatchStatus(!OverwatchEnabled);
		}
	}

	public void SetOverwatchStatus(bool status)
	{
		OverwatchEnabled = status;
		CharacterClassManager component = GetComponent<CharacterClassManager>();
		if (status && component.curClass != 2 && component.curClass >= 0)
		{
			component.SetClassID(2);
		}
		CallTargetSetOverwatch(base.connectionToClient, OverwatchEnabled);
	}

	public void RequestBadge(string token)
	{
		CallCmdRequestBadge(token);
	}

	[TargetRpc(channel = 2)]
	public void TargetSetOverwatch(NetworkConnection conn, bool s)
	{
		GameConsole.Console.singleton.AddLog("Overwatch status: " + ((!s) ? "DISABLED" : "ENABLED"), Color.green);
		AmIInOverwatch = s;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdRequestBadge(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestBadge called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdRequestBadge(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdDoNotTrack(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoNotTrack called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdDoNotTrack();
		}
	}

	protected static void InvokeCmdCmdServerSignatureComplete(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdServerSignatureComplete called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdServerSignatureComplete(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetOverwatchStatus(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetOverwatchStatus called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdSetOverwatchStatus(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdToggleOverwatch(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdToggleOverwatch called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdToggleOverwatch();
		}
	}

	public void CallCmdRequestBadge(string token)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestBadge called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestBadge(token);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestBadge);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(token);
		SendCommandInternal(networkWriter, 2, "CmdRequestBadge");
	}

	public void CallCmdDoNotTrack()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDoNotTrack called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDoNotTrack();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDoNotTrack);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdDoNotTrack");
	}

	public void CallCmdServerSignatureComplete(string challenge, string response, string publickey, bool hide)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdServerSignatureComplete called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdServerSignatureComplete(challenge, response, publickey, hide);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdServerSignatureComplete);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(challenge);
		networkWriter.Write(response);
		networkWriter.Write(publickey);
		networkWriter.Write(hide);
		SendCommandInternal(networkWriter, 2, "CmdServerSignatureComplete");
	}

	public void CallCmdSetOverwatchStatus(bool status)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetOverwatchStatus called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetOverwatchStatus(status);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetOverwatchStatus);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(status);
		SendCommandInternal(networkWriter, 2, "CmdSetOverwatchStatus");
	}

	public void CallCmdToggleOverwatch()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdToggleOverwatch called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdToggleOverwatch();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdToggleOverwatch);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdToggleOverwatch");
	}

	protected static void InvokeRpcRpcResetFixed(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcResetFixed called on server.");
		}
		else
		{
			((ServerRoles)obj).RpcResetFixed();
		}
	}

	protected static void InvokeRpcTargetSetHiddenRole(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetHiddenRole called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetSetHiddenRole(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetSignServerChallenge(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSignServerChallenge called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetSignServerChallenge(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetOpenRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenRemoteAdmin called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetOpenRemoteAdmin(ClientScene.readyConnection, reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcTargetCloseRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetCloseRemoteAdmin called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetCloseRemoteAdmin(ClientScene.readyConnection);
		}
	}

	protected static void InvokeRpcTargetSetOverwatch(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetOverwatch called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetSetOverwatch(ClientScene.readyConnection, reader.ReadBoolean());
		}
	}

	public void CallRpcResetFixed()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcResetFixed called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcResetFixed);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 2, "RpcResetFixed");
	}

	public void CallTargetSetHiddenRole(NetworkConnection connection, string role)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetHiddenRole called on client.");
			return;
		}
       if (connection.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSetHiddenRole called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetHiddenRole);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(role);
		SendTargetRPCInternal(connection, networkWriter, 2, "TargetSetHiddenRole");
	}

	public void CallTargetSignServerChallenge(NetworkConnection target, string challenge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSignServerChallenge called on client.");
			return;
		}
       if (target.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSignServerChallenge called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSignServerChallenge);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(challenge);
		SendTargetRPCInternal(target, networkWriter, 2, "TargetSignServerChallenge");
	}

	public void CallTargetOpenRemoteAdmin(NetworkConnection connection, bool password)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetOpenRemoteAdmin called on client.");
			return;
		}
       if (connection.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetOpenRemoteAdmin called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetOpenRemoteAdmin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(password);
		SendTargetRPCInternal(connection, networkWriter, 0, "TargetOpenRemoteAdmin");
	}

	public void CallTargetCloseRemoteAdmin(NetworkConnection connection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetCloseRemoteAdmin called on client.");
			return;
		}
       if (connection.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetCloseRemoteAdmin called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetCloseRemoteAdmin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendTargetRPCInternal(connection, networkWriter, 0, "TargetCloseRemoteAdmin");
	}

	public void CallTargetSetOverwatch(NetworkConnection conn, bool s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetOverwatch called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSetOverwatch called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetOverwatch);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(s);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetSetOverwatch");
	}

	static ServerRoles()
	{
		kCmdCmdRequestBadge = 1417446350;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdRequestBadge, InvokeCmdCmdRequestBadge);
		kCmdCmdDoNotTrack = -1217759267;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdDoNotTrack, InvokeCmdCmdDoNotTrack);
		kCmdCmdServerSignatureComplete = -834487468;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdServerSignatureComplete, InvokeCmdCmdServerSignatureComplete);
		kCmdCmdSetOverwatchStatus = 200610181;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdSetOverwatchStatus, InvokeCmdCmdSetOverwatchStatus);
		kCmdCmdToggleOverwatch = -571630643;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdToggleOverwatch, InvokeCmdCmdToggleOverwatch);
		kRpcRpcResetFixed = -1154333771;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kRpcRpcResetFixed, InvokeRpcRpcResetFixed);
		kTargetRpcTargetSetHiddenRole = -948979541;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetSetHiddenRole, InvokeRpcTargetSetHiddenRole);
		kTargetRpcTargetSignServerChallenge = 1367769996;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetSignServerChallenge, InvokeRpcTargetSignServerChallenge);
		kTargetRpcTargetOpenRemoteAdmin = 1449538856;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetOpenRemoteAdmin, InvokeRpcTargetOpenRemoteAdmin);
		kTargetRpcTargetCloseRemoteAdmin = -11809912;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetCloseRemoteAdmin, InvokeRpcTargetCloseRemoteAdmin);
		kTargetRpcTargetSetOverwatch = -1052391504;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetSetOverwatch, InvokeRpcTargetSetOverwatch);
		NetworkCRC.RegisterBehaviour("ServerRoles", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(MyColor);
			writer.Write(MyText);
			writer.Write(GlobalBadge);
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
			writer.Write(MyColor);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(MyText);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(GlobalBadge);
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
			MyColor = reader.ReadString();
			MyText = reader.ReadString();
			GlobalBadge = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetColor(reader.ReadString());
		}
		if ((num & 2) != 0)
		{
			SetText(reader.ReadString());
		}
		if ((num & 4) != 0)
		{
			SetBadgeUpdate(reader.ReadString());
		}
	}
}
