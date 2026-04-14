using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cryptography;
using GameConsole;
using MEC;
using UnityEngine;

public class CentralAuth : MonoBehaviour
{
	public static bool GlobalBadgeIssued;

	private byte[] m_Ticket = new byte[1024];

	private string hexticket;

	private string _roleToRequest;

	private ICentralAuth _ica;

	private bool _responded;

	public static CentralAuth singleton;

	private void Awake()
	{
		singleton = this;
	}

	public void GenerateToken(ICentralAuth icaa)
	{
		if (SteamManager.Running)
		{
			GameConsole.Console.singleton.AddLog("Obtaining ticket from Steam...", Color.blue);
			_ica = icaa;
			m_Ticket = SteamManager.GetAuthSessionTicket().Data;
			GameConsole.Console.singleton.AddLog("Ticked obtained from steam.", Color.blue);
			hexticket = BitConverter.ToString(m_Ticket).Replace("-", string.Empty);
			_responded = true;
		}
	}

	private void Update()
	{
		if (_responded)
		{
			_responded = false;
			Timing.RunCoroutine(_RequestToken(), Segment.FixedUpdate);
		}
		if (!string.IsNullOrEmpty(_roleToRequest) && PlayerManager.localPlayer != null && !string.IsNullOrEmpty(PlayerManager.localPlayer.GetComponent<NicknameSync>().myNick))
		{
			GameConsole.Console.singleton.AddLog("Requesting your global badge...", Color.yellow);
			_ica.RequestBadge(_roleToRequest);
			_roleToRequest = string.Empty;
		}
	}

	private IEnumerator<float> _RequestToken()
	{
		while (true)
		{
			GameConsole.Console.singleton.AddLog("Requesting signature from central servers...", Color.blue);
			WWWForm form = new WWWForm();
			form.AddField("publickey", Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(GameConsole.Console.SessionKeys.Public))));
			form.AddField("ticket", hexticket);
			if (GameConsole.Console.RequestDNT)
			{
				form.AddField("DNT", "true");
			}
			if (CustomNetworkManager.isPrivateBeta)
			{
				form.AddField("privatebeta", "true");
			}
			using (WWW www = new WWW(CentralServer.StandardUrl + "requestsignature.php", form))
			{
				yield return Timing.WaitUntilDone(www);
				if (string.IsNullOrEmpty(www.error))
				{
					try
					{
						if (File.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "EnableDebug.txt") || GameConsole.Console.StartupArgs.Contains("-authdebug"))
						{
							string[] array = www.text.Replace("<br>", "\n").Split('\n');
							string[] array2 = array;
							foreach (string text in array2)
							{
								GameConsole.Console.singleton.AddLog("[AUTH DEBUG] " + text, Color.cyan);
							}
						}
						GameConsole.Console.singleton.AddLog("Sending your authentication token to game server...", Color.green);
						string[] array3 = www.text.Split(new string[1] { "=== SECTION ===<br>" }, StringSplitOptions.None);
						_ica.TokenGenerated(array3[0]);
						if (array3[1] != "-")
						{
							_roleToRequest = array3[1];
							GlobalBadgeIssued = true;
						}
						else
						{
							GameConsole.Console.singleton.AddLog("Your account doesn't have any global permissions.", Color.cyan);
							GlobalBadgeIssued = false;
						}
						break;
					}
					catch (Exception ex)
					{
						GameConsole.Console.singleton.AddLog("Error during requesting authentication token: " + ex.Message + ". StackTrace: " + ex.StackTrace, Color.red);
						GameConsole.Console.singleton.AddLog("StackTrace: " + ex.StackTrace, Color.red);
						break;
					}
				}
				GameConsole.Console.singleton.AddLog("Could not request token - " + www.error + " " + CentralServer.SelectedServer, Color.red);
				Debug.LogError("Could not request token - " + www.error + " " + CentralServer.SelectedServer);
				if (CentralServer.ChangeCentralServer(true))
				{
					continue;
				}
				break;
			}
		}
	}

	public void StartValidateToken(ICentralAuth icaa, string token)
	{
		Timing.RunCoroutine(_ValidateToken(icaa, token), Segment.FixedUpdate);
	}

	private IEnumerator<float> _ValidateToken(ICentralAuth icaa, string token)
	{
		try
		{
			string text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
			string text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
			text2 = text2.Replace("<br>", string.Empty);
			if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
			{
				ServerConsole.AddLog("Authentication token signature mismatch.");
				icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to signature mismatch.", "red");
				icaa.FailToken("Failed to validate authentication token signature.");
			}
			else
			{
				string[] source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
				Dictionary<string, string> dictionary = source.Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None)).ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
				if (dictionary["Usage"] != "Authentication")
				{
					ServerConsole.AddLog("Player tried to use token not issued to authentication purposes.");
					icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to invalid purpose of signature.", "red");
					_ica.FailToken("Token supplied by your game can't be used for authentication purposes.");
				}
				else if (dictionary["Test signature"] != "NO" && !CentralServer.TestServer)
				{
					ServerConsole.AddLog("Player tried to use authentication token issued only for testing. Server: " + dictionary["Issued by"] + ".");
					icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to testing signature.", "red");
					_ica.FailToken("Your authentication token is issued only for testing purposes.");
				}
				else
				{
					DateTime dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
					DateTime dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
					if (dateTime < DateTime.UtcNow)
					{
						ServerConsole.AddLog("Player tried to use expired authentication token. Server: " + dictionary["Issued by"] + ".");
						ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
						icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to expired signature.", "red");
						_ica.FailToken("Your authentication token has expired.");
					}
					else if (dateTime2 > DateTime.UtcNow.AddMinutes(20.0))
					{
						ServerConsole.AddLog("Player tried to use non-issued authentication token. Server: " + dictionary["Issued by"] + ".");
						ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
						icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to non-issued signature.", "red");
						_ica.FailToken("Your authentication token has invalid issuance date.");
					}
					else if (CustomNetworkManager.isPrivateBeta && (!dictionary.ContainsKey("Private beta ownership") || dictionary["Private beta ownership"] != "YES"))
					{
						ServerConsole.AddLog("Player " + dictionary["Steam ID"] + " tried to join this server, but is not Private Beta DLC owner. Server: " + dictionary["Issued by"] + ".");
						icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Private Beta DLC ownership is required to join private beta server.", "red");
						_ica.FailToken("Private Beta DLC ownership is required to join private beta server.");
					}
					else
					{
						icaa.GetCcm().GetComponent<ServerRoles>().FirstVerResult = dictionary;
						icaa.Ok(dictionary["Steam ID"], dictionary["Nickname"], dictionary["Global ban"], dictionary["Steam ban"], dictionary["Issued by"], dictionary["Bypass bans"] == "YES", dictionary.ContainsKey("Do Not Track") && dictionary["Do Not Track"] == "YES");
					}
				}
			}
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Error during authentication token verification: " + ex.Message);
			icaa.Fail();
		}
		yield return 0f;
	}

	internal static string ValidateForGlobalBanning(string token, string nickname)
	{
		try
		{
			string text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
			string text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
			text2 = text2.Replace("<br>", string.Empty);
			if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to signature mismatch.", Color.red);
				return "-1";
			}
			string[] source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
			Dictionary<string, string> dictionary = source.Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None)).ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
			if (dictionary["Usage"] != "Authentication")
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to usage mismatch.", Color.red);
				return "-1";
			}
			if (dictionary["Test signature"] != "NO")
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to test flag.", Color.red);
				return "-1";
			}
			if (Misc.Base64Decode(dictionary["Nickname"]) != nickname)
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to nickname mismatch (token issued for " + Misc.Base64Decode(dictionary["Nickname"]) + ").", Color.red);
				return "-1";
			}
			DateTime dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
			DateTime dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
			if (dateTime < DateTime.UtcNow.AddMinutes(-45.0))
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to expiration date.", Color.red);
				return "-1";
			}
			if (dateTime2 > DateTime.UtcNow.AddMinutes(45.0))
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to issuance date.", Color.red);
				return "-1";
			}
			GameConsole.Console.singleton.AddLog("Accepted verification token of user " + dictionary["Steam ID"] + " - " + Misc.Base64Decode(dictionary["Nickname"]) + " signed by " + dictionary["Issued by"] + ".", Color.green);
			return dictionary["Steam ID"];
		}
		catch (Exception ex)
		{
			GameConsole.Console.singleton.AddLog("Error during authentication token verification: " + ex.Message, Color.red);
			return "-1";
		}
	}

	internal static Dictionary<string, string> ValidateBadgeRequest(string token, string steamid, string nickname)
	{
		try
		{
			string text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
			string text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
			text2 = text2.Replace("<br>", string.Empty);
			if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
			{
				ServerConsole.AddLog("Badge request signature mismatch.");
				return null;
			}
			string[] source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
			Dictionary<string, string> dictionary = source.Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None)).ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
			if (dictionary["Usage"] != "Badge request")
			{
				ServerConsole.AddLog("Player tried to use token not issued to request a badge.");
				return null;
			}
			if (dictionary["Test signature"] != "NO")
			{
				ServerConsole.AddLog("Player tried to use badge request token issued only for testing. Server: " + dictionary["Issued by"] + ".");
				return null;
			}
			if (dictionary["Steam ID"] != steamid && !string.IsNullOrEmpty(steamid))
			{
				ServerConsole.AddLog("Player tried to use badge request token issued for different user (Steam ID mismatch). Server: " + dictionary["Issued by"] + ".");
				return null;
			}
			if (Misc.Base64Decode(dictionary["Nickname"]) != nickname)
			{
				ServerConsole.AddLog("Player tried to use badge request token issued for different user (nickname mismatch). Server: " + dictionary["Issued by"] + ".");
				return null;
			}
			DateTime dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
			DateTime dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
			if (dateTime < DateTime.UtcNow)
			{
				ServerConsole.AddLog("Player tried to use expired badge request token. Server: " + dictionary["Issued by"] + ".");
				ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
				return null;
			}
			if (dateTime2 > DateTime.UtcNow.AddMinutes(20.0))
			{
				ServerConsole.AddLog("Player tried to use non-issued badge request token. Server: " + dictionary["Issued by"] + ".");
				ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
				return null;
			}
			return dictionary;
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Error during badge request token verification: " + ex.Message);
			Debug.Log("Error during badge request token verification: " + ex.Message + " StackTrace: " + ex.StackTrace);
			return null;
		}
	}
}
