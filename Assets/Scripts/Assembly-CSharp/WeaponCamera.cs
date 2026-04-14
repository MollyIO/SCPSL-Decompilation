using AmplifyBloom;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class WeaponCamera : MonoBehaviour
{
	private VignetteAndChromaticAberration vaca;

	private VignetteAndChromaticAberration myvaca;

	private PostProcessingBehaviour ppbeh;

	private AmplifyBloomEffect bloom;

	public AnimationCurve intensityOverScreen;

	public float _intens;

	public float _glare;

	private void Start()
	{
		bloom = GetComponent<AmplifyBloomEffect>();
		ppbeh = GetComponent<PostProcessingBehaviour>();
		myvaca = GetComponent<VignetteAndChromaticAberration>();
		vaca = GetComponentInParent<VignetteAndChromaticAberration>();
		bloom.enabled = PlayerPrefs.GetInt("gfxsets_cc", 1) == 1 && !ServerStatic.IsDedicated;
	}

	private void Update()
	{
		myvaca = vaca;
		float num = intensityOverScreen.Evaluate(Screen.height);
		bloom.OverallIntensity = _intens * num;
		bloom.LensGlareInstance.Intensity = _glare * num;
	}
}
