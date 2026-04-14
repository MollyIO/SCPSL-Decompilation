using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Sector")]
public class SECTR_Sector : SECTR_Member
{
	private List<SECTR_Portal> portals = new List<SECTR_Portal>(8);

	private List<SECTR_Member> members = new List<SECTR_Member>(32);

	private bool visited;

	private static List<SECTR_Sector> allSectors = new List<SECTR_Sector>(128);

	[SECTR_ToolTip("The terrain Sector attached on the top side of this Sector.")]
	public SECTR_Sector TopTerrain;

	[SECTR_ToolTip("The terrain Sector attached on the bottom side of this Sector.")]
	public SECTR_Sector BottomTerrain;

	[SECTR_ToolTip("The terrain Sector attached on the left side of this Sector.")]
	public SECTR_Sector LeftTerrain;

	[SECTR_ToolTip("The terrain Sector attached on the right side of this Sector.")]
	public SECTR_Sector RightTerrain;

	public new static List<SECTR_Sector> All
	{
		get
		{
			return allSectors;
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

	public List<SECTR_Portal> Portals
	{
		get
		{
			return portals;
		}
	}

	public List<SECTR_Member> Members
	{
		get
		{
			return members;
		}
	}

	public bool IsConnectedTerrain
	{
		get
		{
			return (bool)LeftTerrain || (bool)RightTerrain || (bool)TopTerrain || (bool)BottomTerrain;
		}
	}

	private SECTR_Sector()
	{
		isSector = true;
	}

	public static void GetContaining(ref List<SECTR_Sector> sectors, Vector3 position)
	{
		sectors.Clear();
		int count = allSectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = allSectors[i];
			if (sECTR_Sector.TotalBounds.Contains(position))
			{
				sectors.Add(sECTR_Sector);
			}
		}
	}

	public static void GetContaining(ref List<SECTR_Sector> sectors, Bounds bounds)
	{
		sectors.Clear();
		int count = allSectors.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Sector sECTR_Sector = allSectors[i];
			if (sECTR_Sector.TotalBounds.Intersects(bounds))
			{
				sectors.Add(sECTR_Sector);
			}
		}
	}

	public void ConnectTerrainNeighbors()
	{
		Terrain terrain = GetTerrain(this);
		if ((bool)terrain)
		{
			terrain.SetNeighbors(GetTerrain(LeftTerrain), GetTerrain(TopTerrain), GetTerrain(RightTerrain), GetTerrain(BottomTerrain));
		}
	}

	public void DisonnectTerrainNeighbors()
	{
		Terrain terrain = GetTerrain(this);
		if ((bool)terrain)
		{
			terrain.SetNeighbors(null, null, null, null);
		}
		Terrain terrain2 = GetTerrain(TopTerrain);
		if ((bool)terrain2)
		{
			terrain2.SetNeighbors(GetTerrain(TopTerrain.LeftTerrain), GetTerrain(TopTerrain.TopTerrain), GetTerrain(TopTerrain.RightTerrain), null);
		}
		Terrain terrain3 = GetTerrain(BottomTerrain);
		if ((bool)terrain3)
		{
			terrain3.SetNeighbors(GetTerrain(BottomTerrain.LeftTerrain), null, GetTerrain(BottomTerrain.RightTerrain), GetTerrain(BottomTerrain.BottomTerrain));
		}
		Terrain terrain4 = GetTerrain(LeftTerrain);
		if ((bool)terrain4)
		{
			terrain4.SetNeighbors(GetTerrain(LeftTerrain.LeftTerrain), GetTerrain(LeftTerrain.TopTerrain), null, GetTerrain(LeftTerrain.BottomTerrain));
		}
		Terrain terrain5 = GetTerrain(RightTerrain);
		if ((bool)terrain5)
		{
			terrain5.SetNeighbors(null, GetTerrain(RightTerrain.TopTerrain), GetTerrain(RightTerrain.RightTerrain), GetTerrain(RightTerrain.BottomTerrain));
		}
	}

	public void Register(SECTR_Portal portal)
	{
		if (!portals.Contains(portal))
		{
			portals.Add(portal);
		}
	}

	public void Deregister(SECTR_Portal portal)
	{
		portals.Remove(portal);
	}

	public void Register(SECTR_Member member)
	{
		members.Add(member);
	}

	public void Deregister(SECTR_Member member)
	{
		members.Remove(member);
	}

	protected override void OnEnable()
	{
		allSectors.Add(this);
		if ((bool)TopTerrain || (bool)BottomTerrain || (bool)RightTerrain || (bool)LeftTerrain)
		{
			ConnectTerrainNeighbors();
		}
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		List<SECTR_Member> list = new List<SECTR_Member>(members);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = list[i];
			if ((bool)sECTR_Member)
			{
				sECTR_Member.SectorDisabled(this);
			}
		}
		allSectors.Remove(this);
		base.OnDisable();
	}

	protected static Terrain GetTerrain(SECTR_Sector sector)
	{
		if ((bool)sector)
		{
			SECTR_Member sECTR_Member = ((!sector.childProxy) ? sector : sector.childProxy);
			return sECTR_Member.GetComponentInChildren<Terrain>();
		}
		return null;
	}
}
