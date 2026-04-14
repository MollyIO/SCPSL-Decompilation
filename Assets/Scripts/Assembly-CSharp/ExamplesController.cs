using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;

public class ExamplesController : MonoBehaviour
{
	public UBER_ExampleObjectParams[] objectsParams;

	public Camera mainCamera;

	public UBER_MouseOrbit_DynamicDistance mouseOrbitController;

	public GameObject InteractiveUI;

	[Space(10f)]
	public GameObject autorotateButtonOn;

	public GameObject autorotateButtonOff;

	public GameObject togglepostFXButtonOn;

	public GameObject togglepostFXButtonOff;

	public float autoRotationSpeed = 30f;

	public bool autoRotation = true;

	[Space(10f)]
	public GameObject skyboxSphere1;

	public Cubemap reflectionCubemap1;

	[Range(0f, 1f)]
	public float exposure1 = 1f;

	public GameObject realTimeLight1;

	public Material skyboxMaterial1;

	public GameObject skyboxSphere2;

	public Cubemap reflectionCubemap2;

	[Range(0f, 1f)]
	public float exposure2 = 1f;

	public GameObject realTimeLight2;

	public Material skyboxMaterial2;

	public GameObject skyboxSphere3;

	public Cubemap reflectionCubemap3;

	[Range(0f, 1f)]
	public float exposure3 = 1f;

	public GameObject realTimeLight3;

	public Material skyboxMaterial3;

	public Material skyboxSphereMaterialActive;

	public Material skyboxSphereMaterialInactive;

	[Space(10f)]
	public Slider materialSlider;

	public Slider exposureSlider;

	public Text titleTextArea;

	public Text descriptionTextArea;

	public Text matParamTextArea;

	[Space(10f)]
	public Button buttonSun;

	public Button buttonFrost;

	[Space(10f)]
	public float hideTimeDelay = 10f;

	private MeshRenderer currentRenderer;

	private Material currentMaterial;

	private Material originalMaterial;

	private float hideTime;

	private int currentTargetIndex;

	private GameObject skyboxSphereActive;

	public void Start()
	{
		RenderSettings.skybox = skyboxMaterial1;
		realTimeLight1.SetActive(true);
		realTimeLight2.SetActive(false);
		realTimeLight3.SetActive(false);
		RenderSettings.customReflection = reflectionCubemap1;
		RenderSettings.reflectionIntensity = exposure1;
		DynamicGI.UpdateEnvironment();
		skyboxSphereActive = skyboxSphere1;
		currentTargetIndex = 0;
		PrepareCurrentObject();
		for (int i = 1; i < objectsParams.Length; i++)
		{
			objectsParams[i].target.SetActive(false);
		}
		hideTime = Time.time + hideTimeDelay;
	}

	public void ClickedAutoRotation()
	{
		autoRotation = !autoRotation;
		autorotateButtonOn.SetActive(autoRotation);
		autorotateButtonOff.SetActive(!autoRotation);
	}

	public void ClickedArrow(bool rightFlag)
	{
		objectsParams[currentTargetIndex].target.transform.rotation = Quaternion.identity;
		objectsParams[currentTargetIndex].target.SetActive(false);
		if (currentRenderer != null && originalMaterial != null)
		{
			Material[] sharedMaterials = currentRenderer.sharedMaterials;
			sharedMaterials[objectsParams[currentTargetIndex].submeshIndex] = originalMaterial;
			currentRenderer.sharedMaterials = sharedMaterials;
			Object.Destroy(currentMaterial);
		}
		if (rightFlag)
		{
			currentTargetIndex = (currentTargetIndex + 1) % objectsParams.Length;
		}
		else
		{
			currentTargetIndex = (currentTargetIndex + objectsParams.Length - 1) % objectsParams.Length;
		}
		PrepareCurrentObject();
		objectsParams[currentTargetIndex].target.SetActive(true);
		mouseOrbitController.target = objectsParams[currentTargetIndex].target;
		mouseOrbitController.targetFocus = objectsParams[currentTargetIndex].target.transform.Find("Focus");
		mouseOrbitController.Reset();
	}

	public void Update()
	{
		skyboxSphereActive.transform.Rotate(Vector3.up, Time.deltaTime * 200f, Space.World);
		if (objectsParams[currentTargetIndex].Title == "Ice block" && Input.GetKeyDown(KeyCode.L))
		{
			GameObject gameObject = objectsParams[currentTargetIndex].target.transform.Find("Amber").gameObject;
			gameObject.SetActive(!gameObject.activeSelf);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ClickedArrow(true);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ClickedArrow(false);
		}
		if (autoRotation)
		{
			objectsParams[currentTargetIndex].target.transform.Rotate(Vector3.up, Time.deltaTime * autoRotationSpeed, Space.World);
		}
		if (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f)
		{
			hideTime = Time.time + hideTimeDelay;
			InteractiveUI.SetActive(true);
		}
		if (Time.time > hideTime)
		{
			InteractiveUI.SetActive(false);
		}
	}

	public void ButtonPressed(Button button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.x += 2f;
		vector.y -= 2f;
		component.anchoredPosition = vector;
	}

	public void ButtonReleased(Button button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.x -= 2f;
		vector.y += 2f;
		component.anchoredPosition = vector;
	}

	public void ButtonEnterScale(Button button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		component.localScale = new Vector3(1.1f, 1.1f, 1.1f);
	}

	public void ButtonLeaveScale(Button button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		component.localScale = new Vector3(1f, 1f, 1f);
	}

	public void SliderChanged(Slider slider)
	{
		mouseOrbitController.disableSteering = true;
		if (objectsParams[currentTargetIndex].materialProperty == "fallIntensity")
		{
			UBER_GlobalParams component = mainCamera.GetComponent<UBER_GlobalParams>();
			component.fallIntensity = slider.value;
		}
		else if (objectsParams[currentTargetIndex].materialProperty == "_SnowColorAndCoverage")
		{
			Color color = currentMaterial.GetColor("_SnowColorAndCoverage");
			color.a = slider.value;
			currentMaterial.SetColor("_SnowColorAndCoverage", color);
			slider.wholeNumbers = false;
		}
		else if (objectsParams[currentTargetIndex].materialProperty == "SPECIAL_Tiling")
		{
			currentMaterial.SetTextureScale("_MainTex", new Vector2(slider.value, slider.value));
			slider.wholeNumbers = true;
		}
		else
		{
			currentMaterial.SetFloat(objectsParams[currentTargetIndex].materialProperty, slider.value);
			slider.wholeNumbers = false;
		}
	}

	public void ExposureChanged(Slider slider)
	{
		TonemappingColorGrading component = mainCamera.gameObject.GetComponent<TonemappingColorGrading>();
		TonemappingColorGrading.TonemappingSettings tonemapping = component.tonemapping;
		tonemapping.exposure = slider.value;
		component.tonemapping = tonemapping;
	}

	public void ClickedSkybox1()
	{
		skyboxSphereActive.transform.rotation = Quaternion.identity;
		Renderer componentInChildren = skyboxSphereActive.GetComponentInChildren<Renderer>();
		componentInChildren.sharedMaterial = skyboxSphereMaterialInactive;
		skyboxSphereActive = skyboxSphere1;
		componentInChildren = skyboxSphereActive.GetComponentInChildren<Renderer>();
		componentInChildren.sharedMaterial = skyboxSphereMaterialActive;
		RenderSettings.customReflection = reflectionCubemap1;
		RenderSettings.reflectionIntensity = exposure1;
		RenderSettings.skybox = skyboxMaterial1;
		realTimeLight1.SetActive(true);
		realTimeLight2.SetActive(false);
		realTimeLight3.SetActive(false);
		DynamicGI.UpdateEnvironment();
	}

	public void ClickedSkybox2()
	{
		skyboxSphereActive.transform.rotation = Quaternion.identity;
		Renderer componentInChildren = skyboxSphereActive.GetComponentInChildren<Renderer>();
		componentInChildren.sharedMaterial = skyboxSphereMaterialInactive;
		skyboxSphereActive = skyboxSphere2;
		componentInChildren = skyboxSphereActive.GetComponentInChildren<Renderer>();
		componentInChildren.sharedMaterial = skyboxSphereMaterialActive;
		RenderSettings.customReflection = reflectionCubemap2;
		RenderSettings.reflectionIntensity = exposure2;
		RenderSettings.skybox = skyboxMaterial2;
		realTimeLight1.SetActive(false);
		realTimeLight2.SetActive(true);
		realTimeLight3.SetActive(false);
		DynamicGI.UpdateEnvironment();
	}

	public void ClickedSkybox3()
	{
		skyboxSphereActive.transform.rotation = Quaternion.identity;
		Renderer componentInChildren = skyboxSphereActive.GetComponentInChildren<Renderer>();
		componentInChildren.sharedMaterial = skyboxSphereMaterialInactive;
		skyboxSphereActive = skyboxSphere3;
		componentInChildren = skyboxSphereActive.GetComponentInChildren<Renderer>();
		componentInChildren.sharedMaterial = skyboxSphereMaterialActive;
		RenderSettings.customReflection = reflectionCubemap3;
		RenderSettings.reflectionIntensity = exposure3;
		RenderSettings.skybox = skyboxMaterial3;
		realTimeLight1.SetActive(false);
		realTimeLight2.SetActive(false);
		realTimeLight3.SetActive(true);
		DynamicGI.UpdateEnvironment();
	}

	public void TogglePostFX()
	{
		TonemappingColorGrading component = mainCamera.gameObject.GetComponent<TonemappingColorGrading>();
		togglepostFXButtonOn.SetActive(!component.enabled);
		togglepostFXButtonOff.SetActive(component.enabled);
		exposureSlider.interactable = !component.enabled;
		component.enabled = !component.enabled;
		Bloom component2 = mainCamera.gameObject.GetComponent<Bloom>();
		component2.enabled = component.enabled;
	}

	public void SetTemperatureSun()
	{
		ColorBlock colors = buttonSun.colors;
		colors.normalColor = new Color(1f, 1f, 1f, 0.7f);
		buttonSun.colors = colors;
		colors = buttonFrost.colors;
		colors.normalColor = new Color(1f, 1f, 1f, 0.2f);
		buttonFrost.colors = colors;
		UBER_GlobalParams component = mainCamera.GetComponent<UBER_GlobalParams>();
		component.temperature = 20f;
	}

	public void SetTemperatureFrost()
	{
		ColorBlock colors = buttonSun.colors;
		colors.normalColor = new Color(1f, 1f, 1f, 0.2f);
		buttonSun.colors = colors;
		colors = buttonFrost.colors;
		colors.normalColor = new Color(1f, 1f, 1f, 0.7f);
		buttonFrost.colors = colors;
		UBER_GlobalParams component = mainCamera.GetComponent<UBER_GlobalParams>();
		component.temperature = -20f;
	}

	private void PrepareCurrentObject()
	{
		currentRenderer = objectsParams[currentTargetIndex].renderer;
		if ((bool)currentRenderer)
		{
			originalMaterial = currentRenderer.sharedMaterials[objectsParams[currentTargetIndex].submeshIndex];
			currentMaterial = Object.Instantiate(originalMaterial);
			Material[] sharedMaterials = currentRenderer.sharedMaterials;
			sharedMaterials[objectsParams[currentTargetIndex].submeshIndex] = currentMaterial;
			currentRenderer.sharedMaterials = sharedMaterials;
		}
		bool flag = string.IsNullOrEmpty(objectsParams[currentTargetIndex].materialProperty);
		if (flag)
		{
			materialSlider.gameObject.SetActive(false);
		}
		else
		{
			materialSlider.gameObject.SetActive(true);
			materialSlider.minValue = objectsParams[currentTargetIndex].SliderRange.x;
			materialSlider.maxValue = objectsParams[currentTargetIndex].SliderRange.y;
			if (objectsParams[currentTargetIndex].materialProperty == "fallIntensity")
			{
				UBER_GlobalParams component = mainCamera.GetComponent<UBER_GlobalParams>();
				materialSlider.value = component.fallIntensity;
				component.UseParticleSystem = true;
				buttonSun.gameObject.SetActive(true);
				buttonFrost.gameObject.SetActive(true);
			}
			else
			{
				UBER_GlobalParams component2 = mainCamera.GetComponent<UBER_GlobalParams>();
				component2.UseParticleSystem = false;
				buttonSun.gameObject.SetActive(false);
				buttonFrost.gameObject.SetActive(false);
				if (originalMaterial.HasProperty(objectsParams[currentTargetIndex].materialProperty))
				{
					if (objectsParams[currentTargetIndex].materialProperty == "_SnowColorAndCoverage")
					{
						Color color = originalMaterial.GetColor("_SnowColorAndCoverage");
						materialSlider.value = color.a;
					}
					else
					{
						materialSlider.value = originalMaterial.GetFloat(objectsParams[currentTargetIndex].materialProperty);
					}
				}
				else if (objectsParams[currentTargetIndex].materialProperty == "SPECIAL_Tiling")
				{
					materialSlider.value = 1f;
				}
			}
		}
		titleTextArea.text = objectsParams[currentTargetIndex].Title;
		descriptionTextArea.text = objectsParams[currentTargetIndex].Description;
		matParamTextArea.text = objectsParams[currentTargetIndex].MatParamName;
		Vector2 anchoredPosition = titleTextArea.rectTransform.anchoredPosition;
		anchoredPosition.y = (float)((!flag) ? 110 : 50) + descriptionTextArea.preferredHeight;
		titleTextArea.rectTransform.anchoredPosition = anchoredPosition;
	}
}
