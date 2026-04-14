using UnityEngine;

public class MaterialBlink : MonoBehaviour
{
	public Material materal;

	public Color lowestColor = Color.white;

	public Color highestColor = Color.white;

	public float speed = 1f;

	public float colorMultiplier = 1f;

	private float time;

	private void Update()
	{
		time += Time.deltaTime * speed;
		if (time > 1f)
		{
			time -= 1f;
		}
		materal.SetColor("_EmissionColor", Color.Lerp(lowestColor, highestColor, Mathf.Abs(Mathf.Lerp(-1f, 1f, time))) * colorMultiplier);
	}

	private void OnDisable()
	{
		materal.SetColor("_EmissionColor", highestColor);
	}
}
