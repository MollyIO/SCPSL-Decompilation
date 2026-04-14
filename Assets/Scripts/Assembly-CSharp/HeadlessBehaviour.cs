using UnityEngine;

public class HeadlessBehaviour : MonoBehaviour
{
	public void NullifyCamera(Camera camera)
	{
		camera.enabled = false;
	}
}
