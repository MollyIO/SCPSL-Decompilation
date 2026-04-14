using System;
using UnityEngine;
using UnityEngine.Networking;

public class Medkit : NetworkBehaviour
{
	[Serializable]
	public struct MedkitInstance
	{
		public string Label;

		public int InventoryID;

		public int MinimumHealthRegeneration;

		public int MaximumHealthRegeneration;
	}

	public MedkitInstance[] Medkits;

	private Inventory inv;

	private PlayerStats ps;

	private KeyCode fireCode;

	private static int kCmdCmdUseMedkit;

	private void Start()
	{
		inv = GetComponent<Inventory>();
		ps = GetComponent<PlayerStats>();
		fireCode = NewInput.GetKey("Shoot");
	}

	private void Update()
	{
		if (!Input.GetKeyDown(fireCode) || Cursor.visible || !(Inventory.inventoryCooldown < 0f) || ps.health >= ps.maxHP)
		{
			return;
		}
		for (int i = 0; i < Medkits.Length; i++)
		{
			if (Medkits[i].InventoryID == inv.curItem)
			{
				inv.SetCurItem(-1);
				CallCmdUseMedkit(i);
				break;
			}
		}
	}

	[Command(channel = 2)]
	private void CmdUseMedkit(int id)
	{
		foreach (Inventory.SyncItemInfo item in inv.items)
		{
			if (item.id == Medkits[id].InventoryID)
			{
				ps.health = Mathf.Clamp(ps.health + UnityEngine.Random.Range(Medkits[id].MinimumHealthRegeneration, Medkits[id].MaximumHealthRegeneration), 0, ps.ccm.klasy[ps.ccm.curClass].maxHP);
				inv.items.Remove(item);
				break;
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdUseMedkit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseMedkit called on client.");
		}
		else
		{
			((Medkit)obj).CmdUseMedkit((int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdUseMedkit(int id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseMedkit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseMedkit(id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseMedkit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)id);
		SendCommandInternal(networkWriter, 2, "CmdUseMedkit");
	}

	static Medkit()
	{
		kCmdCmdUseMedkit = -2049042393;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Medkit), kCmdCmdUseMedkit, InvokeCmdCmdUseMedkit);
		NetworkCRC.RegisterBehaviour("Medkit", 0);
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
