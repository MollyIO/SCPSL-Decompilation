using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WorkStationUpgrader : NetworkBehaviour
{
	private WeaponManager manager;

	public TextMeshProUGUI ms_curModSize;

	public TextMeshProUGUI ms_header;

	public RawImage ms_icon;

	public TextMeshProUGUI ms_stats;

	private int slotID;

	private int curMod;

	public Button ss_barrel;

	public Button ss_other;

	public Button ss_sight;

	private WorkStation ws;

	private void Start()
	{
		ws = GetComponent<WorkStation>();
	}

	public void ChangeSlot(Button button)
	{
		int result = 0;
		int.TryParse(button.name.Remove(1), out result);
		ws.ChangeScreen("mods");
		slotID = result;
		switch (slotID)
		{
		case 0:
			curMod = PlayerManager.localPlayer.GetComponent<Inventory>().GetItemInHand().modSight;
			break;
		case 1:
			curMod = PlayerManager.localPlayer.GetComponent<Inventory>().GetItemInHand().modBarrel;
			break;
		default:
			curMod = PlayerManager.localPlayer.GetComponent<Inventory>().GetItemInHand().modOther;
			break;
		}
		RefreshModSelector();
	}

	public void NextMod()
	{
		if (curMod < GetModLength(slotID) - 1)
		{
			curMod++;
		}
		RefreshModSelector();
	}

	public void PrevMod()
	{
		if (curMod > 0)
		{
			curMod--;
		}
		RefreshModSelector();
	}

	private int GetModLength(int slot)
	{
		int curWeapon = GetCurWeapon();
		if (curWeapon < 0)
		{
			return 0;
		}
		switch (slot)
		{
		case 0:
			return manager.weapons[curWeapon].mod_sights.Length;
		case 1:
			return manager.weapons[curWeapon].mod_barrels.Length;
		default:
			return manager.weapons[curWeapon].mod_others.Length;
		}
	}

	private int GetCurWeapon()
	{
		int curItem = PlayerManager.localPlayer.GetComponent<Inventory>().curItem;
		for (int i = 0; i < manager.weapons.Length; i++)
		{
			if (manager.weapons[i].inventoryID == curItem)
			{
				return i;
			}
		}
		return -1;
	}

	public void RefreshSlotSelector()
	{
		if (manager == null)
		{
			manager = PlayerManager.localPlayer.GetComponent<WeaponManager>();
		}
		int curWeapon = GetCurWeapon();
		if (curWeapon >= 0)
		{
			ss_sight.interactable = manager.weapons[curWeapon].mod_sights.Length > 1;
			ss_sight.GetComponent<TextMeshProUGUI>().text = ss_sight.GetComponent<TextMeshProUGUI>().text.Remove(ss_sight.GetComponent<TextMeshProUGUI>().text.IndexOf('('));
			TextMeshProUGUI component = ss_sight.GetComponent<TextMeshProUGUI>();
			string text = component.text;
			component.text = text + "(" + (manager.weapons[curWeapon].mod_sights.Length - 1) + " ready)";
			ss_barrel.interactable = manager.weapons[curWeapon].mod_barrels.Length > 1;
			ss_barrel.GetComponent<TextMeshProUGUI>().text = ss_barrel.GetComponent<TextMeshProUGUI>().text.Remove(ss_barrel.GetComponent<TextMeshProUGUI>().text.IndexOf('('));
			TextMeshProUGUI component2 = ss_barrel.GetComponent<TextMeshProUGUI>();
			text = component2.text;
			component2.text = text + "(" + (manager.weapons[curWeapon].mod_barrels.Length - 1) + " ready)";
			ss_other.interactable = manager.weapons[curWeapon].mod_others.Length > 1;
			ss_other.GetComponent<TextMeshProUGUI>().text = ss_other.GetComponent<TextMeshProUGUI>().text.Remove(ss_other.GetComponent<TextMeshProUGUI>().text.IndexOf('('));
			TextMeshProUGUI component3 = ss_other.GetComponent<TextMeshProUGUI>();
			text = component3.text;
			component3.text = text + "(" + (manager.weapons[curWeapon].mod_others.Length - 1) + " ready)";
		}
	}

	private void RefreshModSelector()
	{
		int curWeapon = GetCurWeapon();
		if (curWeapon >= 0)
		{
			switch (slotID)
			{
			case 0:
				ms_header.text = manager.weapons[curWeapon].mod_sights[curMod].name;
				ms_icon.texture = manager.weapons[curWeapon].mod_sights[curMod].icon;
				ms_stats.text = manager.weapons[curWeapon].GetStats(ModPrefab.ModType.Sight, curMod);
				break;
			case 1:
				ms_header.text = manager.weapons[curWeapon].mod_barrels[curMod].name;
				ms_icon.texture = manager.weapons[curWeapon].mod_barrels[curMod].icon;
				ms_stats.text = manager.weapons[curWeapon].GetStats(ModPrefab.ModType.Barrel, curMod);
				break;
			case 2:
				ms_header.text = manager.weapons[curWeapon].mod_others[curMod].name;
				ms_icon.texture = manager.weapons[curWeapon].mod_others[curMod].icon;
				ms_stats.text = manager.weapons[curWeapon].GetStats(ModPrefab.ModType.Other, curMod);
				break;
			}
			ms_curModSize.text = curMod + " / " + (GetModLength(slotID) - 1);
			manager.CallCmdChangeModPreferences(curWeapon + ":" + slotID + ":" + curMod);
			PlayerPrefs.SetInt("W_" + curWeapon + "_" + slotID, curMod);
		}
	}

	private void UNetVersion()
	{
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
