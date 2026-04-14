using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AmplifyBloom
{
	[Serializable]
	[AddComponentMenu("")]
	public class AmplifyBloomBase : MonoBehaviour
	{
		public const int MaxGhosts = 5;

		public const int MinDownscales = 1;

		public const int MaxDownscales = 6;

		public const int MaxGaussian = 8;

		private const float MaxDirtIntensity = 1f;

		private const float MaxStarburstIntensity = 1f;

		[SerializeField]
		private Texture m_maskTexture;

		[SerializeField]
		private RenderTexture m_targetTexture;

		[SerializeField]
		private bool m_showDebugMessages = true;

		[SerializeField]
		private int m_softMaxdownscales = 6;

		[SerializeField]
		private DebugToScreenEnum m_debugToScreen;

		[SerializeField]
		private bool m_highPrecision;

		[SerializeField]
		private Vector4 m_bloomRange = new Vector4(500f, 1f, 0f, 0f);

		[SerializeField]
		private float m_overallThreshold = 0.53f;

		[SerializeField]
		private Vector4 m_bloomParams = new Vector4(0.8f, 1f, 1f, 1f);

		[SerializeField]
		private bool m_temporalFilteringActive;

		[SerializeField]
		private float m_temporalFilteringValue = 0.05f;

		[SerializeField]
		private int m_bloomDownsampleCount = 6;

		[SerializeField]
		private AnimationCurve m_temporalFilteringCurve;

		[SerializeField]
		private bool m_separateFeaturesThreshold;

		[SerializeField]
		private float m_featuresThreshold = 0.05f;

		[SerializeField]
		private AmplifyLensFlare m_lensFlare = new AmplifyLensFlare();

		[SerializeField]
		private bool m_applyLensDirt = true;

		[SerializeField]
		private float m_lensDirtStrength = 2f;

		[SerializeField]
		private Texture m_lensDirtTexture;

		[SerializeField]
		private bool m_applyLensStardurst = true;

		[SerializeField]
		private Texture m_lensStardurstTex;

		[SerializeField]
		private float m_lensStarburstStrength = 2f;

		[SerializeField]
		private AmplifyGlare m_anamorphicGlare = new AmplifyGlare();

		[SerializeField]
		private AmplifyBokeh m_bokehFilter = new AmplifyBokeh();

		[SerializeField]
		private float[] m_upscaleWeights = new float[6] { 0.0842f, 0.1282f, 0.1648f, 0.2197f, 0.2197f, 0.1831f };

		[SerializeField]
		private float[] m_gaussianRadius = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

		[SerializeField]
		private int[] m_gaussianSteps = new int[6] { 1, 1, 1, 1, 1, 1 };

		[SerializeField]
		private float[] m_lensDirtWeights = new float[6] { 0.067f, 0.102f, 0.1311f, 0.1749f, 0.2332f, 0.3f };

		[SerializeField]
		private float[] m_lensStarburstWeights = new float[6] { 0.067f, 0.102f, 0.1311f, 0.1749f, 0.2332f, 0.3f };

		[SerializeField]
		private bool[] m_downscaleSettingsFoldout = new bool[6];

		[SerializeField]
		private int m_featuresSourceId;

		[SerializeField]
		private UpscaleQualityEnum m_upscaleQuality;

		[SerializeField]
		private MainThresholdSizeEnum m_mainThresholdSize;

		private Transform m_cameraTransform;

		private Matrix4x4 m_starburstMat;

		private Shader m_bloomShader;

		private Material m_bloomMaterial;

		private Shader m_finalCompositionShader;

		private Material m_finalCompositionMaterial;

		private RenderTexture m_tempFilterBuffer;

		private Camera m_camera;

		private RenderTexture[] m_tempUpscaleRTs = new RenderTexture[6];

		private RenderTexture[] m_tempAuxDownsampleRTs = new RenderTexture[6];

		private Vector2[] m_tempDownsamplesSizes = new Vector2[6];

		private bool silentError;

		public AmplifyGlare LensGlareInstance
		{
			get
			{
				return m_anamorphicGlare;
			}
		}

		public AmplifyBokeh BokehFilterInstance
		{
			get
			{
				return m_bokehFilter;
			}
		}

		public AmplifyLensFlare LensFlareInstance
		{
			get
			{
				return m_lensFlare;
			}
		}

		public bool ApplyLensDirt
		{
			get
			{
				return m_applyLensDirt;
			}
			set
			{
				m_applyLensDirt = value;
			}
		}

		public float LensDirtStrength
		{
			get
			{
				return m_lensDirtStrength;
			}
			set
			{
				m_lensDirtStrength = ((!(value < 0f)) ? value : 0f);
			}
		}

		public Texture LensDirtTexture
		{
			get
			{
				return m_lensDirtTexture;
			}
			set
			{
				m_lensDirtTexture = value;
			}
		}

		public bool ApplyLensStardurst
		{
			get
			{
				return m_applyLensStardurst;
			}
			set
			{
				m_applyLensStardurst = value;
			}
		}

		public Texture LensStardurstTex
		{
			get
			{
				return m_lensStardurstTex;
			}
			set
			{
				m_lensStardurstTex = value;
			}
		}

		public float LensStarburstStrength
		{
			get
			{
				return m_lensStarburstStrength;
			}
			set
			{
				m_lensStarburstStrength = ((!(value < 0f)) ? value : 0f);
			}
		}

		public PrecisionModes CurrentPrecisionMode
		{
			get
			{
				if (m_highPrecision)
				{
					return PrecisionModes.High;
				}
				return PrecisionModes.Low;
			}
			set
			{
				HighPrecision = value == PrecisionModes.High;
			}
		}

		public bool HighPrecision
		{
			get
			{
				return m_highPrecision;
			}
			set
			{
				if (m_highPrecision != value)
				{
					m_highPrecision = value;
					CleanTempFilterRT();
				}
			}
		}

		public float BloomRange
		{
			get
			{
				return m_bloomRange.x;
			}
			set
			{
				m_bloomRange.x = ((!(value < 0f)) ? value : 0f);
			}
		}

		public float OverallThreshold
		{
			get
			{
				return m_overallThreshold;
			}
			set
			{
				m_overallThreshold = ((!(value < 0f)) ? value : 0f);
			}
		}

		public Vector4 BloomParams
		{
			get
			{
				return m_bloomParams;
			}
			set
			{
				m_bloomParams = value;
			}
		}

		public float OverallIntensity
		{
			get
			{
				return m_bloomParams.x;
			}
			set
			{
				m_bloomParams.x = ((!(value < 0f)) ? value : 0f);
			}
		}

		public float BloomScale
		{
			get
			{
				return m_bloomParams.w;
			}
			set
			{
				m_bloomParams.w = ((!(value < 0f)) ? value : 0f);
			}
		}

		public float UpscaleBlurRadius
		{
			get
			{
				return m_bloomParams.z;
			}
			set
			{
				m_bloomParams.z = value;
			}
		}

		public bool TemporalFilteringActive
		{
			get
			{
				return m_temporalFilteringActive;
			}
			set
			{
				if (m_temporalFilteringActive != value)
				{
					CleanTempFilterRT();
				}
				m_temporalFilteringActive = value;
			}
		}

		public float TemporalFilteringValue
		{
			get
			{
				return m_temporalFilteringValue;
			}
			set
			{
				m_temporalFilteringValue = value;
			}
		}

		public int SoftMaxdownscales
		{
			get
			{
				return m_softMaxdownscales;
			}
		}

		public int BloomDownsampleCount
		{
			get
			{
				return m_bloomDownsampleCount;
			}
			set
			{
				m_bloomDownsampleCount = Mathf.Clamp(value, 1, m_softMaxdownscales);
			}
		}

		public int FeaturesSourceId
		{
			get
			{
				return m_featuresSourceId;
			}
			set
			{
				m_featuresSourceId = Mathf.Clamp(value, 0, m_bloomDownsampleCount - 1);
			}
		}

		public bool[] DownscaleSettingsFoldout
		{
			get
			{
				return m_downscaleSettingsFoldout;
			}
		}

		public float[] UpscaleWeights
		{
			get
			{
				return m_upscaleWeights;
			}
		}

		public float[] LensDirtWeights
		{
			get
			{
				return m_lensDirtWeights;
			}
		}

		public float[] LensStarburstWeights
		{
			get
			{
				return m_lensStarburstWeights;
			}
		}

		public float[] GaussianRadius
		{
			get
			{
				return m_gaussianRadius;
			}
		}

		public int[] GaussianSteps
		{
			get
			{
				return m_gaussianSteps;
			}
		}

		public AnimationCurve TemporalFilteringCurve
		{
			get
			{
				return m_temporalFilteringCurve;
			}
			set
			{
				m_temporalFilteringCurve = value;
			}
		}

		public bool SeparateFeaturesThreshold
		{
			get
			{
				return m_separateFeaturesThreshold;
			}
			set
			{
				m_separateFeaturesThreshold = value;
			}
		}

		public float FeaturesThreshold
		{
			get
			{
				return m_featuresThreshold;
			}
			set
			{
				m_featuresThreshold = ((!(value < 0f)) ? value : 0f);
			}
		}

		public DebugToScreenEnum DebugToScreen
		{
			get
			{
				return m_debugToScreen;
			}
			set
			{
				m_debugToScreen = value;
			}
		}

		public UpscaleQualityEnum UpscaleQuality
		{
			get
			{
				return m_upscaleQuality;
			}
			set
			{
				m_upscaleQuality = value;
			}
		}

		public bool ShowDebugMessages
		{
			get
			{
				return m_showDebugMessages;
			}
			set
			{
				m_showDebugMessages = value;
			}
		}

		public MainThresholdSizeEnum MainThresholdSize
		{
			get
			{
				return m_mainThresholdSize;
			}
			set
			{
				m_mainThresholdSize = value;
			}
		}

		public RenderTexture TargetTexture
		{
			get
			{
				return m_targetTexture;
			}
			set
			{
				m_targetTexture = value;
			}
		}

		public Texture MaskTexture
		{
			get
			{
				return m_maskTexture;
			}
			set
			{
				m_maskTexture = value;
			}
		}

		public bool ApplyBokehFilter
		{
			get
			{
				return m_bokehFilter.ApplyBokeh;
			}
			set
			{
				m_bokehFilter.ApplyBokeh = value;
			}
		}

		public bool ApplyLensFlare
		{
			get
			{
				return m_lensFlare.ApplyLensFlare;
			}
			set
			{
				m_lensFlare.ApplyLensFlare = value;
			}
		}

		public bool ApplyLensGlare
		{
			get
			{
				return m_anamorphicGlare.ApplyLensGlare;
			}
			set
			{
				m_anamorphicGlare.ApplyLensGlare = value;
			}
		}

		private void Awake()
		{
			if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
			{
				AmplifyUtils.DebugLog("Null graphics device detected. Skipping effect silently.", LogType.Error);
				silentError = true;
				return;
			}
			if (!AmplifyUtils.IsInitialized)
			{
				AmplifyUtils.InitializeIds();
			}
			for (int i = 0; i < 6; i++)
			{
				m_tempDownsamplesSizes[i] = new Vector2(0f, 0f);
			}
			m_cameraTransform = base.transform;
			m_tempFilterBuffer = null;
			m_starburstMat = Matrix4x4.identity;
			if (m_temporalFilteringCurve == null)
			{
				m_temporalFilteringCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0.999f));
			}
			m_bloomShader = Shader.Find("Hidden/AmplifyBloom");
			if (m_bloomShader != null)
			{
				m_bloomMaterial = new Material(m_bloomShader);
				m_bloomMaterial.hideFlags = HideFlags.DontSave;
			}
			else
			{
				AmplifyUtils.DebugLog("Main Bloom shader not found", LogType.Error);
				base.gameObject.SetActive(false);
			}
			m_finalCompositionShader = Shader.Find("Hidden/BloomFinal");
			if (m_finalCompositionShader != null)
			{
				m_finalCompositionMaterial = new Material(m_finalCompositionShader);
				if (!m_finalCompositionMaterial.GetTag(AmplifyUtils.ShaderModeTag, false).Equals(AmplifyUtils.ShaderModeValue))
				{
					if (m_showDebugMessages)
					{
						AmplifyUtils.DebugLog("Amplify Bloom is running on a limited hardware and may lead to a decrease on its visual quality.", LogType.Warning);
					}
				}
				else
				{
					m_softMaxdownscales = 6;
				}
				m_finalCompositionMaterial.hideFlags = HideFlags.DontSave;
				if (m_lensDirtTexture == null)
				{
					m_lensDirtTexture = m_finalCompositionMaterial.GetTexture(AmplifyUtils.LensDirtRTId);
				}
				if (m_lensStardurstTex == null)
				{
					m_lensStardurstTex = m_finalCompositionMaterial.GetTexture(AmplifyUtils.LensStarburstRTId);
				}
			}
			else
			{
				AmplifyUtils.DebugLog("Bloom Composition shader not found", LogType.Error);
				base.gameObject.SetActive(false);
			}
			m_camera = GetComponent<Camera>();
			m_camera.depthTextureMode |= DepthTextureMode.Depth;
			m_lensFlare.CreateLUTexture();
		}

		private void OnDestroy()
		{
			if (m_bokehFilter != null)
			{
				m_bokehFilter.Destroy();
				m_bokehFilter = null;
			}
			if (m_anamorphicGlare != null)
			{
				m_anamorphicGlare.Destroy();
				m_anamorphicGlare = null;
			}
			if (m_lensFlare != null)
			{
				m_lensFlare.Destroy();
				m_lensFlare = null;
			}
		}

		private void ApplyGaussianBlur(RenderTexture renderTexture, int amount, float radius = 1f, bool applyTemporal = false)
		{
			if (amount == 0)
			{
				return;
			}
			m_bloomMaterial.SetFloat(AmplifyUtils.BlurRadiusId, radius);
			RenderTexture tempRenderTarget = AmplifyUtils.GetTempRenderTarget(renderTexture.width, renderTexture.height);
			for (int i = 0; i < amount; i++)
			{
				tempRenderTarget.DiscardContents();
				Graphics.Blit(renderTexture, tempRenderTarget, m_bloomMaterial, 14);
				if (m_temporalFilteringActive && applyTemporal && i == amount - 1)
				{
					if (m_tempFilterBuffer != null && m_temporalFilteringActive)
					{
						float value = m_temporalFilteringCurve.Evaluate(m_temporalFilteringValue);
						m_bloomMaterial.SetFloat(AmplifyUtils.TempFilterValueId, value);
						m_bloomMaterial.SetTexture(AmplifyUtils.AnamorphicRTS[0], m_tempFilterBuffer);
						renderTexture.DiscardContents();
						Graphics.Blit(tempRenderTarget, renderTexture, m_bloomMaterial, 16);
					}
					else
					{
						renderTexture.DiscardContents();
						Graphics.Blit(tempRenderTarget, renderTexture, m_bloomMaterial, 15);
					}
					bool flag = false;
					if (m_tempFilterBuffer != null)
					{
						if (m_tempFilterBuffer.format != renderTexture.format || m_tempFilterBuffer.width != renderTexture.width || m_tempFilterBuffer.height != renderTexture.height)
						{
							CleanTempFilterRT();
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						CreateTempFilterRT(renderTexture);
					}
					m_tempFilterBuffer.DiscardContents();
					Graphics.Blit(renderTexture, m_tempFilterBuffer);
				}
				else
				{
					renderTexture.DiscardContents();
					Graphics.Blit(tempRenderTarget, renderTexture, m_bloomMaterial, 15);
				}
			}
			AmplifyUtils.ReleaseTempRenderTarget(tempRenderTarget);
		}

		private void CreateTempFilterRT(RenderTexture source)
		{
			if (m_tempFilterBuffer != null)
			{
				CleanTempFilterRT();
			}
			m_tempFilterBuffer = new RenderTexture(source.width, source.height, 0, source.format, AmplifyUtils.CurrentReadWriteMode);
			m_tempFilterBuffer.filterMode = AmplifyUtils.CurrentFilterMode;
			m_tempFilterBuffer.wrapMode = AmplifyUtils.CurrentWrapMode;
			m_tempFilterBuffer.Create();
		}

		private void CleanTempFilterRT()
		{
			if (m_tempFilterBuffer != null)
			{
				RenderTexture.active = null;
				m_tempFilterBuffer.Release();
				UnityEngine.Object.DestroyImmediate(m_tempFilterBuffer);
				m_tempFilterBuffer = null;
			}
		}

		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (silentError)
			{
				return;
			}
			if (!AmplifyUtils.IsInitialized)
			{
				AmplifyUtils.InitializeIds();
			}
			if (m_highPrecision)
			{
				AmplifyUtils.EnsureKeywordEnabled(m_bloomMaterial, AmplifyUtils.HighPrecisionKeyword, true);
				AmplifyUtils.EnsureKeywordEnabled(m_finalCompositionMaterial, AmplifyUtils.HighPrecisionKeyword, true);
				AmplifyUtils.CurrentRTFormat = RenderTextureFormat.DefaultHDR;
			}
			else
			{
				AmplifyUtils.EnsureKeywordEnabled(m_bloomMaterial, AmplifyUtils.HighPrecisionKeyword, false);
				AmplifyUtils.EnsureKeywordEnabled(m_finalCompositionMaterial, AmplifyUtils.HighPrecisionKeyword, false);
				AmplifyUtils.CurrentRTFormat = RenderTextureFormat.Default;
			}
			float num = Mathf.Acos(Vector3.Dot(m_cameraTransform.right, Vector3.right));
			if (Vector3.Cross(m_cameraTransform.right, Vector3.right).y > 0f)
			{
				num = 0f - num;
			}
			RenderTexture renderTexture = null;
			RenderTexture renderTexture2 = null;
			if (!m_highPrecision)
			{
				m_bloomRange.y = 1f / m_bloomRange.x;
				m_bloomMaterial.SetVector(AmplifyUtils.BloomRangeId, m_bloomRange);
				m_finalCompositionMaterial.SetVector(AmplifyUtils.BloomRangeId, m_bloomRange);
			}
			m_bloomParams.y = m_overallThreshold;
			m_bloomMaterial.SetVector(AmplifyUtils.BloomParamsId, m_bloomParams);
			m_finalCompositionMaterial.SetVector(AmplifyUtils.BloomParamsId, m_bloomParams);
			int num2 = 1;
			switch (m_mainThresholdSize)
			{
			case MainThresholdSizeEnum.Half:
				num2 = 2;
				break;
			case MainThresholdSizeEnum.Quarter:
				num2 = 4;
				break;
			}
			RenderTexture tempRenderTarget = AmplifyUtils.GetTempRenderTarget(src.width / num2, src.height / num2);
			if (m_maskTexture != null)
			{
				m_bloomMaterial.SetTexture(AmplifyUtils.MaskTextureId, m_maskTexture);
				Graphics.Blit(src, tempRenderTarget, m_bloomMaterial, 1);
			}
			else
			{
				Graphics.Blit(src, tempRenderTarget, m_bloomMaterial, 0);
			}
			if (m_debugToScreen == DebugToScreenEnum.MainThreshold)
			{
				Graphics.Blit(tempRenderTarget, dest, m_bloomMaterial, 33);
				AmplifyUtils.ReleaseAllRT();
				return;
			}
			bool flag = true;
			RenderTexture renderTexture3 = tempRenderTarget;
			if (m_bloomDownsampleCount > 0)
			{
				flag = false;
				int num3 = tempRenderTarget.width;
				int num4 = tempRenderTarget.height;
				for (int i = 0; i < m_bloomDownsampleCount; i++)
				{
					m_tempDownsamplesSizes[i].x = num3;
					m_tempDownsamplesSizes[i].y = num4;
					num3 = num3 + 1 >> 1;
					num4 = num4 + 1 >> 1;
					m_tempAuxDownsampleRTs[i] = AmplifyUtils.GetTempRenderTarget(num3, num4);
					if (i == 0)
					{
						if (!m_temporalFilteringActive || m_gaussianSteps[i] != 0)
						{
							if (m_upscaleQuality == UpscaleQualityEnum.Realistic)
							{
								Graphics.Blit(renderTexture3, m_tempAuxDownsampleRTs[i], m_bloomMaterial, 10);
							}
							else
							{
								Graphics.Blit(renderTexture3, m_tempAuxDownsampleRTs[i], m_bloomMaterial, 11);
							}
						}
						else
						{
							if (m_tempFilterBuffer != null && m_temporalFilteringActive)
							{
								float value = m_temporalFilteringCurve.Evaluate(m_temporalFilteringValue);
								m_bloomMaterial.SetFloat(AmplifyUtils.TempFilterValueId, value);
								m_bloomMaterial.SetTexture(AmplifyUtils.AnamorphicRTS[0], m_tempFilterBuffer);
								if (m_upscaleQuality == UpscaleQualityEnum.Realistic)
								{
									Graphics.Blit(renderTexture3, m_tempAuxDownsampleRTs[i], m_bloomMaterial, 12);
								}
								else
								{
									Graphics.Blit(renderTexture3, m_tempAuxDownsampleRTs[i], m_bloomMaterial, 13);
								}
							}
							else if (m_upscaleQuality == UpscaleQualityEnum.Realistic)
							{
								Graphics.Blit(renderTexture3, m_tempAuxDownsampleRTs[i], m_bloomMaterial, 10);
							}
							else
							{
								Graphics.Blit(renderTexture3, m_tempAuxDownsampleRTs[i], m_bloomMaterial, 11);
							}
							bool flag2 = false;
							if (m_tempFilterBuffer != null)
							{
								if (m_tempFilterBuffer.format != m_tempAuxDownsampleRTs[i].format || m_tempFilterBuffer.width != m_tempAuxDownsampleRTs[i].width || m_tempFilterBuffer.height != m_tempAuxDownsampleRTs[i].height)
								{
									CleanTempFilterRT();
									flag2 = true;
								}
							}
							else
							{
								flag2 = true;
							}
							if (flag2)
							{
								CreateTempFilterRT(m_tempAuxDownsampleRTs[i]);
							}
							m_tempFilterBuffer.DiscardContents();
							Graphics.Blit(m_tempAuxDownsampleRTs[i], m_tempFilterBuffer);
							if (m_debugToScreen == DebugToScreenEnum.TemporalFilter)
							{
								Graphics.Blit(m_tempAuxDownsampleRTs[i], dest);
								AmplifyUtils.ReleaseAllRT();
								return;
							}
						}
					}
					else
					{
						Graphics.Blit(m_tempAuxDownsampleRTs[i - 1], m_tempAuxDownsampleRTs[i], m_bloomMaterial, 9);
					}
					if (m_gaussianSteps[i] > 0)
					{
						ApplyGaussianBlur(m_tempAuxDownsampleRTs[i], m_gaussianSteps[i], m_gaussianRadius[i], i == 0);
						if (m_temporalFilteringActive && m_debugToScreen == DebugToScreenEnum.TemporalFilter)
						{
							Graphics.Blit(m_tempAuxDownsampleRTs[i], dest);
							AmplifyUtils.ReleaseAllRT();
							return;
						}
					}
				}
				renderTexture3 = m_tempAuxDownsampleRTs[m_featuresSourceId];
				AmplifyUtils.ReleaseTempRenderTarget(tempRenderTarget);
			}
			if (m_bokehFilter.ApplyBokeh && m_bokehFilter.ApplyOnBloomSource)
			{
				m_bokehFilter.ApplyBokehFilter(renderTexture3, m_bloomMaterial);
				if (m_debugToScreen == DebugToScreenEnum.BokehFilter)
				{
					Graphics.Blit(renderTexture3, dest);
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}
			RenderTexture renderTexture4 = null;
			bool flag3 = false;
			if (m_separateFeaturesThreshold)
			{
				m_bloomParams.y = m_featuresThreshold;
				m_bloomMaterial.SetVector(AmplifyUtils.BloomParamsId, m_bloomParams);
				m_finalCompositionMaterial.SetVector(AmplifyUtils.BloomParamsId, m_bloomParams);
				renderTexture4 = AmplifyUtils.GetTempRenderTarget(renderTexture3.width, renderTexture3.height);
				flag3 = true;
				Graphics.Blit(renderTexture3, renderTexture4, m_bloomMaterial, 0);
				if (m_debugToScreen == DebugToScreenEnum.FeaturesThreshold)
				{
					Graphics.Blit(renderTexture4, dest);
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}
			else
			{
				renderTexture4 = renderTexture3;
			}
			if (m_bokehFilter.ApplyBokeh && !m_bokehFilter.ApplyOnBloomSource)
			{
				if (!flag3)
				{
					flag3 = true;
					renderTexture4 = AmplifyUtils.GetTempRenderTarget(renderTexture3.width, renderTexture3.height);
					Graphics.Blit(renderTexture3, renderTexture4);
				}
				m_bokehFilter.ApplyBokehFilter(renderTexture4, m_bloomMaterial);
				if (m_debugToScreen == DebugToScreenEnum.BokehFilter)
				{
					Graphics.Blit(renderTexture4, dest);
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}
			if (m_lensFlare.ApplyLensFlare && m_debugToScreen != DebugToScreenEnum.Bloom)
			{
				renderTexture = m_lensFlare.ApplyFlare(m_bloomMaterial, renderTexture4);
				ApplyGaussianBlur(renderTexture, m_lensFlare.LensFlareGaussianBlurAmount);
				if (m_debugToScreen == DebugToScreenEnum.LensFlare)
				{
					Graphics.Blit(renderTexture, dest);
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}
			if (m_anamorphicGlare.ApplyLensGlare && m_debugToScreen != DebugToScreenEnum.Bloom)
			{
				renderTexture2 = AmplifyUtils.GetTempRenderTarget(renderTexture3.width, renderTexture3.height);
				m_anamorphicGlare.OnRenderImage(m_bloomMaterial, renderTexture4, renderTexture2, num);
				if (m_debugToScreen == DebugToScreenEnum.LensGlare)
				{
					Graphics.Blit(renderTexture2, dest);
					AmplifyUtils.ReleaseAllRT();
					return;
				}
			}
			if (flag3)
			{
				AmplifyUtils.ReleaseTempRenderTarget(renderTexture4);
			}
			if (flag)
			{
				ApplyGaussianBlur(renderTexture3, m_gaussianSteps[0], m_gaussianRadius[0]);
			}
			if (m_bloomDownsampleCount > 0)
			{
				if (m_bloomDownsampleCount == 1)
				{
					if (m_upscaleQuality == UpscaleQualityEnum.Realistic)
					{
						ApplyUpscale();
						m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[0], m_tempUpscaleRTs[0]);
					}
					else
					{
						m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[0], m_tempAuxDownsampleRTs[0]);
					}
					m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[0], m_upscaleWeights[0]);
				}
				else if (m_upscaleQuality == UpscaleQualityEnum.Realistic)
				{
					ApplyUpscale();
					for (int j = 0; j < m_bloomDownsampleCount; j++)
					{
						int num5 = m_bloomDownsampleCount - j - 1;
						m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[num5], m_tempUpscaleRTs[j]);
						m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[num5], m_upscaleWeights[j]);
					}
				}
				else
				{
					for (int k = 0; k < m_bloomDownsampleCount; k++)
					{
						int num6 = m_bloomDownsampleCount - 1 - k;
						m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[num6], m_tempAuxDownsampleRTs[num6]);
						m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[num6], m_upscaleWeights[k]);
					}
				}
			}
			else
			{
				m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[0], renderTexture3);
				m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[0], 1f);
			}
			if (m_debugToScreen == DebugToScreenEnum.Bloom)
			{
				m_finalCompositionMaterial.SetFloat(AmplifyUtils.SourceContributionId, 0f);
				FinalComposition(0f, 1f, src, dest, 0);
				return;
			}
			if (m_bloomDownsampleCount > 1)
			{
				for (int l = 0; l < m_bloomDownsampleCount; l++)
				{
					m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensDirtWeightsStr[m_bloomDownsampleCount - l - 1], m_lensDirtWeights[l]);
					m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensStarburstWeightsStr[m_bloomDownsampleCount - l - 1], m_lensStarburstWeights[l]);
				}
			}
			else
			{
				m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensDirtWeightsStr[0], m_lensDirtWeights[0]);
				m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensStarburstWeightsStr[0], m_lensStarburstWeights[0]);
			}
			if (m_lensFlare.ApplyLensFlare)
			{
				m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensFlareRTId, renderTexture);
			}
			if (m_anamorphicGlare.ApplyLensGlare)
			{
				m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensGlareRTId, renderTexture2);
			}
			if (m_applyLensDirt)
			{
				m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensDirtRTId, m_lensDirtTexture);
				m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensDirtStrengthId, m_lensDirtStrength * 1f);
				if (m_debugToScreen == DebugToScreenEnum.LensDirt)
				{
					FinalComposition(0f, 0f, src, dest, 2);
					return;
				}
			}
			if (m_applyLensStardurst)
			{
				m_starburstMat[0, 0] = Mathf.Cos(num);
				m_starburstMat[0, 1] = 0f - Mathf.Sin(num);
				m_starburstMat[1, 0] = Mathf.Sin(num);
				m_starburstMat[1, 1] = Mathf.Cos(num);
				m_finalCompositionMaterial.SetMatrix(AmplifyUtils.LensFlareStarMatrixId, m_starburstMat);
				m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensFlareStarburstStrengthId, m_lensStarburstStrength * 1f);
				m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensStarburstRTId, m_lensStardurstTex);
				if (m_debugToScreen == DebugToScreenEnum.LensStarburst)
				{
					FinalComposition(0f, 0f, src, dest, 1);
					return;
				}
			}
			if (m_targetTexture != null)
			{
				m_targetTexture.DiscardContents();
				FinalComposition(0f, 1f, src, m_targetTexture, -1);
				Graphics.Blit(src, dest);
			}
			else
			{
				FinalComposition(1f, 1f, src, dest, -1);
			}
		}

		private void FinalComposition(float srcContribution, float upscaleContribution, RenderTexture src, RenderTexture dest, int forcePassId)
		{
			m_finalCompositionMaterial.SetFloat(AmplifyUtils.SourceContributionId, srcContribution);
			m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleContributionId, upscaleContribution);
			int num = 0;
			if (forcePassId > -1)
			{
				num = forcePassId;
			}
			else
			{
				if (LensFlareInstance.ApplyLensFlare)
				{
					num |= 8;
				}
				if (LensGlareInstance.ApplyLensGlare)
				{
					num |= 4;
				}
				if (m_applyLensDirt)
				{
					num |= 2;
				}
				if (m_applyLensStardurst)
				{
					num |= 1;
				}
			}
			num += (m_bloomDownsampleCount - 1) * 16;
			Graphics.Blit(src, dest, m_finalCompositionMaterial, num);
			AmplifyUtils.ReleaseAllRT();
		}

		private void ApplyUpscale()
		{
			int num = m_bloomDownsampleCount - 1;
			int num2 = 0;
			for (int num3 = num; num3 > -1; num3--)
			{
				m_tempUpscaleRTs[num2] = AmplifyUtils.GetTempRenderTarget((int)m_tempDownsamplesSizes[num3].x, (int)m_tempDownsamplesSizes[num3].y);
				if (num3 == num)
				{
					Graphics.Blit(m_tempAuxDownsampleRTs[num], m_tempUpscaleRTs[num2], m_bloomMaterial, 17);
				}
				else
				{
					m_bloomMaterial.SetTexture(AmplifyUtils.AnamorphicRTS[0], m_tempUpscaleRTs[num2 - 1]);
					Graphics.Blit(m_tempAuxDownsampleRTs[num3], m_tempUpscaleRTs[num2], m_bloomMaterial, 18);
				}
				num2++;
			}
		}
	}
}
