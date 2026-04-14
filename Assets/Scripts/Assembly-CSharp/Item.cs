using System;
using UnityEngine;

[Serializable]
public class Item
{
	public string label;

	public Texture2D icon;

	public GameObject prefab;

	public float pickingtime = 1f;

	public string[] permissions;

	public GameObject firstpersonModel;

	public float durability;

	public bool noEquipable;

	public Texture crosshair;

	public Color crosshairColor;

	[HideInInspector]
	public int id;

	public Item(Item item)
	{
		label = item.label;
		icon = item.icon;
		prefab = item.prefab;
		pickingtime = item.pickingtime;
		permissions = item.permissions;
		firstpersonModel = item.firstpersonModel;
		durability = item.durability;
		id = item.id;
		crosshair = item.crosshair;
		crosshairColor = item.crosshairColor;
	}
}
