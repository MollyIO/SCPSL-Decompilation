using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

public class DiscordRpc
{
	public delegate void OnDisconnectedInfo(int errorCode, string message);

	public delegate void OnErrorInfo(int errorCode, string message);

	public delegate void OnJoinInfo(string secret);

	public delegate void OnReadyInfo(ref DiscordUser connectedUser);

	public delegate void OnRequestInfo(ref DiscordUser request);

	public delegate void OnSpectateInfo(string secret);

	public enum Reply
	{
		No = 0,
		Yes = 1,
		Ignore = 2
	}

	public struct EventHandlers
	{
		public OnReadyInfo readyCallback;

		public OnDisconnectedInfo disconnectedCallback;

		public OnErrorInfo errorCallback;

		public OnJoinInfo joinCallback;

		public OnSpectateInfo spectateCallback;

		public OnRequestInfo requestCallback;
	}

	[Serializable]
	public struct RichPresenceStruct
	{
		public IntPtr state;

		public IntPtr details;

		public long startTimestamp;

		public long endTimestamp;

		public IntPtr largeImageKey;

		public IntPtr largeImageText;

		public IntPtr smallImageKey;

		public IntPtr smallImageText;

		public IntPtr partyId;

		public int partySize;

		public int partyMax;

		public IntPtr matchSecret;

		public IntPtr joinSecret;

		public IntPtr spectateSecret;

		public bool instance;
	}

	[Serializable]
	public struct RichPresencePrefab
	{
		public string state;

		public string details;

		public long startTimestamp;

		public long endTimestamp;

		public string largeImageKey;

		public string largeImageText;

		public string smallImageKey;

		public string smallImageText;

		public string partyId;

		public int partySize;

		public int partyMax;

		public string matchSecret;

		public string joinSecret;

		public string spectateSecret;

		public bool instance;
	}

	[Serializable]
	public struct DiscordUser
	{
		public string userId;

		public string username;

		public string discriminator;

		public string avatar;
	}

	public class RichPresence
	{
		private readonly List<IntPtr> _buffers = new List<IntPtr>(10);

		private RichPresenceStruct _presence;

		public string details;

		public long endTimestamp;

		public bool instance;

		public string joinSecret;

		public string largeImageKey;

		public string largeImageText;

		public string matchSecret;

		public string partyId;

		public int partyMax;

		public int partySize;

		public string smallImageKey;

		public string smallImageText;

		public string spectateSecret;

		public long startTimestamp;

		public string state;

		internal RichPresenceStruct GetStruct()
		{
			if (_buffers.Count > 0)
			{
				FreeMem();
			}
			_presence.state = StrToPtr(state);
			_presence.details = StrToPtr(details);
			_presence.startTimestamp = startTimestamp;
			_presence.endTimestamp = endTimestamp;
			_presence.largeImageKey = StrToPtr(largeImageKey);
			_presence.largeImageText = StrToPtr(largeImageText);
			_presence.smallImageKey = StrToPtr(smallImageKey);
			_presence.smallImageText = StrToPtr(smallImageText);
			_presence.partyId = StrToPtr(partyId);
			_presence.partySize = partySize;
			_presence.partyMax = partyMax;
			_presence.matchSecret = StrToPtr(matchSecret);
			_presence.joinSecret = StrToPtr(joinSecret);
			_presence.spectateSecret = StrToPtr(spectateSecret);
			_presence.instance = instance;
			return _presence;
		}

		private IntPtr StrToPtr(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return IntPtr.Zero;
			}
			int byteCount = Encoding.UTF8.GetByteCount(input);
			IntPtr intPtr = Marshal.AllocHGlobal(byteCount + 1);
			for (int i = 0; i < byteCount + 1; i++)
			{
				Marshal.WriteByte(intPtr, i, 0);
			}
			_buffers.Add(intPtr);
			Marshal.Copy(Encoding.UTF8.GetBytes(input), 0, intPtr, byteCount);
			return intPtr;
		}

		private static string StrToUtf8NullTerm(string toconv)
		{
			string text = toconv.Trim();
			byte[] bytes = Encoding.Default.GetBytes(text);
			if (bytes.Length > 0 && bytes[bytes.Length - 1] != 0)
			{
				text += "\0\0";
			}
			return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
		}

		internal void FreeMem()
		{
			for (int num = _buffers.Count - 1; num >= 0; num--)
			{
				Marshal.FreeHGlobal(_buffers[num]);
				_buffers.RemoveAt(num);
			}
		}
	}

	private static EventHandlers Callbacks { get; set; }

	[MonoPInvokeCallback(typeof(OnReadyInfo))]
	public static void ReadyCallback(ref DiscordUser connectedUser)
	{
		Callbacks.readyCallback(ref connectedUser);
	}

	[MonoPInvokeCallback(typeof(OnDisconnectedInfo))]
	public static void DisconnectedCallback(int errorCode, string message)
	{
		Callbacks.disconnectedCallback(errorCode, message);
	}

	[MonoPInvokeCallback(typeof(OnErrorInfo))]
	public static void ErrorCallback(int errorCode, string message)
	{
		Callbacks.errorCallback(errorCode, message);
	}

	[MonoPInvokeCallback(typeof(OnJoinInfo))]
	public static void JoinCallback(string secret)
	{
		Callbacks.joinCallback(secret);
	}

	[MonoPInvokeCallback(typeof(OnSpectateInfo))]
	public static void SpectateCallback(string secret)
	{
		Callbacks.spectateCallback(secret);
	}

	[MonoPInvokeCallback(typeof(OnRequestInfo))]
	public static void RequestCallback(ref DiscordUser request)
	{
		Callbacks.requestCallback(ref request);
	}

	public static RichPresence FromPrefab(RichPresencePrefab prefab)
	{
		RichPresence richPresence = new RichPresence();
		richPresence.state = prefab.state;
		richPresence.details = prefab.details;
		richPresence.startTimestamp = prefab.startTimestamp;
		richPresence.endTimestamp = prefab.endTimestamp;
		richPresence.largeImageKey = prefab.largeImageKey;
		richPresence.largeImageText = prefab.largeImageText;
		richPresence.smallImageKey = prefab.smallImageKey;
		richPresence.smallImageText = prefab.smallImageText;
		richPresence.partyId = prefab.partyId;
		richPresence.partySize = prefab.partySize;
		richPresence.partyMax = prefab.partyMax;
		richPresence.matchSecret = prefab.matchSecret;
		richPresence.joinSecret = prefab.joinSecret;
		richPresence.spectateSecret = prefab.spectateSecret;
		richPresence.instance = prefab.instance;
		return richPresence;
	}

	public static void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId)
	{
		Callbacks = handlers;
		EventHandlers handlers2 = default(EventHandlers);
		handlers2.readyCallback = (OnReadyInfo)Delegate.Combine(handlers2.readyCallback, new OnReadyInfo(ReadyCallback));
		handlers2.disconnectedCallback = (OnDisconnectedInfo)Delegate.Combine(handlers2.disconnectedCallback, new OnDisconnectedInfo(DisconnectedCallback));
		handlers2.errorCallback = (OnErrorInfo)Delegate.Combine(handlers2.errorCallback, new OnErrorInfo(ErrorCallback));
		handlers2.joinCallback = (OnJoinInfo)Delegate.Combine(handlers2.joinCallback, new OnJoinInfo(JoinCallback));
		handlers2.spectateCallback = (OnSpectateInfo)Delegate.Combine(handlers2.spectateCallback, new OnSpectateInfo(SpectateCallback));
		handlers2.requestCallback = (OnRequestInfo)Delegate.Combine(handlers2.requestCallback, new OnRequestInfo(RequestCallback));
		InitializeInternal(applicationId, ref handlers2, autoRegister, optionalSteamId);
	}

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Initialize")]
	private static extern void InitializeInternal(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Shutdown")]
	public static extern void Shutdown();

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_RunCallbacks")]
	public static extern void RunCallbacks();

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_UpdatePresence")]
	private static extern void UpdatePresenceNative(ref RichPresenceStruct presence);

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_ClearPresence")]
	public static extern void ClearPresence();

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Respond")]
	public static extern void Respond(string userId, Reply reply);

	[DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_UpdateHandlers")]
	public static extern void UpdateHandlers(ref EventHandlers handlers);

	public static void UpdatePresence(RichPresence presence)
	{
		RichPresenceStruct presence2 = presence.GetStruct();
		UpdatePresenceNative(ref presence2);
		presence.FreeMem();
	}
}
