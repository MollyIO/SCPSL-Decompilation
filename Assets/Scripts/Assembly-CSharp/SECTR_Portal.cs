using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Portal")]
public class SECTR_Portal : SECTR_Hull
{
	[Flags]
	public enum PortalFlags
	{
		Closed = 1,
		Locked = 2,
		PassThrough = 4
	}

	[SerializeField]
	[HideInInspector]
	private SECTR_Sector frontSector;

	[HideInInspector]
	[SerializeField]
	private SECTR_Sector backSector;

	private bool visited;

	private static List<SECTR_Portal> allPortals = new List<SECTR_Portal>(128);

	[SECTR_ToolTip("Flags for this Portal. Used in graph traversals and the like.", null, typeof(PortalFlags))]
	public PortalFlags Flags;

	public static List<SECTR_Portal> All
	{
		get
		{
			return allPortals;
		}
	}

	public SECTR_Sector FrontSector
	{
		get
		{
			return (!frontSector || !frontSector.enabled) ? null : frontSector;
		}
		set
		{
			if (frontSector != value)
			{
				if ((bool)frontSector)
				{
					frontSector.Deregister(this);
				}
				frontSector = value;
				if ((bool)frontSector)
				{
					frontSector.Register(this);
				}
			}
		}
	}

	public SECTR_Sector BackSector
	{
		get
		{
			return (!backSector || !backSector.enabled) ? null : backSector;
		}
		set
		{
			if (backSector != value)
			{
				if ((bool)backSector)
				{
					backSector.Deregister(this);
				}
				backSector = value;
				if ((bool)backSector)
				{
					backSector.Register(this);
				}
			}
		}
	}

	public bool Visited
	{
		get
		{
			return visited;
		}
		set
		{
			visited = value;
		}
	}

	public void Setup()
	{
		bool flag = Mathf.RoundToInt(Vector3.Angle(base.transform.forward, Vector3.forward)) % 180 == 0;
		base.transform.position += Vector3.up / 2f;
		RaycastHit hitInfo;
		if (Physics.Raycast(base.transform.position - base.transform.forward, Vector3.down, out hitInfo))
		{
			FrontSector = hitInfo.collider.GetComponentInParent<SECTR_Sector>();
		}
		if (Physics.Raycast(base.transform.position + base.transform.forward, Vector3.down, out hitInfo))
		{
			BackSector = hitInfo.collider.GetComponentInParent<SECTR_Sector>();
		}
	}

	public Vector3 GetRandomSectorPos()
	{
		return (UnityEngine.Random.Range(0, 100) >= 50) ? backSector.transform.position : frontSector.transform.position;
	}

	public IEnumerable<SECTR_Sector> GetSectors()
	{
		yield return FrontSector;
		yield return BackSector;
	}

	public void SetFlag(PortalFlags flag, bool on)
	{
		if (on)
		{
			Flags |= flag;
		}
		else
		{
			Flags &= ~flag;
		}
	}

	private void OnEnable()
	{
		allPortals.Add(this);
		if ((bool)frontSector)
		{
			frontSector.Register(this);
		}
		if ((bool)backSector)
		{
			backSector.Register(this);
		}
	}

	private void OnDisable()
	{
		allPortals.Remove(this);
		if ((bool)frontSector)
		{
			frontSector.Deregister(this);
		}
		if ((bool)backSector)
		{
			backSector.Deregister(this);
		}
	}
}
