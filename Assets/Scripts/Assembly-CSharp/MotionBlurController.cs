using UnityEngine;
using UnityEngine.PostProcessing;

public class MotionBlurController : MonoBehaviour
{
	private int f;

	private float t;

	private bool b;

	private PostProcessingProfile[] profiles;

	private void Start()
	{
		profiles = Resources.FindObjectsOfTypeAll<PostProcessingProfile>();
	}

	private void Update()
	{
		t += Time.deltaTime;
		f++;
		if (t > 1f)
		{
			t -= 1f;
			if ((b && f < 30) || (!b && f > 50))
			{
				Change();
			}
			f = 0;
		}
	}

	private void Change()
	{
		b = !b;
		if (PlayerPrefs.GetInt("gfxsets_mb", 1) == 1 && !ServerStatic.IsDedicated)
		{
			PostProcessingProfile[] array = profiles;
			foreach (PostProcessingProfile postProcessingProfile in array)
			{
				postProcessingProfile.motionBlur.enabled = false;
			}
		}
	}
}
