using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class PlayerRecord : MonoBehaviour
	{
		public Text myText;

		public bool isSelected;

		public string playerId;

		public static List<PlayerRecord> records = new List<PlayerRecord>();

		public void Toggle()
		{
			bool flag = !isSelected;
			if (!Input.GetKey(KeyCode.LeftControl))
			{
				foreach (PlayerRecord record in records)
				{
					if (record.isSelected)
					{
						record.isSelected = false;
						flag = true;
					}
				}
			}
			isSelected = flag;
			if (playerId == "unconnected")
			{
				isSelected = false;
			}
		}

		private void Update()
		{
			if ((!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) || !Input.GetKeyDown(KeyCode.A))
			{
				return;
			}
			foreach (PlayerRecord record in records)
			{
				record.isSelected = true;
			}
		}

		private void LateUpdate()
		{
			myText.GetComponent<Outline>().enabled = isSelected;
		}

		private void Start()
		{
			records.Add(this);
		}

		public void Setup(Color c)
		{
			myText.color = c;
		}
	}
}
