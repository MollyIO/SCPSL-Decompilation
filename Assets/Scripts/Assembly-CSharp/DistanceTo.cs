using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DistanceTo : NetworkBehaviour
{
	private PlayerManager pm;

	private static CharacterClassManager localPlayerCcm;

	public float distanceToLocalPlayer;

	public GameObject spectCamera;

	private IEnumerator Start()
	{
		spectCamera = Object.FindObjectOfType<SpectatorCamera>().gameObject;
		pm = PlayerManager.singleton;
		if (!base.isLocalPlayer)
		{
			yield break;
		}
		localPlayerCcm = GetComponent<CharacterClassManager>();
		while (true)
		{
			GameObject[] players = pm.players;
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i] != null)
				{
					DistanceTo component = players[i].GetComponent<DistanceTo>();
					if (localPlayerCcm.curClass == 7)
					{
						component.distanceToLocalPlayer = 5f;
					}
					else
					{
						component.CalculateDistanceToLocalPlayer();
					}
				}
				if (i % 4 == 0)
				{
					yield return new WaitForEndOfFrame();
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void CalculateDistanceToLocalPlayer()
	{
		distanceToLocalPlayer = Vector3.Distance(base.transform.position, localPlayerCcm.transform.position);
	}

	public bool IsInRange()
	{
		if (localPlayerCcm != null && localPlayerCcm.curClass == 2)
		{
			return true;
		}
		return (!(base.transform.position.y > 800f)) ? (distanceToLocalPlayer < 70f) : (distanceToLocalPlayer < 500f);
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
