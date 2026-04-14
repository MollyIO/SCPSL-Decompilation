using UnityEngine;

public class shooter : MonoBehaviour
{
	public int mtpl = 5;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			ScreenCapture.CaptureScreenshot("Taken" + Random.Range(0, 1000) + ".png", mtpl);
		}
	}
}
