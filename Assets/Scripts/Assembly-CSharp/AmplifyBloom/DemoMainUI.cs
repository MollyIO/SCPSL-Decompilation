using UnityEngine;
using UnityEngine.UI;

namespace AmplifyBloom
{
	public class DemoMainUI : MonoBehaviour
	{
		private const string VERTICAL_GAMEPAD = "Vertical";

		private const string HORIZONTAL_GAMEPAD = "Horizontal";

		private const string SUBMIT_BUTTON = "Submit";

		public Toggle BloomToggle;

		public Toggle HighPrecision;

		public Toggle UpscaleType;

		public Toggle TemporalFilter;

		public Toggle BokehToggle;

		public Toggle LensFlareToggle;

		public Toggle LensGlareToggle;

		public Toggle LensDirtToggle;

		public Toggle LensStarburstToggle;

		public Slider ThresholdSlider;

		public Slider DownscaleAmountSlider;

		public Slider IntensitySlider;

		public Slider ThresholdSizeSlider;

		private AmplifyBloomEffect _amplifyBloomEffect;

		private Camera _camera;

		private DemoUIElement[] m_uiElements;

		private bool m_gamePadMode;

		private int m_currentOption;

		private int m_lastOption;

		private int m_lastAxisValue;

		private void Awake()
		{
			_camera = Camera.main;
			_amplifyBloomEffect = _camera.GetComponent<AmplifyBloomEffect>();
			BloomToggle.isOn = _amplifyBloomEffect.enabled;
			HighPrecision.isOn = _amplifyBloomEffect.HighPrecision;
			UpscaleType.isOn = _amplifyBloomEffect.UpscaleQuality == UpscaleQualityEnum.Realistic;
			TemporalFilter.isOn = _amplifyBloomEffect.TemporalFilteringActive;
			BokehToggle.isOn = _amplifyBloomEffect.BokehFilterInstance.ApplyBokeh;
			LensFlareToggle.isOn = _amplifyBloomEffect.LensFlareInstance.ApplyLensFlare;
			LensGlareToggle.isOn = _amplifyBloomEffect.LensGlareInstance.ApplyLensGlare;
			LensDirtToggle.isOn = _amplifyBloomEffect.ApplyLensDirt;
			LensStarburstToggle.isOn = _amplifyBloomEffect.ApplyLensStardurst;
			BloomToggle.onValueChanged.AddListener(OnBloomToggle);
			HighPrecision.onValueChanged.AddListener(OnHighPrecisionToggle);
			UpscaleType.onValueChanged.AddListener(OnUpscaleTypeToogle);
			TemporalFilter.onValueChanged.AddListener(OnTemporalFilterToggle);
			BokehToggle.onValueChanged.AddListener(OnBokehFilterToggle);
			LensFlareToggle.onValueChanged.AddListener(OnLensFlareToggle);
			LensGlareToggle.onValueChanged.AddListener(OnLensGlareToggle);
			LensDirtToggle.onValueChanged.AddListener(OnLensDirtToggle);
			LensStarburstToggle.onValueChanged.AddListener(OnLensStarburstToggle);
			ThresholdSlider.value = _amplifyBloomEffect.OverallThreshold;
			ThresholdSlider.onValueChanged.AddListener(OnThresholdSlider);
			DownscaleAmountSlider.value = _amplifyBloomEffect.BloomDownsampleCount;
			DownscaleAmountSlider.onValueChanged.AddListener(OnDownscaleAmount);
			IntensitySlider.value = _amplifyBloomEffect.OverallIntensity;
			IntensitySlider.onValueChanged.AddListener(OnIntensitySlider);
			ThresholdSizeSlider.value = (float)_amplifyBloomEffect.MainThresholdSize;
			ThresholdSizeSlider.onValueChanged.AddListener(OnThresholdSize);
			if (Input.GetJoystickNames().Length > 0)
			{
				m_gamePadMode = true;
				m_uiElements = new DemoUIElement[13];
				m_uiElements[0] = BloomToggle.GetComponent<DemoUIElement>();
				m_uiElements[1] = HighPrecision.GetComponent<DemoUIElement>();
				m_uiElements[2] = UpscaleType.GetComponent<DemoUIElement>();
				m_uiElements[3] = TemporalFilter.GetComponent<DemoUIElement>();
				m_uiElements[4] = BokehToggle.GetComponent<DemoUIElement>();
				m_uiElements[5] = LensFlareToggle.GetComponent<DemoUIElement>();
				m_uiElements[6] = LensGlareToggle.GetComponent<DemoUIElement>();
				m_uiElements[7] = LensDirtToggle.GetComponent<DemoUIElement>();
				m_uiElements[8] = LensStarburstToggle.GetComponent<DemoUIElement>();
				m_uiElements[9] = ThresholdSlider.GetComponent<DemoUIElement>();
				m_uiElements[10] = DownscaleAmountSlider.GetComponent<DemoUIElement>();
				m_uiElements[11] = IntensitySlider.GetComponent<DemoUIElement>();
				m_uiElements[12] = ThresholdSizeSlider.GetComponent<DemoUIElement>();
				for (int i = 0; i < m_uiElements.Length; i++)
				{
					m_uiElements[i].Init();
				}
				m_uiElements[m_currentOption].Select = true;
			}
		}

		public void OnThresholdSize(float selection)
		{
			_amplifyBloomEffect.MainThresholdSize = (MainThresholdSizeEnum)selection;
		}

		public void OnThresholdSlider(float value)
		{
			_amplifyBloomEffect.OverallThreshold = value;
		}

		public void OnDownscaleAmount(float value)
		{
			_amplifyBloomEffect.BloomDownsampleCount = (int)value;
		}

		public void OnIntensitySlider(float value)
		{
			_amplifyBloomEffect.OverallIntensity = value;
		}

		public void OnBloomToggle(bool value)
		{
			_amplifyBloomEffect.enabled = BloomToggle.isOn;
		}

		public void OnHighPrecisionToggle(bool value)
		{
			_amplifyBloomEffect.HighPrecision = value;
		}

		public void OnUpscaleTypeToogle(bool value)
		{
			_amplifyBloomEffect.UpscaleQuality = ((!value) ? UpscaleQualityEnum.Natural : UpscaleQualityEnum.Realistic);
		}

		public void OnTemporalFilterToggle(bool value)
		{
			_amplifyBloomEffect.TemporalFilteringActive = value;
		}

		public void OnBokehFilterToggle(bool value)
		{
			_amplifyBloomEffect.BokehFilterInstance.ApplyBokeh = BokehToggle.isOn;
		}

		public void OnLensFlareToggle(bool value)
		{
			_amplifyBloomEffect.LensFlareInstance.ApplyLensFlare = LensFlareToggle.isOn;
		}

		public void OnLensGlareToggle(bool value)
		{
			_amplifyBloomEffect.LensGlareInstance.ApplyLensGlare = LensGlareToggle.isOn;
		}

		public void OnLensDirtToggle(bool value)
		{
			_amplifyBloomEffect.ApplyLensDirt = LensDirtToggle.isOn;
		}

		public void OnLensStarburstToggle(bool value)
		{
			_amplifyBloomEffect.ApplyLensStardurst = LensStarburstToggle.isOn;
		}

		public void OnQuitButton()
		{
			Application.Quit();
		}

		private void Update()
		{
			if (m_gamePadMode)
			{
				int num = (int)Input.GetAxis("Vertical");
				if (num != m_lastAxisValue)
				{
					m_lastAxisValue = num;
					switch (num)
					{
					case 1:
						m_currentOption = (m_currentOption + 1) % m_uiElements.Length;
						break;
					case -1:
						m_currentOption = ((m_currentOption != 0) ? (m_currentOption - 1) : (m_uiElements.Length - 1));
						break;
					}
					m_uiElements[m_lastOption].Select = false;
					m_uiElements[m_currentOption].Select = true;
					m_lastOption = m_currentOption;
				}
				if (Input.GetButtonDown("Submit"))
				{
					m_uiElements[m_currentOption].DoAction(DemoUIElementAction.Press);
				}
				float axis = Input.GetAxis("Horizontal");
				if (Mathf.Abs(axis) > 0f)
				{
					m_uiElements[m_currentOption].DoAction(DemoUIElementAction.Slide, axis * Time.deltaTime);
				}
				else
				{
					m_uiElements[m_currentOption].Idle();
				}
			}
			if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.Q))
			{
				OnQuitButton();
			}
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				_camera.orthographic = !_camera.orthographic;
			}
			bool isOn;
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				AmplifyBloomEffect amplifyBloomEffect = _amplifyBloomEffect;
				isOn = !BloomToggle.isOn;
				BloomToggle.isOn = isOn;
				amplifyBloomEffect.enabled = isOn;
			}
			Toggle bokehToggle = BokehToggle;
			isOn = BloomToggle.isOn;
			IntensitySlider.interactable = isOn;
			isOn = isOn;
			HighPrecision.interactable = isOn;
			isOn = isOn;
			DownscaleAmountSlider.interactable = isOn;
			isOn = isOn;
			ThresholdSlider.interactable = isOn;
			isOn = isOn;
			LensStarburstToggle.interactable = isOn;
			isOn = isOn;
			LensDirtToggle.interactable = isOn;
			isOn = isOn;
			LensGlareToggle.interactable = isOn;
			isOn = isOn;
			LensFlareToggle.interactable = isOn;
			bokehToggle.interactable = isOn;
			if (BloomToggle.isOn)
			{
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					AmplifyBokeh bokehFilterInstance = _amplifyBloomEffect.BokehFilterInstance;
					isOn = !BokehToggle.isOn;
					BokehToggle.isOn = isOn;
					bokehFilterInstance.ApplyBokeh = isOn;
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					AmplifyLensFlare lensFlareInstance = _amplifyBloomEffect.LensFlareInstance;
					isOn = !LensFlareToggle.isOn;
					LensFlareToggle.isOn = isOn;
					lensFlareInstance.ApplyLensFlare = isOn;
				}
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					AmplifyGlare lensGlareInstance = _amplifyBloomEffect.LensGlareInstance;
					isOn = !LensGlareToggle.isOn;
					LensGlareToggle.isOn = isOn;
					lensGlareInstance.ApplyLensGlare = isOn;
				}
				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					AmplifyBloomEffect amplifyBloomEffect2 = _amplifyBloomEffect;
					isOn = !LensDirtToggle.isOn;
					LensDirtToggle.isOn = isOn;
					amplifyBloomEffect2.ApplyLensDirt = isOn;
				}
				if (Input.GetKeyDown(KeyCode.Alpha6))
				{
					AmplifyBloomEffect amplifyBloomEffect3 = _amplifyBloomEffect;
					isOn = !LensStarburstToggle.isOn;
					LensStarburstToggle.isOn = isOn;
					amplifyBloomEffect3.ApplyLensStardurst = isOn;
				}
			}
		}
	}
}
