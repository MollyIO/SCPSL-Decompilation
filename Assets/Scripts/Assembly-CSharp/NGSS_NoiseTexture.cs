using UnityEngine;

[ExecuteInEditMode]
public class NGSS_NoiseTexture : MonoBehaviour
{
	public Texture noiseTex;

	[Range(0f, 1f)]
	public float noiseScale = 1f;

	private bool isTextureSet;

	private void Update()
	{
		Shader.SetGlobalFloat("NGSS_NOISE_TEXTURE_SCALE", noiseScale);
		if (!isTextureSet && !(noiseTex == null))
		{
			Shader.SetGlobalTexture("NGSS_NOISE_TEXTURE", noiseTex);
			isTextureSet = true;
		}
	}
}
