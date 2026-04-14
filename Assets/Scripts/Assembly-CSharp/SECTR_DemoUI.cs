using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SECTR_FPSController))]
[AddComponentMenu("SECTR/Demos/SECTR Demo UI")]
public class SECTR_DemoUI : MonoBehaviour
{
	protected enum WatermarkLocation
	{
		UpperLeft = 0,
		UpperCenter = 1,
		UpperRight = 2
	}

	protected delegate void DemoButtonPressedDelegate(bool active);

	private class DemoButton
	{
		public KeyCode key;

		public string activeHint;

		public string inactiveHint;

		public bool active;

		public bool pressed;

		public DemoButtonPressedDelegate demoButtonPressed;

		public DemoButton(KeyCode key, string activeHint, string inactiveHint, DemoButtonPressedDelegate demoButtonPressed)
		{
			this.key = key;
			this.activeHint = activeHint;
			this.inactiveHint = ((!string.IsNullOrEmpty(inactiveHint)) ? inactiveHint : activeHint);
			this.demoButtonPressed = demoButtonPressed;
		}
	}

	protected bool passedIntro;

	protected SECTR_FPSController cachedController;

	protected GUIStyle demoButtonStyle;

	protected WatermarkLocation watermarkLocation = WatermarkLocation.UpperRight;

	private List<DemoButton> demoButtons = new List<DemoButton>(4);

	[SECTR_ToolTip("Texture to display as a watermark.")]
	public Texture2D Watermark;

	[SECTR_ToolTip("Link to a controllable ghost/spectator camera.")]
	public SECTR_GhostController PipController;

	[SECTR_ToolTip("Message to display at start of demo.")]
	[Multiline]
	public string DemoMessage;

	[SECTR_ToolTip("Disables HUD for video captures.")]
	public bool CaptureMode;

	public bool PipActive
	{
		get
		{
			return (bool)PipController && PipController.enabled;
		}
	}

	protected virtual void OnEnable()
	{
		if ((bool)PipController)
		{
			PipController.enabled = false;
			AddButton(KeyCode.P, "Control Player", "Control PiP", PressedPip);
		}
		cachedController = GetComponent<SECTR_FPSController>();
		passedIntro = string.IsNullOrEmpty(DemoMessage) || CaptureMode;
		if (!passedIntro)
		{
			cachedController.enabled = false;
			if ((bool)PipController)
			{
				PipController.GetComponent<Camera>().enabled = false;
			}
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)PipController)
		{
			PipController.enabled = false;
		}
		cachedController = null;
		demoButtons.Clear();
	}

	protected virtual void OnGUI()
	{
		if (CaptureMode)
		{
			return;
		}
		float num = 25f;
		if ((bool)Watermark)
		{
			float num2 = (float)Screen.height * 0.1f;
			float num3 = (float)Watermark.width / (float)Watermark.height * num2;
			GUI.color = new Color(1f, 1f, 1f, 0.2f);
			switch (watermarkLocation)
			{
			case WatermarkLocation.UpperLeft:
				GUI.DrawTexture(new Rect(num, num, num3, num2), Watermark);
				break;
			case WatermarkLocation.UpperCenter:
				GUI.DrawTexture(new Rect((float)Screen.width * 0.5f - num3 * 0.5f, num, num3, num2), Watermark);
				break;
			case WatermarkLocation.UpperRight:
				GUI.DrawTexture(new Rect((float)Screen.width - num - num3, num, num3, num2), Watermark);
				break;
			}
			GUI.color = Color.white;
		}
		if (demoButtonStyle == null)
		{
			demoButtonStyle = new GUIStyle(GUI.skin.box);
			demoButtonStyle.alignment = TextAnchor.UpperCenter;
			demoButtonStyle.wordWrap = true;
			demoButtonStyle.fontSize = 20;
			demoButtonStyle.normal.textColor = Color.white;
		}
		if (!passedIntro)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			string demoMessage = DemoMessage;
			demoMessage = ((!Input.multiTouchEnabled || Application.isEditor) ? (demoMessage + "\n\nPress Any Key to Continue") : (demoMessage + "\n\nPress to Continue"));
			GUIContent content = new GUIContent(demoMessage);
			float num4 = (float)Screen.width * 0.75f;
			float num5 = demoButtonStyle.CalcHeight(content, num4);
			Rect position = new Rect((float)Screen.width * 0.5f - num4 * 0.5f, (float)Screen.height * 0.5f - num5 * 0.5f, num4, num5);
			GUI.Box(position, content, demoButtonStyle);
			if (Event.current.type == EventType.KeyDown)
			{
				passedIntro = true;
				cachedController.enabled = true;
				if ((bool)PipController)
				{
					PipController.GetComponent<Camera>().enabled = true;
				}
			}
		}
		else
		{
			if (demoButtons.Count <= 0)
			{
				return;
			}
			int count = demoButtons.Count;
			float b = (float)(Screen.width / count) - num * 2f;
			float num6 = Mathf.Min(150f, b);
			float num7 = (float)count * num6 + (float)(count - 1) * num;
			float num8 = (float)Screen.width * 0.5f - num7 * 0.5f;
			for (int i = 0; i < count; i++)
			{
				DemoButton demoButton = demoButtons[i];
				bool flag = Input.multiTouchEnabled && !Application.isEditor;
				string text = demoButton.key.ToString();
				switch (demoButton.key)
				{
				case KeyCode.Alpha0:
				case KeyCode.Alpha1:
				case KeyCode.Alpha2:
				case KeyCode.Alpha3:
				case KeyCode.Alpha4:
				case KeyCode.Alpha5:
				case KeyCode.Alpha6:
				case KeyCode.Alpha7:
				case KeyCode.Alpha8:
				case KeyCode.Alpha9:
					text = text.Replace("Alpha", string.Empty);
					break;
				}
				GUIContent content2 = new GUIContent(text);
				float num9 = ((!flag) ? 5f : 0f);
				float num10 = 50f;
				float num11 = ((!flag) ? demoButtonStyle.CalcHeight(content2, num10) : 0f);
				string text2 = ((!demoButton.active) ? demoButton.inactiveHint : demoButton.activeHint);
				GUIContent content3 = new GUIContent(text2);
				float num12 = demoButtonStyle.CalcHeight(content3, num6);
				Rect position2 = new Rect(num8 + (num6 + num) * (float)i, (float)Screen.height - num12 - num9 - num11 - num, num6, num12);
				if (flag && !demoButton.pressed)
				{
					demoButton.pressed = GUI.Button(position2, content3, demoButtonStyle);
				}
				else if (!flag)
				{
					GUI.Box(position2, content3, demoButtonStyle);
					Rect position3 = new Rect(num8 + (num6 + num) * (float)i + num6 * 0.5f - num10 * 0.5f, (float)Screen.height - num11 - num, num10, num11);
					GUI.Box(position3, content2, demoButtonStyle);
				}
				if (demoButton.pressed || (Event.current.type == EventType.KeyUp && Event.current.keyCode == demoButton.key))
				{
					demoButton.pressed = false;
					demoButton.active = !demoButton.active;
					DemoButtonPressedDelegate demoButtonPressed = demoButton.demoButtonPressed;
					if (demoButtonPressed != null)
					{
						demoButtonPressed(demoButton.active);
					}
				}
			}
		}
	}

	protected void AddButton(KeyCode key, string activeHint, string inactiveHint, DemoButtonPressedDelegate buttonPressedDelegate)
	{
		demoButtons.Add(new DemoButton(key, activeHint, inactiveHint, buttonPressedDelegate));
	}

	private void PressedPip(bool active)
	{
		if (PipActive)
		{
			PipController.enabled = false;
			cachedController.enabled = true;
		}
		else
		{
			PipController.enabled = true;
			cachedController.enabled = false;
		}
	}
}
