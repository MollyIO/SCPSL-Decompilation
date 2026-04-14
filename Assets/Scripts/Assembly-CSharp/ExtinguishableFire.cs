using System.Collections;
using UnityEngine;

public class ExtinguishableFire : MonoBehaviour
{
	public ParticleSystem fireParticleSystem;

	public ParticleSystem smokeParticleSystem;

	protected bool m_isExtinguished;

	private const float m_FireStartingTime = 2f;

	private void Start()
	{
		m_isExtinguished = true;
		smokeParticleSystem.Stop();
		fireParticleSystem.Stop();
		StartCoroutine(StartingFire());
	}

	public void Extinguish()
	{
		if (!m_isExtinguished)
		{
			m_isExtinguished = true;
			StartCoroutine(Extinguishing());
		}
	}

	private IEnumerator Extinguishing()
	{
		fireParticleSystem.Stop();
		smokeParticleSystem.time = 0f;
		smokeParticleSystem.Play();
		for (float elapsedTime = 0f; elapsedTime < 2f; elapsedTime += Time.deltaTime)
		{
			float ratio = Mathf.Max(0f, 1f - elapsedTime / 2f);
			fireParticleSystem.transform.localScale = Vector3.one * ratio;
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		smokeParticleSystem.Stop();
		fireParticleSystem.transform.localScale = Vector3.one;
		yield return new WaitForSeconds(4f);
		StartCoroutine(StartingFire());
	}

	private IEnumerator StartingFire()
	{
		smokeParticleSystem.Stop();
		fireParticleSystem.time = 0f;
		fireParticleSystem.Play();
		for (float elapsedTime = 0f; elapsedTime < 2f; elapsedTime += Time.deltaTime)
		{
			float ratio = Mathf.Min(1f, elapsedTime / 2f);
			fireParticleSystem.transform.localScale = Vector3.one * ratio;
			yield return null;
		}
		fireParticleSystem.transform.localScale = Vector3.one;
		m_isExtinguished = false;
	}
}
