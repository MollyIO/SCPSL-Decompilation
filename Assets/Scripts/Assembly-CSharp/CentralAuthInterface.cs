using GameConsole;
using UnityEngine;

public class CentralAuthInterface : ICentralAuth
{
	private readonly CharacterClassManager _s;

	private readonly bool _is;

	public CentralAuthInterface(CharacterClassManager sync, bool server)
	{
		_s = sync;
		_is = server;
	}

	public CharacterClassManager GetCcm()
	{
		return _s;
	}

	public void TokenGenerated(string token)
	{
		Console.singleton.AddLog("Authentication token obtained from central server.", Color.green);
		_s.CallCmdSendToken(token);
	}

	public void RequestBadge(string token)
	{
		_s.GetComponent<ServerRoles>().RequestBadge(token);
	}

	public void Fail()
	{
		if (_is)
		{
			ServerConsole.AddLog("Failed to validate authentication token.");
			ServerConsole.Disconnect(_s.connectionToClient, "Failed to validate authentication token.");
		}
		else
		{
			Console.singleton.AddLog("Failed to obtain authentication token from central server.", Color.red);
			_s.connectionToServer.Disconnect();
			_s.connectionToServer.Dispose();
		}
	}

	public void Ok(string steamId, string nickname, string ban, string steamban, string server, bool bypass, bool DNT)
	{
		ServerConsole.AddLog("Accepted authentication token of user " + steamId + " with global ban status " + ban + " signed by " + server + " server.");
		_s.TargetConsolePrint(_s.connectionToClient, "Accepted your authentication token (your steam id " + steamId + ") with global ban status " + ban + " signed by " + server + " server.", "green");
		ServerRoles component = _s.GetComponent<ServerRoles>();
		if (DNT)
		{
			component.SetDoNotTrack();
		}
		if ((!bypass || !ServerStatic.GetPermissionsHandler().IsVerified) && BanHandler.QueryBan(steamId, null).Key != null)
		{
			_s.TargetConsolePrint(_s.connectionToClient, "You are banned from this server.", "red");
			ServerConsole.AddLog("Player kicked due to local SteamID ban.");
			ServerConsole.Disconnect(_s.connectionToClient, "You are banned from this server.");
			return;
		}
		if ((!bypass || !ServerStatic.GetPermissionsHandler().IsVerified) && !WhiteList.IsWhitelisted(steamId))
		{
			_s.TargetConsolePrint(_s.connectionToClient, "You are not on the whitelist!", "red");
			ServerConsole.AddLog("Player kicked due to whitelist enabled.");
			ServerConsole.Disconnect(_s.connectionToClient, "You are not on the whitelist for this server.");
			return;
		}
		if ((ConfigFile.ServerConfig.GetBool("use_vac", true) || ServerStatic.PermissionsHandler.IsVerified) && steamban != "0")
		{
			_s.TargetConsolePrint(_s.connectionToClient, "You have been globally banned: " + steamban + ".", "red");
			ServerConsole.AddLog("Player kicked due to active global ban (" + steamban + ").");
			ServerConsole.Disconnect(_s.connectionToClient, "You have been globally banned: " + steamban + ".");
			return;
		}
		if ((ConfigFile.ServerConfig.GetBool("global_bans_cheating", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "1")
		{
			_s.TargetConsolePrint(_s.connectionToClient, "You have been globally banned for cheating.", "red");
			ServerConsole.AddLog("Player kicked due to global ban for cheating.");
			ServerConsole.Disconnect(_s.connectionToClient, "You have been globally banned for cheating.");
			return;
		}
		if ((ConfigFile.ServerConfig.GetBool("global_bans_exploiting", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "2")
		{
			_s.TargetConsolePrint(_s.connectionToClient, "You have been globally banned for exploiting.", "red");
			ServerConsole.AddLog("Player kicked due to global ban for exploiting.");
			ServerConsole.Disconnect(_s.connectionToClient, "You have been globally banned for exploiting.");
			return;
		}
		if ((ConfigFile.ServerConfig.GetBool("global_bans_griefing", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "5")
		{
			_s.TargetConsolePrint(_s.connectionToClient, "You have been globally banned for griefing.", "red");
			ServerConsole.AddLog("Player kicked due to global ban for griefing.");
			ServerConsole.Disconnect(_s.connectionToClient, "You have been globally banned for griefing.");
			return;
		}
		if (MuteHandler.QueryPersistantMute(steamId))
		{
			_s.NetworkMuted = true;
			_s.NetworkIntercomMuted = true;
			_s.TargetConsolePrint(_s.connectionToClient, "You are muted on the voice chat by the server administrator.", "red");
		}
		else if ((ConfigFile.ServerConfig.GetBool("global_mutes_voicechat", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "3")
		{
			_s.NetworkMuted = true;
			_s.NetworkIntercomMuted = true;
			_s.TargetConsolePrint(_s.connectionToClient, "You are globally muted on the voice chat.", "red");
		}
		else if (MuteHandler.QueryPersistantMute("ICOM-" + steamId))
		{
			_s.NetworkIntercomMuted = true;
			_s.TargetConsolePrint(_s.connectionToClient, "You are muted on the intercom by the server administrator.", "red");
		}
		else if ((ConfigFile.ServerConfig.GetBool("global_mutes_intercom", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "4")
		{
			_s.NetworkIntercomMuted = true;
			_s.TargetConsolePrint(_s.connectionToClient, "You are globally muted on the intercom.", "red");
		}
		component.BypassStaff = component.BypassStaff || bypass;
		if (component.BypassStaff)
		{
			component.GetComponent<CharacterClassManager>().TargetConsolePrint(_s.connectionToClient, "You have the ban bypass flag, so you can't be banned from this server.", "cyan");
		}
		component.StartServerChallenge(0);
	}

	public void FailToken(string reason)
	{
		_s.TargetConsolePrint(_s.connectionToClient, "Your authentication token is invalid - " + reason + ".", "red");
		ServerConsole.AddLog("Rejected invalid authentication token.");
		ServerConsole.Disconnect(_s.connectionToClient, reason);
	}
}
