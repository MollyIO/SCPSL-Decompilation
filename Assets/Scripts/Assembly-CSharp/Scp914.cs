using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class Scp914 : NetworkBehaviour
{
	[Serializable]
	public class Recipe
	{
		[Serializable]
		public class Output
		{
			public List<int> outputs = new List<int>();
		}

		public List<Output> outputs = new List<Output>();
	}

	public static Scp914 singleton;

	public Texture burntIcon;

	public AudioSource soundSource;

	public Transform doors;

	public Transform knob;

	public Transform intake_obj;

	public Transform output_obj;

	public float colliderSize;

	public Recipe[] recipes;

	[SyncVar(hook = "SetStatus")]
	public int knobStatus;

	private int prevStatus = -1;

	private float cooldown;

	public bool working;

	public int NetworkknobStatus
	{
		get
		{
			return knobStatus;
		}
		[param: In]
		set
		{
			Scp914 scp = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetStatus(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref scp.knobStatus, 1u);
		}
	}

	private void Awake()
	{
		singleton = this;
	}

	private void SetStatus(int i)
	{
		NetworkknobStatus = i;
	}

	public void ChangeKnobStatus()
	{
		if (!working && cooldown < 0f)
		{
			cooldown = 0.2f;
			NetworkknobStatus = knobStatus + 1;
			if (knobStatus >= 5)
			{
				NetworkknobStatus = 0;
			}
		}
	}

	public void StartRefining()
	{
		if (!working)
		{
			working = true;
			Timing.RunCoroutine(_Animation(), Segment.Update);
		}
	}

	private void Update()
	{
		if (knobStatus != prevStatus)
		{
			knob.GetComponent<AudioSource>().Play();
			prevStatus = knobStatus;
		}
		if (cooldown >= 0f)
		{
			cooldown -= Time.deltaTime;
		}
		knob.transform.localRotation = Quaternion.Lerp(knob.transform.localRotation, Quaternion.Euler(Vector3.forward * Mathf.Lerp(-89f, 89f, (float)knobStatus / 4f)), Time.deltaTime * 4f);
	}

	private IEnumerator<float> _Animation()
	{
		soundSource.Play();
		yield return Timing.WaitForSeconds(1f);
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * 0.85f;
			doors.transform.localPosition = Vector3.right * Mathf.Lerp(1.74f, 0f, t);
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(6.28f);
		UpgradeItems();
		yield return Timing.WaitForSeconds(5.5f);
		while (t > 0f)
		{
			t -= Time.deltaTime * 0.85f;
			SetDoorPos(t);
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(1f);
		working = false;
	}

	[ServerCallback]
	private void UpgradeItems()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		Collider[] array = Physics.OverlapBox(intake_obj.position, Vector3.one * colliderSize / 2f);
		foreach (Collider collider in array)
		{
			Pickup component = collider.GetComponent<Pickup>();
			PlayerStats componentInParent = collider.GetComponentInParent<PlayerStats>();
			if (!(component != null))
			{
				continue;
			}
			GameObject gameObject = null;
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject2 in players)
			{
				if (gameObject2.GetComponent<QueryProcessor>().PlayerId == component.info.ownerPlayerID)
				{
					gameObject = gameObject2;
				}
			}
			component.transform.position = component.transform.position + (output_obj.position - intake_obj.position) + Vector3.up;
			if (component.info.itemId >= recipes.Length)
			{
				continue;
			}
			int[] array2 = recipes[component.info.itemId].outputs[knobStatus].outputs.ToArray();
			int num = array2[UnityEngine.Random.Range(0, array2.Length)];
			if (num < 0)
			{
				component.Delete();
				if (TutorialManager.status)
				{
					UnityEngine.Object.FindObjectOfType<TutorialManager>().Tutorial3_KeycardBurnt();
				}
				continue;
			}
			if (num <= 11 && gameObject != null && gameObject.GetComponent<CharacterClassManager>().curClass == 6)
			{
				GameObject[] players2 = PlayerManager.singleton.players;
				foreach (GameObject gameObject3 in players2)
				{
					if (gameObject3.GetComponent<CharacterClassManager>().curClass == 1 && Vector3.Distance(gameObject3.transform.position, gameObject.transform.position) < 10f)
					{
						PlayerManager.localPlayer.GetComponent<PlayerStats>().CallTargetAchieve(gameObject.GetComponent<CharacterClassManager>().connectionToClient, "friendship");
					}
				}
			}
			Pickup.PickupInfo info = component.info;
			info.itemId = num;
			component.Networkinfo = info;
			component.RefreshDurability();
		}
	}

	private void SetDoorPos(float t)
	{
		doors.transform.localPosition = Vector3.right * Mathf.Lerp(1.74f, 0f, t);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)knobStatus);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)knobStatus);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			knobStatus = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetStatus((int)reader.ReadPackedUInt32());
		}
	}
}
