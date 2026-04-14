using System;
using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
	[Serializable]
	public class PickupPositionRelation
	{
		public Pickup pickup;

		public int itemID;

		public string posID;
	}

	[Serializable]
	public class PositionPosIdRelation
	{
		public string posID;

		public Transform position;

		public int index;
	}

	public PickupPositionRelation[] pickups;

	public PositionPosIdRelation[] posIds;

	public void RefreshIndexes()
	{
		for (int i = 0; i < posIds.Length; i++)
		{
			posIds[i].index = i;
		}
	}
}
