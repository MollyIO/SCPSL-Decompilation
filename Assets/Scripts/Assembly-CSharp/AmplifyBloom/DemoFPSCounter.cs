using UnityEngine;
using UnityEngine.UI;

namespace AmplifyBloom
{
	public class DemoFPSCounter : MonoBehaviour
	{
		public float UpdateInterval = 0.5f;

		private Text m_fpsText;

		private float m_accum;

		private int m_frames;

		private float m_timeleft;

		private float m_fps;

		private string m_format;

		private void Start()
		{
			m_fpsText = GetComponent<Text>();
			m_timeleft = UpdateInterval;
		}

		private void Update()
		{
			m_timeleft -= Time.deltaTime;
			m_accum += Time.timeScale / Time.deltaTime;
			m_frames++;
			if ((double)m_timeleft <= 0.0)
			{
				m_fps = m_accum / (float)m_frames;
				m_format = string.Format("{0:F2} FPS", m_fps);
				m_fpsText.text = m_format;
				if (m_fps < 50f)
				{
					m_fpsText.color = Color.yellow;
				}
				else if (m_fps < 30f)
				{
					m_fpsText.color = Color.red;
				}
				else
				{
					m_fpsText.color = Color.green;
				}
				m_timeleft = UpdateInterval;
				m_accum = 0f;
				m_frames = 0;
			}
		}
	}
}
