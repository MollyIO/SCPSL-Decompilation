using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BanHandler : MonoBehaviour
{
	public enum BanType
	{
		NULL = -1,
		Steam = 0,
		IP = 1
	}

	public static BanType GetBanType(int type)
	{
		if (type > Enum.GetValues(typeof(BanType)).Cast<int>().Max() || type < Enum.GetValues(typeof(BanType)).Cast<int>().Min())
		{
			return BanType.Steam;
		}
		return (BanType)type;
	}

	private void Start()
	{
		try
		{
			if (!Directory.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs)))
			{
				Directory.CreateDirectory(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs));
			}
			if (!File.Exists(GetPath(BanType.Steam)))
			{
				File.Create(GetPath(BanType.Steam)).Close();
			}
			else
			{
				FileManager.RemoveEmptyLines(GetPath(BanType.Steam));
			}
			if (!File.Exists(GetPath(BanType.IP)))
			{
				File.Create(GetPath(BanType.IP)).Close();
			}
			else
			{
				FileManager.RemoveEmptyLines(GetPath(BanType.IP));
			}
		}
		catch
		{
			ServerConsole.AddLog("Can't create ban files!");
		}
		ValidateBans();
	}

	public static bool IssueBan(BanDetails ban, BanType banType)
	{
		try
		{
			ban.OriginalName = ban.OriginalName.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			List<BanDetails> bans = GetBans(banType);
			if (!bans.Where((BanDetails b) => b.Id == ban.Id).Any())
			{
				FileManager.AppendFile(ban.ToString(), GetPath(banType));
				FileManager.RemoveEmptyLines(GetPath(banType));
			}
			else
			{
				RemoveBan(ban.Id, banType);
				IssueBan(ban, banType);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static void ValidateBans()
	{
		ValidateBans(BanType.Steam);
		ValidateBans(BanType.IP);
	}

	public static void ValidateBans(BanType banType)
	{
		List<string> list = FileManager.ReadAllLines(GetPath(banType)).ToList();
		List<int> list2 = new List<int>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string ban = list[num];
			if (ProcessBanItem(ban) == null || !CheckExpiration(ProcessBanItem(ban), BanType.NULL))
			{
				list2.Add(num);
			}
		}
		List<int> list3 = new List<int>();
		foreach (int item in list2)
		{
			if (!list3.Contains(item))
			{
				list3.Add(item);
			}
		}
		foreach (int item2 in list3.OrderByDescending((int index) => index))
		{
			list.RemoveAt(item2);
		}
		if (FileManager.ReadAllLines(GetPath(banType)) != list.ToArray())
		{
			FileManager.WriteToFile(list.ToArray(), GetPath(banType));
		}
	}

	public static bool CheckExpiration(BanDetails ban, BanType banType)
	{
		int num = Convert.ToInt32(banType);
		if (ban == null)
		{
			return false;
		}
		if (TimeBehaviour.ValidateTimestamp(ban.Expires, TimeBehaviour.CurrentTimestamp(), 0L))
		{
			return true;
		}
		if (num >= 0)
		{
			RemoveBan(ban.Id, banType);
		}
		return false;
	}

	public static BanDetails ReturnChecks(BanDetails ban, BanType banType)
	{
		return (!CheckExpiration(ban, banType)) ? null : ban;
	}

	public static void RemoveBan(string id, BanType banType)
	{
		id = id.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
		string[] data = (from l in FileManager.ReadAllLines(GetPath(banType))
			where ProcessBanItem(l) != null && ProcessBanItem(l).Id != id
			select l).ToArray();
		FileManager.WriteToFile(data, GetPath(banType));
	}

	public static List<BanDetails> GetBans(BanType banType)
	{
		string[] source = FileManager.ReadAllLines(GetPath(banType));
		return (from b in source.Select(ProcessBanItem)
			where b != null
			select b).ToList();
	}

	public static KeyValuePair<BanDetails, BanDetails> QueryBan(string steamId, string ip)
	{
		string ban = null;
		string ban2 = null;
		if (!string.IsNullOrEmpty(steamId))
		{
			steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			string[] source = FileManager.ReadAllLines(GetPath(BanType.Steam));
			ban = source.Where((string b) => ProcessBanItem(b) != null && ProcessBanItem(b).Id == steamId).FirstOrDefault();
		}
		if (!string.IsNullOrEmpty(ip))
		{
			ip = ip.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			string[] source2 = FileManager.ReadAllLines(GetPath(BanType.IP));
			ban2 = source2.Where((string b) => ProcessBanItem(b) != null && ProcessBanItem(b).Id == ip).FirstOrDefault();
		}
		return new KeyValuePair<BanDetails, BanDetails>(ReturnChecks(ProcessBanItem(ban), BanType.Steam), ReturnChecks(ProcessBanItem(ban2), BanType.IP));
	}

	public static BanDetails ProcessBanItem(string ban)
	{
		if (string.IsNullOrEmpty(ban) || !ban.Contains(";"))
		{
			return null;
		}
		string[] array = ban.Split(';');
		if (array.Length != 6)
		{
			return null;
		}
		BanDetails banDetails = new BanDetails();
		banDetails.OriginalName = array[0];
		banDetails.Id = array[1].Trim();
		banDetails.Expires = Convert.ToInt64(array[2].Trim());
		banDetails.Reason = array[3];
		banDetails.Issuer = array[4];
		banDetails.IssuanceTime = Convert.ToInt64(array[5].Trim());
		return banDetails;
	}

	public static string GetPath(BanType banType)
	{
		switch (banType)
		{
		case BanType.Steam:
			return FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "SteamIdBans.txt";
		case BanType.IP:
			return FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "IpBans.txt";
		default:
			return FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "SteamIdBans.txt";
		}
	}
}
