using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class CPPS_NV : CustomPostProcessingSight
{
	public Slider distanceSlider;

	public AnimationCurve sliderValueOverDistance;

	public Text infoText;

	private void Start()
	{
		wm = GetComponentInParent<WeaponManager>();
		if (!wm.isLocalPlayer)
		{
			Object.Destroy(this);
			return;
		}
		ppb = wm.weaponModelCamera.GetComponent<PostProcessingBehaviour>();
		CustomPostProcessingSight.singleton = this;
	}

	private void Update()
	{
		if (ppb.profile.name.Equals(targetProfile.name))
		{
			canvas.SetActive(true);
			CustomPostProcessingSight.raycast_bool = Physics.Raycast(new Ray(ppb.transform.position, ppb.transform.forward), out CustomPostProcessingSight.raycast_hit, 100f, wm.raycastMask);
			distanceSlider.value = sliderValueOverDistance.Evaluate((!CustomPostProcessingSight.raycast_bool) ? 100f : CustomPostProcessingSight.raycast_hit.distance);
			infoText.text = GetAmmoLeft().ToString("00");
		}
		else
		{
			canvas.SetActive(false);
		}
	}
}
