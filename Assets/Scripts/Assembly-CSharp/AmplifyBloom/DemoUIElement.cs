using UnityEngine;
using UnityEngine.UI;

namespace AmplifyBloom
{
	public class DemoUIElement : MonoBehaviour
	{
		private bool m_isSelected;

		private Text m_text;

		private Color m_selectedColor = new Color(1f, 1f, 1f);

		private Color m_unselectedColor;

		public bool Select
		{
			get
			{
				return m_isSelected;
			}
			set
			{
				m_isSelected = value;
				m_text.color = ((!value) ? m_unselectedColor : m_selectedColor);
			}
		}

		public void Init()
		{
			m_text = base.transform.GetComponentInChildren<Text>();
			m_unselectedColor = m_text.color;
		}

		public virtual void DoAction(DemoUIElementAction action, params object[] vars)
		{
		}

		public virtual void Idle()
		{
		}
	}
}
