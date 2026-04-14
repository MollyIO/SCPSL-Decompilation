using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class GrenadeManager : NetworkBehaviour
{
	public GrenadeSettings[] availableGrenades;

	public static List<Grenade> grenadesOnScene;

	private Inventory inv;

	public static bool flashfire;

	private bool isThrowing;

	private int throwInteger;

	internal static int GrenadeChainLimit;

	internal static int GrenadeChainLengthLimit;

	[SyncVar]
	private bool _syncFlashfire;

	private static int kCmdCmdThrowGrenade;

	private static int kRpcRpcThrowGrenade;

	private static int kRpcRpcExplode;

	private static int kRpcRpcUpdate;

	public bool Network_syncFlashfire
	{
		get
		{
			return _syncFlashfire;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _syncFlashfire, 1u);
		}
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			Network_syncFlashfire = ConfigFile.ServerConfig.GetBool("friendly_flash");
			GrenadeChainLimit = ConfigFile.ServerConfig.GetInt("grenade_chain_limit", 10);
			GrenadeChainLengthLimit = ConfigFile.ServerConfig.GetInt("grenade_chain_length_limit", 4);
		}
		inv = GetComponent<Inventory>();
		if (base.isLocalPlayer)
		{
			grenadesOnScene = new List<Grenade>();
		}
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			CheckForInput();
		}
		if (base.name == "Host")
		{
			flashfire = _syncFlashfire;
		}
	}

	private void CheckForInput()
	{
		bool keyDown = Input.GetKeyDown(NewInput.GetKey("Shoot"));
		bool keyDown2 = Input.GetKeyDown(NewInput.GetKey("Zoom"));
		if (isThrowing || (!keyDown && !keyDown2) || !(Inventory.inventoryCooldown <= 0f) || Cursor.visible)
		{
			return;
		}
		for (int i = 0; i < availableGrenades.Length; i++)
		{
			if (availableGrenades[i].inventoryID == inv.curItem)
			{
				isThrowing = true;
				Timing.RunCoroutine(_ThrowGrenade(i, keyDown2), Segment.FixedUpdate);
				break;
			}
		}
	}

	[Server]
	public void ChangeIntoGrenade(Pickup pickup, int id, int ti_pid, int ti_int, Vector3 dir, Vector3 pos, int chain)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GrenadeManager::ChangeIntoGrenade(Pickup,System.Int32,System.Int32,System.Int32,UnityEngine.Vector3,UnityEngine.Vector3,System.Int32)' called on client");
			return;
		}
		pickup.Delete();
		CallRpcThrowGrenade(id, ti_pid, ti_int, dir, true, pos, false, chain);
	}

	private IEnumerator<float> _ThrowGrenade(int gId, bool slow)
	{
		GetComponent<MicroHID_GFX>().onFire = true;
		inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>().SetBool("Throw", true);
		for (int i = 1; (float)i <= availableGrenades[gId].throwAnimationDuration * 50f; i++)
		{
			yield return 0f;
		}
		throwInteger++;
		float throwForce = ((!slow) ? 1f : 0.5f) * availableGrenades[gId].throwForce;
		Grenade g = Object.Instantiate(availableGrenades[gId].grenadeInstance).GetComponent<Grenade>();
		g.id = GetComponent<QueryProcessor>().PlayerId + ":" + throwInteger;
		grenadesOnScene.Add(g);
		g.SyncMovement(availableGrenades[gId].GetStartPos(base.gameObject), (GetComponent<Scp049PlayerScript>().plyCam.transform.forward + Vector3.up / 4f).normalized * throwForce, Quaternion.Euler(availableGrenades[gId].startRotation), availableGrenades[gId].angularVelocity);
		CallCmdThrowGrenade(gId, throwInteger, GetComponent<Scp049PlayerScript>().plyCam.transform.forward, slow);
		inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>().SetBool("Throw", false);
		GetComponent<MicroHID_GFX>().onFire = false;
		inv.SetCurItem(-1);
		isThrowing = false;
	}

	[Command]
	private void CmdThrowGrenade(int id, int ti, Vector3 direction, bool slowThrow)
	{
		for (int i = 0; i < inv.items.Count; i++)
		{
			if (inv.items[i].id == availableGrenades[id].inventoryID)
			{
				CallRpcThrowGrenade(id, GetComponent<QueryProcessor>().PlayerId, ti, direction.normalized, false, Vector3.zero, slowThrow, 0);
				inv.items.RemoveAt(i);
				break;
			}
		}
	}

	[ClientRpc]
	private void RpcThrowGrenade(int id, int ti_pid, int ti_int, Vector3 dir, bool isEnvironmentallyTriggered, Vector3 optionalParam, bool slowThrow, int chain)
	{
		Timing.RunCoroutine(_RpcThrowGrenade(id, ti_pid, ti_int, dir, isEnvironmentallyTriggered, optionalParam, slowThrow, chain), Segment.FixedUpdate);
	}

	private IEnumerator<float> _RpcThrowGrenade(int id, int ti_pid, int ti_int, Vector3 dir, bool isEnvironmentallyTriggered, Vector3 optionalParamenter, bool slowThrow, int chain)
	{
		Grenade g = null;
		if (!base.isLocalPlayer || isEnvironmentallyTriggered)
		{
			g = Object.Instantiate(availableGrenades[id].grenadeInstance).GetComponent<Grenade>();
			g.id = ((!isEnvironmentallyTriggered) ? string.Empty : "SERVER_") + ti_pid + ":" + ti_int;
			g.chain = chain;
			grenadesOnScene.Add(g);
			float num = ((!slowThrow) ? 1f : 0.5f) * availableGrenades[id].throwForce;
			if (isEnvironmentallyTriggered)
			{
				g.SyncMovement(optionalParamenter, dir, Quaternion.Euler(Vector3.zero), Vector3.zero);
			}
			else
			{
				g.SyncMovement(base.transform.position + Vector3.up * 0.8380203f, (dir + Vector3.up / 4f).normalized * num * dir.magnitude, Quaternion.Euler(availableGrenades[id].startRotation), availableGrenades[id].angularVelocity);
			}
		}
		else
		{
			foreach (Grenade item in grenadesOnScene)
			{
				if (item.id == ti_pid + ":" + ti_int)
				{
					g = item;
				}
			}
		}
		if (!NetworkServer.active)
		{
			yield break;
		}
		for (float i = 1f; i <= availableGrenades[id].timeUnitilDetonation * 50f; i += 1f)
		{
			if (g != null)
			{
				CallRpcUpdate(g.id, g.transform.position, g.transform.rotation, g.transform.GetComponent<Rigidbody>().velocity, g.transform.GetComponent<Rigidbody>().angularVelocity);
			}
			yield return 0f;
		}
		if (g != null)
		{
			CallRpcExplode(g.id, ti_pid);
		}
	}

	[ClientRpc]
	private void RpcExplode(string id, int playerID)
	{
		foreach (Grenade item in grenadesOnScene)
		{
			if (item.id == id)
			{
				item.Explode(playerID);
				break;
			}
		}
	}

	[ClientRpc]
	private void RpcUpdate(string id, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
	{
		if (NetworkServer.active)
		{
			return;
		}
		foreach (Grenade item in grenadesOnScene)
		{
			if (item.id == id)
			{
				item.SyncMovement(pos, vel, rot, angVel);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdThrowGrenade(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdThrowGrenade called on client.");
		}
		else
		{
			((GrenadeManager)obj).CmdThrowGrenade((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), reader.ReadVector3(), reader.ReadBoolean());
		}
	}

	public void CallCmdThrowGrenade(int id, int ti, Vector3 direction, bool slowThrow)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdThrowGrenade called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdThrowGrenade(id, ti, direction, slowThrow);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdThrowGrenade);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)id);
		networkWriter.WritePackedUInt32((uint)ti);
		networkWriter.Write(direction);
		networkWriter.Write(slowThrow);
		SendCommandInternal(networkWriter, 0, "CmdThrowGrenade");
	}

	protected static void InvokeRpcRpcThrowGrenade(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcThrowGrenade called on server.");
		}
		else
		{
			((GrenadeManager)obj).RpcThrowGrenade((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), reader.ReadVector3(), reader.ReadBoolean(), reader.ReadVector3(), reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcExplode(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcExplode called on server.");
		}
		else
		{
			((GrenadeManager)obj).RpcExplode(reader.ReadString(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcUpdate(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdate called on server.");
		}
		else
		{
			((GrenadeManager)obj).RpcUpdate(reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadVector3(), reader.ReadVector3());
		}
	}

	public void CallRpcThrowGrenade(int id, int ti_pid, int ti_int, Vector3 dir, bool isEnvironmentallyTriggered, Vector3 optionalParam, bool slowThrow, int chain)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcThrowGrenade called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcThrowGrenade);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)id);
		networkWriter.WritePackedUInt32((uint)ti_pid);
		networkWriter.WritePackedUInt32((uint)ti_int);
		networkWriter.Write(dir);
		networkWriter.Write(isEnvironmentallyTriggered);
		networkWriter.Write(optionalParam);
		networkWriter.Write(slowThrow);
		networkWriter.WritePackedUInt32((uint)chain);
		SendRPCInternal(networkWriter, 0, "RpcThrowGrenade");
	}

	public void CallRpcExplode(string id, int playerID)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcExplode called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcExplode);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		networkWriter.WritePackedUInt32((uint)playerID);
		SendRPCInternal(networkWriter, 0, "RpcExplode");
	}

	public void CallRpcUpdate(string id, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUpdate called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUpdate);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(id);
		networkWriter.Write(pos);
		networkWriter.Write(rot);
		networkWriter.Write(vel);
		networkWriter.Write(angVel);
		SendRPCInternal(networkWriter, 0, "RpcUpdate");
	}

	static GrenadeManager()
	{
		kCmdCmdThrowGrenade = 724004359;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GrenadeManager), kCmdCmdThrowGrenade, InvokeCmdCmdThrowGrenade);
		kRpcRpcThrowGrenade = 807436509;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GrenadeManager), kRpcRpcThrowGrenade, InvokeRpcRpcThrowGrenade);
		kRpcRpcExplode = 391825004;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GrenadeManager), kRpcRpcExplode, InvokeRpcRpcExplode);
		kRpcRpcUpdate = 462949854;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GrenadeManager), kRpcRpcUpdate, InvokeRpcRpcUpdate);
		NetworkCRC.RegisterBehaviour("GrenadeManager", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_syncFlashfire);
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
			writer.Write(_syncFlashfire);
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
			_syncFlashfire = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			_syncFlashfire = reader.ReadBoolean();
		}
	}
}
