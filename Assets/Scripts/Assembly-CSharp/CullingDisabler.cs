using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class CullingDisabler : MonoBehaviour
{
	public Behaviour camera;

	public Behaviour culler;

	private bool state;

	private void Start()
	{
		NetworkBehaviour componentInParent = GetComponentInParent<NetworkBehaviour>();
		if (componentInParent != null && !componentInParent.isLocalPlayer)
		{
			Object.Destroy(culler);
			Object.Destroy(GetComponent<GlobalFog>());
			Object.Destroy(GetComponent<VignetteAndChromaticAberration>());
			Object.Destroy(GetComponent<NoiseAndGrain>());
			Object.Destroy(GetComponent<FlareLayer>());
			Object.Destroy(camera.GetComponent<PostProcessingBehaviour>());
			Object.Destroy(camera);
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (state != camera.enabled)
		{
			state = !state;
			culler.enabled = state;
		}
	}
}
