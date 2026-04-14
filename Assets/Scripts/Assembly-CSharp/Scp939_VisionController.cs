using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Scp939_VisionController : NetworkBehaviour
{
	[Serializable]
	public class Scp939_Vision
	{
		public Scp939PlayerScript scp;

		public float remainingTime;
	}

	public float noise;

	public float minimumSilenceTime = 2.5f;

	public float minimumNoiseLevel = 2f;

	public List<Scp939_Vision> seeingSCPs = new List<Scp939_Vision>();

	private CharacterClassManager ccm;

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
	}

	public bool CanSee(Scp939PlayerScript scp939)
	{
		if (!scp939.iAm939)
		{
			return false;
		}
		foreach (Scp939_Vision seeingSCP in seeingSCPs)
		{
			if (seeingSCP.scp == scp939)
			{
				return true;
			}
		}
		return false;
	}

	private void FixedUpdate()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		foreach (Scp939PlayerScript instance in Scp939PlayerScript.instances)
		{
			if (Vector3.Distance(base.transform.position, instance.transform.position) < noise)
			{
				AddVision(instance);
			}
		}
		noise = minimumNoiseLevel;
		UpdateVisions();
	}

	private void AddVision(Scp939PlayerScript scp939)
	{
		for (int i = 0; i < seeingSCPs.Count; i++)
		{
			if (seeingSCPs[i].scp == scp939)
			{
				seeingSCPs[i].remainingTime = minimumSilenceTime;
				return;
			}
		}
		seeingSCPs.Add(new Scp939_Vision
		{
			scp = scp939,
			remainingTime = minimumSilenceTime
		});
	}

	private void UpdateVisions()
	{
		for (int i = 0; i < seeingSCPs.Count; i++)
		{
			seeingSCPs[i].remainingTime -= 0.02f;
			if (seeingSCPs[i].scp == null || !seeingSCPs[i].scp.iAm939 || seeingSCPs[i].remainingTime <= 0f)
			{
				seeingSCPs.RemoveAt(i);
				break;
			}
		}
	}

	[Server]
	public void MakeNoise(float distanceIntensity)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Scp939_VisionController::MakeNoise(System.Single)' called on client");
		}
		else if (ccm.IsHuman() && noise < distanceIntensity)
		{
			noise = distanceIntensity;
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
