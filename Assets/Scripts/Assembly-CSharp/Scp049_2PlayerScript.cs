using System.Collections.Generic;
using Dissonance.Integrations.UNet_HLAPI;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class Scp049_2PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Transform plyCam;

	public Animator animator;

	public bool iAm049_2;

	public bool sameClass;

	[Header("Attack")]
	public float distance = 2.4f;

	public int damage = 60;

	[Header("Boosts")]
	public AnimationCurve multiplier;

	private static int kCmdCmdHurtPlayer;

	private static int kCmdCmdShootAnim;

	private static int kRpcRpcShootAnim;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			Timing.RunCoroutine(_UpdateInput(), Segment.FixedUpdate);
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		iAm049_2 = classID == 10;
		animator.gameObject.SetActive(base.isLocalPlayer && iAm049_2);
	}

	private IEnumerator<float> _UpdateInput()
	{
		while (this != null)
		{
			if (iAm049_2 && Input.GetKey(NewInput.GetKey("Shoot")))
			{
				float mt = multiplier.Evaluate(GetComponent<PlayerStats>().GetHealthPercent());
				CallCmdShootAnim();
				animator.SetTrigger("Shoot");
				animator.speed = mt;
				yield return Timing.WaitForSeconds(0.65f / mt);
				Attack();
				yield return Timing.WaitForSeconds(1f / mt);
			}
			yield return 0f;
		}
	}

	private void Attack()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, distance))
		{
			Scp049_2PlayerScript scp049_2PlayerScript = hitInfo.transform.GetComponent<Scp049_2PlayerScript>();
			if (scp049_2PlayerScript == null)
			{
				scp049_2PlayerScript = hitInfo.transform.GetComponentInParent<Scp049_2PlayerScript>();
			}
			if (scp049_2PlayerScript != null && !scp049_2PlayerScript.sameClass)
			{
				Hitmarker.Hit();
				CallCmdHurtPlayer(hitInfo.transform.gameObject, GetComponent<HlapiPlayer>().PlayerId);
			}
		}
	}

	[Command(channel = 2)]
	private void CmdHurtPlayer(GameObject ply, string id)
	{
		if (Vector3.Distance(GetComponent<PlyMovementSync>().position, ply.transform.position) <= distance * 1.5f && iAm049_2)
		{
			Vector3 position = ply.transform.position;
			GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(damage, GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ")", DamageTypes.Scp0492, GetComponent<QueryProcessor>().PlayerId), ply);
			GetComponent<CharacterClassManager>().CallRpcPlaceBlood(position, 0, (ply.GetComponent<CharacterClassManager>().curClass != 2) ? 0.5f : 1.3f);
		}
	}

	[Command(channel = 1)]
	private void CmdShootAnim()
	{
		CallRpcShootAnim();
	}

	[ClientRpc]
	private void RpcShootAnim()
	{
		GetComponent<AnimationController>().DoAnimation("Shoot");
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdHurtPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHurtPlayer called on client.");
		}
		else
		{
			((Scp049_2PlayerScript)obj).CmdHurtPlayer(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdShootAnim(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShootAnim called on client.");
		}
		else
		{
			((Scp049_2PlayerScript)obj).CmdShootAnim();
		}
	}

	public void CallCmdHurtPlayer(GameObject ply, string id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdHurtPlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdHurtPlayer(ply, id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdHurtPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ply);
		networkWriter.Write(id);
		SendCommandInternal(networkWriter, 2, "CmdHurtPlayer");
	}

	public void CallCmdShootAnim()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdShootAnim called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdShootAnim();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdShootAnim);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 1, "CmdShootAnim");
	}

	protected static void InvokeRpcRpcShootAnim(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShootAnim called on server.");
		}
		else
		{
			((Scp049_2PlayerScript)obj).RpcShootAnim();
		}
	}

	public void CallRpcShootAnim()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShootAnim called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShootAnim);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcShootAnim");
	}

	static Scp049_2PlayerScript()
	{
		kCmdCmdHurtPlayer = 21222532;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp049_2PlayerScript), kCmdCmdHurtPlayer, InvokeCmdCmdHurtPlayer);
		kCmdCmdShootAnim = 1794565020;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp049_2PlayerScript), kCmdCmdShootAnim, InvokeCmdCmdShootAnim);
		kRpcRpcShootAnim = 201633926;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp049_2PlayerScript), kRpcRpcShootAnim, InvokeRpcRpcShootAnim);
		NetworkCRC.RegisterBehaviour("Scp049_2PlayerScript", 0);
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
