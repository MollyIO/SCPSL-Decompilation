using System;
using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;

public class ProjectorInitializer : MonoBehaviour
{
	[Serializable]
	public class LightStruct
	{
		public string label;

		public Color normalColor;

		public Light targetLight;

		public AnimationCurve curve;

		public void SetLight(float time)
		{
			targetLight.color = Color.Lerp(Color.black, normalColor, curve.Evaluate(time));
		}
	}

	public LightStruct[] lights;

	public TextMeshProUGUI projector_label;

	public AudioSource src;

	public AudioClip c_st;

	public AudioClip c_lp;

	public AudioClip c_sp;

	public Transform[] spools;

	private float time;

	public bool started;

	private bool prevStarted;

	private bool dir;

	private IEnumerator<float> _StartProjector()
	{
		src.Stop();
		src.PlayOneShot(c_st);
		Invoke("InitLoop", 4f);
		yield return Timing.WaitForSeconds(1f);
		dir = true;
	}

	private IEnumerator<float> _StopProjector()
	{
		src.Stop();
		src.PlayOneShot(c_sp);
		yield return Timing.WaitForSeconds(1f);
		dir = false;
	}

	private void InitLoop()
	{
		src.Stop();
		src.PlayOneShot(c_lp);
	}

	private void Update()
	{
		if (started != prevStarted)
		{
			if (started)
			{
				Timing.RunCoroutine(_StartProjector());
				prevStarted = true;
			}
			else
			{
				Timing.RunCoroutine(_StopProjector());
				prevStarted = false;
			}
		}
		time += Time.deltaTime * (float)((!dir) ? (-2) : 2);
		time = Mathf.Clamp01(time / 4f) * 4f;
		Transform[] array = spools;
		foreach (Transform transform in array)
		{
			transform.Rotate(Vector3.up * time / 4f);
		}
		LightStruct[] array2 = lights;
		foreach (LightStruct lightStruct in array2)
		{
			lightStruct.SetLight(time);
		}
	}
}
