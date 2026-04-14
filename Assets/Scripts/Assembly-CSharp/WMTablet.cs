using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WMTablet : MonoBehaviour
{
	public GameObject[] submenus;

	public int curMenu;

	public int selectedWeapon;

	public Text ct_text;

	private int ct_amountToDrop;

	private Inventory inv;

	private WeaponManager wm;

	public GameObject list_template;

	public Transform list_parent;

	public Text list_info;

	public Color list_normalColor;

	public Color list_selectedColor;

	private List<Image> templates = new List<Image>();

	private string translatedInfo;

	private void Start()
	{
		wm = GetComponent<WeaponManager>();
		if (!wm.isLocalPlayer)
		{
			Object.Destroy(this);
			return;
		}
		inv = GetComponent<Inventory>();
		for (int i = 0; i < wm.weapons.Length; i++)
		{
			GameObject gameObject = Object.Instantiate(list_template, list_parent);
			gameObject.transform.localScale = Vector3.one;
			gameObject.GetComponentInChildren<Text>().text = inv.availableItems[wm.weapons[i].inventoryID].label;
			templates.Add(gameObject.GetComponent<Image>());
		}
		Object.Destroy(list_template);
	}

	private void Update()
	{
		if (inv.curItem != 19)
		{
			return;
		}
		bool keyDown = Input.GetKeyDown(KeyCode.UpArrow);
		bool keyDown2 = Input.GetKeyDown(KeyCode.DownArrow);
		bool keyDown3 = Input.GetKeyDown(KeyCode.LeftArrow);
		bool keyDown4 = Input.GetKeyDown(KeyCode.RightArrow);
		if (keyDown3)
		{
			curMenu--;
		}
		curMenu = Mathf.Clamp(curMenu, 0, 2);
		for (int i = 0; i < submenus.Length; i++)
		{
			submenus[i].SetActive(i == curMenu);
		}
		WeaponManager.Weapon weapon = wm.weapons[selectedWeapon];
		AmmoBox component = GetComponent<AmmoBox>();
		if (curMenu == 0 && (keyDown || keyDown4 || keyDown2))
		{
			curMenu++;
			return;
		}
		switch (curMenu)
		{
		case 1:
		{
			if (keyDown)
			{
				selectedWeapon--;
			}
			if (keyDown2)
			{
				selectedWeapon++;
			}
			selectedWeapon = Mathf.Clamp(selectedWeapon, 0, wm.weapons.Length - 1);
			if (keyDown4)
			{
				curMenu++;
				break;
			}
			for (int j = 0; j < templates.Count; j++)
			{
				templates[j].color = ((j != selectedWeapon) ? list_normalColor : list_selectedColor);
			}
			if (string.IsNullOrEmpty(translatedInfo))
			{
				translatedInfo = TranslationReader.Get("WeaponManager", 2);
			}
			string text = translatedInfo;
			text = text.Replace("[var_name]", inv.availableItems[weapon.inventoryID].label);
			text = text.Replace("[var_atype]", component.types[weapon.ammoType].label);
			text = text.Replace("[var_ares]", component.GetAmmo(weapon.ammoType).ToString());
			text = text.Replace("[var_mag]", weapon.maxAmmo.ToString());
			text = text.Replace("[var_maxdmg]", (weapon.damageOverDistance.Evaluate(0f) * weapon.allEffects.damageMultiplier * wm.overallDamagerFactor).ToString());
			text = text.Replace("[var_effdmg]", (weapon.damageOverDistance.Evaluate(10f) * weapon.allEffects.damageMultiplier * wm.overallDamagerFactor).ToString());
			text = text.Replace("[var_sps]", weapon.shotsPerSecond.ToString());
			text = text.Replace("[var_custs]", (weapon.mod_barrels.Length + weapon.mod_others.Length + weapon.mod_sights.Length - 3).ToString());
			list_info.text = text;
			break;
		}
		case 2:
			if (component.GetAmmo(weapon.ammoType) >= 15)
			{
				ct_amountToDrop = Mathf.Clamp(ct_amountToDrop, 15, component.GetAmmo(weapon.ammoType));
				ct_text.text = TranslationReader.Get("WeaponManager", 3).Replace("[var_nom]", "<b>" + ct_amountToDrop + " x " + component.types[weapon.ammoType].label + "</b>");
			}
			else
			{
				ct_text.text = TranslationReader.Get("WeaponManager", 4).Replace("[var_nom]", component.types[weapon.ammoType].label);
				ct_amountToDrop = 0;
			}
			if (keyDown)
			{
				ct_amountToDrop++;
			}
			if (keyDown2)
			{
				ct_amountToDrop--;
			}
			if (keyDown4)
			{
				component.CallCmdDrop(ct_amountToDrop, weapon.ammoType);
			}
			break;
		}
	}
}
