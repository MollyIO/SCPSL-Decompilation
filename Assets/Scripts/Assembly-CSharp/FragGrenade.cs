using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class FragGrenade : Grenade
{
	public GameObject explosionEffects;

	public AnimationCurve shakeOverDistance;

	public AnimationCurve damageOverDistance;

	public LayerMask layerMask;

	public LayerMask triggerMask;

	public float triggerOtherNadesDistance = 12f;

	private static int thrownFrags;

	public override void ClientsideExplosion(int pId)
	{
		Object.Destroy(Object.Instantiate(explosionEffects, base.transform.position, explosionEffects.transform.rotation), 10f);
		GrenadeManager.grenadesOnScene.Remove(this);
		ExplosionCameraShake.singleton.Shake(shakeOverDistance.Evaluate(Vector3.Distance(base.transform.position, PlayerManager.localPlayer.transform.position)));
		Object.Destroy(base.gameObject);
	}

	public override void ServersideExplosion(GameObject thrower)
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, triggerOtherNadesDistance, triggerMask);
		int num = 0;
		if (thrower != null)
		{
			num = thrower.GetComponent<QueryProcessor>().PlayerId;
		}
		if (NetworkServer.active)
		{
			int num2 = 0;
			int num3 = chain + 1;
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				Pickup componentInChildren = collider.GetComponentInChildren<Pickup>();
				if ((GrenadeManager.GrenadeChainLengthLimit == -1 || GrenadeManager.GrenadeChainLengthLimit > num3) && componentInChildren != null && componentInChildren.info.itemId == 25)
				{
					num2++;
					if (GrenadeManager.GrenadeChainLimit != -1 && num2 > GrenadeManager.GrenadeChainLimit)
					{
						break;
					}
					thrownFrags++;
					PlayerManager.localPlayer.GetComponent<GrenadeManager>().ChangeIntoGrenade(componentInChildren, 0, num, thrownFrags, ((componentInChildren.transform.position - base.transform.position).normalized + Vector3.up / 3f).normalized * 16f, componentInChildren.transform.position, num3);
				}
				BreakableWindow component = collider.GetComponent<BreakableWindow>();
				if (component != null)
				{
					if (Vector3.Distance(component.transform.position, base.transform.position) < triggerOtherNadesDistance / 2f)
					{
						component.ServerDamageWindow(500f);
					}
					continue;
				}
				Door componentInParent = collider.GetComponentInParent<Door>();
				if (!(componentInParent == null) && Vector3.Distance(componentInParent.transform.position, base.transform.position) < triggerOtherNadesDistance / 2f && !componentInParent.commandlock && !componentInParent.decontlock && !componentInParent.lockdown && !componentInParent.GrenadesResistant)
				{
					componentInParent.DestroyDoor(true);
				}
			}
		}
		bool friendlyFire = ServerConsole.FriendlyFire;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			PlayerStats component2 = gameObject.GetComponent<PlayerStats>();
			if (component2 == null || component2.ccm.curClass == 2)
			{
				continue;
			}
			float num4 = damageOverDistance.Evaluate(Vector3.Distance(base.transform.position, component2.transform.position));
			num4 = ((!component2.ccm.IsHuman()) ? (num4 * ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f)) : (num4 * ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f)));
			if (!(num4 > 5f) || (!friendlyFire && gameObject != thrower && (thrower == null || !thrower.GetComponent<WeaponManager>().GetShootPermission(component2.ccm))))
			{
				continue;
			}
			Transform[] grenadePoints = component2.grenadePoints;
			foreach (Transform transform in grenadePoints)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(new Ray(base.transform.position, (transform.position - base.transform.position).normalized), out hitInfo, 100f, layerMask) && hitInfo.collider.GetComponentInParent<PlayerStats>() == component2)
				{
					component2.HurtPlayer(new PlayerStats.HitInfo(num4, (!(thrower != null)) ? "(UNKNOWN)" : (thrower.GetComponent<NicknameSync>().myNick + " (" + thrower.GetComponent<CharacterClassManager>().SteamId + ")"), DamageTypes.Grenade, num), gameObject);
					break;
				}
			}
		}
	}
}
