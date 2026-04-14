using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public sealed class AmplifyBokeh : IAmplifyItem, ISerializationCallbackReceiver
	{
		private const int PerPassSampleCount = 8;

		[SerializeField]
		private bool m_isActive;

		[SerializeField]
		private bool m_applyOnBloomSource;

		[SerializeField]
		private float m_bokehSampleRadius = 0.5f;

		[SerializeField]
		private Vector4 m_bokehCameraProperties = new Vector4(0.05f, 0.018f, 1.34f, 0.18f);

		[SerializeField]
		private float m_offsetRotation;

		[SerializeField]
		private ApertureShape m_apertureShape = ApertureShape.Hexagon;

		private List<AmplifyBokehData> m_bokehOffsets;

		public ApertureShape ApertureShape
		{
			get
			{
				return m_apertureShape;
			}
			set
			{
				if (m_apertureShape != value)
				{
					m_apertureShape = value;
					CreateBokehOffsets(value);
				}
			}
		}

		public bool ApplyBokeh
		{
			get
			{
				return m_isActive;
			}
			set
			{
				m_isActive = value;
			}
		}

		public bool ApplyOnBloomSource
		{
			get
			{
				return m_applyOnBloomSource;
			}
			set
			{
				m_applyOnBloomSource = value;
			}
		}

		public float BokehSampleRadius
		{
			get
			{
				return m_bokehSampleRadius;
			}
			set
			{
				m_bokehSampleRadius = value;
			}
		}

		public float OffsetRotation
		{
			get
			{
				return m_offsetRotation;
			}
			set
			{
				m_offsetRotation = value;
			}
		}

		public Vector4 BokehCameraProperties
		{
			get
			{
				return m_bokehCameraProperties;
			}
			set
			{
				m_bokehCameraProperties = value;
			}
		}

		public float Aperture
		{
			get
			{
				return m_bokehCameraProperties.x;
			}
			set
			{
				m_bokehCameraProperties.x = value;
			}
		}

		public float FocalLength
		{
			get
			{
				return m_bokehCameraProperties.y;
			}
			set
			{
				m_bokehCameraProperties.y = value;
			}
		}

		public float FocalDistance
		{
			get
			{
				return m_bokehCameraProperties.z;
			}
			set
			{
				m_bokehCameraProperties.z = value;
			}
		}

		public float MaxCoCDiameter
		{
			get
			{
				return m_bokehCameraProperties.w;
			}
			set
			{
				m_bokehCameraProperties.w = value;
			}
		}

		public AmplifyBokeh()
		{
			m_bokehOffsets = new List<AmplifyBokehData>();
			CreateBokehOffsets(ApertureShape.Hexagon);
		}

		public void Destroy()
		{
			for (int i = 0; i < m_bokehOffsets.Count; i++)
			{
				m_bokehOffsets[i].Destroy();
			}
		}

		private void CreateBokehOffsets(ApertureShape shape)
		{
			m_bokehOffsets.Clear();
			switch (shape)
			{
			case ApertureShape.Square:
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation)));
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation + 90f)));
				break;
			case ApertureShape.Hexagon:
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation)));
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation - 75f)));
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation + 75f)));
				break;
			case ApertureShape.Octagon:
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation)));
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation + 65f)));
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation + 90f)));
				m_bokehOffsets.Add(new AmplifyBokehData(CalculateBokehSamples(8, m_offsetRotation + 115f)));
				break;
			}
		}

		private Vector4[] CalculateBokehSamples(int sampleCount, float angle)
		{
			Vector4[] array = new Vector4[sampleCount];
			float f = (float)Math.PI / 180f * angle;
			float num = (float)Screen.width / (float)Screen.height;
			Vector4 vector = new Vector4(m_bokehSampleRadius * Mathf.Cos(f), m_bokehSampleRadius * Mathf.Sin(f));
			vector.x /= num;
			for (int i = 0; i < sampleCount; i++)
			{
				float t = (float)i / ((float)sampleCount - 1f);
				array[i] = Vector4.Lerp(-vector, vector, t);
			}
			return array;
		}

		public void ApplyBokehFilter(RenderTexture source, Material material)
		{
			for (int i = 0; i < m_bokehOffsets.Count; i++)
			{
				m_bokehOffsets[i].BokehRenderTexture = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
			}
			material.SetVector(AmplifyUtils.BokehParamsId, m_bokehCameraProperties);
			for (int j = 0; j < m_bokehOffsets.Count; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[k], m_bokehOffsets[j].Offsets[k]);
				}
				Graphics.Blit(source, m_bokehOffsets[j].BokehRenderTexture, material, 27);
			}
			for (int l = 0; l < m_bokehOffsets.Count - 1; l++)
			{
				material.SetTexture(AmplifyUtils.AnamorphicRTS[l], m_bokehOffsets[l].BokehRenderTexture);
			}
			source.DiscardContents();
			Graphics.Blit(m_bokehOffsets[m_bokehOffsets.Count - 1].BokehRenderTexture, source, material, 28 + (m_bokehOffsets.Count - 2));
			for (int m = 0; m < m_bokehOffsets.Count; m++)
			{
				AmplifyUtils.ReleaseTempRenderTarget(m_bokehOffsets[m].BokehRenderTexture);
				m_bokehOffsets[m].BokehRenderTexture = null;
			}
		}

		public void OnAfterDeserialize()
		{
			CreateBokehOffsets(m_apertureShape);
		}

		public void OnBeforeSerialize()
		{
		}
	}
}
