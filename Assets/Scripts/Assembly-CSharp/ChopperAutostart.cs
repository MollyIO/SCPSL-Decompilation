using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class ChopperAutostart : NetworkBehaviour
{
	[SyncVar(hook = "SetState")]
	public bool isLanded = true;

	public bool NetworkisLanded
	{
		get
		{
			return isLanded;
		}
		[param: In]
		set
		{
			ChopperAutostart chopperAutostart = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetState(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref chopperAutostart.isLanded, 1u);
		}
	}

	public void SetState(bool b)
	{
		NetworkisLanded = b;
		RefreshState();
	}

	private void Start()
	{
		RefreshState();
	}

	private void RefreshState()
	{
		GetComponent<Animator>().SetBool("IsLanded", isLanded);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(isLanded);
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
			writer.Write(isLanded);
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
			isLanded = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetState(reader.ReadBoolean());
		}
	}
}
