using Kino;
using UnityEngine;

public class ExplosionCameraShake : MonoBehaviour
{
	public float force;

	public float deductSpeed;

	public AnalogGlitch glitch;

	public static ExplosionCameraShake singleton;

	private void Update()
	{
		glitch.enabled = glitch.horizontalShake > 0f;
		force -= Time.deltaTime / deductSpeed;
		force = Mathf.Clamp01(force);
		glitch.scanLineJitter = force;
		glitch.horizontalShake = force;
		glitch.colorDrift = force;
	}

	private void Awake()
	{
		singleton = this;
	}

	public void Shake(float explosionForce)
	{
		if (explosionForce > force)
		{
			force = explosionForce;
		}
	}
}
