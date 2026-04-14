using UnityEngine;

public class SkyboxFollower : MonoBehaviour
{
	public Transform camera;

	public static bool iAm939;

	private void Update()
	{
		if (iAm939 || camera.position.y < 800f)
		{
			base.transform.position = Vector3.down * 12345f;
		}
		else
		{
			base.transform.position = camera.position;
		}
	}
}
