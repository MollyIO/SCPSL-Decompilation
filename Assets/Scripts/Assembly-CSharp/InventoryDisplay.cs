using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
	[HideInInspector]
	public Inventory localplayer;

	public GameObject rootObject;

	public RawImage[] itemSlots;

	public Image[] backgroundHighlights;

	public ItemDescriptionValue[] itemDescriptions;

	public int hoveredID;

	public bool isSCP;

	public float colorLerpSpeed = 5f;

	public Color highlightColor = Color.white;

	public Color selectedColor = Color.white;

	public float itemAlpha = 0.5f;

	private KeyCode changeVisibilityKey;

	private bool isVisible;

	private MicroHID_GFX microHID;

	public AnimationCurve distanceByRatio;

	private void Start()
	{
		changeVisibilityKey = NewInput.GetKey("Inventory");
	}

	private void Update()
	{
		if (localplayer == null)
		{
			if (PlayerManager.localPlayer != null)
			{
				localplayer = PlayerManager.localPlayer.GetComponent<Inventory>();
			}
			return;
		}
		if (microHID == null)
		{
			microHID = localplayer.GetComponent<MicroHID_GFX>();
		}
		UpdateItems();
		UpdateVisibility();
		UpdateSelectedItem();
		UpdateHighlights();
	}

	private void UpdateItems()
	{
		for (int i = 0; i < 8; i++)
		{
			if (i < localplayer.items.Count)
			{
				itemSlots[i].color = new Color(1f, 1f, 1f, itemAlpha);
				itemSlots[i].texture = localplayer.availableItems[localplayer.items[i].id].icon;
			}
			else
			{
				itemSlots[i].color = Color.clear;
			}
		}
	}

	private void UpdateHighlights()
	{
		for (int i = 0; i < 8; i++)
		{
			Color b = Color.clear;
			if (i == hoveredID && i == localplayer.GetItemIndex())
			{
				b = Color.Lerp(highlightColor, selectedColor, 0.5f);
			}
			else if (i == hoveredID)
			{
				b = highlightColor;
			}
			else if (i == localplayer.GetItemIndex())
			{
				b = selectedColor;
			}
			backgroundHighlights[i].color = Color.Lerp(backgroundHighlights[i].color, b, (!isVisible) ? 1f : (Time.deltaTime * colorLerpSpeed));
		}
		if (!isVisible)
		{
			return;
		}
		ItemDescriptionValue[] array = itemDescriptions;
		foreach (ItemDescriptionValue itemDescriptionValue in array)
		{
			itemDescriptionValue.title.text = string.Empty;
		}
		for (int k = 0; k < itemDescriptions.Length; k++)
		{
			itemDescriptions[k].gameObject.SetActive(false);
			if (hoveredID < 0 || hoveredID >= localplayer.items.Count || localplayer.items[hoveredID].id != k)
			{
				continue;
			}
			string[] array2 = TranslationReader.Get("NewInventory", k).Split(':');
			itemDescriptions[k].gameObject.SetActive(true);
			itemDescriptions[k].title.text = array2[0];
			if (itemDescriptions[k].idvKeycard.accessImages.Length > 0)
			{
				itemDescriptions[k].idvKeycard.Update(localplayer.availableItems[k].permissions);
			}
			else if (itemDescriptions[k].idvWeapon.s_ammo != null)
			{
				WeaponManager.Weapon[] weapons = localplayer.GetComponent<WeaponManager>().weapons;
				foreach (WeaponManager.Weapon weapon in weapons)
				{
					if (weapon.inventoryID == k)
					{
						itemDescriptions[k].idvWeapon.Update(weapon, localplayer.items[hoveredID], localplayer.GetComponent<AmmoBox>().types[weapon.ammoType].label, hoveredID);
					}
				}
			}
			else
			{
				itemDescriptions[k].idvMisc.Update(array2[1]);
			}
		}
	}

	private void UpdateSelectedItem()
	{
		if (!isVisible)
		{
			hoveredID = -1;
			return;
		}
		float num = (float)Screen.width / (float)Screen.height;
		Vector2 vector = new Vector2(Mathf.Lerp(-1f, 1f, Mathf.Clamp01(Input.mousePosition.x / (float)Screen.width)) * num, Mathf.Lerp(-1f, 1f, Input.mousePosition.y / (float)Screen.height));
		float num2 = Vector2.Angle(Vector2.up, vector.normalized);
		if (vector.x < 0f)
		{
			num2 = 360f - num2;
		}
		int num3 = 0;
		while (num2 > 45f)
		{
			num2 -= 45f;
			num3++;
		}
		hoveredID = ((!(vector.magnitude / distanceByRatio.Evaluate(num) > 1f)) ? 8 : Mathf.Clamp(num3, 0, 7));
		if (hoveredID >= localplayer.items.Count)
		{
			hoveredID = -1;
		}
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			localplayer.CallCmdSetUnic((hoveredID < 0) ? (-1) : localplayer.items[hoveredID].uniq);
			localplayer.NetworkcurItem = ((hoveredID < 0) ? (-1) : localplayer.items[hoveredID].id);
			ToggleInventory();
		}
		if (Input.GetKeyDown(KeyCode.Mouse1) && hoveredID >= 0 && hoveredID < localplayer.items.Count)
		{
			localplayer.DropItem(hoveredID, localplayer.items[hoveredID].modSight, localplayer.items[hoveredID].modBarrel, localplayer.items[hoveredID].modOther);
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ToggleInventory();
		}
	}

	private void UpdateVisibility()
	{
		if (Input.GetKeyDown(changeVisibilityKey) && !isSCP && (isVisible || !Cursor.visible) && !microHID.onFire)
		{
			ToggleInventory();
		}
		if (!Cursor.visible && Input.GetKeyDown(KeyCode.P))
		{
			Canvas[] array = Object.FindObjectsOfType<Canvas>();
			foreach (Canvas canvas in array)
			{
				if (canvas.transform.parent == null)
				{
					canvas.enabled = !canvas.enabled;
				}
			}
		}
		if (isVisible)
		{
			Inventory.inventoryCooldown = 0.2f;
			if (isSCP || microHID.onFire)
			{
				ToggleInventory();
			}
		}
	}

	private void ToggleInventory()
	{
		isVisible = !isVisible;
		localplayer.GetComponent<FirstPersonController>().m_MouseLook.isOpenEq = isVisible;
		CursorManager.singleton.eqOpen = isVisible;
		rootObject.SetActive(isVisible);
	}
}
