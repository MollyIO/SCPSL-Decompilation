using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionValue : MonoBehaviour
{
	[Serializable]
	public class IdvWeapon
	{
		public Image s_damage;

		public Image s_fireRate;

		public Image s_ammo;

		public TextMeshProUGUI t_damage;

		public TextMeshProUGUI t_fireRate;

		public TextMeshProUGUI t_ammo;

		public TextMeshProUGUI ammoType;

		public TextMeshProUGUI attachments;

		public float maxSliderDistance;

		public float sliderThickness;

		public void Update(WeaponManager.Weapon w, Inventory.SyncItemInfo iteminfo, string ammunitionType, int inventoryIndex)
		{
			WeaponManager.Weapon.WeaponMod.WeaponModEffects allEffects = w.GetAllEffects(new int[3] { iteminfo.modSight, iteminfo.modBarrel, iteminfo.modOther });
			float num = Mathf.Round(w.damageOverDistance.Evaluate(0f) * allEffects.damageMultiplier * 16.5f) / 10f;
			t_damage.text = num.ToString();
			s_damage.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, maxSliderDistance, num / 35f), sliderThickness);
			float num2 = Mathf.Round(w.shotsPerSecond * allEffects.firerateMultiplier * 600f) / 10f;
			t_fireRate.text = num2.ToString();
			s_fireRate.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, maxSliderDistance, num2 / 700f), sliderThickness);
			int num3 = (int)iteminfo.durability;
			t_ammo.text = num3.ToString();
			s_ammo.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0f, maxSliderDistance, (float)num3 / (float)w.maxAmmo), sliderThickness);
			ammoType.text = ammunitionType;
			attachments.text = string.Empty;
			Inventory.SyncItemInfo syncItemInfo = PlayerManager.localPlayer.GetComponent<Inventory>().items[inventoryIndex];
			if (syncItemInfo.modSight > 0)
			{
				TextMeshProUGUI textMeshProUGUI = attachments;
				textMeshProUGUI.text = textMeshProUGUI.text + w.mod_sights[syncItemInfo.modSight].name + "\n";
			}
			if (syncItemInfo.modBarrel > 0)
			{
				TextMeshProUGUI textMeshProUGUI2 = attachments;
				textMeshProUGUI2.text = textMeshProUGUI2.text + w.mod_barrels[syncItemInfo.modBarrel].name + "\n";
			}
			if (syncItemInfo.modOther > 0)
			{
				TextMeshProUGUI textMeshProUGUI3 = attachments;
				textMeshProUGUI3.text = textMeshProUGUI3.text + w.mod_others[syncItemInfo.modOther].name + "\n";
			}
			if (string.IsNullOrEmpty(attachments.text))
			{
				attachments.text = "---";
			}
		}
	}

	[Serializable]
	public class IdvKeycard
	{
		public string[] accessNames;

		public Image[] accessImages;

		public Image referenceColor;

		public void Update(string[] keycardSettings)
		{
			for (int i = 0; i < accessImages.Length; i++)
			{
				accessImages[i].color = Color.Lerp(referenceColor.color, Color.clear, 0.6f);
				foreach (string text in keycardSettings)
				{
					if (accessNames[i].ToUpper() == text.ToUpper())
					{
						accessImages[i].color = referenceColor.color;
					}
				}
			}
		}
	}

	[Serializable]
	public class IdvMisc
	{
		public TextMeshProUGUI description;

		public void Update(string desc)
		{
			description.text = desc;
		}
	}

	public TextMeshProUGUI title;

	public IdvWeapon idvWeapon;

	public IdvKeycard idvKeycard;

	public IdvMisc idvMisc;
}
