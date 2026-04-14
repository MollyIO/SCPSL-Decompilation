using UnityEngine;

public class HoloSight : MonoBehaviour
{
	public Transform glass;

	public Transform reference;

	public Vector3 offset;

	private void LateUpdate()
	{
		glass.rotation = Quaternion.Euler(reference.rotation.eulerAngles + offset);
	}
}
