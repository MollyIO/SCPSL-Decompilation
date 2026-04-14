using UnityEngine;

public class LightBlink : MonoBehaviour
{
	private float startIntes;

	public float noshadowIntensMultiplier = 1f;

	public float innerVariationPercent = 10f;

	private float outerVaration = 0.1f;

	private float curOut;

	private float curIn;

	private float innerVariation;

	public float FREQ = 12f;

	private Light l;

	public bool disabled;

	private int i;

	private void Start()
	{
		if (QualitySettings.shadows != ShadowQuality.Disable)
		{
			noshadowIntensMultiplier = 1f;
		}
		startIntes = GetComponent<Light>().intensity * 1.2f;
		outerVaration = startIntes * noshadowIntensMultiplier / 10f;
		innerVariation = startIntes * noshadowIntensMultiplier * (innerVariationPercent / 100f);
		l = GetComponent<Light>();
		RandomOuter();
		if (innerVariationPercent < 100f)
		{
			InvokeRepeating("RefreshLight", 0f, 1f / FREQ);
		}
	}

	private void FixedUpdate()
	{
		if (!disabled && innerVariationPercent == 100f)
		{
			i++;
			if (i > 3)
			{
				i = 0;
				l.enabled = !l.enabled;
			}
		}
		else
		{
			l.enabled = true;
		}
	}

	private void RandomOuter()
	{
		curOut = Random.Range(0f - outerVaration, outerVaration);
		Invoke("RandomOuter", Random.Range(1, 3));
	}

	private void RefreshLight()
	{
		if (!disabled)
		{
			curIn = Random.Range(startIntes * noshadowIntensMultiplier + innerVariation, startIntes * noshadowIntensMultiplier - innerVariation);
			l.intensity = curIn + curOut;
		}
		else
		{
			l.enabled = true;
			l.intensity = startIntes;
		}
	}
}
