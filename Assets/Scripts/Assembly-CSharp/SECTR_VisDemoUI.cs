using UnityEngine;

[AddComponentMenu("SECTR/Demos/SECTR Vis Demo UI")]
public class SECTR_VisDemoUI : SECTR_DemoUI
{
	private string originalDemoMessage;

	[Multiline]
	public string Unity4PerfMessage;

	private void Start()
	{
		if ((bool)PipController && PipController.GetComponent<SECTR_CullingCamera>() == null && (bool)GetComponent<SECTR_CullingCamera>() && (bool)GetComponent<Camera>())
		{
			SECTR_CullingCamera sECTR_CullingCamera = PipController.gameObject.AddComponent<SECTR_CullingCamera>();
			sECTR_CullingCamera.cullingProxy = GetComponent<Camera>();
		}
	}

	protected override void OnEnable()
	{
		originalDemoMessage = DemoMessage;
		watermarkLocation = WatermarkLocation.UpperCenter;
		AddButton(KeyCode.C, "Enable Culling", "Disable Culling", ToggleCulling);
		base.OnEnable();
	}

	protected override void OnGUI()
	{
		if (Application.isEditor && Application.isPlaying && !string.IsNullOrEmpty(Unity4PerfMessage))
		{
			DemoMessage = originalDemoMessage;
			DemoMessage += "\n\n";
			DemoMessage += Unity4PerfMessage;
		}
		base.OnGUI();
		if (passedIntro && !CaptureMode)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			SECTR_CullingCamera component = GetComponent<SECTR_CullingCamera>();
			if ((bool)component)
			{
				num += component.RenderersCulled;
				num2 += component.LightsCulled;
				num3 += component.TerrainsCulled;
			}
			string text = "Culling Stats\n";
			string text2 = text;
			text = text2 + "Renderers: " + num + "\n";
			text2 = text;
			text = text2 + "Lights: " + num2 + "\n";
			text = text + "Terrains: " + num3;
			GUIContent content = new GUIContent(text);
			float num4 = (float)Screen.width * 0.33f;
			float height = demoButtonStyle.CalcHeight(content, num4);
			Rect position = new Rect((float)Screen.width - num4, 0f, num4, height);
			GUI.Box(position, content, demoButtonStyle);
		}
	}

	protected void ToggleCulling(bool active)
	{
		SECTR_CullingCamera component = GetComponent<SECTR_CullingCamera>();
		if ((bool)component)
		{
			component.enabled = !active;
			component.ResetStats();
		}
	}
}
