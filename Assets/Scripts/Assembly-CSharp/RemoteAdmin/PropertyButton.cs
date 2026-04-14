using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class PropertyButton : MonoBehaviour
	{
		private PropertyButton[] _otherbuttons;

		private Outline _outline;

		private Color _color;

		public int argumentId;

		public string value;

		private void Start()
		{
			_color = GetComponentInParent<SubmenuSelector>().c_selected;
			_otherbuttons = base.transform.parent.GetComponentsInChildren<PropertyButton>(true);
		}

		public void Click()
		{
			PropertyButton[] otherbuttons = _otherbuttons;
			foreach (PropertyButton propertyButton in otherbuttons)
			{
				propertyButton.SetStatus(false);
			}
			SetStatus(true);
		}

		private void OnEnable()
		{
			SetStatus(false);
		}

		private void SetStatus(bool b)
		{
			if (_outline == null)
			{
				_outline = GetComponent<Outline>();
			}
			_outline.effectColor = ((!b) ? Color.white : _color);
			if (b)
			{
				GetComponentInParent<SubmenuSelector>().SetProperty(argumentId, value);
			}
		}
	}
}
