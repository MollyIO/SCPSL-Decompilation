using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class AmmoBox : NetworkBehaviour
{
	[Serializable]
	public class AmmoType
	{
		public string label;

		public int inventoryID;
	}

	private Inventory inv;

	private CharacterClassManager ccm;

	public AmmoType[] types;

	[SyncVar(hook = "SetAmount")]
	public string amount;

	private static int kCmdCmdDrop;

	public string Networkamount
	{
		get
		{
			return amount;
		}
		[param: In]
		set
		{
			AmmoBox ammoBox = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetAmount(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref ammoBox.amount, 1u);
		}
	}

	public void SetAmount(string am)
	{
		Networkamount = am;
	}

	public void SetOneAmount(int type, string value)
	{
		string[] array = amount.Split(':');
		array[type] = value;
		SetAmount(array[0] + ":" + array[1] + ":" + array[2]);
	}

	private void Start()
	{
		inv = GetComponent<Inventory>();
		ccm = GetComponent<CharacterClassManager>();
	}

	public void SetAmmoAmount()
	{
		int[] ammoTypes = ccm.klasy[ccm.curClass].ammoTypes;
		Networkamount = ammoTypes[0] + ":" + ammoTypes[1] + ":" + ammoTypes[2];
	}

	public int GetAmmo(int type)
	{
		int result = 0;
		if (amount.Contains(":") && !int.TryParse(amount.Split(':')[Mathf.Clamp(type, 0, 2)], out result))
		{
			MonoBehaviour.print("Parse failed");
		}
		return result;
	}

	[Command(channel = 2)]
	public void CmdDrop(int _toDrop, int type)
	{
		for (int i = 0; i < 3; i++)
		{
			if (i == type)
			{
				_toDrop = Mathf.Clamp(_toDrop, 0, GetAmmo(i));
				if (_toDrop >= 15)
				{
					string[] array = amount.Split(':');
					array[i] = (GetAmmo(i) - _toDrop).ToString();
					inv.SetPickup(types[i].inventoryID, _toDrop, base.transform.position, inv.camera.transform.rotation, 0, 0, 0);
					Networkamount = array[0] + ":" + array[1] + ":" + array[2];
				}
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdDrop(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDrop called on client.");
		}
		else
		{
			((AmmoBox)obj).CmdDrop((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdDrop(int _toDrop, int type)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDrop called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDrop(_toDrop, type);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDrop);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)_toDrop);
		networkWriter.WritePackedUInt32((uint)type);
		SendCommandInternal(networkWriter, 2, "CmdDrop");
	}

	static AmmoBox()
	{
		kCmdCmdDrop = -1122225972;
		NetworkBehaviour.RegisterCommandDelegate(typeof(AmmoBox), kCmdCmdDrop, InvokeCmdCmdDrop);
		NetworkCRC.RegisterBehaviour("AmmoBox", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(amount);
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
			writer.Write(amount);
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
			amount = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetAmount(reader.ReadString());
		}
	}
}
