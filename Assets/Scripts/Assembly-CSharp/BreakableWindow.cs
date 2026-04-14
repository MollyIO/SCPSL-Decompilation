using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class BreakableWindow : NetworkBehaviour
{
	public struct BreakableWindowStatus
	{
		public Vector3 position;

		public Quaternion rotation;

		public bool broken;

		public bool IsEqual(BreakableWindowStatus stat)
		{
			return position == stat.position && rotation == stat.rotation && broken == stat.broken;
		}
	}

	public GameObject template;

	public Transform parent;

	public Vector3 size;

	private BreakableWindowStatus prevStatus;

	[SyncVar(hook = "SetStatus")]
	public BreakableWindowStatus syncStatus;

	public float health = 30f;

	public bool isBroken;

	private MeshRenderer[] meshRenderers;

	public BreakableWindowStatus NetworksyncStatus
	{
		get
		{
			return syncStatus;
		}
		[param: In]
		set
		{
			BreakableWindow breakableWindow = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetStatus(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref breakableWindow.syncStatus, 1u);
		}
	}

	public void SetStatus(BreakableWindowStatus s)
	{
		NetworksyncStatus = s;
	}

	[ServerCallback]
	private void UpdateStatus(BreakableWindowStatus s)
	{
		if (NetworkServer.active)
		{
			SetStatus(s);
		}
	}

	[ServerCallback]
	public void ServerDamageWindow(float damage)
	{
		if (NetworkServer.active)
		{
			health -= damage;
			if (health <= 0f)
			{
				StartCoroutine(BreakWindow());
			}
		}
	}

	private void Awake()
	{
		meshRenderers = GetComponentsInChildren<MeshRenderer>();
		GetComponent<Collider>().enabled = false;
		Invoke("EnableColliders", 1f);
	}

	private void EnableColliders()
	{
		GetComponent<Collider>().enabled = true;
	}

	private void Update()
	{
		if (base.transform.position == syncStatus.position && base.transform.rotation == syncStatus.rotation && isBroken == syncStatus.broken)
		{
			return;
		}
		if (NetworkServer.active)
		{
			BreakableWindowStatus s = new BreakableWindowStatus
			{
				position = base.transform.position,
				rotation = base.transform.rotation,
				broken = isBroken
			};
			UpdateStatus(s);
			return;
		}
		if (!isBroken && syncStatus.broken)
		{
			StartCoroutine(BreakWindow());
		}
		base.transform.position = syncStatus.position;
		base.transform.rotation = syncStatus.rotation;
		isBroken = syncStatus.broken;
	}

	private void LateUpdate()
	{
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.shadowCastingMode = (isBroken ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.Off);
			meshRenderer.enabled = !isBroken;
			meshRenderer.gameObject.layer = ((!isBroken) ? 14 : 28);
		}
	}

	private IEnumerator BreakWindow()
	{
		isBroken = true;
		if (ServerStatic.IsDedicated)
		{
			yield break;
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collider.enabled = false;
		}
		GameObject g = Object.Instantiate(template, parent);
		g.transform.localScale = Vector3.one;
		g.transform.localPosition = Vector3.zero;
		g.transform.localRotation = Quaternion.Euler(Vector3.zero);
		Rigidbody[] rbs = g.GetComponentsInChildren<Rigidbody>();
		List<Vector3> scales = new List<Vector3>();
		g = null;
		Rigidbody[] array = rbs;
		foreach (Rigidbody rigidbody in array)
		{
			rigidbody.angularVelocity = new Vector3(Random.Range(-360, 360), Random.Range(-360, 360), Random.Range(-360, 360));
			rigidbody.velocity = new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2));
			scales.Add(rigidbody.transform.localScale);
		}
		for (int k = 0; k < 250; k++)
		{
			for (int l = 0; l < scales.Count; l++)
			{
				rbs[l].transform.localScale = Vector3.Lerp(scales[l], scales[l] / 2f, (float)k / 75f);
			}
			yield return 0f;
		}
		for (float i2 = 0f; i2 < 150f; i2 += 1f)
		{
			for (int m = 0; m < scales.Count; m++)
			{
				rbs[m].transform.localScale = Vector3.Lerp(scales[m] / 2f, Vector3.zero, i2 / 150f);
			}
			yield return 0f;
		}
		Rigidbody[] array2 = rbs;
		foreach (Rigidbody rigidbody2 in array2)
		{
			Object.Destroy(rigidbody2.gameObject, 1f);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteBreakableWindowStatus_BreakableWindow(writer, syncStatus);
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
			GeneratedNetworkCode._WriteBreakableWindowStatus_BreakableWindow(writer, syncStatus);
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
			syncStatus = GeneratedNetworkCode._ReadBreakableWindowStatus_BreakableWindow(reader);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetStatus(GeneratedNetworkCode._ReadBreakableWindowStatus_BreakableWindow(reader));
		}
	}
}
