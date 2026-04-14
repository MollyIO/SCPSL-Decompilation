using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PocketDimensionTeleport : NetworkBehaviour
{
	public enum PDTeleportType
	{
		Killer = 0,
		Exit = 1
	}

	private PDTeleportType type;

	public bool RefreshExit;

	public void SetType(PDTeleportType t)
	{
		type = t;
	}

	public PDTeleportType GetTeleportType()
	{
		return type;
	}

	private void Start()
	{
		RefreshExit = ConfigFile.ServerConfig.GetBool("pd_refresh_exit");
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		NetworkIdentity component = other.GetComponent<NetworkIdentity>();
		if (!(component != null))
		{
			return;
		}
		if (type == PDTeleportType.Killer || Object.FindObjectOfType<BlastDoor>().isClosed)
		{
			component.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", DamageTypes.Pocket, 0), other.gameObject);
		}
		else if (type == PDTeleportType.Exit)
		{
			List<Vector3> list = new List<Vector3>();
			List<string> list2 = new List<string>(ConfigFile.ServerConfig.GetStringList("pd_random_exit_rids"));
			if (GameObject.Find("Host").GetComponent<DecontaminationLCZ>().GetCurAnnouncement() > 5)
			{
				list2 = new List<string>(ConfigFile.ServerConfig.GetStringList("pd_random_exit_rids_after_decontamination"));
			}
			if (list2.Count > 0)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("RoomID");
				foreach (GameObject gameObject in array)
				{
					if (gameObject.GetComponent<Rid>() != null && list2.Contains(gameObject.GetComponent<Rid>().id))
					{
						list.Add(gameObject.transform.position);
					}
				}
				if (list2.Contains("PORTAL"))
				{
					Scp106PlayerScript[] array2 = Object.FindObjectsOfType<Scp106PlayerScript>();
					foreach (Scp106PlayerScript scp106PlayerScript in array2)
					{
						if (scp106PlayerScript.portalPosition != Vector3.zero)
						{
							list.Add(scp106PlayerScript.portalPosition);
						}
					}
				}
			}
			if (list == null || list.Count == 0)
			{
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("PD_EXIT");
				foreach (GameObject gameObject2 in array3)
				{
					list.Add(gameObject2.transform.position);
				}
			}
			Vector3 position = list[Random.Range(0, list.Count)];
			position.y += 2f;
			other.GetComponent<PlyMovementSync>().SetPosition(position);
			PlayerManager.localPlayer.GetComponent<PlayerStats>().CallTargetAchieve(component.connectionToClient, "larryisyourfriend");
		}
		if (RefreshExit)
		{
			ImageGenerator.pocketDimensionGenerator.GenerateRandom();
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
