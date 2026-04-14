using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("SECTR/Vis/SECTR Occluder")]
public class SECTR_Occluder : SECTR_Hull
{
	public enum OrientationAxis
	{
		None = 0,
		XYZ = 1,
		XZ = 2,
		XY = 3,
		YZ = 4
	}

	private SECTR_Member cachedMember;

	private List<SECTR_Sector> currentSectors = new List<SECTR_Sector>(4);

	private static List<SECTR_Occluder> allOccluders = new List<SECTR_Occluder>(32);

	private static Dictionary<SECTR_Sector, List<SECTR_Occluder>> occluderTable = new Dictionary<SECTR_Sector, List<SECTR_Occluder>>(32);

	[SECTR_ToolTip("The axes that should orient towards the camera during culling (if any).")]
	public OrientationAxis AutoOrient;

	public static List<SECTR_Occluder> All
	{
		get
		{
			return allOccluders;
		}
	}

	public SECTR_Member Member
	{
		get
		{
			return cachedMember;
		}
	}

	public Vector3 MeshNormal
	{
		get
		{
			ComputeVerts();
			return meshNormal;
		}
	}

	public static List<SECTR_Occluder> GetOccludersInSector(SECTR_Sector sector)
	{
		List<SECTR_Occluder> value = null;
		occluderTable.TryGetValue(sector, out value);
		return value;
	}

	public Matrix4x4 GetCullingMatrix(Vector3 cameraPos)
	{
		if (AutoOrient == OrientationAxis.None)
		{
			return base.transform.localToWorldMatrix;
		}
		ComputeVerts();
		Vector3 position = base.transform.position;
		Vector3 toDirection = cameraPos - position;
		switch (AutoOrient)
		{
		case OrientationAxis.XY:
			toDirection.z = 0f;
			break;
		case OrientationAxis.XZ:
			toDirection.y = 0f;
			break;
		case OrientationAxis.YZ:
			toDirection.x = 0f;
			break;
		}
		return Matrix4x4.TRS(position, Quaternion.FromToRotation(meshNormal, toDirection), base.transform.lossyScale);
	}

	private void OnEnable()
	{
		cachedMember = GetComponent<SECTR_Member>();
		cachedMember.Changed += _MembershipChanged;
		allOccluders.Add(this);
	}

	private void OnDisable()
	{
		allOccluders.Remove(this);
		cachedMember.Changed -= _MembershipChanged;
		cachedMember = null;
	}

	private void _MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined)
	{
		if (joined != null)
		{
			int count = joined.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_Sector sECTR_Sector = joined[i];
				if ((bool)sECTR_Sector)
				{
					List<SECTR_Occluder> value;
					if (!occluderTable.TryGetValue(sECTR_Sector, out value))
					{
						value = new List<SECTR_Occluder>(4);
						occluderTable[sECTR_Sector] = value;
					}
					value.Add(this);
					currentSectors.Add(sECTR_Sector);
				}
			}
		}
		if (left == null)
		{
			return;
		}
		int count2 = left.Count;
		for (int j = 0; j < count2; j++)
		{
			SECTR_Sector sECTR_Sector2 = left[j];
			if ((bool)sECTR_Sector2 && currentSectors.Contains(sECTR_Sector2))
			{
				List<SECTR_Occluder> value2;
				if (occluderTable.TryGetValue(sECTR_Sector2, out value2))
				{
					value2.Remove(this);
				}
				currentSectors.Remove(sECTR_Sector2);
			}
		}
	}
}
