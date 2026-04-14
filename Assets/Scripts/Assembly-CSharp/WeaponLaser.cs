using UnityEngine;

public class WeaponLaser : MonoBehaviour
{
	public GameObject forwardDirection;

	public Light light;

	public AnimationCurve sizeOverDistance;

	private Quaternion localRot;

	public float speedLerp;

	public float maxAngle;

	private Vector3 rotCam;

	private Vector3 rotBar;

	private Vector3 hitPoint;

	public LayerMask raycastMask;

	private RaycastHit hit;

	private void LateUpdate()
	{
		if (forwardDirection == null)
		{
			if (light != null)
			{
				light.enabled = false;
			}
			return;
		}
		light.enabled = true;
		float num = Vector3.Angle(base.transform.forward, forwardDirection.transform.forward);
		rotCam = base.transform.rotation.eulerAngles;
		rotBar = forwardDirection.transform.rotation.eulerAngles;
		rotBar.z = 0f;
		rotCam.z = 0f;
		Quaternion quaternion = Quaternion.Euler((rotBar - rotCam) * 4f);
		localRot = ((!(num > maxAngle)) ? Quaternion.Euler(Vector3.zero) : quaternion);
		Physics.Raycast(base.transform.position, base.transform.forward, out hit, 1000f, raycastMask);
		hitPoint = hit.point;
		light.spotAngle = sizeOverDistance.Evaluate(hit.distance);
		light.transform.localPosition = Vector3.forward * hit.distance * 0.75f;
		light.transform.localRotation = Quaternion.Lerp(light.transform.localRotation, localRot, Time.deltaTime * speedLerp);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(hit.point, 0.5f);
	}
}
