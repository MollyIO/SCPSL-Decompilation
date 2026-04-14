using UnityEngine.UI;

namespace AmplifyBloom
{
	public sealed class DemoUISlider : DemoUIElement
	{
		public bool SingleStep;

		private Slider m_slider;

		private bool m_lastStep;

		private void Start()
		{
			m_slider = GetComponent<Slider>();
		}

		public override void DoAction(DemoUIElementAction action, object[] vars)
		{
			if (!m_slider.IsInteractable() || action != DemoUIElementAction.Slide)
			{
				return;
			}
			float num = (float)vars[0];
			if (SingleStep)
			{
				if (m_lastStep)
				{
					return;
				}
				m_lastStep = true;
			}
			if (m_slider.wholeNumbers)
			{
				if (num > 0f)
				{
					m_slider.value += 1f;
				}
				else if (num < 0f)
				{
					m_slider.value -= 1f;
				}
			}
			else
			{
				m_slider.value += num;
			}
		}

		public override void Idle()
		{
			m_lastStep = false;
		}
	}
}
