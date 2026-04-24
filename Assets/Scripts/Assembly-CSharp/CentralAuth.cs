using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

    private bool _requestInProgress;

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
            if (!TryGetSteamTicketData(false))
            {
                GameConsole.Console.singleton.AddLog("Failed to obtain steam auth ticket", Color.red);
                return;
            }

            GameConsole.Console.singleton.AddLog("Ticked obtained from steam.", Color.blue);
            _responded = true;
        }
    }

    private bool TryGetSteamTicketData(bool forceRefresh)
    {
        if (forceRefresh)
        {
            SteamManager.CancelTicket();
        }

        var authTicket = SteamManager.GetAuthSessionTicket();
        if (authTicket == null || authTicket.Data == null || authTicket.Data.Length == 0)
        {
            return false;
        }

        m_Ticket = authTicket.Data;
        hexticket = BuildHexTicket(m_Ticket, false);
        return true;
    }

    private static string BuildHexTicket(byte[] ticket, bool trimTrailingZeros)
    {
        if (ticket == null || ticket.Length == 0)
        {
            return string.Empty;
        }

        var length = ticket.Length;
        if (trimTrailingZeros)
        {
            while (length > 0 && ticket[length - 1] == 0)
            {
                length--;
            }
        }

        if (length <= 0)
        {
            return string.Empty;
        }

        if (length == ticket.Length)
        {
            return BitConverter.ToString(ticket).Replace("-", string.Empty);
        }

        var resized = new byte[length];
        Buffer.BlockCopy(ticket, 0, resized, 0, length);
        return BitConverter.ToString(resized).Replace("-", string.Empty);
    }

    private void Update()
    {
        if (_responded && !_requestInProgress)
        {
            _responded = false;
            _requestInProgress = true;
            Timing.RunCoroutine(_RequestToken(), Segment.FixedUpdate);
        }

        if (!string.IsNullOrEmpty(_roleToRequest) && PlayerManager.localPlayer != null &&
            !string.IsNullOrEmpty(PlayerManager.localPlayer.GetComponent<NicknameSync>().myNick))
        {
            GameConsole.Console.singleton.AddLog("Requesting your global badge...", Color.yellow);
            _ica.RequestBadge(_roleToRequest);
            _roleToRequest = string.Empty;
        }
    }

    private IEnumerator<float> _RequestToken()
    {
        bool shouldRetry;
        bool retriedWithFreshTicket = false;
        bool retriedWithTrimmedTicket = false;
        while (true)
        {
            GameConsole.Console.singleton.AddLog("Requesting signature from central servers...", Color.blue);

            var requestUrl = CentralServer.StandardUrl + "requestsignature.php";
            var requestData = BuildTokenRequestData();
            string responseText;
            string requestError;
            int statusCode;

            var done = false;
            responseText = string.Empty;
            requestError = null;
            statusCode = 0;

            new Thread(() =>
            {
                try
                {
                    requestError = SendTokenRequest(requestUrl, requestData, out responseText, out statusCode);
                }
                finally
                {
                    done = true;
                }
            }).Start();

            while (!done)
            {
                yield return 0f;
            }

            shouldRetry = false;
            if (string.IsNullOrEmpty(requestError))
            {
                try
                {
                    if (File.Exists(FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "EnableDebug.txt") ||
                        GameConsole.Console.StartupArgs.Contains("-authdebug"))
                    {
                        var array = responseText.Replace("<br>", "\n").Split('\n');
                        foreach (var text in array)
                        {
                            GameConsole.Console.singleton.AddLog("[AUTH DEBUG] " + text, Color.cyan);
                        }
                    }

                    GameConsole.Console.singleton.AddLog("Sending your authentication token to game server...",
                        Color.green);
                    var array3 = responseText.Split(new[] { "=== SECTION ===<br>" }, StringSplitOptions.None);
                    _ica.TokenGenerated(array3[0]);
                    if (array3[1] != "-")
                    {
                        _roleToRequest = array3[1];
                        GlobalBadgeIssued = true;
                    }
                    else
                    {
                        GameConsole.Console.singleton.AddLog("Your account doesn't have any global permissions.",
                            Color.cyan);
                        GlobalBadgeIssued = false;
                    }

                    break;
                }
                catch (Exception ex)
                {
                    GameConsole.Console.singleton.AddLog(
                        "Error during requesting authentication token: " + ex.Message + ". StackTrace: " +
                        ex.StackTrace, Color.red);
                    GameConsole.Console.singleton.AddLog("StackTrace: " + ex.StackTrace, Color.red);
                    break;
                }
            }

            var isForbidden = statusCode == 403 ||
                              (!string.IsNullOrEmpty(requestError) &&
                               requestError.IndexOf("403", StringComparison.OrdinalIgnoreCase) >= 0);

            if (!retriedWithFreshTicket && isForbidden && TryGetSteamTicketData(true))
            {
                retriedWithFreshTicket = true;
                GameConsole.Console.singleton.AddLog("Steam auth ticket refreshed, retrying...", Color.yellow);
                shouldRetry = true;
                continue;
            }

            if (!retriedWithTrimmedTicket && isForbidden)
            {
                var trimmedHex = BuildHexTicket(m_Ticket, true);
                if (!string.IsNullOrEmpty(trimmedHex) && trimmedHex != hexticket)
                {
                    hexticket = trimmedHex;
                    retriedWithTrimmedTicket = true;
                    GameConsole.Console.singleton.AddLog("Steam auth ticket trimmed, retrying...", Color.yellow);
                    shouldRetry = true;
                    continue;
                }
            }

            if (isForbidden)
            {
                var debugBody = string.IsNullOrEmpty(responseText) ? "(empty)" : responseText;
                if (debugBody.Length > 300)
                {
                    debugBody = debugBody.Substring(0, 300);
                }

                GameConsole.Console.singleton.AddLog(
                    "Auth 403 details: URL=" + requestUrl + ", TicketHexLen=" +
                    ((hexticket != null) ? hexticket.Length : 0) + ", Body=" + debugBody, Color.yellow);
                Debug.LogError(
                    "Auth 403 details: URL=" + requestUrl + ", TicketHexLen=" +
                    ((hexticket != null) ? hexticket.Length : 0) + ", Body=" + debugBody);
            }

            GameConsole.Console.singleton.AddLog(
                "Could not request token - " + requestError + " " + CentralServer.SelectedServer, Color.red);
            Debug.LogError("Could not request token - " + requestError + " " + CentralServer.SelectedServer);

            if (CentralServer.ChangeCentralServer(true))
            {
                retriedWithFreshTicket = false;
                retriedWithTrimmedTicket = false;
                shouldRetry = true;
                continue;
            }

            break;
        }

        _requestInProgress = false;

        if (shouldRetry)
        {
            _responded = true;
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
            var text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
            var text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
            text2 = text2.Replace("<br>", string.Empty);
            if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
            {
                ServerConsole.AddLog("Authentication token signature mismatch.");
                icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient,
                    "Authentication token rejected due to signature mismatch.", "red");
                icaa.FailToken("Failed to validate authentication token signature.");
            }
            else
            {
                var source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
                var dictionary = source
                    .Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None))
                    .ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
                if (dictionary["Usage"] != "Authentication")
                {
                    ServerConsole.AddLog("Player tried to use token not issued to authentication purposes.");
                    icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient,
                        "Authentication token rejected due to invalid purpose of signature.", "red");
                    _ica.FailToken("Token supplied by your game can't be used for authentication purposes.");
                }
                else if (dictionary["Test signature"] != "NO" && !CentralServer.TestServer)
                {
                    ServerConsole.AddLog("Player tried to use authentication token issued only for testing. Server: " +
                                         dictionary["Issued by"] + ".");
                    icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient,
                        "Authentication token rejected due to testing signature.", "red");
                    _ica.FailToken("Your authentication token is issued only for testing purposes.");
                }
                else
                {
                    var dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
                    var dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
                    if (dateTime < DateTime.UtcNow)
                    {
                        ServerConsole.AddLog("Player tried to use expired authentication token. Server: " +
                                             dictionary["Issued by"] + ".");
                        ServerConsole.AddLog(
                            "Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
                        icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient,
                            "Authentication token rejected due to expired signature.", "red");
                        _ica.FailToken("Your authentication token has expired.");
                    }
                    else if (dateTime2 > DateTime.UtcNow.AddMinutes(20.0))
                    {
                        ServerConsole.AddLog("Player tried to use non-issued authentication token. Server: " +
                                             dictionary["Issued by"] + ".");
                        ServerConsole.AddLog(
                            "Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
                        icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient,
                            "Authentication token rejected due to non-issued signature.", "red");
                        _ica.FailToken("Your authentication token has invalid issuance date.");
                    }
                    else if (CustomNetworkManager.isPrivateBeta && (!dictionary.ContainsKey("Private beta ownership") ||
                                                                    dictionary["Private beta ownership"] != "YES"))
                    {
                        ServerConsole.AddLog("Player " + dictionary["Steam ID"] +
                                             " tried to join this server, but is not Private Beta DLC owner. Server: " +
                                             dictionary["Issued by"] + ".");
                        icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient,
                            "Private Beta DLC ownership is required to join private beta server.", "red");
                        _ica.FailToken("Private Beta DLC ownership is required to join private beta server.");
                    }
                    else
                    {
                        icaa.GetCcm().GetComponent<ServerRoles>().FirstVerResult = dictionary;
                        icaa.Ok(dictionary["Steam ID"], dictionary["Nickname"], dictionary["Global ban"],
                            dictionary["Steam ban"], dictionary["Issued by"], dictionary["Bypass bans"] == "YES",
                            dictionary.ContainsKey("Do Not Track") && dictionary["Do Not Track"] == "YES");
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
            var text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
            var text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
            text2 = text2.Replace("<br>", string.Empty);
            if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
            {
                GameConsole.Console.singleton.AddLog("Authentication token rejected due to signature mismatch.",
                    Color.red);
                return "-1";
            }

            var source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
            var dictionary = source
                .Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None))
                .ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
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
                GameConsole.Console.singleton.AddLog(
                    "Authentication token rejected due to nickname mismatch (token issued for " +
                    Misc.Base64Decode(dictionary["Nickname"]) + ").", Color.red);
                return "-1";
            }

            var dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
            var dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
            if (dateTime < DateTime.UtcNow.AddMinutes(-45.0))
            {
                GameConsole.Console.singleton.AddLog("Authentication token rejected due to expiration date.",
                    Color.red);
                return "-1";
            }

            if (dateTime2 > DateTime.UtcNow.AddMinutes(45.0))
            {
                GameConsole.Console.singleton.AddLog("Authentication token rejected due to issuance date.", Color.red);
                return "-1";
            }

            GameConsole.Console.singleton.AddLog(
                "Accepted verification token of user " + dictionary["Steam ID"] + " - " +
                Misc.Base64Decode(dictionary["Nickname"]) + " signed by " + dictionary["Issued by"] + ".", Color.green);
            return dictionary["Steam ID"];
        }
        catch (Exception ex)
        {
            GameConsole.Console.singleton.AddLog("Error during authentication token verification: " + ex.Message,
                Color.red);
            return "-1";
        }
    }

    internal static Dictionary<string, string> ValidateBadgeRequest(string token, string steamid, string nickname)
    {
        try
        {
            var text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
            var text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
            text2 = text2.Replace("<br>", string.Empty);
            if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
            {
                ServerConsole.AddLog("Badge request signature mismatch.");
                return null;
            }

            var source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
            var dictionary = source
                .Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None))
                .ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
            if (dictionary["Usage"] != "Badge request")
            {
                ServerConsole.AddLog("Player tried to use token not issued to request a badge.");
                return null;
            }

            if (dictionary["Test signature"] != "NO")
            {
                ServerConsole.AddLog("Player tried to use badge request token issued only for testing. Server: " +
                                     dictionary["Issued by"] + ".");
                return null;
            }

            if (dictionary["Steam ID"] != steamid && !string.IsNullOrEmpty(steamid))
            {
                ServerConsole.AddLog(
                    "Player tried to use badge request token issued for different user (Steam ID mismatch). Server: " +
                    dictionary["Issued by"] + ".");
                return null;
            }

            if (Misc.Base64Decode(dictionary["Nickname"]) != nickname)
            {
                ServerConsole.AddLog(
                    "Player tried to use badge request token issued for different user (nickname mismatch). Server: " +
                    dictionary["Issued by"] + ".");
                return null;
            }

            var dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
            var dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
            if (dateTime < DateTime.UtcNow)
            {
                ServerConsole.AddLog("Player tried to use expired badge request token. Server: " +
                                     dictionary["Issued by"] + ".");
                ServerConsole.AddLog(
                    "Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
                return null;
            }

            if (dateTime2 > DateTime.UtcNow.AddMinutes(20.0))
            {
                ServerConsole.AddLog("Player tried to use non-issued badge request token. Server: " +
                                     dictionary["Issued by"] + ".");
                ServerConsole.AddLog(
                    "Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
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

    private string BuildTokenRequestData()
    {
        var payload = "publickey=" + Uri.EscapeDataString(Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(GameConsole.Console.SessionKeys.Public)))) +
                      "&ticket=" + Uri.EscapeDataString(hexticket ?? string.Empty);

        if (GameConsole.Console.RequestDNT)
        {
            payload += "&DNT=true";
        }

        if (CustomNetworkManager.isPrivateBeta)
        {
            payload += "&privatebeta=true";
        }

        return payload;
    }

    private static string SendTokenRequest(string url, string data, out string responseText, out int statusCode)
    {
        responseText = string.Empty;
        statusCode = 0;

        try
        {
            var bytes = Encoding.UTF8.GetBytes(data ?? string.Empty);
            var webRequest = WebRequest.Create(url);
            ServicePointManager.Expect100Continue = true;
            var httpWebRequest = (HttpWebRequest)webRequest;
            httpWebRequest.UserAgent = "SCP SL";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = bytes.Length;

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                statusCode = (int)response.StatusCode;
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            responseText = streamReader.ReadToEnd();
                        }
                    }
                }
            }

            return null;
        }
        catch (WebException ex)
        {
            if (ex.Response is HttpWebResponse errorResponse)
            {
                statusCode = (int)errorResponse.StatusCode;
                using (errorResponse)
                {
                    using (var responseStream = errorResponse.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                responseText = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return ex.Message;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}