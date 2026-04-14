using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BloodDrawer : NetworkBehaviour
{
	[Serializable]
	public class BloodType
	{
		public GameObject[] prefabs;
	}

	public LayerMask mask;

	private static List<Transform> instances = new List<Transform>();

	public int maxBlood = 500;

	public BloodType[] bloodTypes;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			instances = new List<Transform>();
			instances.Clear();
		}
		maxBlood = PlayerPrefs.GetInt("gfxsets_maxblood", 250);
	}

	public void DrawBlood(Vector3 pos, Quaternion rot, int bloodType)
	{
		if (ServerStatic.IsDedicated || bloodType < 0 || maxBlood <= 0)
		{
			return;
		}
		Transform transform;
		if (instances.Count < maxBlood)
		{
			GameObject[] prefabs = bloodTypes[bloodType].prefabs;
			transform = UnityEngine.Object.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], pos, rot).transform;
			instances.Add(transform);
		}
		else
		{
			transform = instances[0];
			instances.Add(transform);
			instances.RemoveAt(0);
			transform.transform.position = pos;
			transform.transform.rotation = rot;
		}
		transform.Rotate(0f, UnityEngine.Random.Range(0, 360), 0f, Space.Self);
		float num = UnityEngine.Random.Range(1.1f, 2f);
		transform.localScale = new Vector3(num, num, num);
		RaycastHit hitInfo;
		if (Physics.Raycast(transform.position - transform.forward / 4f, transform.forward, out hitInfo, 0.6f, mask))
		{
			if (hitInfo.collider.transform.tag == "Door")
			{
				transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 0.05f);
			}
			transform.SetParent(hitInfo.collider.transform);
		}
	}

	public void PlaceUnderneath(Transform obj, int type, float amountMultiplier = 1f)
	{
		PlaceUnderneath(obj.position, type, amountMultiplier);
	}

	public void PlaceUnderneath(Vector3 pos, int type, float amountMultiplier = 1f)
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(pos, Vector3.down, out hitInfo, 3f, mask))
		{
			GameObject[] prefabs = bloodTypes[type].prefabs;
			Transform transform = UnityEngine.Object.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal)).transform;
			transform.Rotate(0f, UnityEngine.Random.Range(0, 360), 0f, Space.Self);
			float num = UnityEngine.Random.Range(0.8f, 1.6f) * amountMultiplier;
			transform.localScale = new Vector3(num, num, num);
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
