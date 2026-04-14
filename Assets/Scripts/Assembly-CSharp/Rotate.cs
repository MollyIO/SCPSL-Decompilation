using UnityEngine;

public class Rotate : MonoBehaviour
{
	private Vector3 speed;

	private void Update()
	{
		base.transform.Rotate(speed * Time.deltaTime);
	}
}
