using System.Collections.Generic;
using MEC;
using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
	public Material[] textures;

	public Renderer targetRenderer;

	public float cooldown;

	public Light optionalLight;

	public int lightRange;

	private void Start()
	{
		Timing.RunCoroutine(_Animate(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Animate()
	{
		while (this != null)
		{
			for (int i = 0; i < textures.Length; i++)
			{
				optionalLight.enabled = i < lightRange;
				targetRenderer.material = textures[i];
				yield return Timing.WaitForSeconds(cooldown);
			}
		}
	}
}
