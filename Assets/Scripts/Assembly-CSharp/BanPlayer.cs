using System;
using UnityEngine;
using UnityEngine.Networking;

public class BanPlayer : NetworkBehaviour
{
	public bool BanUser(GameObject user, int duration, string reason, string issuer)
	{
		try
		{
			if (duration > 0 && (!ServerStatic.PermissionsHandler.IsVerified || !user.GetComponent<ServerRoles>().BypassStaff))
			{
				string originalName = user.GetComponent<NicknameSync>().myNick ?? "Missing Nick";
				if (ConfigFile.ServerConfig.GetBool("online_mode"))
				{
					BanDetails banDetails = new BanDetails();
					banDetails.OriginalName = originalName;
					banDetails.Id = user.GetComponent<CharacterClassManager>().SteamId;
					banDetails.IssuanceTime = TimeBehaviour.CurrentTimestamp();
					banDetails.Expires = DateTime.UtcNow.AddMinutes(duration).Ticks;
					banDetails.Reason = reason;
					banDetails.Issuer = issuer;
					BanDetails ban = banDetails;
					BanHandler.IssueBan(ban, BanHandler.BanType.Steam);
				}
				if (ConfigFile.ServerConfig.GetBool("ip_banning"))
				{
					BanDetails banDetails = new BanDetails();
					banDetails.OriginalName = originalName;
					banDetails.Id = user.GetComponent<NetworkIdentity>().connectionToClient.address;
					banDetails.IssuanceTime = TimeBehaviour.CurrentTimestamp();
					banDetails.Expires = DateTime.UtcNow.AddMinutes(duration).Ticks;
					banDetails.Reason = reason;
					banDetails.Issuer = issuer;
					BanDetails ban2 = banDetails;
					BanHandler.IssueBan(ban2, BanHandler.BanType.IP);
				}
			}
		}
		catch
		{
			return false;
		}
		string text = ((duration <= 0) ? "kicked" : "banned");
		string text2 = "You have been " + text + ". ";
		if (!string.IsNullOrEmpty(reason))
		{
			text2 = text2 + "Reason: " + reason;
		}
		ServerConsole.Disconnect(user, text2);
		return true;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
