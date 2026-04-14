using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Outside : MonoBehaviour
{
	private bool isOutside = true;

	private Transform listenerPos;

	private void Update()
	{
		if (listenerPos == null)
		{
			SpectatorCamera spectatorCamera = Object.FindObjectOfType<SpectatorCamera>();
			if (spectatorCamera != null)
			{
				listenerPos = spectatorCamera.cam.transform;
			}
		}
		else if (listenerPos.position.y > 900f && !isOutside)
		{
			isOutside = true;
			SetOutside(true);
		}
		else if (listenerPos.position.y < 900f && isOutside)
		{
			isOutside = false;
			SetOutside(false);
		}
	}

	private void SetOutside(bool b)
	{
		GameObject gameObject = GameObject.Find("Directional light");
		if (gameObject != null)
		{
			gameObject.GetComponent<Light>().enabled = b;
		}
		Camera[] componentsInChildren = GetComponentsInChildren<Camera>(true);
		foreach (Camera camera in componentsInChildren)
		{
			if (camera.farClipPlane == 600f || camera.farClipPlane == 47f)
			{
				camera.farClipPlane = ((!b) ? 47 : 600);
				if (camera.clearFlags <= CameraClearFlags.Color)
				{
					camera.clearFlags = (b ? CameraClearFlags.Skybox : CameraClearFlags.Color);
				}
			}
		}
		GlobalFog[] componentsInChildren2 = GetComponentsInChildren<GlobalFog>(true);
		foreach (GlobalFog globalFog in componentsInChildren2)
		{
			globalFog.startDistance = ((!b) ? 5 : 50);
		}
	}
}
