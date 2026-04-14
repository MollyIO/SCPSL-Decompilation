using UnityEngine;
using UnityEngine.PostProcessing;

public class CPPS_Sniper : CustomPostProcessingSight
{
	private void Start()
	{
		wm = GetComponentInParent<WeaponManager>();
		if (!wm.isLocalPlayer)
		{
			Object.Destroy(this);
		}
		else
		{
			ppb = wm.weaponModelCamera.GetComponent<PostProcessingBehaviour>();
		}
	}

	private void Update()
	{
		canvas.SetActive(ppb.profile.name.Equals(targetProfile.name));
	}
}
