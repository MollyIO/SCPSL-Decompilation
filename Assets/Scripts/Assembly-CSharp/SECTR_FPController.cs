using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public abstract class SECTR_FPController : MonoBehaviour
{
	protected class TrackedTouch
	{
		public Vector2 startPos;

		public Vector2 currentPos;
	}

	private Vector2 _mouseAbsolute;

	private Vector2 _smoothMouse;

	private Vector2 _clampInDegrees = new Vector2(360f, 180f);

	private Vector2 _targetDirection;

	private bool focused = true;

	protected Dictionary<int, TrackedTouch> _touches = new Dictionary<int, TrackedTouch>();

	[SECTR_ToolTip("Whether to lock the cursor when this camera is active.")]
	public bool LockCursor = true;

	[SECTR_ToolTip("Scalar for mouse sensitivity.")]
	public Vector2 Sensitivity = new Vector2(2f, 2f);

	[SECTR_ToolTip("Scalar for mouse smoothing.")]
	public Vector2 Smoothing = new Vector2(3f, 3f);

	[SECTR_ToolTip("Adjusts the size of the virtual joystick.")]
	public float TouchScreenLookScale = 1f;

	private void Start()
	{
		_targetDirection = base.transform.localRotation.eulerAngles;
	}

	private void OnApplicationFocus(bool focused)
	{
		this.focused = focused;
	}

	protected virtual void Update()
	{
		if (focused)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Quaternion quaternion = Quaternion.Euler(_targetDirection);
			Vector2 screenJoystick = default(Vector2);
			if (Input.multiTouchEnabled && !Application.isEditor)
			{
				_UpdateTouches();
				screenJoystick = GetScreenJoystick(true);
			}
			else
			{
				screenJoystick.x = Input.GetAxisRaw("Mouse X");
				screenJoystick.y = Input.GetAxisRaw("Mouse Y");
			}
			screenJoystick = Vector2.Scale(screenJoystick, new Vector2(Sensitivity.x * Smoothing.x, Sensitivity.y * Smoothing.y));
			if (Input.multiTouchEnabled)
			{
				_smoothMouse = screenJoystick;
			}
			else
			{
				_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, screenJoystick.x, 1f / Smoothing.x);
				_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, screenJoystick.y, 1f / Smoothing.y);
			}
			_mouseAbsolute += _smoothMouse;
			if (_clampInDegrees.x < 360f)
			{
				_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, (0f - _clampInDegrees.x) * 0.5f, _clampInDegrees.x * 0.5f);
			}
			Quaternion localRotation = Quaternion.AngleAxis(0f - _mouseAbsolute.y, quaternion * Vector3.right);
			base.transform.localRotation = localRotation;
			if (_clampInDegrees.y < 360f)
			{
				_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, (0f - _clampInDegrees.y) * 0.5f, _clampInDegrees.y * 0.5f);
			}
			base.transform.localRotation *= quaternion;
			Quaternion quaternion2 = Quaternion.AngleAxis(_mouseAbsolute.x, base.transform.InverseTransformDirection(Vector3.up));
			base.transform.localRotation *= quaternion2;
		}
	}

	protected Vector2 GetScreenJoystick(bool left)
	{
		foreach (TrackedTouch value in _touches.Values)
		{
			float num = (float)Screen.width * 0.5f;
			if ((left && value.startPos.x < num) || (!left && value.startPos.x > num))
			{
				Vector2 result = value.currentPos - value.startPos;
				result.x = Mathf.Clamp(result.x / (num * 0.5f * TouchScreenLookScale), -1f, 1f);
				result.y = Mathf.Clamp(result.y / ((float)Screen.height * 0.5f * TouchScreenLookScale), -1f, 1f);
				return result;
			}
		}
		return Vector2.zero;
	}

	private void _UpdateTouches()
	{
		int touchCount = Input.touchCount;
		for (int i = 0; i < touchCount; i++)
		{
			Touch touch = Input.touches[i];
			TrackedTouch value;
			if (touch.phase == TouchPhase.Began)
			{
				Debug.Log("Touch " + touch.fingerId + "Started at : " + touch.position);
				TrackedTouch trackedTouch = new TrackedTouch();
				trackedTouch.startPos = touch.position;
				trackedTouch.currentPos = touch.position;
				_touches.Add(touch.fingerId, trackedTouch);
			}
			else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
			{
				Debug.Log("Touch " + touch.fingerId + "Ended at : " + touch.position);
				_touches.Remove(touch.fingerId);
			}
			else if (_touches.TryGetValue(touch.fingerId, out value))
			{
				value.currentPos = touch.position;
			}
		}
	}
}
