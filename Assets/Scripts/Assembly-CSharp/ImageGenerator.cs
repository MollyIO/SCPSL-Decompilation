using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageGenerator : MonoBehaviour
{
	[Serializable]
	public class ColorMap
	{
		public Color color = Color.white;

		public RoomType type;

		public float rotationY;

		public Vector2 centerOffset;
	}

	[Serializable]
	public class RoomsOfType
	{
		public List<Room> roomsOfType = new List<Room>();

		public int amount;
	}

	[Serializable]
	public class Room
	{
		public List<GameObject> room = new List<GameObject>();

		public RoomType type;

		public bool required;

		public Texture iconMinimap;

		public string label;

		public Room(Room r)
		{
			room = r.room;
			type = r.type;
			required = r.required;
		}
	}

	[Serializable]
	public class MinimapElement
	{
		public string roomName;

		public Texture icon;

		public Vector2 position;

		public int rotation;

		public GameObject roomSource;
	}

	[Serializable]
	public class MinimapLegend
	{
		public string containsInName;

		public Texture icon;

		public string label;
	}

	public enum RoomType
	{
		Straight = 0,
		Curve = 1,
		RoomT = 2,
		Cross = 3,
		Endoff = 4,
		Prison = 5
	}

	public int height;

	public Texture2D[] maps;

	private Texture2D map;

	private Color[] copy;

	public float gridSize;

	public float minimapSize;

	public List<ColorMap> colorMap = new List<ColorMap>();

	public List<Room> availableRooms = new List<Room>();

	public List<GameObject> doors = new List<GameObject>();

	private List<MinimapElement> minimap = new List<MinimapElement>();

	public MinimapLegend[] legend;

	public RectTransform minimapTarget;

	private Vector3 offset;

	public float y_offset;

	public static PocketDimensionGenerator pocketDimensionGenerator;

	private Transform entrRooms;

	public Font minimapFont;

	public RoomsOfType[] roomsOfType;

	public bool GenerateMap(int seed)
	{
		if (!NonFacilityCompatibility.currentSceneSettings.enableWorldGeneration)
		{
			return true;
		}
		foreach (Room availableRoom in availableRooms)
		{
			foreach (GameObject item in availableRoom.room)
			{
				item.SetActive(false);
			}
		}
		pocketDimensionGenerator = GetComponent<PocketDimensionGenerator>();
		pocketDimensionGenerator.GenerateMap(seed);
		UnityEngine.Random.InitState(seed);
		map = maps[UnityEngine.Random.Range(0, maps.Length)];
		InitEntrance();
		copy = map.GetPixels();
		GeneratorTask_CheckRooms();
		GeneratorTask_RemoveNotRequired();
		GeneratorTask_SetRooms();
		GeneratorTask_Cleanup();
		GeneratorTask_RemoveDoubledDoorPoints();
		GeneratorTask_CreateMinimap();
		map.SetPixels(copy);
		map.Apply();
		if (entrRooms != null)
		{
			entrRooms.parent = null;
		}
		return true;
	}

	private void InitEntrance()
	{
		if (height != -1001)
		{
			return;
		}
		Transform transform = GameObject.Find("Root_Checkpoint").transform;
		entrRooms = GameObject.Find("EntranceRooms").transform;
		for (int i = 0; i < map.height; i++)
		{
			for (int j = 0; j < map.width; j++)
			{
				Color pixel = map.GetPixel(j, i);
				if (pixel == Color.white)
				{
					offset = -new Vector3((float)j * gridSize, 0f, (float)i * gridSize) / 3f;
				}
			}
		}
		offset += Vector3.up;
	}

	private void GeneratorTask_Cleanup()
	{
		RoomsOfType[] array = this.roomsOfType;
		foreach (RoomsOfType roomsOfType in array)
		{
			foreach (Room item in roomsOfType.roomsOfType)
			{
				foreach (GameObject item2 in item.room)
				{
					if (item.type != RoomType.Prison)
					{
						item2.SetActive(false);
					}
				}
			}
		}
	}

	private void GeneratorTask_CreateMinimap()
	{
		if (minimapTarget == null)
		{
			return;
		}
		foreach (MinimapElement item in minimap)
		{
			GameObject gameObject = new GameObject("MINIMAP:" + item.roomSource.name);
			gameObject.transform.SetParent(minimapTarget.transform);
			RawImage rawImage = gameObject.AddComponent<RawImage>();
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.sizeDelta = Vector2.one * 1024f;
			component.localPosition = item.position * minimapSize;
			component.localRotation = Quaternion.Euler(Vector3.back * item.rotation);
			component.localScale = new Vector3(1f, 1f, 1f);
			rawImage.texture = item.icon;
			rawImage.color = new Color(1f, 1f, 1f, 0.02f);
			GameObject gameObject2 = new GameObject("Text");
			gameObject2.transform.SetParent(rawImage.transform);
			gameObject2.AddComponent<Text>();
			gameObject2.GetComponent<RectTransform>().anchorMin = Vector2.zero;
			gameObject2.GetComponent<RectTransform>().anchorMax = Vector2.one;
			gameObject2.GetComponent<RectTransform>().localPosition = Vector3.zero;
			gameObject2.GetComponent<RectTransform>().localScale = Vector2.one;
			gameObject2.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
			gameObject2.GetComponent<RectTransform>().localRotation = Quaternion.Euler(Vector3.forward * item.rotation);
			gameObject2.GetComponent<Text>().font = minimapFont;
			gameObject2.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
			gameObject2.GetComponent<Text>().resizeTextForBestFit = true;
			gameObject2.GetComponent<Text>().resizeTextMaxSize = 180;
			gameObject2.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0.4f);
			rawImage.GetComponentInChildren<Text>().text = item.roomName;
		}
	}

	private void GeneratorTask_RemoveDoubledDoorPoints()
	{
		if (doors.Count == 0)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = GameObject.FindGameObjectsWithTag("DoorPoint" + height);
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		foreach (GameObject item2 in list)
		{
			foreach (GameObject item3 in list)
			{
				if (Vector3.Distance(item2.transform.position, item3.transform.position) < 2f && item2 != item3)
				{
					UnityEngine.Object.DestroyImmediate(item3);
					GeneratorTask_RemoveDoubledDoorPoints();
					return;
				}
			}
		}
		List<SECTR_Portal> list2 = new List<SECTR_Portal>();
		for (int j = 0; j < doors.Count; j++)
		{
			try
			{
				if (j < list.Count)
				{
					doors[j].transform.position = list[j].transform.position;
					doors[j].transform.rotation = list[j].transform.rotation;
					SECTR_Portal component = list[j].GetComponent<SECTR_Portal>();
					if (component != null)
					{
						list2.Add(component);
						if (height % 2 == 0)
						{
							doors[j].GetComponent<Door>().SetPortal(component);
						}
					}
				}
				else
				{
					doors[j].SetActive(false);
				}
			}
			catch
			{
				Debug.LogError("Not enough doors!");
			}
		}
		foreach (SECTR_Portal item4 in list2)
		{
			item4.Setup();
		}
	}

	private void GeneratorTask_SetRooms()
	{
		for (int i = 0; i < map.height; i++)
		{
			for (int j = 0; j < map.width; j++)
			{
				Color pixel = map.GetPixel(j, i);
				foreach (ColorMap item in colorMap)
				{
					if (item.color == pixel)
					{
						Vector2 pos = new Vector2(j, i) + item.centerOffset;
						PlaceRoom(pos, item);
					}
				}
			}
		}
	}

	private void GeneratorTask_RemoveNotRequired()
	{
		foreach (ColorMap item in colorMap)
		{
			bool flag = false;
			while (!flag)
			{
				int num = 0;
				foreach (Room item2 in roomsOfType[(int)item.type].roomsOfType)
				{
					num += item2.room.Count;
				}
				if (num <= roomsOfType[(int)item.type].amount)
				{
					break;
				}
				flag = true;
				for (int i = 0; i < roomsOfType[(int)item.type].roomsOfType.Count; i++)
				{
					if (!roomsOfType[(int)item.type].roomsOfType[i].required && roomsOfType[(int)item.type].roomsOfType[i].room.Count > 0)
					{
						roomsOfType[(int)item.type].roomsOfType[i].room[0].SetActive(false);
						roomsOfType[(int)item.type].roomsOfType[i].room.RemoveAt(0);
						flag = false;
						break;
					}
				}
			}
		}
	}

	private void GeneratorTask_CheckRooms()
	{
		for (int i = 0; i < map.height; i++)
		{
			for (int j = 0; j < map.width; j++)
			{
				Color pixel = map.GetPixel(j, i);
				foreach (ColorMap item in colorMap)
				{
					if (!(item.color == pixel))
					{
						continue;
					}
					BlankSquare(new Vector2(j, i) + item.centerOffset);
					roomsOfType[(int)item.type].amount++;
					List<Room> list = new List<Room>();
					bool flag = false;
					for (int k = 0; k < availableRooms.Count; k++)
					{
						if (availableRooms[k].type == item.type && availableRooms[k].room.Count > 0 && availableRooms[k].required)
						{
							flag = true;
						}
					}
					bool flag2 = false;
					do
					{
						flag2 = false;
						for (int l = 0; l < availableRooms.Count; l++)
						{
							if (availableRooms[l].type == item.type && availableRooms[l].room.Count > 0 && (availableRooms[l].required || !flag))
							{
								list.Add(new Room(availableRooms[l]));
								availableRooms.RemoveAt(l);
								flag2 = true;
								break;
							}
						}
					}
					while (flag2);
					foreach (Room item2 in list)
					{
						roomsOfType[(int)item.type].roomsOfType.Add(new Room(item2));
					}
				}
			}
		}
		map.SetPixels(copy);
		map.Apply();
	}

	private void PlaceRoom(Vector2 pos, ColorMap type)
	{
		string text = string.Empty;
		try
		{
			text = "blanking";
			BlankSquare(pos);
			Room room = null;
			text = "do";
			do
			{
				text = "rand";
				int num = UnityEngine.Random.Range(0, roomsOfType[(int)type.type].roomsOfType.Count);
				text = "rset " + (int)type.type + "/" + roomsOfType.Length + num;
				room = roomsOfType[(int)type.type].roomsOfType[num];
				if (room.room.Count == 0)
				{
					text = "remove";
					roomsOfType[(int)type.type].roomsOfType.RemoveAt(num);
				}
			}
			while (room.room.Count == 0);
			text = "pos";
			room.room[0].transform.localPosition = new Vector3(pos.x * gridSize / 3f, height, pos.y * gridSize / 3f) + offset;
			text = "rot";
			room.room[0].transform.localRotation = Quaternion.Euler(Vector3.up * (type.rotationY + y_offset));
			text = "ver";
			if (minimapTarget != null)
			{
				MinimapLegend minimapLegend = null;
				MinimapLegend[] array = legend;
				foreach (MinimapLegend minimapLegend2 in array)
				{
					if (room.room[0].name.Contains(minimapLegend2.containsInName))
					{
						minimapLegend = minimapLegend2;
					}
				}
				if (minimapLegend != null)
				{
					minimap.Add(new MinimapElement
					{
						icon = minimapLegend.icon,
						position = pos,
						roomName = minimapLegend.label,
						rotation = (int)type.rotationY,
						roomSource = room.room[0].gameObject
					});
				}
			}
			room.room[0].SetActive(true);
			room.room.RemoveAt(0);
		}
		catch (Exception ex)
		{
			Debug.LogError(text + ex.Message);
		}
	}

	private void BlankSquare(Vector2 centerPoint)
	{
		centerPoint = new Vector2(centerPoint.x - 1f, centerPoint.y - 1f);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				map.SetPixel((int)centerPoint.x + i, (int)centerPoint.y + j, new Color(0.3921f, 0.3921f, 0.3921f, 1f));
			}
		}
		map.Apply();
	}

	private void Awake()
	{
		foreach (GameObject door in doors)
		{
			if (door != null)
			{
				door.GetComponent<Door>().SetZero();
			}
		}
	}
}
