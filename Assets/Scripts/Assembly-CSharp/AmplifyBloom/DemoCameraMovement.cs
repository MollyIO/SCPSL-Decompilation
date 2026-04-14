using UnityEngine;
using UnityEngine.EventSystems;

namespace AmplifyBloom
{
	public class DemoCameraMovement : MonoBehaviour
	{
		private const string X_AXIS_KEYBOARD = "Mouse X";

		private const string Y_AXIS_KEYBOARD = "Mouse Y";

		private const string X_AXIS_GAMEPAD = "Horizontal";

		private const string Y_AXIS_GAMEPAD = "Vertical";

		private bool m_gamePadMode;

		public float moveSpeed = 1f;

		public float yawSpeed = 3f;

		public float pitchSpeed = 3f;

		private float _yaw;

		private float _pitch;

		private Transform _transform;

		public bool GamePadMode
		{
			get
			{
				return m_gamePadMode;
			}
		}

		private void Start()
		{
			_transform = base.transform;
			_pitch = _transform.localEulerAngles.x;
			_yaw = _transform.localEulerAngles.y;
			if (Input.GetJoystickNames().Length > 0)
			{
				m_gamePadMode = true;
			}
		}

		private void Update()
		{
			if (m_gamePadMode)
			{
				ChangeYaw(Input.GetAxisRaw("Horizontal") * yawSpeed);
				ChangePitch((0f - Input.GetAxisRaw("Vertical")) * pitchSpeed);
			}
			else if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
			{
				ChangeYaw(Input.GetAxisRaw("Mouse X") * yawSpeed);
				ChangePitch((0f - Input.GetAxisRaw("Mouse Y")) * pitchSpeed);
			}
		}

		private void MoveForwards(float delta)
		{
			_transform.position += delta * _transform.forward;
		}

		private void Strafe(float delta)
		{
			_transform.position += delta * _transform.right;
		}

		private void ChangeYaw(float delta)
		{
			_yaw += delta;
			WrapAngle(ref _yaw);
			_transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
		}

		private void ChangePitch(float delta)
		{
			_pitch += delta;
			WrapAngle(ref _pitch);
			_transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
		}

		public void WrapAngle(ref float angle)
		{
			if (angle < 0f)
			{
				angle = 360f + angle;
			}
			if (angle > 360f)
			{
				angle -= 360f;
			}
		}
	}
}
