using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

public class CustomVisibility : NetworkBehaviour
{
	public float visRange;

	private void Start()
	{
		Timing.RunCoroutine(_Start(), Segment.Update);
	}

	private IEnumerator<float> _Start()
	{
		NetworkIdentity _myNetworkId = GetComponent<NetworkIdentity>();
		while (NetworkServer.active)
		{
			if (GetComponent<CharacterClassManager>().curClass < 0)
			{
				yield return Timing.WaitForSeconds(20f);
			}
			_myNetworkId.RebuildObservers(false);
			yield return Timing.WaitForSeconds(0.5f);
		}
	}

	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, visRange);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			NetworkIdentity component = collider.GetComponent<NetworkIdentity>();
			if (component != null && component.connectionToClient != null)
			{
				observers.Add(component.connectionToClient);
			}
		}
		return true;
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
