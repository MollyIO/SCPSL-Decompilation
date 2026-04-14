using System.Collections.Generic;
using UnityEngine;

public class DecontaminationGas : MonoBehaviour
{
	public static List<DecontaminationGas> gases = new List<DecontaminationGas>();

	public static void TurnOn()
	{
		if (ServerStatic.IsDedicated)
		{
			return;
		}
		foreach (DecontaminationGas gase in gases)
		{
			if (gase != null)
			{
				gase.gameObject.SetActive(true);
			}
		}
	}

	private void Start()
	{
		gases.Add(this);
		base.gameObject.SetActive(false);
	}
}
