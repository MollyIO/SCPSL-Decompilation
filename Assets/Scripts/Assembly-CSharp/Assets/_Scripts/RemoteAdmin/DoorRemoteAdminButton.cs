using RemoteAdmin;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.RemoteAdmin
{
	internal class DoorRemoteAdminButton : MonoBehaviour
	{
		public Door Door;

		public string OvrValue;

		public bool Selected;

		private Outline _outline;

		private Image _image;

		public static DoorRemoteAdminButton[] Buttons;

		public void Click()
		{
			DoorRemoteAdminButton[] buttons = Buttons;
			foreach (DoorRemoteAdminButton doorRemoteAdminButton in buttons)
			{
				doorRemoteAdminButton.SetStatus(false);
			}
			SetStatus(true);
		}

		private void OnEnable()
		{
			SetStatus(false);
		}

		public void SetStatus(bool b)
		{
			if (_outline == null)
			{
				_outline = GetComponent<Outline>();
			}
			if (_image == null)
			{
				_image = GetComponent<Image>();
			}
			Selected = b;
			if (Selected)
			{
				DoorPrinter.SelectedDoors = ((!(Door != null)) ? OvrValue : Door.DoorName);
			}
			UpdateColor();
		}

		public void UpdateColor()
		{
			_image.color = ((Door == null) ? DoorColor.singleton.Close : (Door.destroyed ? DoorColor.singleton.LockedUnselected : ((!Door.isOpen) ? DoorColor.singleton.Close : DoorColor.singleton.Open)));
			if (Door == null)
			{
				_outline.effectColor = ((!Selected) ? DoorColor.singleton.UnlockedUnselected : DoorColor.singleton.UnlockedSelected);
			}
			else if (Door.locked)
			{
				_outline.effectColor = ((!Selected) ? DoorColor.singleton.LockedUnselected : DoorColor.singleton.LockedSelected);
			}
			else
			{
				_outline.effectColor = ((!Selected) ? DoorColor.singleton.UnlockedUnselected : DoorColor.singleton.UnlockedSelected);
			}
		}
	}
}
