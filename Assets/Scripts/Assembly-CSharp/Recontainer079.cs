using System.Collections.Generic;
using MEC;
using UnityEngine;

public class Recontainer079 : MonoBehaviour
{
	public static Recontainer079 singleton;

	public static bool isLocked;

	private void Awake()
	{
		isLocked = false;
		singleton = this;
	}

	public static void BeginContainment()
	{
		Timing.RunCoroutine(singleton._Recontain(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Recontain()
	{
		MTFRespawn mtf = PlayerManager.localPlayer.GetComponent<MTFRespawn>();
		PlayerStats ps = PlayerManager.localPlayer.GetComponent<PlayerStats>();
		NineTailedFoxAnnouncer annc = NineTailedFoxAnnouncer.singleton;
		while (annc.queue.Count > 0 || AlphaWarheadController.host.inProgress)
		{
			yield return 0f;
		}
		mtf.CallRpcPlayCustomAnnouncement("SCP079RECON5", false);
		for (int i = 0; i < 2750; i++)
		{
			yield return 0f;
		}
		while (annc.queue.Count > 0 || AlphaWarheadController.host.inProgress)
		{
			yield return 0f;
		}
		mtf.CallRpcPlayCustomAnnouncement("SCP079RECON6", true);
		mtf.CallRpcPlayCustomAnnouncement((Scp079PlayerScript.instances.Count <= 0) ? "FACILITY IS BACK IN OPERATIONAL MODE" : "SCP 0 7 9 CONTAINEDSUCCESSFULLY", false);
		for (int j = 0; j < 350; j++)
		{
			yield return 0f;
		}
		Generator079.generators[0].CallRpcOvercharge();
		Door[] array = Object.FindObjectsOfType<Door>();
		foreach (Door door in array)
		{
			Scp079Interactable component = door.GetComponent<Scp079Interactable>();
			if (component.currentZonesAndRooms[0].currentZone == "HeavyRooms" && door.isOpen && !door.locked)
			{
				door.ChangeState(true);
			}
		}
		isLocked = true;
		foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
		{
			ps.HurtPlayer(new PlayerStats.HitInfo(1000001f, "WORLD", DamageTypes.Tesla, 0), instance.gameObject);
		}
		for (int l = 0; l < 500; l++)
		{
			yield return 0f;
		}
		isLocked = false;
	}
}
