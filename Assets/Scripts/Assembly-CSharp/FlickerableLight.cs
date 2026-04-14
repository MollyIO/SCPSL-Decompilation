using UnityEngine;

public class FlickerableLight : MonoBehaviour
{
	private float remainingFlicker;

	private float curAnimationProgress;

	private MeshRenderer renderer;

	[HideInInspector]
	public Color colorMaterial;

	[HideInInspector]
	public Color colorLight;

	private Light lightSource;

	private bool isEnabled;

	public AnimationCurve animationEnable;

	public AnimationCurve animationDisable;

	public int materialId;

	private Material startMaterial;

	private bool warheadEnabled;

	private void Start()
	{
		lightSource = GetComponentInChildren<Light>();
		renderer = GetComponent<MeshRenderer>();
		if (renderer != null)
		{
			startMaterial = new Material(renderer.materials[materialId]);
			renderer.materials[materialId] = new Material(renderer.materials[materialId]);
			colorMaterial = renderer.materials[materialId].GetColor("_EmissionColor");
		}
		if (lightSource != null)
		{
			colorLight = lightSource.color;
		}
	}

	public void OnWarheadEnable()
	{
		warheadEnabled = true;
		renderer.materials[materialId].SetColor("_EmissionColor", Color.red);
	}

	public void OnWarheadDisable()
	{
		warheadEnabled = false;
		renderer.materials[materialId].SetColor("_EmissionColor", startMaterial.GetColor("_EmissionColor"));
	}

	private void Update()
	{
		if (warheadEnabled)
		{
			return;
		}
		if (remainingFlicker > 0f)
		{
			isEnabled = true;
			curAnimationProgress += Time.deltaTime;
			curAnimationProgress = Mathf.Clamp01(curAnimationProgress);
			float t = animationDisable.Evaluate(curAnimationProgress);
			if (renderer != null)
			{
				renderer.materials[materialId].SetColor("_EmissionColor", Color.Lerp(Color.black, colorMaterial, t));
			}
			if (lightSource != null)
			{
				lightSource.color = Color.Lerp(Color.black, colorLight, t);
			}
			remainingFlicker -= Time.deltaTime;
			if (remainingFlicker <= 0f)
			{
				curAnimationProgress = 0f;
			}
		}
		else if (isEnabled)
		{
			curAnimationProgress += Time.deltaTime;
			curAnimationProgress = Mathf.Clamp01(curAnimationProgress);
			float t2 = animationEnable.Evaluate(curAnimationProgress);
			if (renderer != null)
			{
				renderer.materials[materialId].SetColor("_EmissionColor", Color.Lerp(Color.black, colorMaterial, t2));
			}
			if (lightSource != null)
			{
				lightSource.color = Color.Lerp(Color.black, colorLight, t2);
			}
			if (curAnimationProgress == 1f)
			{
				isEnabled = false;
			}
		}
	}

	public bool IsDisabled()
	{
		return curAnimationProgress == 1f && isEnabled;
	}

	public bool EnableFlickering(float dur)
	{
		if (remainingFlicker > 0f)
		{
			return false;
		}
		remainingFlicker = dur;
		curAnimationProgress = 0f;
		return true;
	}
}
