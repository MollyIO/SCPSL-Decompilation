using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AntiFaker;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

public class PlyMovementSync : NetworkBehaviour
{
	public Vector3 position;

	private Vector3 teleportPosition;

	private Vector3 _lastPos;

	private float _lastRot;

	private float _lastX;

	[HideInInspector]
	public CharacterClassManager ccm;

	private AntiFakeCommands speedhack;

	private Scp106PlayerScript _106;

	private Transform plyCam;

	private FootstepSync _fsync;

	private FallDamage fdmg;

	public bool isGrounded;

	private bool allowInput;

	private bool unstuckRequired;

	private bool _wasUsingPortal;

	public float FlyTime;

	public float rotation;

	public float groundedY;

	private float myRotation;

	[SyncVar]
	public float rotX;

	private static int kCmdCmdSyncData;

	private static int kTargetRpcTargetSetPosition;

	private static int kTargetRpcTargetSetRotation;

	public float NetworkrotX
	{
		get
		{
			return rotX;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref rotX, 1u);
		}
	}

	public void SetupPosRot(Vector3 _p, float _r)
	{
		position = _p;
		rotation = _r;
	}

	private void FixedUpdate()
	{
		if (base.isLocalPlayer)
		{
			myRotation = base.transform.rotation.eulerAngles.y;
		}
		TransmitData();
	}

	[ClientCallback]
	private void TransmitData()
	{
		if (NetworkClient.active && base.isLocalPlayer)
		{
			CallCmdSyncData(myRotation, base.transform.position, GetComponent<PlayerInteract>().playerCamera.transform.localRotation.eulerAngles.x);
		}
	}

	private void CheckGround(Vector3 pos)
	{
		if (ccm.curClass == 2 || ccm.curClass == -1 || isGrounded || ccm.curClass == 7)
		{
			FlyTime = 0f;
			_wasUsingPortal = false;
			groundedY = pos.y;
			return;
		}
		FlyTime += Time.deltaTime;
		if (_106.iAm106 && (_106.goingViaThePortal || _wasUsingPortal))
		{
			_wasUsingPortal = true;
			if (FlyTime < 4.5f)
			{
				return;
			}
		}
		if (!isGrounded)
		{
			if (groundedY < pos.y - 3f)
			{
				if (!RoundStart.RoundJustStarted)
				{
					ccm.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(2000000f, "*Killed by anticheat for flying (code: 1.3).", DamageTypes.Flying, 0), base.gameObject);
				}
				else
				{
					unstuckRequired = true;
				}
				return;
			}
			if (groundedY > pos.y)
			{
				groundedY = pos.y;
			}
		}
		Vector3 vector = pos;
		vector.y -= 50f;
		if (!Physics.Linecast(pos, vector, speedhack.mask))
		{
			vector.y += 23.8f;
			if (Physics.OverlapBox(vector, new Vector3(0.5f, 25f, 0.5f), new Quaternion(0f, 0f, 0f, 0f), speedhack.mask).Length == 0)
			{
				if (!RoundStart.RoundJustStarted)
				{
					ccm.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(2000000f, "*Killed by anticheat for flying (code: 1.2).", DamageTypes.Flying, 0), base.gameObject);
				}
				else
				{
					unstuckRequired = true;
				}
				return;
			}
		}
		if (!(FlyTime < 2.2f))
		{
			if (!RoundStart.RoundJustStarted)
			{
				ccm.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(2000000f, "*Killed by anticheat for flying (code: 1.1).", DamageTypes.Flying, 0), base.gameObject);
			}
			else
			{
				unstuckRequired = true;
			}
		}
	}

	private void Start()
	{
		FlyTime = 0f;
		plyCam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		speedhack = GetComponent<AntiFakeCommands>();
		ccm = GetComponent<CharacterClassManager>();
		teleportPosition = Vector3.zero;
		_106 = GetComponent<Scp106PlayerScript>();
		fdmg = GetComponent<FallDamage>();
		allowInput = true;
		if (NetworkServer.active && RoundStart.RoundJustStarted)
		{
			Timing.RunCoroutine(UnstuckCheck(), Segment.Update);
		}
	}

	[Command(channel = 5)]
	private void CmdSyncData(float rot, Vector3 pos, float x)
	{
		if (Math.Abs(_lastRot - rot) <= 0f && Math.Abs(x - _lastX) <= 0f && Math.Abs(pos.x - _lastPos.x) <= 0f && Math.Abs(pos.y - _lastPos.y) <= 0f && Math.Abs(pos.z - _lastPos.z) <= 0f)
		{
			return;
		}
		_lastPos = pos;
		_lastRot = rot;
		_lastX = x;
		rotation = rot;
		if (teleportPosition != Vector3.zero)
		{
			position = teleportPosition;
			speedhack.SetPosition(teleportPosition);
			base.transform.position = teleportPosition;
			teleportPosition = Vector3.zero;
		}
		else if (allowInput && speedhack.CheckMovement(pos))
		{
			if (ccm.curClass == 2)
			{
				pos = new Vector3(0f, 2048f, 0f);
			}
			fdmg.CalculateGround();
			isGrounded = fdmg.isCloseToGround;
			CheckGround(pos);
			position = pos;
			if (isGrounded)
			{
				groundedY = pos.y;
			}
		}
		else
		{
			CallTargetSetPosition(base.connectionToClient, position);
		}
		NetworkrotX = x;
		plyCam.transform.localRotation = Quaternion.Euler(x, 0f, 0f);
	}

	[TargetRpc]
	private void TargetSetPosition(NetworkConnection target, Vector3 pos)
	{
		base.transform.position = pos;
		position = pos;
	}

	[TargetRpc]
	private void TargetSetRotation(NetworkConnection target, float rot)
	{
		myRotation = rot;
		rotation = rot;
		base.transform.rotation = Quaternion.Euler(0f, rot, 0f);
		try
		{
			FirstPersonController component = GetComponent<FirstPersonController>();
			if (component != null)
			{
				component.m_MouseLook.SetRotation(rot);
			}
		}
		catch
		{
		}
	}

	[Client]
	public void ClientSetRotation(float rot)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void PlyMovementSync::ClientSetRotation(System.Single)' called on server");
		}
		else
		{
			myRotation = rot;
		}
	}

	[Server]
	public void SetPosition(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::SetPosition(UnityEngine.Vector3)' called on client");
			return;
		}
		teleportPosition = pos;
		position = pos;
		base.transform.position = pos;
		speedhack.SetPosition(pos);
		fdmg.isGrounded = true;
		fdmg.isCloseToGround = true;
		fdmg.previousHeight = pos.y;
		CallTargetSetPosition(base.connectionToClient, pos);
	}

	private IEnumerator<float> UnstuckCheck()
	{
		while (RoundStart.RoundJustStarted)
		{
			if (unstuckRequired)
			{
				Unstuck();
				unstuckRequired = false;
			}
			yield return Timing.WaitForSeconds(1f);
			CheckGround(base.transform.position);
		}
	}

	[Server]
	internal void Unstuck()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::Unstuck()' called on client");
			return;
		}
		Vector3 constantRespawnPoint = NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
		if (constantRespawnPoint == Vector3.zero)
		{
			SetPosition(UnityEngine.Object.FindObjectOfType<SpawnpointManager>().GetRandomPosition(ccm.curClass).transform.position);
		}
		else
		{
			SetPosition(constantRespawnPoint);
		}
	}

	[Server]
	public void SetRotation(float rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::SetRotation(System.Single)' called on client");
			return;
		}
		rotation = rot;
		myRotation = rot;
		CallTargetSetRotation(base.connectionToClient, rot);
	}

	[Server]
	public void SetAllowInput(bool b)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlyMovementSync::SetAllowInput(System.Boolean)' called on client");
		}
		else
		{
			allowInput = b;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSyncData(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSyncData called on client.");
		}
		else
		{
			((PlyMovementSync)obj).CmdSyncData(reader.ReadSingle(), reader.ReadVector3(), reader.ReadSingle());
		}
	}

	public void CallCmdSyncData(float rot, Vector3 pos, float x)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSyncData called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSyncData(rot, pos, x);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSyncData);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(rot);
		networkWriter.Write(pos);
		networkWriter.Write(x);
		SendCommandInternal(networkWriter, 5, "CmdSyncData");
	}

	protected static void InvokeRpcTargetSetPosition(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetPosition called on server.");
		}
		else
		{
			((PlyMovementSync)obj).TargetSetPosition(ClientScene.readyConnection, reader.ReadVector3());
		}
	}

	protected static void InvokeRpcTargetSetRotation(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetRotation called on server.");
		}
		else
		{
			((PlyMovementSync)obj).TargetSetRotation(ClientScene.readyConnection, reader.ReadSingle());
		}
	}

	public void CallTargetSetPosition(NetworkConnection target, Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetPosition called on client.");
			return;
		}
       if (target.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSetPosition called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetPosition);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(pos);
		SendTargetRPCInternal(target, networkWriter, 0, "TargetSetPosition");
	}

	public void CallTargetSetRotation(NetworkConnection target, float rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetRotation called on client.");
			return;
		}
       if (target.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSetRotation called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetRotation);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(rot);
		SendTargetRPCInternal(target, networkWriter, 0, "TargetSetRotation");
	}

	static PlyMovementSync()
	{
		kCmdCmdSyncData = -1186400596;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlyMovementSync), kCmdCmdSyncData, InvokeCmdCmdSyncData);
		kTargetRpcTargetSetPosition = 1295245089;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlyMovementSync), kTargetRpcTargetSetPosition, InvokeRpcTargetSetPosition);
		kTargetRpcTargetSetRotation = 507139446;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlyMovementSync), kTargetRpcTargetSetRotation, InvokeRpcTargetSetRotation);
		NetworkCRC.RegisterBehaviour("PlyMovementSync", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(rotX);
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
			writer.Write(rotX);
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
			rotX = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			rotX = reader.ReadSingle();
		}
	}
}
