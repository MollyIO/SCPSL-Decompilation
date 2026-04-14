using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Searching : NetworkBehaviour
{
	private CharacterClassManager ccm;

	private Inventory inv;

	private bool isHuman;

	private GameObject pickup;

	private Transform cam;

	private FirstPersonController fpc;

	private AmmoBox ammobox;

	private float timeToPickUp;

	private float errorMsgDur;

	private GameObject overloaderror;

	private Slider progress;

	private GameObject progressGO;

	public float rayDistance;

	private GameObject _pickupObjectServer;

	private float _pickupProgressServer;

	private bool _pickupInProgressServer;

	private static int kCmdCmdPickupItem;

	private static int kCmdCmdStartPickup;

	private static int kCmdCmdAbortPickup;

	private void Start()
	{
		fpc = GetComponent<FirstPersonController>();
		cam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
		inv = GetComponent<Inventory>();
		overloaderror = UserMainInterface.singleton.overloadMsg;
		progress = UserMainInterface.singleton.searchProgress;
		progressGO = UserMainInterface.singleton.searchOBJ;
		ammobox = GetComponent<AmmoBox>();
	}

	public void Init(bool isNotHuman)
	{
		isHuman = !isNotHuman;
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			Raycast();
			ContinuePickup();
			ErrorMessage();
		}
		if (_pickupInProgressServer)
		{
			_pickupProgressServer -= Time.deltaTime;
			if (!(_pickupProgressServer > -3.5f))
			{
				_pickupInProgressServer = false;
				_pickupProgressServer = 0f;
				_pickupObjectServer = null;
			}
		}
	}

	public void ShowErrorMessage()
	{
		errorMsgDur = 2f;
	}

	private void ErrorMessage()
	{
		if (errorMsgDur > 0f)
		{
			errorMsgDur -= Time.deltaTime;
		}
		overloaderror.SetActive(errorMsgDur > 0f);
	}

	private void ContinuePickup()
	{
		if (pickup != null)
		{
			if (!Input.GetKey(NewInput.GetKey("Interact")))
			{
				pickup = null;
				fpc.isSearching = false;
				progressGO.SetActive(false);
				return;
			}
			timeToPickUp -= Time.deltaTime;
			progressGO.SetActive(true);
			progress.value = progress.maxValue - timeToPickUp;
			if (!(timeToPickUp <= 0f))
			{
				return;
			}
			if (pickup.GetComponent<Pickup>() != null)
			{
				WeaponManager.Weapon[] weapons = GetComponent<WeaponManager>().weapons;
				foreach (WeaponManager.Weapon weapon in weapons)
				{
					if (weapon.inventoryID == pickup.GetComponent<Pickup>().info.itemId)
					{
						AchievementManager.Achieve("thatcanbeusefull");
					}
				}
			}
			progressGO.SetActive(false);
			CallCmdPickupItem(pickup);
			fpc.isSearching = false;
			pickup = null;
		}
		else
		{
			if (fpc.isSearching)
			{
				CallCmdAbortPickup();
			}
			fpc.isSearching = false;
			progressGO.SetActive(false);
		}
	}

	private void Raycast()
	{
		RaycastHit hitInfo;
		if (!Input.GetKeyDown(NewInput.GetKey("Interact")) || !AllowPickup() || !Physics.Raycast(new Ray(cam.position, cam.forward), out hitInfo, rayDistance, GetComponent<PlayerInteract>().mask))
		{
			return;
		}
		Pickup componentInParent = hitInfo.transform.GetComponentInParent<Pickup>();
		Locker componentInParent2 = hitInfo.transform.GetComponentInParent<Locker>();
		if (componentInParent != null)
		{
			if (inv.items.Count < 8 || inv.availableItems[componentInParent.info.itemId].noEquipable)
			{
				CallCmdStartPickup(componentInParent.gameObject);
				timeToPickUp = componentInParent.searchTime;
				progress.maxValue = componentInParent.searchTime;
				fpc.isSearching = true;
				pickup = componentInParent.gameObject;
			}
			else
			{
				ShowErrorMessage();
			}
		}
		if (componentInParent2 != null)
		{
			CallCmdPickupItem(componentInParent2.gameObject);
		}
	}

	private bool AllowPickup()
	{
		if (!isHuman)
		{
			return false;
		}
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<Handcuffs>().cuffTarget == base.gameObject)
			{
				return false;
			}
		}
		return true;
	}

	[Command(channel = 2)]
	private void CmdPickupItem(GameObject t)
	{
		if (t == null || !ccm.IsHuman() || Vector3.Distance(GetComponent<PlyMovementSync>().position, t.transform.position) > 3.5f)
		{
			return;
		}
		Pickup component = t.GetComponent<Pickup>();
		if (component != null)
		{
			if (!_pickupInProgressServer || t != _pickupObjectServer || _pickupProgressServer > 0.25f)
			{
				return;
			}
			int itemId = component.info.itemId;
			component.Delete();
			if (itemId != -1)
			{
				AddItem(itemId, (!(t.GetComponent<Pickup>() == null)) ? component.info.durability : (-1f), component.info.weaponMods);
			}
		}
		Locker component2 = t.GetComponent<Locker>();
		if (component2 != null && !component2.isOpen)
		{
			component2.Open();
		}
	}

	[Command(channel = 2)]
	private void CmdStartPickup(GameObject t)
	{
		if (!(t == null) && ccm.IsHuman() && !(Vector3.Distance(GetComponent<PlyMovementSync>().position, t.transform.position) > 3.5f))
		{
			Pickup component = t.GetComponent<Pickup>();
			if (!(component == null))
			{
				_pickupObjectServer = t;
				_pickupProgressServer = component.searchTime;
				_pickupInProgressServer = true;
			}
		}
	}

	[Command(channel = 2)]
	private void CmdAbortPickup()
	{
		_pickupInProgressServer = false;
		_pickupObjectServer = null;
		_pickupProgressServer = 4144959f;
	}

	public void AddItem(int id, float dur, int[] mods)
	{
		if (mods == null)
		{
			mods = new int[3];
		}
		if (mods.Length != 3)
		{
			Array.Resize(ref mods, 3);
		}
		if (id == -1)
		{
			return;
		}
		if (!inv.availableItems[id].noEquipable)
		{
			WeaponManager.Weapon[] weapons = GetComponent<WeaponManager>().weapons;
			foreach (WeaponManager.Weapon weapon in weapons)
			{
				if (weapon.inventoryID == id)
				{
					mods[0] = Mathf.Clamp(mods[0], 0, weapon.mod_sights.Length - 1);
					mods[1] = Mathf.Clamp(mods[1], 0, weapon.mod_barrels.Length - 1);
					mods[2] = Mathf.Clamp(mods[2], 0, weapon.mod_others.Length - 1);
				}
			}
			inv.AddNewItem(id, (dur != -1f) ? dur : inv.availableItems[id].durability, mods[0], mods[1], mods[2]);
			return;
		}
		string[] array = ammobox.amount.Split(':');
		for (int j = 0; j < 3; j++)
		{
			if (ammobox.types[j].inventoryID == id)
			{
				array[j] = ((float)ammobox.GetAmmo(j) + dur).ToString();
			}
		}
		ammobox.Networkamount = array[0] + ":" + array[1] + ":" + array[2];
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdPickupItem(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickupItem called on client.");
		}
		else
		{
			((Searching)obj).CmdPickupItem(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdStartPickup(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartPickup called on client.");
		}
		else
		{
			((Searching)obj).CmdStartPickup(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAbortPickup(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAbortPickup called on client.");
		}
		else
		{
			((Searching)obj).CmdAbortPickup();
		}
	}

	public void CallCmdPickupItem(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPickupItem called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPickupItem(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPickupItem);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdPickupItem");
	}

	public void CallCmdStartPickup(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdStartPickup called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdStartPickup(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdStartPickup);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdStartPickup");
	}

	public void CallCmdAbortPickup()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAbortPickup called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAbortPickup();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAbortPickup);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdAbortPickup");
	}

	static Searching()
	{
		kCmdCmdPickupItem = 2021286825;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Searching), kCmdCmdPickupItem, InvokeCmdCmdPickupItem);
		kCmdCmdStartPickup = 427909316;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Searching), kCmdCmdStartPickup, InvokeCmdCmdStartPickup);
		kCmdCmdAbortPickup = -1878987502;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Searching), kCmdCmdAbortPickup, InvokeCmdCmdAbortPickup);
		NetworkCRC.RegisterBehaviour("Searching", 0);
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
