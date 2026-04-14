using System.Collections.Generic;
using UnityEngine;

public class PocketDimensionGenerator : MonoBehaviour
{
	private List<PocketDimensionTeleport> pdtps = new List<PocketDimensionTeleport>();

	public void GenerateMap(int seed)
	{
		Random.InitState(seed);
		GenerateRandom();
	}

	public void GenerateRandom()
	{
		List<PocketDimensionTeleport> list = SMPrepTeleports();
		for (int i = 0; i < ConfigFile.ServerConfig.GetInt("pd_exit_count", 2); i++)
		{
			if (!SMContainsKiller(list))
			{
				break;
			}
			int num = -1;
			while ((num < 0 || list[num].GetTeleportType() == PocketDimensionTeleport.PDTeleportType.Exit) && SMContainsKiller(list))
			{
				num = Random.Range(0, list.Count);
			}
			list[Mathf.Clamp(num, 0, list.Count - 1)].SetType(PocketDimensionTeleport.PDTeleportType.Exit);
		}
	}

	private List<PocketDimensionTeleport> SMPrepTeleports()
	{
		List<PocketDimensionTeleport> list = new List<PocketDimensionTeleport>(Object.FindObjectsOfType<PocketDimensionTeleport>());
		foreach (PocketDimensionTeleport item in list)
		{
			item.SetType(PocketDimensionTeleport.PDTeleportType.Killer);
		}
		return list;
	}

	private bool SMContainsKiller(List<PocketDimensionTeleport> pdtps)
	{
		foreach (PocketDimensionTeleport pdtp in pdtps)
		{
			if (pdtp.GetTeleportType() == PocketDimensionTeleport.PDTeleportType.Killer)
			{
				return true;
			}
		}
		return false;
	}
}
