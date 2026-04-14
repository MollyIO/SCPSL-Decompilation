using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LCZ_LabelManager : MonoBehaviour
{
	[Serializable]
	public class LCZ_Label_Preset
	{
		public string nameToContain;

		public Material mat;
	}

	private List<LCZ_Label> labels = new List<LCZ_Label>();

	public LCZ_Label_Preset[] chars;

	public Material[] numbers;

	private List<GameObject> rooms = new List<GameObject>();

	private void Start()
	{
		labels = UnityEngine.Object.FindObjectsOfType<LCZ_Label>().ToList();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (base.transform.GetChild(i).name.StartsWith("Root_"))
			{
				rooms.Add(base.transform.GetChild(i).gameObject);
			}
		}
	}

	public void RefreshLabels()
	{
		foreach (LCZ_Label label in labels)
		{
			bool flag = true;
			Vector3 position = label.transform.position;
			position += label.transform.forward * 10f;
			Debug.DrawLine(position, position + Vector3.up * 30f, Color.red, 20f);
			foreach (GameObject room in rooms)
			{
				if (!(Vector3.Distance(room.transform.position, position) < 10f))
				{
					continue;
				}
				int num = 0;
				LCZ_Label_Preset[] array = chars;
				foreach (LCZ_Label_Preset lCZ_Label_Preset in array)
				{
					if (!room.name.Contains(lCZ_Label_Preset.nameToContain))
					{
						continue;
					}
					flag = false;
					int num2 = 0;
					if (room.name.Contains("("))
					{
						try
						{
							string text = room.name.Remove(0, room.name.IndexOf('(') + 1);
							text = text.Remove(text.IndexOf(')'));
							num2 = int.Parse(text);
						}
						catch
						{
						}
					}
					label.Refresh(lCZ_Label_Preset.mat, numbers[num2], num.ToString());
				}
				num++;
			}
			if (flag)
			{
				label.Refresh(chars[0].mat, numbers[0], "none");
			}
		}
	}
}
