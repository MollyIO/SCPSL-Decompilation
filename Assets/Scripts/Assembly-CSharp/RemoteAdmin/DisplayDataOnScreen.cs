using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class DisplayDataOnScreen : MonoBehaviour
	{
		public static DisplayDataOnScreen singleton;

		private void Awake()
		{
			singleton = this;
		}

		public void Show(Text text, string content)
		{
			text.text = content;
		}

		public void Show(TextMeshProUGUI text, string content)
		{
			text.text = content;
		}

		public void Show(int menuId, string content)
		{
			GetComponent<SubmenuSelector>().menus[menuId].optionalDisplay.text = content;
		}
	}
}
