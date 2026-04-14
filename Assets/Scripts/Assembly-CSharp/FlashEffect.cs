using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class FlashEffect : NetworkBehaviour
{
	public CameraFilterPack_Colors_Brightness e1;

	public CameraFilterPack_TV_Vignetting e2;

	private float curP;

	[SyncVar]
	public bool sync_blind;

	public static bool isBlind;

	private static int kCmdCmdBlind;

	public bool Networksync_blind
	{
		get
		{
			return sync_blind;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref sync_blind, 1u);
		}
	}

	[Command]
	private void CmdBlind(bool value)
	{
		Networksync_blind = value;
	}

	public void Play(float power)
	{
		if (GetComponent<CharacterClassManager>().IsHuman())
		{
			curP = power;
		}
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			if (curP > 0f)
			{
				curP -= Time.deltaTime / 3f;
				e1.enabled = true;
				e2.enabled = true;
				e1._Brightness = Mathf.Clamp(curP * 1.25f + 1f, 1f, 2.5f);
				e2.Vignetting = Mathf.Clamp01(curP);
				e2.VignettingFull = Mathf.Clamp01(curP);
				e2.VignettingDirt = Mathf.Clamp01(curP);
			}
			else
			{
				curP = 0f;
				e1.enabled = false;
				e2.enabled = false;
			}
			isBlind = curP > 1f;
			if (isBlind != sync_blind)
			{
				CallCmdBlind(isBlind);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdBlind(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBlind called on client.");
		}
		else
		{
			((FlashEffect)obj).CmdBlind(reader.ReadBoolean());
		}
	}

	public void CallCmdBlind(bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdBlind called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdBlind(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdBlind);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdBlind");
	}

	static FlashEffect()
	{
		kCmdCmdBlind = -951436780;
		NetworkBehaviour.RegisterCommandDelegate(typeof(FlashEffect), kCmdCmdBlind, InvokeCmdCmdBlind);
		NetworkCRC.RegisterBehaviour("FlashEffect", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(sync_blind);
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
			writer.Write(sync_blind);
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
			sync_blind = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			sync_blind = reader.ReadBoolean();
		}
	}
}
