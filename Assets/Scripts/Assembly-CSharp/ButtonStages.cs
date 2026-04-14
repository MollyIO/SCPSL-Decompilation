using System;
using UnityEngine;

public class ButtonStages : MonoBehaviour
{
	[Serializable]
	public class Stage
	{
		public Sprite texture;

		public Material mat;

		[Multiline]
		public string info;
	}

	[Serializable]
	public class DoorType
	{
		public Stage[] stages;

		public string description;
	}

	public DoorType[] inspectorTypes;

	public static DoorType[] types;

	private void Start()
	{
		types = inspectorTypes;
	}
}
