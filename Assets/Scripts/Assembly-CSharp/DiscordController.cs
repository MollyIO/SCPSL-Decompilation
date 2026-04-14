using System;
using System.Linq;
using System.Text;
using GameConsole;
using TMPro;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
	public const string applicationId = "420676877766623232";

	public const string optionalSteamId = "700330";

	private static GameConsole.Console console;

	private static DiscordRpc.EventHandlers handlers;

	public static Animator joinAnimator;

	public static DiscordRpc.DiscordUser joinRequest;

	public static TextMeshProUGUI joinText;

	public static DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();

	public static void RequestRespondYes()
	{
		try
		{
			Debug.Log("Discord: responding yes to Ask to Join request");
			joinAnimator.SetBool("Requested", false);
			console.AddLog("Discord: Accepted join request.", new Color32(114, 137, 218, byte.MaxValue));
			DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void RequestRespondNo()
	{
		try
		{
			Debug.Log("Discord: responding no to Ask to Join request");
			joinAnimator.SetBool("Requested", false);
			console.AddLog("Discord: Join request rejected.", new Color32(114, 137, 218, byte.MaxValue));
			DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
	{
		try
		{
			Debug.Log(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
			console.AddLog("Discord: ready!", new Color32(114, 137, 218, byte.MaxValue));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void DisconnectedCallback(int errorCode, string message)
	{
		try
		{
			Debug.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void ErrorCallback(int errorCode, string message)
	{
		try
		{
			Debug.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
			console.AddLog(string.Format("Discord: error - {0} ({1})", errorCode, message), new Color32(114, 137, 218, byte.MaxValue));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void JoinCallback(string secret)
	{
		try
		{
			Debug.Log(string.Format("Discord: join ({0})", secret));
			string text = Encoding.UTF8.GetString(Convert.FromBase64String(secret));
			try
			{
				CustomNetworkManager customNetworkManager = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
				string[] ipAndPort = text.Split(':');
				int result = 0;
				if (!int.TryParse(ipAndPort[1], out result))
				{
					throw new Exception("No specified port Exception");
				}
				customNetworkManager.networkAddress = ipAndPort[0];
				CustomNetworkManager.ConnectionIp = ipAndPort[0];
				customNetworkManager.networkPort = result;
				if (CustomNetworkManager.CompatibleVersions.Any((string item) => item == ipAndPort[2]))
				{
					customNetworkManager.ShowLog(13, string.Empty, string.Empty);
					customNetworkManager.StartClient();
				}
				else
				{
					console.AddLog("Discord: Could not join the server - version mismatch.", new Color32(114, 137, 218, byte.MaxValue));
				}
			}
			catch
			{
				console.AddLog("Discord: Could not join the server - incorrect IP address - " + text, new Color32(114, 137, 218, byte.MaxValue));
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void SpectateCallback(string secret)
	{
		try
		{
			Debug.Log(string.Format("Discord: spectate ({0})", secret));
			console.AddLog("Discord: SpectateCallback fired.", new Color32(114, 137, 218, byte.MaxValue));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void RequestCallback(ref DiscordRpc.DiscordUser request)
	{
		try
		{
			Debug.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
			joinAnimator.SetBool("Requested", true);
			joinText.text = string.Format("<b><color=#7289DA>{0}<color=#99AAB5>#</color>{1}</color></b> would like to join your match!", request.username, request.discriminator);
			console.AddLog(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId), new Color32(114, 137, 218, byte.MaxValue));
			joinRequest = request;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Start()
	{
		try
		{
			joinAnimator = GameObject.Find("Join").GetComponent<Animator>();
			joinText = GameObject.Find("Nickname").GetComponent<TextMeshProUGUI>();
			DiscordRpc.UpdatePresence(presence);
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			long startTimestamp = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
			presence.startTimestamp = startTimestamp;
			console = GameConsole.Console.singleton;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void Update()
	{
		try
		{
			DiscordRpc.RunCallbacks();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnEnable()
	{
		try
		{
			Debug.Log("Discord: init");
			handlers = default(DiscordRpc.EventHandlers);
			handlers.readyCallback = (DiscordRpc.OnReadyInfo)Delegate.Combine(handlers.readyCallback, new DiscordRpc.OnReadyInfo(ReadyCallback));
			handlers.disconnectedCallback = (DiscordRpc.OnDisconnectedInfo)Delegate.Combine(handlers.disconnectedCallback, new DiscordRpc.OnDisconnectedInfo(DisconnectedCallback));
			handlers.errorCallback = (DiscordRpc.OnErrorInfo)Delegate.Combine(handlers.errorCallback, new DiscordRpc.OnErrorInfo(ErrorCallback));
			handlers.joinCallback = (DiscordRpc.OnJoinInfo)Delegate.Combine(handlers.joinCallback, new DiscordRpc.OnJoinInfo(JoinCallback));
			handlers.spectateCallback = (DiscordRpc.OnSpectateInfo)Delegate.Combine(handlers.spectateCallback, new DiscordRpc.OnSpectateInfo(SpectateCallback));
			handlers.requestCallback = (DiscordRpc.OnRequestInfo)Delegate.Combine(handlers.requestCallback, new DiscordRpc.OnRequestInfo(RequestCallback));
			DiscordRpc.Initialize("420676877766623232", ref handlers, true, "700330");
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void OnDisable()
	{
		try
		{
			Debug.Log("Discord: shutdown");
			DiscordRpc.Shutdown();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
