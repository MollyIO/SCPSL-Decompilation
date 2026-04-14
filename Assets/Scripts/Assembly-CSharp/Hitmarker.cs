using UnityEngine;

public class Hitmarker : MonoBehaviour
{
	public static Hitmarker singleton;

	public AnimationCurve size;

	public AnimationCurve opacity;

	private float t = 10f;

	public CanvasRenderer targetImage;

	private float multiplier;

	private void Awake()
	{
		singleton = this;
	}

	public static void Hit(float size = 1f)
	{
		singleton.Trigger(size);
	}

	private void Trigger(float size = 1f)
	{
		t = 0f;
		multiplier = size;
	}

	private void Update()
	{
		if (t < 10f)
		{
			t += Time.deltaTime;
			targetImage.SetAlpha(opacity.Evaluate(t));
			targetImage.transform.localScale = Vector3.one * size.Evaluate(t) * multiplier;
		}
	}
}
