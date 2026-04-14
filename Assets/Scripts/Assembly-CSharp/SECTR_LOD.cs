using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("SECTR/Vis/SECTR LOD")]
public class SECTR_LOD : MonoBehaviour
{
	[Serializable]
	public class LODEntry
	{
		public GameObject gameObject;

		public Renderer lightmapSource;
	}

	[Serializable]
	public class LODSet
	{
		[SerializeField]
		private List<LODEntry> lodEntries = new List<LODEntry>(16);

		[SerializeField]
		private float threshold;

		public List<LODEntry> LODEntries
		{
			get
			{
				return lodEntries;
			}
		}

		public float Threshold
		{
			get
			{
				return threshold;
			}
			set
			{
				threshold = value;
			}
		}

		public LODEntry Add(GameObject gameObject, Renderer lightmapSource)
		{
			if (GetEntry(gameObject) == null)
			{
				LODEntry lODEntry = new LODEntry();
				lODEntry.gameObject = gameObject;
				lODEntry.lightmapSource = lightmapSource;
				lodEntries.Add(lODEntry);
				return lODEntry;
			}
			return null;
		}

		public void Remove(GameObject gameObject)
		{
			int num = 0;
			while (num < lodEntries.Count)
			{
				if (lodEntries[num].gameObject == gameObject)
				{
					lodEntries.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}

		public LODEntry GetEntry(GameObject gameObject)
		{
			int count = lodEntries.Count;
			for (int i = 0; i < count; i++)
			{
				LODEntry lODEntry = lodEntries[i];
				if (lODEntry.gameObject == gameObject)
				{
					return lODEntry;
				}
			}
			return null;
		}
	}

	[Flags]
	public enum SiblinglFlags
	{
		Behaviors = 1,
		Renderers = 2,
		Lights = 4,
		Colliders = 8,
		RigidBodies = 0x10
	}

	[SerializeField]
	[HideInInspector]
	private Vector3 boundsOffset;

	[SerializeField]
	[HideInInspector]
	private float boundsRadius;

	[HideInInspector]
	[SerializeField]
	private bool boundsUpdated;

	private int activeLOD;

	private bool siblingsDisabled;

	private SECTR_Member cachedMember;

	private List<GameObject> toHide = new List<GameObject>(32);

	private List<LODEntry> toShow = new List<LODEntry>(32);

	private static List<SECTR_LOD> allLODs = new List<SECTR_LOD>(128);

	public List<LODSet> LODs = new List<LODSet>();

	[SECTR_ToolTip("Determines which sibling components are disabled when the LOD is culled.", null, typeof(SiblinglFlags))]
	public SiblinglFlags CullSiblings;

	public static List<SECTR_LOD> All
	{
		get
		{
			return allLODs;
		}
	}

	public void SelectLOD(Camera renderCamera)
	{
		if (!renderCamera)
		{
			return;
		}
		if (!boundsUpdated)
		{
			_CalculateBounds();
		}
		Vector3 b = base.transform.localToWorldMatrix.MultiplyPoint3x4(boundsOffset);
		float num = Vector3.Distance(renderCamera.transform.position, b);
		float num2 = boundsRadius / (Mathf.Tan(renderCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * num) * 2f;
		int num3 = -1;
		int count = LODs.Count;
		for (int i = 0; i < count; i++)
		{
			float num4 = LODs[i].Threshold;
			if (i == activeLOD)
			{
				num4 -= num4 * 0.1f;
			}
			if (num2 >= num4)
			{
				num3 = i;
				break;
			}
		}
		if (num3 != activeLOD)
		{
			_ActivateLOD(num3);
		}
	}

	private void OnEnable()
	{
		allLODs.Add(this);
		cachedMember = GetComponent<SECTR_Member>();
		SECTR_CullingCamera sECTR_CullingCamera = ((SECTR_CullingCamera.All.Count <= 0) ? null : SECTR_CullingCamera.All[0]);
		if ((bool)sECTR_CullingCamera)
		{
			SelectLOD(sECTR_CullingCamera.GetComponent<Camera>());
		}
		else
		{
			_ActivateLOD(0);
		}
	}

	private void OnDisable()
	{
		allLODs.Remove(this);
		cachedMember = null;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.localToWorldMatrix.MultiplyPoint(boundsOffset), boundsRadius);
	}

	private void _ActivateLOD(int lodIndex)
	{
		toHide.Clear();
		toShow.Clear();
		if (activeLOD >= 0 && activeLOD < LODs.Count)
		{
			LODSet lODSet = LODs[activeLOD];
			int count = lODSet.LODEntries.Count;
			for (int i = 0; i < count; i++)
			{
				LODEntry lODEntry = lODSet.LODEntries[i];
				if ((bool)lODEntry.gameObject)
				{
					toHide.Add(lODEntry.gameObject);
				}
			}
		}
		if (lodIndex >= 0 && lodIndex < LODs.Count)
		{
			LODSet lODSet2 = LODs[lodIndex];
			int count2 = lODSet2.LODEntries.Count;
			for (int j = 0; j < count2; j++)
			{
				LODEntry lODEntry2 = lODSet2.LODEntries[j];
				if ((bool)lODEntry2.gameObject)
				{
					toHide.Remove(lODEntry2.gameObject);
					toShow.Add(lODEntry2);
				}
			}
		}
		int count3 = toHide.Count;
		for (int k = 0; k < count3; k++)
		{
			toHide[k].SetActive(false);
		}
		int count4 = toShow.Count;
		for (int l = 0; l < count4; l++)
		{
			LODEntry lODEntry3 = toShow[l];
			lODEntry3.gameObject.SetActive(true);
			if ((bool)lODEntry3.lightmapSource)
			{
				Renderer component = lODEntry3.gameObject.GetComponent<Renderer>();
				if ((bool)component)
				{
					component.lightmapIndex = lODEntry3.lightmapSource.lightmapIndex;
					component.lightmapScaleOffset = lODEntry3.lightmapSource.lightmapScaleOffset;
				}
			}
		}
		activeLOD = lodIndex;
		if (CullSiblings != 0 && ((activeLOD == -1 && !siblingsDisabled) || (activeLOD != -1 && siblingsDisabled)))
		{
			siblingsDisabled = activeLOD == -1;
			if ((CullSiblings & SiblinglFlags.Behaviors) != 0)
			{
				MonoBehaviour[] components = base.gameObject.GetComponents<MonoBehaviour>();
				int num = components.Length;
				for (int m = 0; m < num; m++)
				{
					MonoBehaviour monoBehaviour = components[m];
					if (monoBehaviour != this && monoBehaviour != cachedMember)
					{
						monoBehaviour.enabled = !siblingsDisabled;
					}
				}
			}
			if ((CullSiblings & SiblinglFlags.Renderers) != 0)
			{
				Renderer[] components2 = base.gameObject.GetComponents<Renderer>();
				int num2 = components2.Length;
				for (int n = 0; n < num2; n++)
				{
					components2[n].enabled = !siblingsDisabled;
				}
			}
			if ((CullSiblings & SiblinglFlags.Lights) != 0)
			{
				Light[] components3 = base.gameObject.GetComponents<Light>();
				int num3 = components3.Length;
				for (int num4 = 0; num4 < num3; num4++)
				{
					components3[num4].enabled = !siblingsDisabled;
				}
			}
			if ((CullSiblings & SiblinglFlags.Colliders) != 0)
			{
				Collider[] components4 = base.gameObject.GetComponents<Collider>();
				int num5 = components4.Length;
				for (int num6 = 0; num6 < num5; num6++)
				{
					components4[num6].enabled = !siblingsDisabled;
				}
			}
			if ((CullSiblings & SiblinglFlags.RigidBodies) != 0)
			{
				Rigidbody[] components5 = base.gameObject.GetComponents<Rigidbody>();
				int num7 = components5.Length;
				for (int num8 = 0; num8 < num7; num8++)
				{
					if (siblingsDisabled)
					{
						components5[num8].Sleep();
					}
					else
					{
						components5[num8].WakeUp();
					}
				}
			}
		}
		cachedMember.ForceUpdate(true);
	}

	private void _CalculateBounds()
	{
		Bounds bounds = default(Bounds);
		int count = LODs.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			LODSet lODSet = LODs[i];
			int count2 = lODSet.LODEntries.Count;
			for (int j = 0; j < count2; j++)
			{
				GameObject gameObject = lODSet.LODEntries[j].gameObject;
				Renderer renderer = ((!gameObject) ? null : gameObject.GetComponent<Renderer>());
				if ((bool)renderer && renderer.bounds.extents != Vector3.zero)
				{
					if (!flag)
					{
						bounds = renderer.bounds;
						flag = true;
					}
					else
					{
						bounds.Encapsulate(renderer.bounds);
					}
				}
			}
		}
		boundsOffset = base.transform.worldToLocalMatrix.MultiplyPoint(bounds.center);
		boundsRadius = bounds.extents.magnitude;
		boundsUpdated = true;
	}
}
