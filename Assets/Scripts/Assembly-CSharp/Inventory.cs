using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Inventory : NetworkBehaviour
{
	[Serializable]
	public struct SyncItemInfo
	{
		public int id;

		public float durability;

		public int uniq;

		public int modSight;

		public int modBarrel;

		public int modOther;
	}

	public class SyncListItemInfo : SyncListStruct<SyncItemInfo>
	{
		public void ModifyDuration(int index, float value)
		{
			SyncItemInfo value2 = base[index];
			value2.durability = value;
			base[index] = value2;
		}

		public void ModifyAttachments(int index, int s, int b, int o)
		{
			SyncItemInfo value = base[index];
			value.modSight = s;
			value.modBarrel = b;
			value.modOther = o;
			base[index] = value;
		}

        protected override void SerializeItem(NetworkWriter writer, SyncItemInfo item)
		{
			writer.WritePackedUInt32((uint)item.id);
			writer.Write(item.durability);
			writer.WritePackedUInt32((uint)item.uniq);
			writer.WritePackedUInt32((uint)item.modSight);
			writer.WritePackedUInt32((uint)item.modBarrel);
			writer.WritePackedUInt32((uint)item.modOther);
		}

		protected override SyncItemInfo DeserializeItem(NetworkReader reader)
		{
			return new SyncItemInfo
			{
				id = (int)reader.ReadPackedUInt32(),
				durability = reader.ReadSingle(),
				uniq = (int)reader.ReadPackedUInt32(),
				modSight = (int)reader.ReadPackedUInt32(),
				modBarrel = (int)reader.ReadPackedUInt32(),
				modOther = (int)reader.ReadPackedUInt32()
			};
		}
	}

	public SyncListItemInfo items = new SyncListItemInfo();

	public Item[] availableItems;

	private AnimationController ac;

	private WeaponManager weaponManager;

	public static float inventoryCooldown;

	[SyncVar(hook = "SetCurItem")]
	public int curItem;

	public GameObject camera;

	[SyncVar(hook = "SetUniq")]
	public int itemUniq;

	public GameObject pickupPrefab;

	private RawImage crosshair;

	private CharacterClassManager ccm;

	private static int uniqid;

	public static bool collectionModified;

	private int prevIt = -10;

	private float crosshairAlpha = 1f;

	public static float targetCrosshairAlpha;

	private bool gotO5;

	private float pickupanimation;

	private static int kListitems;

	private static int kCmdCmdSetUnic;

	private static int kCmdCmdSyncItem;

	private static int kCmdCmdDropItem;

	public int NetworkcurItem
	{
		get
		{
			return curItem;
		}
		[param: In]
		set
		{
			Inventory inventory = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetCurItem(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref inventory.curItem, 2u);
		}
	}

	public int NetworkitemUniq
	{
		get
		{
			return itemUniq;
		}
		[param: In]
		set
		{
			Inventory inventory = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetUniq(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref inventory.itemUniq, 4u);
		}
	}

	public void SetUniq(int i)
	{
		NetworkitemUniq = i;
	}

	[Command(channel = 2)]
	public void CmdSetUnic(int i)
	{
		NetworkitemUniq = i;
	}

	private void Awake()
	{
		for (int i = 0; i < availableItems.Length; i++)
		{
			availableItems[i].id = i;
		}
		items.InitializeBehaviour(this, kListitems);
	}

	private void Log(string msg)
	{
	}

	public void SetCurItem(int ci)
	{
		if (!GetComponent<MicroHID_GFX>().onFire)
		{
			NetworkcurItem = ci;
		}
	}

	public SyncItemInfo GetItemInHand()
	{
		foreach (SyncItemInfo item in items)
		{
			if (item.uniq == itemUniq)
			{
				return item;
			}
		}
		return default(SyncItemInfo);
	}

	private IEnumerator<float> _RefreshPickups()
	{
		while (this != null)
		{
			int i = 0;
			foreach (Pickup instance in Pickup.instances)
			{
				if (instance != null)
				{
					instance.CheckForRefresh();
					if (!NetworkServer.active)
					{
						if (Vector3.Distance(instance.transform.position, instance.info.position) > 2f)
						{
							instance.transform.position = instance.info.position;
							instance.transform.rotation = instance.info.rotation;
						}
						instance.transform.position = Vector3.Lerp(instance.transform.position, instance.info.position, 0.099999994f);
						instance.transform.rotation = Quaternion.Lerp(instance.transform.rotation, instance.info.rotation, 0.099999994f);
					}
				}
				i++;
				if (collectionModified)
				{
					collectionModified = false;
					break;
				}
			}
			yield return 0f;
		}
	}

	private void Start()
	{
		weaponManager = GetComponent<WeaponManager>();
		ccm = GetComponent<CharacterClassManager>();
		crosshair = GameObject.Find("CrosshairImage").GetComponent<RawImage>();
		ac = GetComponent<AnimationController>();
		if (base.isLocalPlayer)
		{
			Pickup.inv = this;
			Pickup.instances = new List<Pickup>();
			Timing.RunCoroutine(_RefreshPickups(), Segment.FixedUpdate);
			UnityEngine.Object.FindObjectOfType<InventoryDisplay>().localplayer = this;
		}
	}

	private void RefreshModels()
	{
		for (int i = 0; i < availableItems.Length; i++)
		{
			try
			{
				availableItems[i].firstpersonModel.SetActive(base.isLocalPlayer & (i == curItem));
			}
			catch
			{
			}
		}
	}

	public void DropItem(int id, int _s, int _b, int _o)
	{
		if (base.isLocalPlayer)
		{
			if (items[id].id == curItem)
			{
				NetworkcurItem = -1;
			}
			CallCmdDropItem(id, items[id].id, _s, _b, _o);
		}
	}

	public void ServerDropAll()
	{
		foreach (SyncItemInfo item in items)
		{
			SetPickup(item.id, item.durability, base.transform.position, camera.transform.rotation, item.modSight, item.modBarrel, item.modOther);
		}
		AmmoBox component = GetComponent<AmmoBox>();
		for (int i = 0; i < 3; i++)
		{
			if (component.GetAmmo(i) != 0)
			{
				SetPickup(component.types[i].inventoryID, component.GetAmmo(i), base.transform.position, camera.transform.rotation, 0, 0, 0);
			}
		}
		items.Clear();
		component.Networkamount = "0:0:0";
	}

	public void Clear()
	{
		items.Clear();
		GetComponent<AmmoBox>().Networkamount = "0:0:0";
	}

	public int GetItemIndex()
	{
		int num = 0;
		foreach (SyncItemInfo item in items)
		{
			if (itemUniq == item.uniq)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void AddNewItem(int id, float dur = -4.6566467E+11f, int _s = 0, int _b = 0, int _o = 0)
	{
		uniqid++;
		Item item = new Item(availableItems[id]);
		if (items.Count >= 8 && !item.noEquipable)
		{
			return;
		}
		SyncItemInfo item2 = new SyncItemInfo
		{
			id = item.id,
			durability = item.durability,
			uniq = uniqid
		};
		if (dur != -4.6566467E+11f)
		{
			item2.durability = dur;
			item2.modSight = _s;
			item2.modBarrel = _b;
			item2.modOther = _o;
		}
		else
		{
			for (int i = 0; i < weaponManager.weapons.Length; i++)
			{
				if (weaponManager.weapons[i].inventoryID == id)
				{
					item2.modSight = weaponManager.modPreferences[i, 0];
					item2.modBarrel = weaponManager.modPreferences[i, 1];
					item2.modOther = weaponManager.modPreferences[i, 2];
				}
			}
		}
		items.Add(item2);
	}

	[Command(channel = 3)]
	private void CmdSyncItem(int i)
	{
		foreach (SyncItemInfo item in items)
		{
			if (item.id == i)
			{
				NetworkcurItem = i;
				return;
			}
		}
		NetworkcurItem = -1;
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			if (pickupanimation > 0f)
			{
				pickupanimation -= Time.deltaTime;
			}
			if (!gotO5 && curItem == 11)
			{
				gotO5 = true;
				AchievementManager.Achieve("power");
			}
			inventoryCooldown -= Time.deltaTime;
			CallCmdSyncItem(curItem);
			int num = Mathf.Clamp(curItem, 0, availableItems.Length - 1);
			if (ccm.curClass >= 0 && ccm.klasy[ccm.curClass].forcedCrosshair != -1)
			{
				num = ccm.klasy[ccm.curClass].forcedCrosshair;
			}
			crosshairAlpha = Mathf.Lerp(crosshairAlpha, targetCrosshairAlpha, Time.deltaTime * 5f);
			crosshair.texture = availableItems[num].crosshair;
			crosshair.color = Color.Lerp(Color.clear, availableItems[num].crosshairColor, crosshairAlpha);
		}
		if (prevIt == curItem)
		{
			return;
		}
		RefreshModels();
		prevIt = curItem;
		if (base.isLocalPlayer)
		{
			WeaponManager.Weapon[] weapons = weaponManager.weapons;
			foreach (WeaponManager.Weapon weapon in weapons)
			{
				if (weapon.inventoryID == curItem)
				{
					if (weapon.useProceduralPickupAnimation)
					{
						weaponManager.weaponInventoryGroup.localPosition = Vector3.down * 0.4f;
					}
					pickupanimation = 4f;
				}
			}
		}
		if (NetworkServer.active)
		{
			RefreshWeapon();
		}
	}

	public bool WeaponReadyToInstantPickup()
	{
		return pickupanimation <= 0f;
	}

	[ServerCallback]
	private void RefreshWeapon()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		int num = 0;
		int networkcurWeapon = -1;
		WeaponManager.Weapon[] weapons = weaponManager.weapons;
		foreach (WeaponManager.Weapon weapon in weapons)
		{
			if (weapon.inventoryID == curItem)
			{
				networkcurWeapon = num;
			}
			num++;
		}
		weaponManager.NetworkcurWeapon = networkcurWeapon;
	}

	[Command(channel = 2)]
	private void CmdDropItem(int itemInventoryIndex, int itemId, int _s, int _b, int _o)
	{
		if (items[itemInventoryIndex].id == itemId)
		{
			SetPickup(itemId, items[itemInventoryIndex].durability, base.transform.position, camera.transform.rotation, _s, _b, _o);
			items.RemoveAt(itemInventoryIndex);
		}
	}

	public void SetPickup(int dropedItemID, float dur, Vector3 pos, Quaternion rot, int _s, int _b, int _o)
	{
		if (dropedItemID >= 0)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(pickupPrefab);
			NetworkServer.Spawn(gameObject);
			if (dur == -4.6566467E+11f)
			{
				dur = availableItems[dropedItemID].durability;
			}
			gameObject.GetComponent<Pickup>().SetupPickup(new Pickup.PickupInfo
			{
				position = pos,
				rotation = rot,
				itemId = dropedItemID,
				durability = dur,
				weaponMods = new int[3] { _s, _b, _o },
				ownerPlayerID = GetComponent<QueryProcessor>().PlayerId
			});
		}
	}

	static Inventory()
	{
		targetCrosshairAlpha = 1f;
		kCmdCmdSetUnic = 1995465433;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Inventory), kCmdCmdSetUnic, InvokeCmdCmdSetUnic);
		kCmdCmdSyncItem = 2140153578;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Inventory), kCmdCmdSyncItem, InvokeCmdCmdSyncItem);
		kCmdCmdDropItem = -109121218;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Inventory), kCmdCmdDropItem, InvokeCmdCmdDropItem);
		kListitems = 1683194626;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(Inventory), kListitems, InvokeSyncListitems);
		NetworkCRC.RegisterBehaviour("Inventory", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListitems(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList items called on server.");
		}
		else
		{
			((Inventory)obj).items.HandleMsg(reader);
		}
	}

	protected static void InvokeCmdCmdSetUnic(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetUnic called on client.");
		}
		else
		{
			((Inventory)obj).CmdSetUnic((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSyncItem(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSyncItem called on client.");
		}
		else
		{
			((Inventory)obj).CmdSyncItem((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdDropItem(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropItem called on client.");
		}
		else
		{
			((Inventory)obj).CmdDropItem((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdSetUnic(int i)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetUnic called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetUnic(i);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetUnic);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)i);
		SendCommandInternal(networkWriter, 2, "CmdSetUnic");
	}

	public void CallCmdSyncItem(int i)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSyncItem called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSyncItem(i);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSyncItem);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)i);
		SendCommandInternal(networkWriter, 3, "CmdSyncItem");
	}

	public void CallCmdDropItem(int itemInventoryIndex, int itemId, int _s, int _b, int _o)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDropItem called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDropItem(itemInventoryIndex, itemId, _s, _b, _o);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDropItem);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)itemInventoryIndex);
		networkWriter.WritePackedUInt32((uint)itemId);
		networkWriter.WritePackedUInt32((uint)_s);
		networkWriter.WritePackedUInt32((uint)_b);
		networkWriter.WritePackedUInt32((uint)_o);
		SendCommandInternal(networkWriter, 2, "CmdDropItem");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteStructSyncListItemInfo_Inventory(writer, items);
			writer.WritePackedUInt32((uint)curItem);
			writer.WritePackedUInt32((uint)itemUniq);
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
			GeneratedNetworkCode._WriteStructSyncListItemInfo_Inventory(writer, items);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)curItem);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)itemUniq);
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
			GeneratedNetworkCode._ReadStructSyncListItemInfo_Inventory(reader, items);
			curItem = (int)reader.ReadPackedUInt32();
			itemUniq = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			GeneratedNetworkCode._ReadStructSyncListItemInfo_Inventory(reader, items);
		}
		if ((num & 2) != 0)
		{
			SetCurItem((int)reader.ReadPackedUInt32());
		}
		if ((num & 4) != 0)
		{
			SetUniq((int)reader.ReadPackedUInt32());
		}
	}
}
