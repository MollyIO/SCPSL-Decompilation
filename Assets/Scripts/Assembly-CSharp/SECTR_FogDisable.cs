using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SECTR_FogDisable : MonoBehaviour
{
	private bool previousFogState;

	private void OnPreRender()
	{
		previousFogState = RenderSettings.fog;
		RenderSettings.fog = false;
	}

	private void OnPostRender()
	{
		RenderSettings.fog = previousFogState;
	}
}
