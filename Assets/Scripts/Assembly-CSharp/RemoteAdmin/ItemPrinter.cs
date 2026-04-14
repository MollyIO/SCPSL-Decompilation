using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class ItemPrinter : MonoBehaviour
	{
		public GameObject template;

		public Transform parent;

		private IEnumerator Start()
		{
			while (PlayerManager.localPlayer == null)
			{
				yield return new WaitForEndOfFrame();
			}
			Item[] items = PlayerManager.localPlayer.GetComponent<Inventory>().availableItems;
			for (int i = 1; i < items.Length; i++)
			{
				GameObject gameObject = Object.Instantiate(template, parent);
				gameObject.transform.localScale = Vector3.one;
				gameObject.GetComponentInChildren<RawImage>().texture = items[i].icon;
				gameObject.GetComponent<PropertyButton>().value = i.ToString();
				gameObject.GetComponent<PropertyButton>().argumentId = 1;
			}
		}
	}
}
