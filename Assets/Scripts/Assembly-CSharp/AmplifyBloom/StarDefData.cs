using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class StarDefData
	{
		[SerializeField]
		private StarLibType m_starType;

		[SerializeField]
		private string m_starName = string.Empty;

		[SerializeField]
		private int m_starlinesCount = 2;

		[SerializeField]
		private int m_passCount = 4;

		[SerializeField]
		private float m_sampleLength = 1f;

		[SerializeField]
		private float m_attenuation = 0.85f;

		[SerializeField]
		private float m_inclination;

		[SerializeField]
		private float m_rotation;

		[SerializeField]
		private StarLineData[] m_starLinesArr;

		[SerializeField]
		private float m_customIncrement = 90f;

		[SerializeField]
		private float m_longAttenuation;

		public StarLibType StarType
		{
			get
			{
				return m_starType;
			}
			set
			{
				m_starType = value;
			}
		}

		public string StarName
		{
			get
			{
				return m_starName;
			}
			set
			{
				m_starName = value;
			}
		}

		public int StarlinesCount
		{
			get
			{
				return m_starlinesCount;
			}
			set
			{
				m_starlinesCount = value;
				CalculateStarData();
			}
		}

		public int PassCount
		{
			get
			{
				return m_passCount;
			}
			set
			{
				m_passCount = value;
				CalculateStarData();
			}
		}

		public float SampleLength
		{
			get
			{
				return m_sampleLength;
			}
			set
			{
				m_sampleLength = value;
				CalculateStarData();
			}
		}

		public float Attenuation
		{
			get
			{
				return m_attenuation;
			}
			set
			{
				m_attenuation = value;
				CalculateStarData();
			}
		}

		public float Inclination
		{
			get
			{
				return m_inclination;
			}
			set
			{
				m_inclination = value;
				CalculateStarData();
			}
		}

		public float CameraRotInfluence
		{
			get
			{
				return m_rotation;
			}
			set
			{
				m_rotation = value;
			}
		}

		public StarLineData[] StarLinesArr
		{
			get
			{
				return m_starLinesArr;
			}
		}

		public float CustomIncrement
		{
			get
			{
				return m_customIncrement;
			}
			set
			{
				m_customIncrement = value;
				CalculateStarData();
			}
		}

		public float LongAttenuation
		{
			get
			{
				return m_longAttenuation;
			}
			set
			{
				m_longAttenuation = value;
				CalculateStarData();
			}
		}

		public StarDefData()
		{
		}

		public StarDefData(StarLibType starType, string starName, int starLinesCount, int passCount, float sampleLength, float attenuation, float inclination, float rotation, float longAttenuation = 0f, float customIncrement = -1f)
		{
			m_starType = starType;
			m_starName = starName;
			m_passCount = passCount;
			m_sampleLength = sampleLength;
			m_attenuation = attenuation;
			m_starlinesCount = starLinesCount;
			m_inclination = inclination;
			m_rotation = rotation;
			m_customIncrement = customIncrement;
			m_longAttenuation = longAttenuation;
			CalculateStarData();
		}

		public void Destroy()
		{
			m_starLinesArr = null;
		}

		public void CalculateStarData()
		{
			if (m_starlinesCount == 0)
			{
				return;
			}
			m_starLinesArr = new StarLineData[m_starlinesCount];
			float num = ((!(m_customIncrement > 0f)) ? (180f / (float)m_starlinesCount) : m_customIncrement);
			num *= (float)Math.PI / 180f;
			for (int i = 0; i < m_starlinesCount; i++)
			{
				m_starLinesArr[i] = new StarLineData();
				m_starLinesArr[i].PassCount = m_passCount;
				m_starLinesArr[i].SampleLength = m_sampleLength;
				if (m_longAttenuation > 0f)
				{
					m_starLinesArr[i].Attenuation = ((i % 2 != 0) ? m_attenuation : m_longAttenuation);
				}
				else
				{
					m_starLinesArr[i].Attenuation = m_attenuation;
				}
				m_starLinesArr[i].Inclination = num * (float)i;
			}
		}
	}
}
