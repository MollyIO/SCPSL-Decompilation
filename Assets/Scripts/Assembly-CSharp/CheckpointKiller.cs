using UnityEngine;
using UnityEngine.Networking;

public class CheckpointKiller : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (NetworkServer.active)
		{
			PlayerStats component = other.GetComponent<PlayerStats>();
			if (component != null)
			{
				component.HurtPlayer(new PlayerStats.HitInfo(99999f, "WORLD", DamageTypes.Wall, 0), component.gameObject);
			}
		}
	}
}
