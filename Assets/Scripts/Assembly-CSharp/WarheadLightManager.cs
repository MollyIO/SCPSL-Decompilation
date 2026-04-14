using System;
using UnityEngine;

public class WarheadLightManager : MonoBehaviour
{
	[Serializable]
	public class MaterialColorChange
	{
		public Material targetMaterial;

		public Color normalColor;

		public Color targetColor;

		public float multiplier;

		public void SetStatus(bool b)
		{
			targetMaterial.SetColor("_EmissionColor", ((!b) ? normalColor : targetColor) * multiplier);
		}
	}

	public static WarheadLightManager singleton;

	private WarheadLight[] lightlist = new WarheadLight[0];

	public MaterialColorChange[] materials;

	private bool prevStatus;

	private void Awake()
	{
		singleton = this;
		MaterialColorChange[] array = materials;
		foreach (MaterialColorChange materialColorChange in array)
		{
			materialColorChange.SetStatus(false);
		}
	}

	public static void AddLight(WarheadLight l)
	{
		int num = singleton.lightlist.Length;
		Array.Resize(ref singleton.lightlist, num + 1);
		singleton.lightlist[num] = l;
	}

	private void LateUpdate()
	{
		bool flag = AlphaWarheadController.host != null && AlphaWarheadController.host.inProgress;
		if (prevStatus == flag)
		{
			return;
		}
		prevStatus = flag;
		WarheadLight[] array = lightlist;
		foreach (WarheadLight warheadLight in array)
		{
			if (flag)
			{
				warheadLight.WarheadEnable();
			}
			else
			{
				warheadLight.WarheadDisable();
			}
		}
		MaterialColorChange[] array2 = materials;
		foreach (MaterialColorChange materialColorChange in array2)
		{
			materialColorChange.SetStatus(flag);
		}
	}
}
