using RemoteAdmin;
using UnityEngine;

public class FlashGrenade : Grenade
{
	public GameObject explosionEffects;

	public AnimationCurve shakeOverDistance;

	public AnimationCurve powerOverDistance;

	public AnimationCurve powerOverDot;

	public LayerMask viewLayerMask;

	public float distanceMultiplierSurface;

	public float distanceMultiplierFacility;

	public override void ServersideExplosion(GameObject thrower)
	{
		ServerLogs.AddLog(ServerLogs.Modules.Logger, "Player " + ((!(thrower != null)) ? "(UNKNOWN)" : (thrower.GetComponent<CharacterClassManager>().SteamId + " (" + thrower.GetComponent<NicknameSync>().myNick + ")")) + " threw flash grenade.", ServerLogs.ServerLogType.GameEvent);
	}

	public override void ClientsideExplosion(int grenadeOwnerPlayerID)
	{
		Object.Destroy(Object.Instantiate(explosionEffects, base.transform.position, explosionEffects.transform.rotation), 10f);
		GrenadeManager.grenadesOnScene.Remove(this);
		ExplosionCameraShake.singleton.Shake(shakeOverDistance.Evaluate(Vector3.Distance(base.transform.position, PlayerManager.localPlayer.transform.position)));
		Transform transform = PlayerManager.localPlayer.GetComponent<Scp049PlayerScript>().plyCam.transform;
		if (!GrenadeManager.flashfire)
		{
			GameObject gameObject = null;
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject2 in players)
			{
				if (gameObject2.GetComponent<QueryProcessor>().PlayerId == grenadeOwnerPlayerID)
				{
					gameObject = gameObject2;
				}
			}
			if (gameObject != PlayerManager.localPlayer && (gameObject == null || !gameObject.GetComponent<WeaponManager>().GetShootPermission(PlayerManager.localPlayer.GetComponent<CharacterClassManager>(), true)))
			{
				Object.Destroy(base.gameObject);
				return;
			}
		}
		RaycastHit hitInfo;
		if (Physics.Raycast(transform.position, -(transform.position - base.transform.position).normalized, out hitInfo, 1000f, viewLayerMask) && hitInfo.collider.gameObject.layer == 20)
		{
			PlayerManager.localPlayer.GetComponent<FlashEffect>().Play(powerOverDistance.Evaluate(Vector3.Distance(PlayerManager.localPlayer.transform.position, base.transform.position) / ((!(base.transform.position.y > 900f)) ? distanceMultiplierFacility : distanceMultiplierSurface)) * powerOverDot.Evaluate(Vector3.Dot(transform.forward, (transform.position - base.transform.position).normalized)));
		}
		Object.Destroy(base.gameObject);
	}
}
