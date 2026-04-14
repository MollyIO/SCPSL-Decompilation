using System.Collections.Generic;
using MEC;
using UnityEngine;

public class WarheadLight : MonoBehaviour
{
	public Color NormalColor;

	public Color RedColor;

	private float startIntensity;

	public float IntensityMultiplier = 1f;

	private void Awake()
	{
		startIntensity = GetComponent<Light>().intensity;
	}

	private void Start()
	{
		WarheadLightManager.AddLight(this);
	}

	public void WarheadEnable()
	{
		if (GetComponent<FlickerableLight>() != null)
		{
			GetComponent<FlickerableLight>().OnWarheadEnable();
		}
		if (GetComponentInParent<FlickerableLight>() != null)
		{
			GetComponentInParent<FlickerableLight>().OnWarheadEnable();
		}
		Timing.KillCoroutines(base.gameObject);
		GetComponent<Light>().color = RedColor;
		GetComponent<Light>().intensity = startIntensity * IntensityMultiplier;
	}

	public void WarheadDisable()
	{
		Timing.KillCoroutines(base.gameObject);
		Timing.RunCoroutine(_FadeoffAnimation(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _FadeoffAnimation()
	{
		if (GetComponent<FlickerableLight>() != null)
		{
			GetComponent<FlickerableLight>().OnWarheadDisable();
		}
		if (GetComponentInParent<FlickerableLight>() != null)
		{
			GetComponentInParent<FlickerableLight>().OnWarheadDisable();
		}
		Light l = GetComponent<Light>();
		for (float i = 1f; i <= 30f; i += 1f)
		{
			l.color = Color.Lerp(RedColor, NormalColor, i / 30f);
			l.intensity = Mathf.Lerp(startIntensity * IntensityMultiplier, startIntensity, i / 30f);
			yield return 0f;
		}
	}
}
