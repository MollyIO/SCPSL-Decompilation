using System.Runtime.InteropServices;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class DisableUselessComponents : NetworkBehaviour
{
	private CharacterClassManager _ccm;

	private NicknameSync _ns;

	private bool _added;

	[SerializeField]
	private Behaviour[] uselessComponents;

	[SyncVar(hook = "SetName")]
	private string label = "Player";

	[SyncVar(hook = "SetServer")]
	public bool isDedicated = true;

	public string Networklabel
	{
		get
		{
			return label;
		}
		[param: In]
		set
		{
			DisableUselessComponents disableUselessComponents = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetName(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref disableUselessComponents.label, 1u);
		}
	}

	public bool NetworkisDedicated
	{
		get
		{
			return isDedicated;
		}
		[param: In]
		set
		{
			DisableUselessComponents disableUselessComponents = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetServer(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref disableUselessComponents.isDedicated, 2u);
		}
	}

	private void Start()
	{
		_ns = GetComponent<NicknameSync>();
		if (NetworkServer.active)
		{
			CmdSetName((!base.isLocalPlayer) ? "Player" : "Host", base.isLocalPlayer && ServerStatic.IsDedicated);
		}
		_ccm = GetComponent<CharacterClassManager>();
		if (!base.isLocalPlayer)
		{
			Object.DestroyImmediate(GetComponent<FirstPersonController>());
			Behaviour[] array = uselessComponents;
			foreach (Behaviour behaviour in array)
			{
				behaviour.enabled = false;
			}
			Object.Destroy(GetComponent<CharacterController>());
		}
		else
		{
			PlayerManager.localPlayer = base.gameObject;
			PlayerManager.spect = GetComponent<SpectatorManager>();
			GetComponent<FirstPersonController>().enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (!_added && _ccm.IsVerified && !string.IsNullOrEmpty(_ns.myNick))
		{
			_added = true;
			if (!isDedicated)
			{
				PlayerManager.singleton.AddPlayer(base.gameObject);
			}
			if (NetworkServer.active)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Networking, "Player connected and authenticated from IP " + base.connectionToClient.address + " with SteamID " + ((!string.IsNullOrEmpty(GetComponent<CharacterClassManager>().SteamId)) ? GetComponent<CharacterClassManager>().SteamId : "(unavailable)") + " and nickname " + GetComponent<NicknameSync>().myNick + ". Assigned Player ID: " + GetComponent<QueryProcessor>().PlayerId + ".", ServerLogs.ServerLogType.ConnectionUpdate);
			}
		}
		base.name = label;
	}

	private void OnDestroy()
	{
		if (!base.isLocalPlayer && PlayerManager.singleton != null)
		{
			PlayerManager.singleton.RemovePlayer(base.gameObject);
		}
	}

	[ServerCallback]
	private void CmdSetName(string n, bool b)
	{
		if (NetworkServer.active)
		{
			SetName(n);
			SetServer(b);
		}
	}

	private void SetName(string n)
	{
		Networklabel = n;
	}

	private void SetServer(bool b)
	{
		NetworkisDedicated = b;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(label);
			writer.Write(isDedicated);
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
			writer.Write(label);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isDedicated);
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
			label = reader.ReadString();
			isDedicated = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetName(reader.ReadString());
		}
		if ((num & 2) != 0)
		{
			SetServer(reader.ReadBoolean());
		}
	}
}
