using UnityEngine;

public class CameraFocuser : MonoBehaviour
{
	public Transform lookTarget;

	public float targetFovScale = 1f;

	public float minimumAngle;

	private void OnTriggerStay(Collider other)
	{
		Scp049PlayerScript componentInParent = other.GetComponentInParent<Scp049PlayerScript>();
		if (componentInParent != null && componentInParent.isLocalPlayer)
		{
			base.transform.LookAt(lookTarget);
			float value = Quaternion.Angle(componentInParent.plyCam.transform.rotation, base.transform.rotation);
			value = Mathf.Clamp(value, minimumAngle, 70f);
		}
	}
}
