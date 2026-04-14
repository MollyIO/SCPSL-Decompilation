using UnityEngine;

[AddComponentMenu("UBER/Apply Light for Deferred")]
[ExecuteInEditMode]
public class UBER_applyLightForDeferred : MonoBehaviour
{
	public Light lightForSelfShadowing;

	private Renderer _renderer;

	private void Start()
	{
		Reset();
	}

	private void Reset()
	{
		if ((bool)GetComponent<Light>() && lightForSelfShadowing == null)
		{
			lightForSelfShadowing = GetComponent<Light>();
		}
		if ((bool)GetComponent<Renderer>() && _renderer == null)
		{
			_renderer = GetComponent<Renderer>();
		}
	}

	private void Update()
	{
		if (!lightForSelfShadowing)
		{
			return;
		}
		if ((bool)_renderer)
		{
			if (lightForSelfShadowing.type == LightType.Directional)
			{
				for (int i = 0; i < _renderer.sharedMaterials.Length; i++)
				{
					_renderer.sharedMaterials[i].SetVector("_WorldSpaceLightPosCustom", -lightForSelfShadowing.transform.forward);
				}
			}
			else
			{
				for (int j = 0; j < _renderer.materials.Length; j++)
				{
					_renderer.sharedMaterials[j].SetVector("_WorldSpaceLightPosCustom", new Vector4(lightForSelfShadowing.transform.position.x, lightForSelfShadowing.transform.position.y, lightForSelfShadowing.transform.position.z, 1f));
				}
			}
		}
		else if (lightForSelfShadowing.type == LightType.Directional)
		{
			Shader.SetGlobalVector("_WorldSpaceLightPosCustom", -lightForSelfShadowing.transform.forward);
		}
		else
		{
			Shader.SetGlobalVector("_WorldSpaceLightPosCustom", new Vector4(lightForSelfShadowing.transform.position.x, lightForSelfShadowing.transform.position.y, lightForSelfShadowing.transform.position.z, 1f));
		}
	}
}
