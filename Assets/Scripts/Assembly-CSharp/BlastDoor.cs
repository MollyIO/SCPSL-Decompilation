using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class BlastDoor : NetworkBehaviour
{
	[SyncVar(hook = "SetClosed")]
	public bool isClosed;

	public bool NetworkisClosed
	{
		get
		{
			return isClosed;
		}
		[param: In]
		set
		{
			BlastDoor blastDoor = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetClosed(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref blastDoor.isClosed, 1u);
		}
	}

	public void SetClosed(bool b)
	{
		NetworkisClosed = b;
		if (isClosed)
		{
			GetComponent<Animator>().SetTrigger("Close");
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(isClosed);
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
			writer.Write(isClosed);
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
			isClosed = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetClosed(reader.ReadBoolean());
		}
	}
}
