using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Scp079Interactable : MonoBehaviour
{
	[Serializable]
	public struct ZoneAndRoom
	{
		public string currentZone;

		public string currentRoom;
	}

	public enum InteractableType
	{
		Camera = 0,
		Door = 1,
		Tesla = 2,
		Light = 3,
		Speaker = 4,
		ElevatorTeleport = 5,
		Lockdown = 6,
		ElevatorUse = 7
	}

	public List<ZoneAndRoom> currentZonesAndRooms = new List<ZoneAndRoom>();

	public InteractableType type;

	public bool sameRoomOnly;

	public GameObject optionalObject;

	public string optionalParameter;

	public void OnMapGenerate()
	{
		Vector3[] array = new Vector3[4]
		{
			Vector3.left,
			Vector3.right,
			Vector3.forward,
			Vector3.back
		};
		foreach (Vector3 vector in array)
		{
			ZoneAndRoom item = default(ZoneAndRoom);
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(base.transform.position + Vector3.up + vector, Vector3.down), out hitInfo, 50f, Interface079.singleton.roomDetectionMask))
			{
				Transform parent = hitInfo.transform;
				while (parent != null && !parent.transform.name.ToUpper().Contains("ROOT"))
				{
					parent = parent.transform.parent;
				}
				if (parent != null)
				{
					item = new ZoneAndRoom
					{
						currentRoom = parent.transform.name,
						currentZone = parent.transform.parent.name
					};
				}
			}
			if (!currentZonesAndRooms.Contains(item))
			{
				currentZonesAndRooms.Add(item);
			}
		}
	}

	public bool IsVisible(string curZone, string curRoom)
	{
		if (!sameRoomOnly)
		{
			return true;
		}
		foreach (ZoneAndRoom currentZonesAndRoom in currentZonesAndRooms)
		{
			if (currentZonesAndRoom.currentZone == curZone && currentZonesAndRoom.currentRoom == curRoom)
			{
				return true;
			}
		}
		return false;
	}
}
