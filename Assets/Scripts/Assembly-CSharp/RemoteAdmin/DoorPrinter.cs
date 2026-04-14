using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.RemoteAdmin;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	internal class DoorPrinter : MonoBehaviour
	{
		public GameObject Template;

		public Transform Parent;

		public static string SelectedDoors;

		public static readonly string[] SpecialValues = new string[2] { "*", "!*" };

		public static readonly string[] SpecialTexts = new string[2] { "(All listed)", "(All not listed)" };

		private IEnumerator Start()
		{
			while (PlayerManager.localPlayer == null)
			{
				yield return new WaitForEndOfFrame();
			}
			Door[] alldoors = Object.FindObjectsOfType<Door>();
			List<Door> list = alldoors.Where((Door item) => !string.IsNullOrEmpty(item.DoorName)).ToList();
			list.Sort();
			for (int num = 0; num < SpecialValues.Length; num++)
			{
				GameObject gameObject = Object.Instantiate(Template, Parent);
				gameObject.transform.localScale = Vector3.one;
				gameObject.GetComponentInChildren<Text>().text = SpecialTexts[num];
				gameObject.GetComponent<DoorRemoteAdminButton>().OvrValue = SpecialValues[num];
			}
			foreach (Door item in list)
			{
				GameObject gameObject2 = Object.Instantiate(Template, Parent);
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.GetComponentInChildren<Text>().text = item.DoorName;
				gameObject2.GetComponent<DoorRemoteAdminButton>().Door = item;
				item.RemoteAdminButton = gameObject2.GetComponent<DoorRemoteAdminButton>();
			}
			DoorRemoteAdminButton.Buttons = base.transform.GetComponentsInChildren<DoorRemoteAdminButton>(true);
		}
	}
}
