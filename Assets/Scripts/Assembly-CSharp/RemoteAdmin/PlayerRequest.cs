using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class PlayerRequest : MonoBehaviour
	{
		private readonly List<GameObject> _spawns = new List<GameObject>();

		public Transform parent;

		public GameObject template;

		public static PlayerRequest singleton;

		private void Awake()
		{
			singleton = this;
		}

		public void ResponsePlayerList(string data, bool isSuccess, bool showClasses)
		{
			if (!isSuccess)
			{
				return;
			}
			List<string> list = new List<string>();
			foreach (PlayerRecord record in PlayerRecord.records)
			{
				if (record.isSelected)
				{
					list.Add(record.playerId);
				}
			}
			PlayerRecord.records = new List<PlayerRecord>();
			foreach (GameObject spawn in _spawns)
			{
				UnityEngine.Object.Destroy(spawn);
			}
			string[] array = data.Split(new string[1] { "\n" }, StringSplitOptions.None);
			foreach (string text in array)
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				bool flag = text.Contains("<OVRM>");
				GameObject gameObject = UnityEngine.Object.Instantiate(template, parent);
				PlayerRecord componentInChildren = gameObject.GetComponentInChildren<PlayerRecord>();
				gameObject.transform.localScale = Vector3.one;
				gameObject.GetComponentInChildren<Text>().text = text.Replace("<OVRM>", string.Empty);
				_spawns.Add(gameObject);
				componentInChildren.Setup(Color.white);
				string text2 = text.Replace("<OVRM>", string.Empty);
				text2 = text2.Remove(0, text2.IndexOf("(", StringComparison.Ordinal) + 1);
				text2 = (componentInChildren.playerId = text2.Remove(text2.IndexOf(")", StringComparison.Ordinal)));
				if (list.Contains(text2))
				{
					componentInChildren.Toggle();
				}
				if (flag)
				{
					componentInChildren.Setup(new Color(0f, 128f, 128f));
				}
				else
				{
					if (!showClasses)
					{
						continue;
					}
					GameObject[] players = PlayerManager.singleton.players;
					foreach (GameObject gameObject2 in players)
					{
						if (!(gameObject2.GetComponent<QueryProcessor>().PlayerId.ToString() != text2))
						{
							CharacterClassManager component = gameObject2.GetComponent<CharacterClassManager>();
							componentInChildren.Setup((component.curClass == 15) ? new Color(0.7f, 0.7f, 0.7f) : ((component.curClass >= 0) ? component.klasy[component.curClass].classColor : Color.white));
						}
					}
				}
			}
		}

		public void ResponsePlayerSpecific(string data, bool isSuccess)
		{
			if (!isSuccess)
			{
				data = "<color=red>" + data + "</color>";
			}
			GetComponent<DisplayDataOnScreen>().Show(1, data);
		}
	}
}
