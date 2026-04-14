using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class MouseLook
	{
		public float XSensitivity = 2f;

		public float YSensitivity = 2f;

		public bool clampVerticalRotation = true;

		public float MinimumX = -90f;

		public float MaximumX = 90f;

		public bool smooth;

		public float smoothTime = 5f;

		public static bool invert;

		public float sensitivityMultiplier = 1f;

		public bool isOpenEq;

		public bool scp106_eq;

		private Quaternion m_CharacterTargetRot;

		private Quaternion m_CameraTargetRot;

		private Transform charact;

		private Transform cam;

		public void Init(Transform character, Transform camera)
		{
			m_CharacterTargetRot = character.localRotation;
			m_CameraTargetRot = camera.localRotation;
		}

		public void SetRotation(float rot)
		{
			LookRotation(charact, cam, rot);
		}

		public void Recoil(float _x, float _y)
		{
			LookRotation(charact, cam, _y, _x);
		}

		public void LookRotation(Transform character, Transform camera, float plusY = 0f, float plusX = 0f)
		{
			charact = character;
			cam = camera;
			float num = 0f;
			float num2 = 0f;
			num = Input.GetAxis("Mouse X") * XSensitivity * Sensitivity.sens * (float)((Cursor.lockState == CursorLockMode.Locked) ? 1 : 0);
			num2 = Input.GetAxis("Mouse Y") * YSensitivity * Sensitivity.sens * (float)((Cursor.lockState == CursorLockMode.Locked) ? 1 : 0) * (float)((!invert) ? 1 : (-1));
			num2 *= sensitivityMultiplier;
			num *= sensitivityMultiplier;
			m_CharacterTargetRot *= Quaternion.Euler(0f, num + plusY, 0f);
			m_CameraTargetRot *= Quaternion.Euler(0f - num2 - plusX, 0f, 0f);
			if (clampVerticalRotation)
			{
				m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
			}
			if (smooth)
			{
				character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.fixedDeltaTime);
				camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.fixedDeltaTime);
				return;
			}
			character.localRotation = m_CharacterTargetRot;
			if (!float.IsNaN(m_CameraTargetRot.x))
			{
				camera.localRotation = m_CameraTargetRot;
			}
		}

		private Quaternion ClampRotationAroundXAxis(Quaternion q)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1f;
			float value = 114.59156f * Mathf.Atan(q.x);
			value = Mathf.Clamp(value, MinimumX, MaximumX);
			q.x = Mathf.Tan((float)Math.PI / 360f * value);
			return q;
		}
	}
}
