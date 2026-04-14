using System;
using UnityEngine;

[Serializable]
public class GrenadeSettings
{
	public string apiName;

	public int inventoryID;

	public float throwAnimationDuration;

	public float timeUnitilDetonation;

	public Vector3 startPointOffset;

	public Vector3 startRotation;

	public Vector3 angularVelocity;

	public float throwForce;

	public GameObject grenadeInstance;

	public Vector3 GetStartPos(GameObject ply)
	{
		Transform transform = ply.GetComponent<Scp049PlayerScript>().plyCam.transform;
		return transform.TransformPoint(startPointOffset);
	}
}
