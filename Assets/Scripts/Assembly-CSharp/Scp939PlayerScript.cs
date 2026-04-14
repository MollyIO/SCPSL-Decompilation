using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dissonance;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class Scp939PlayerScript : NetworkBehaviour
{
	public bool iAm939;

	public bool sameClass;

	public LayerMask normalVision;

	public LayerMask scpVision;

	public Camera visionCamera;

	public Behaviour[] visualEffects;

	public LayerMask attackMask;

	public AudioClip[] attackSounds;

	public float attackDistance;

	[SyncVar]
	public float speedMultiplier;

	private Camera plyCam;

	public static List<Scp939PlayerScript> instances;

	[SyncVar]
	public bool usingHumanChat;

	private bool prevuhc;

	private KeyCode speechKey;

	private float cooldown;

	private static int kCmdCmdChangeHumanchatThing;

	private static int kCmdCmdShoot;

	private static int kRpcRpcShoot;

	public float NetworkspeedMultiplier
	{
		get
		{
			return speedMultiplier;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref speedMultiplier, 1u);
		}
	}

	public bool NetworkusingHumanChat
	{
		get
		{
			return usingHumanChat;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref usingHumanChat, 2u);
		}
	}

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			speechKey = NewInput.GetKey("939's speech");
			instances = new List<Scp939PlayerScript>();
			plyCam = GetComponent<Scp049PlayerScript>().plyCam.GetComponent<Camera>();
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		iAm939 = c.fullName.Contains("939");
		if (iAm939 && !instances.Contains(this))
		{
			instances.Add(this);
		}
		if (base.isLocalPlayer)
		{
			Behaviour[] array = visualEffects;
			foreach (Behaviour behaviour in array)
			{
				behaviour.enabled = iAm939;
			}
			plyCam.renderingPath = ((!iAm939) ? RenderingPath.DeferredShading : RenderingPath.VertexLit);
			plyCam.cullingMask = ((!iAm939) ? normalVision : scpVision);
			visionCamera.gameObject.SetActive(iAm939);
			visionCamera.fieldOfView = plyCam.fieldOfView;
			visionCamera.farClipPlane = plyCam.farClipPlane;
		}
	}

	private void Update()
	{
		CheckInstances();
		CheckForInput();
		ServersideCode();
		ClientsideMisc();
	}

	private void ServersideCode()
	{
		if (NetworkServer.active)
		{
			if (cooldown >= 0f)
			{
				cooldown -= Time.deltaTime;
			}
			if (speedMultiplier > 1f)
			{
				NetworkspeedMultiplier = speedMultiplier - Time.deltaTime / 3f;
			}
			if (speedMultiplier < 1f)
			{
				NetworkspeedMultiplier = 1f;
			}
		}
	}

	private void ClientsideMisc()
	{
		if (base.isLocalPlayer)
		{
			if (iAm939)
			{
				FirstPersonController.speedMultiplier939 = speedMultiplier;
			}
			else
			{
				FirstPersonController.speedMultiplier939 = 1f;
			}
		}
	}

	private void CheckForInput()
	{
		if (base.isLocalPlayer)
		{
			if (iAm939 && Input.GetKey(NewInput.GetKey("Shoot")))
			{
				Shoot();
			}
			bool flag = Input.GetKey(speechKey) && iAm939;
			if (prevuhc != flag)
			{
				prevuhc = flag;
				CallCmdChangeHumanchatThing(flag);
				VoiceBroadcastTrigger.is939Speaking = flag;
			}
		}
	}

	[Command]
	private void CmdChangeHumanchatThing(bool newValue)
	{
		NetworkusingHumanChat = iAm939 && newValue;
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(plyCam.transform.position, plyCam.transform.forward), out hitInfo, attackDistance))
		{
			Scp939PlayerScript componentInParent = hitInfo.collider.GetComponentInParent<Scp939PlayerScript>();
			if (componentInParent != null && !componentInParent.sameClass)
			{
				CallCmdShoot(componentInParent.gameObject);
			}
		}
	}

	[Command]
	private void CmdShoot(GameObject target)
	{
		if (iAm939 && Vector3.Distance(target.transform.position, base.transform.position) < attackDistance * 1.2f && cooldown <= 0f)
		{
			cooldown = 1f;
			GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(Random.Range(50, 80), GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ")", DamageTypes.Scp939, GetComponent<QueryProcessor>().PlayerId), target);
			GetComponent<CharacterClassManager>().CallRpcPlaceBlood(target.transform.position, 0, 2f);
			CallRpcShoot();
		}
	}

	[ClientRpc]
	private void RpcShoot()
	{
		Animator component = GetComponent<CharacterClassManager>().myModel.GetComponent<Animator>();
		component.GetComponent<AudioSource>().PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)]);
		if (base.isLocalPlayer)
		{
			Hitmarker.Hit(1.5f);
		}
		else
		{
			component.SetTrigger("Attack");
		}
	}

	private void CheckInstances()
	{
		if (!(base.name == "Host"))
		{
			return;
		}
		foreach (Scp939PlayerScript instance in instances)
		{
			if (instance == null || !instance.iAm939)
			{
				instances.Remove(instance);
				break;
			}
		}
	}

	static Scp939PlayerScript()
	{
		instances = new List<Scp939PlayerScript>();
		kCmdCmdChangeHumanchatThing = 1697597860;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp939PlayerScript), kCmdCmdChangeHumanchatThing, InvokeCmdCmdChangeHumanchatThing);
		kCmdCmdShoot = -1473386604;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp939PlayerScript), kCmdCmdShoot, InvokeCmdCmdShoot);
		kRpcRpcShoot = -1725253250;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp939PlayerScript), kRpcRpcShoot, InvokeRpcRpcShoot);
		NetworkCRC.RegisterBehaviour("Scp939PlayerScript", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdChangeHumanchatThing(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHumanchatThing called on client.");
		}
		else
		{
			((Scp939PlayerScript)obj).CmdChangeHumanchatThing(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdShoot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShoot called on client.");
		}
		else
		{
			((Scp939PlayerScript)obj).CmdShoot(reader.ReadGameObject());
		}
	}

	public void CallCmdChangeHumanchatThing(bool newValue)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdChangeHumanchatThing called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdChangeHumanchatThing(newValue);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdChangeHumanchatThing);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(newValue);
		SendCommandInternal(networkWriter, 0, "CmdChangeHumanchatThing");
	}

	public void CallCmdShoot(GameObject target)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdShoot called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdShoot(target);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdShoot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(target);
		SendCommandInternal(networkWriter, 0, "CmdShoot");
	}

	protected static void InvokeRpcRpcShoot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShoot called on server.");
		}
		else
		{
			((Scp939PlayerScript)obj).RpcShoot();
		}
	}

	public void CallRpcShoot()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShoot called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShoot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcShoot");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(speedMultiplier);
			writer.Write(usingHumanChat);
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
			writer.Write(speedMultiplier);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(usingHumanChat);
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
			speedMultiplier = reader.ReadSingle();
			usingHumanChat = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			speedMultiplier = reader.ReadSingle();
		}
		if ((num & 2) != 0)
		{
			usingHumanChat = reader.ReadBoolean();
		}
	}
}
