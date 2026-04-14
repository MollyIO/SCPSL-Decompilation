using UnityEngine;
using UnityEngine.Networking;

public class FootstepSync : NetworkBehaviour
{
	private AnimationController controller;

	private CharacterClassManager ccm;

	private Scp939_VisionController visionController;

	private static int kCmdCmdSyncFoot;

	private static int kRpcRpcSyncFoot;

	private void Start()
	{
		visionController = GetComponent<Scp939_VisionController>();
		ccm = GetComponent<CharacterClassManager>();
		controller = GetComponent<AnimationController>();
	}

	public void SyncFoot(bool run)
	{
		if (base.isLocalPlayer)
		{
			CallCmdSyncFoot(run);
			AudioClip[] stepClips = ccm.klasy[ccm.curClass].stepClips;
			controller.walkSource.PlayOneShot(stepClips[Random.Range(0, stepClips.Length)], (!run) ? 0.6f : 1f);
		}
	}

	public void SyncWalk()
	{
		SyncFoot(false);
	}

	public void SyncRun()
	{
		if (ccm.klasy[ccm.curClass].team != Team.SCP || ccm.klasy[ccm.curClass].fullName.Contains("939"))
		{
			SyncFoot(true);
		}
		else
		{
			SyncFoot(false);
		}
	}

	public void SetLoundness(Team t, bool is939)
	{
		if (t != Team.SCP || is939)
		{
			switch (t)
			{
			case Team.CHI:
				break;
			case Team.RSC:
			case Team.CDP:
				controller.runSource.maxDistance = 20f;
				controller.walkSource.maxDistance = 10f;
				return;
			default:
				controller.runSource.maxDistance = 30f;
				controller.walkSource.maxDistance = 15f;
				return;
			}
		}
		controller.runSource.maxDistance = 50f;
		controller.walkSource.maxDistance = 50f;
	}

	[Command(channel = 1)]
	private void CmdSyncFoot(bool run)
	{
		visionController.MakeNoise(controller.runSource.maxDistance * ((!run) ? 0.4f : 0.7f));
		CallRpcSyncFoot(run);
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncFoot(bool run)
	{
		if (!base.isLocalPlayer && ccm != null)
		{
			AudioClip[] stepClips = ccm.klasy[ccm.curClass].stepClips;
			if (run || ccm.klasy[ccm.curClass].team == Team.SCP)
			{
				controller.runSource.PlayOneShot(stepClips[Random.Range(0, stepClips.Length)]);
			}
			else
			{
				controller.walkSource.PlayOneShot(stepClips[Random.Range(0, stepClips.Length)]);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSyncFoot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSyncFoot called on client.");
		}
		else
		{
			((FootstepSync)obj).CmdSyncFoot(reader.ReadBoolean());
		}
	}

	public void CallCmdSyncFoot(bool run)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSyncFoot called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSyncFoot(run);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSyncFoot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(run);
		SendCommandInternal(networkWriter, 1, "CmdSyncFoot");
	}

	protected static void InvokeRpcRpcSyncFoot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncFoot called on server.");
		}
		else
		{
			((FootstepSync)obj).RpcSyncFoot(reader.ReadBoolean());
		}
	}

	public void CallRpcSyncFoot(bool run)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSyncFoot called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSyncFoot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(run);
		SendRPCInternal(networkWriter, 1, "RpcSyncFoot");
	}

	static FootstepSync()
	{
		kCmdCmdSyncFoot = -789180642;
		NetworkBehaviour.RegisterCommandDelegate(typeof(FootstepSync), kCmdCmdSyncFoot, InvokeCmdCmdSyncFoot);
		kRpcRpcSyncFoot = -840565516;
		NetworkBehaviour.RegisterRpcDelegate(typeof(FootstepSync), kRpcRpcSyncFoot, InvokeRpcRpcSyncFoot);
		NetworkCRC.RegisterBehaviour("FootstepSync", 0);
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
