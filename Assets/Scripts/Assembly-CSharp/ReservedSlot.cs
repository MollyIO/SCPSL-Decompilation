using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class ReservedSlot
{
	public static char[] splitChars = new char[1] { ';' };

	private static List<string> commentSyms = new List<string> { "#", "//" };

	private readonly string InitIP;

	private readonly string InitSteamID;

	private readonly string InitComment;

	private string ip;

	private string steamID;

	public string Comment;

	private readonly string InitSlotEntry;

	public static string SMReservedSlotDir
	{
		get
		{
			string text = ConfigFile.ServerConfig.GetString("reserved_slots_location", FileManager.GetAppFolder(ServerStatic.ShareNonConfigs));
			return text.Replace("[appdata]", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		}
	}

	public static string SMReservedSlotFileName
	{
		[CompilerGenerated]
		get
		{
			return ConfigFile.ServerConfig.GetString("reserved_slots_file_name", "ReservedSlots.txt");
		}
	}

	public static string SMReservedSlotFile
	{
		get
		{
			string sMReservedSlotDir = SMReservedSlotDir;
			return ((sMReservedSlotDir.EndsWith("/") || sMReservedSlotDir.EndsWith("\\")) ? sMReservedSlotDir : (sMReservedSlotDir + Path.DirectorySeparatorChar)) + SMReservedSlotFileName;
		}
	}

	public static string SMOldReservedSlotFile
	{
		get
		{
			string sMReservedSlotDir = SMReservedSlotDir;
			return ((sMReservedSlotDir.EndsWith("/") || sMReservedSlotDir.EndsWith("\\")) ? sMReservedSlotDir : (sMReservedSlotDir + Path.DirectorySeparatorChar)) + "Reserved Slots.txt";
		}
	}

	public static List<string> CommentSyms
	{
		get
		{
			string text = ConfigFile.ServerConfig.GetString("reserved_slots_comment_symbol", commentSyms[0]);
			if (commentSyms[0] != text)
			{
				while (commentSyms.Contains(text))
				{
					commentSyms.Remove(text);
				}
				commentSyms.Insert(0, text);
			}
			return commentSyms;
		}
	}

	public string IP
	{
		get
		{
			return ip;
		}
		set
		{
			ip = TrimIPAddress(value);
		}
	}

	public string SteamID
	{
		get
		{
			return steamID;
		}
		set
		{
			if (IsSteamID(value))
			{
				steamID = value.Trim();
			}
		}
	}

	public string SlotEntry
	{
		get
		{
			string firstCommentSymbol = GetFirstCommentSymbol(InitSlotEntry);
			return (((!string.IsNullOrEmpty(IP)) ? IP : string.Empty) + ((!string.IsNullOrEmpty(SteamID)) ? (((!string.IsNullOrEmpty(IP)) ? splitChars[0].ToString() : string.Empty) + SteamID) : string.Empty) + ((!string.IsNullOrEmpty(Comment)) ? (" " + (StartsWithCommentSymbol(Comment) ? Comment : ((firstCommentSymbol ?? CommentSyms[0]) + Comment))) : string.Empty)).Trim();
		}
	}

	public ReservedSlot(string IP, string SteamID, string Comment)
	{
		InitIP = TrimIPAddress(IP);
		this.IP = InitIP;
		InitSteamID = (string.IsNullOrEmpty(SteamID) ? SteamID : ((!IsSteamID(SteamID.Trim())) ? null : SteamID.Trim()));
		this.SteamID = InitSteamID;
		InitComment = Comment;
		this.Comment = InitComment;
		InitSlotEntry = SlotEntry;
	}

	public static ReservedSlot ParseString(string line)
	{
		if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()))
		{
			return null;
		}
		string text = line.Trim();
		string comment = null;
		if (ContainsCommentSymbol(text))
		{
			string firstCommentSymbol = GetFirstCommentSymbol(text);
			comment = text.Remove(0, text.IndexOf(firstCommentSymbol) + firstCommentSymbol.Length);
			text = text.Substring(0, text.IndexOf(firstCommentSymbol)).Trim();
		}
		string[] array = text.Split(splitChars, 2);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		if (array.Length > 0 && string.IsNullOrEmpty(array[0]))
		{
			ServerConsole.AddLog("RESERVED_SLOTS # Error, invalid entry on line \"" + line + "\" - Maybe incorrect formatting?");
			return null;
		}
		if (array.Length == 2 && string.IsNullOrEmpty(array[1]))
		{
			array = new string[1] { array[0] };
		}
		switch (array.Length)
		{
		case 1:
			return (!IsSteamID(array[0])) ? new ReservedSlot(array[0], null, comment) : new ReservedSlot(null, array[0], comment);
		case 2:
			return new ReservedSlot(array[0], array[1], comment);
		default:
			ServerConsole.AddLog("RESERVED_SLOTS # Error, invalid entry on line \"" + line + "\" - Invalid number of splits (\"" + splitChars[0] + "\").");
			return null;
		}
	}

	public static bool IsSteamID(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return false;
		}
		input = input.Trim();
		if (input.Length != 17)
		{
			return false;
		}
		string text = input;
		foreach (char c in text)
		{
			if (!char.IsNumber(c))
			{
				return false;
			}
		}
		return true;
	}

	public static bool StartsWithCommentSymbol(string comment)
	{
		if (string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(comment.Trim()))
		{
			return false;
		}
		foreach (string commentSym in CommentSyms)
		{
			if (comment.StartsWith(commentSym) || comment.Trim().StartsWith(commentSym))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsCommentSymbol(string comment)
	{
		if (string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(comment.Trim()))
		{
			return false;
		}
		foreach (string commentSym in CommentSyms)
		{
			if (comment.Contains(commentSym))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetFirstCommentSymbol(string comment)
	{
		if (string.IsNullOrEmpty(comment) || string.IsNullOrEmpty(comment.Trim()))
		{
			return null;
		}
		string result = null;
		int num = -1;
		foreach (string commentSym in CommentSyms)
		{
			if (comment.Contains(commentSym))
			{
				int num2 = comment.IndexOf(commentSym);
				if (num < 0 || num2 < num)
				{
					result = commentSym;
					num = num2;
				}
			}
		}
		return result;
	}

	private static bool MakeFile()
	{
		try
		{
			if (!Directory.Exists(SMReservedSlotDir))
			{
				Directory.CreateDirectory(SMReservedSlotDir);
				return false;
			}
		}
		catch
		{
			ServerConsole.AddLog("RESERVED_SLOTS # Configuration file directory creation failed (" + (SMReservedSlotDir ?? "Null") + ").");
			return false;
		}
		try
		{
			if (!File.Exists(SMReservedSlotFile))
			{
				if (SMReservedSlotFile != SMOldReservedSlotFile && File.Exists(SMOldReservedSlotFile))
				{
					File.Copy(SMOldReservedSlotFile, SMReservedSlotFile);
					ServerConsole.AddLog("Automatically copied \"" + SMOldReservedSlotFile + "\" to \"" + SMReservedSlotFile + "\"");
					File.Move(SMOldReservedSlotFile, SMOldReservedSlotFile + ".backup");
					ServerConsole.AddLog("Automatically renamed \"" + SMOldReservedSlotFile + "\" to \"" + SMOldReservedSlotFile + ".backup\"");
					ServerConsole.AddLog("Your new Reserved Slots file will be \"" + SMReservedSlotFile + "\", this file should have all the same contents as what the old file had.");
				}
				else
				{
					File.Create(SMReservedSlotFile);
				}
				return false;
			}
		}
		catch
		{
			ServerConsole.AddLog("RESERVED_SLOTS # Configuration file creation failed (" + (SMReservedSlotFile ?? "Null") + ").");
			return false;
		}
		return true;
	}

	public void AppendToFile(int lineNum, bool allowDuplicates = false)
	{
		if (!MakeFile())
		{
			return;
		}
		string[] array = File.ReadAllLines(SMReservedSlotFile);
		if (array == null)
		{
			return;
		}
		List<string> list = new List<string>(array);
		if (!ContainsSlot(this) || allowDuplicates)
		{
			if (lineNum < 0)
			{
				list.Add(SlotEntry.Trim());
			}
			else
			{
				list.Insert(lineNum, SlotEntry.Trim());
			}
		}
		File.WriteAllLines(SMReservedSlotFile, list.ToArray());
	}

	public void AppendToFile(bool allowDuplicates = false)
	{
		AppendToFile(-1, allowDuplicates);
	}

	public static ReservedSlot[] GetSlots()
	{
		List<ReservedSlot> list = new List<ReservedSlot>();
		if (MakeFile())
		{
			string[] array = File.ReadAllLines(SMReservedSlotFile);
			for (int i = 0; i < array.Length; i++)
			{
				if (!ShouldIgnoreEntry(array[i]))
				{
					ReservedSlot reservedSlot = ParseString(array[i]);
					if (reservedSlot != null)
					{
						list.Add(reservedSlot);
					}
					else
					{
						ServerConsole.AddLog("RESERVED_SLOTS # Error on line number " + i + ", there's probably an error message directly above this.");
					}
				}
			}
		}
		return list.ToArray();
	}

	public static ReservedSlot SlotFromSteamID(string SteamID)
	{
		SteamID = SteamID.Trim();
		if (IsSteamID(SteamID))
		{
			GameObject[] players = PlayerManager.singleton.players;
			GameObject[] array = players;
			foreach (GameObject gameObject in array)
			{
				string steamId = gameObject.GetComponent<CharacterClassManager>().SteamId;
				string address = gameObject.GetComponent<NetworkIdentity>().connectionToClient.address;
				if (steamId == SteamID)
				{
					return new ReservedSlot(address, steamId, null);
				}
			}
		}
		return null;
	}

	public void UpdateFromSteamID()
	{
		if (string.IsNullOrEmpty(SteamID))
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			string steamId = gameObject.GetComponent<CharacterClassManager>().SteamId;
			string text = TrimIPAddress(gameObject.GetComponent<NetworkIdentity>().connectionToClient.address);
			if (steamId == SteamID && text != IP)
			{
				IP = text;
				UpdateSlotInFile();
				ServerConsole.AddLog("RESERVED_SLOTS # Updated SteamID slot \"" + SteamID + "\" with IP \"" + text + "\".");
				break;
			}
		}
	}

	public static string TrimIPAddress(string IPAddress)
	{
		if (string.IsNullOrEmpty(IPAddress))
		{
			return IPAddress;
		}
		string[] array = IPAddress.Trim().Split(':');
		if (array.Length <= 0)
		{
			return IPAddress;
		}
		return (array.Length <= 1) ? array[0].Trim() : array[array.Length - 1].Trim();
	}

	public void UpdateSlotInFile()
	{
		if (!MakeFile())
		{
			return;
		}
		string[] array = File.ReadAllLines(SMReservedSlotFile);
		for (int i = 0; i < array.Length; i++)
		{
			string text = "Checkpoint 1";
			try
			{
				if (!ShouldIgnoreEntry(array[i]))
				{
					text = "Checkpoint 2";
					ReservedSlot reservedSlot = ParseString(array[i]);
					text = "Checkpoint 3";
					if (reservedSlot != null && reservedSlot.SteamID == InitSteamID && reservedSlot.IP == InitIP)
					{
						array[i] = SlotEntry;
						text = "Checkpoint 4";
					}
				}
			}
			catch (Exception ex)
			{
				ServerConsole.AddLog("RESERVED_SLOTS # UpdateSlotInFile Error: " + ex.Message);
				ServerConsole.AddLog("RESERVED_SLOTS # UpdateSlotInFile Error StackTrace: " + ex.StackTrace);
				ServerConsole.AddLog("RESERVED_SLOTS # UpdateSlotInFile Error Message: " + text);
			}
		}
		File.WriteAllLines(SMReservedSlotFile, array);
	}

	public void RemoveSlotFromFile()
	{
		if (!MakeFile())
		{
			return;
		}
		List<string> list = new List<string>(File.ReadAllLines(SMReservedSlotFile));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string text = "Checkpoint 1";
			try
			{
				if (!ShouldIgnoreEntry(list[num]))
				{
					text = "Checkpoint 2";
					ReservedSlot reservedSlot = ParseString(list[num]);
					text = "Checkpoint 3";
					if (reservedSlot != null && reservedSlot.SteamID == InitSteamID && reservedSlot.IP == InitIP)
					{
						list.RemoveAt(num);
						text = "Checkpoint 4";
					}
				}
			}
			catch (Exception ex)
			{
				ServerConsole.AddLog("RESERVED_SLOTS # RemoveSlotFromFile Error: " + ex.Message);
				ServerConsole.AddLog("RESERVED_SLOTS # RemoveSlotFromFile Error StackTrace: " + ex.StackTrace);
				ServerConsole.AddLog("RESERVED_SLOTS # RemoveSlotFromFile Error Message: " + text);
			}
		}
		File.WriteAllLines(SMReservedSlotFile, list.ToArray());
	}

	private static bool ShouldIgnoreEntry(string entry)
	{
		return string.IsNullOrEmpty(entry) || string.IsNullOrEmpty(entry.Trim()) || StartsWithCommentSymbol(entry);
	}

	public static bool ContainsIP(string ipAddress)
	{
		if (string.IsNullOrEmpty(ipAddress))
		{
			return false;
		}
		ReservedSlot[] slots = GetSlots();
		foreach (ReservedSlot reservedSlot in slots)
		{
			if (reservedSlot.IP == TrimIPAddress(ipAddress))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsSteamID(string steamID)
	{
		if (string.IsNullOrEmpty(steamID))
		{
			return false;
		}
		ReservedSlot[] slots = GetSlots();
		foreach (ReservedSlot reservedSlot in slots)
		{
			if (reservedSlot.SteamID == steamID.Trim())
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsSlot(ReservedSlot containedSlot)
	{
		ReservedSlot[] slots = GetSlots();
		foreach (ReservedSlot reservedSlot in slots)
		{
			if (reservedSlot.IP == containedSlot.IP && reservedSlot.SteamID == containedSlot.SteamID)
			{
				return true;
			}
		}
		return false;
	}

	public static void UpdateSteamIDSlots()
	{
		ReservedSlot[] slots = GetSlots();
		foreach (ReservedSlot reservedSlot in slots)
		{
			reservedSlot.UpdateFromSteamID();
		}
	}
}
