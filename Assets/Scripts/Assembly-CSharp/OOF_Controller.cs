using System.Collections.Generic;
using MEC;
using UnityEngine;

public class OOF_Controller : MonoBehaviour
{
	public static OOF_Controller singleton;

	public CameraFilterPack_AAA_Blood_Hit blood;

	public CameraFilterPack_Color_GrayScale grayScale;

	public PlayerStats playerStats;

	public CharacterClassManager ccm;

	public float deductSpeed = 0.2f;

	public AudioSource audioSource;

	public AudioClip[] clips;

	public Behaviour[] blurs;

	private int curBlur;

	private const int blurIteration = 2;

	private float minimalDmg;

	public AnimationCurve intensityOverDmg;

	public AnimationCurve grayScaleOverDmg;

	public AnimationCurve overallOverDmg;

	private void Start()
	{
		if (ccm.isLocalPlayer)
		{
			Timing.RunCoroutine(_ContinuousProcessing(), Segment.FixedUpdate);
		}
	}

	private void LateUpdate()
	{
		if (ccm.isLocalPlayer)
		{
			bool flag = CursorManager.ShouldBeBlurred();
			UserMainInterface.singleton.dimSph.enabled = flag;
			Behaviour[] array = blurs;
			foreach (Behaviour behaviour in array)
			{
				behaviour.enabled = flag;
			}
		}
	}

	private IEnumerator<float> _ContinuousProcessing()
	{
		singleton = this;
		while (this != null)
		{
			DeductBlood(deductSpeed / 50f);
			if (playerStats.GetHealthPercent() < minimalDmg)
			{
				minimalDmg = playerStats.GetHealthPercent();
				DeductBlood(1f);
			}
			else
			{
				minimalDmg = Mathf.Lerp(minimalDmg, playerStats.GetHealthPercent(), 6f);
			}
			yield return 0f;
		}
	}

	public void AddBlood(Vector3 hitDir, float fullBlood)
	{
		if (fullBlood > 0.2f && clips.Length > 0)
		{
			audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
		}
		if (hitDir.x > 0f)
		{
			blood.Hit_Right += hitDir.x * 2f;
			blood.Blood_Hit_Right += hitDir.x / 2f;
		}
		if (hitDir.x < 0f)
		{
			blood.Hit_Left += (0f - hitDir.x) * 2f;
			blood.Blood_Hit_Left += (0f - hitDir.x) / 2f;
		}
		if (hitDir.z > 0f)
		{
			blood.Hit_Up += hitDir.z * 2f;
			blood.Blood_Hit_Up += hitDir.z / 2f;
		}
		if (hitDir.z < 0f)
		{
			blood.Hit_Down += (0f - hitDir.z) * 2f;
			blood.Blood_Hit_Down += (0f - hitDir.z) / 2f;
		}
		blood.Hit_Full += fullBlood;
		blood.Blood_Hit_Full_1 += fullBlood / 3f;
		blood.Blood_Hit_Full_2 += fullBlood / 3f;
		blood.Blood_Hit_Full_3 += fullBlood / 3f;
	}

	private void DeductBlood(float speed)
	{
		float b = intensityOverDmg.Evaluate(minimalDmg);
		blood.Hit_Right = Mathf.Lerp(blood.Hit_Right, 0f, speed);
		blood.Hit_Left = Mathf.Lerp(blood.Hit_Left, 0f, speed);
		blood.Hit_Up = Mathf.Lerp(blood.Hit_Up, 0f, speed);
		blood.Hit_Down = Mathf.Lerp(blood.Hit_Down, 0f, speed);
		blood.Blood_Hit_Right = Mathf.Lerp(blood.Blood_Hit_Right, 0f, speed * 1.5f);
		blood.Blood_Hit_Left = Mathf.Lerp(blood.Blood_Hit_Left, 0f, speed * 1.5f);
		blood.Blood_Hit_Up = Mathf.Lerp(blood.Blood_Hit_Up, 0f, speed * 1.5f);
		blood.Blood_Hit_Down = Mathf.Lerp(blood.Blood_Hit_Down, 0f, speed * 1.5f);
		blood.Hit_Full = Mathf.Lerp(blood.Hit_Full, b, speed);
		blood.Blood_Hit_Full_1 = Mathf.Lerp(blood.Blood_Hit_Full_1, b, speed);
		blood.Blood_Hit_Full_2 = Mathf.Lerp(blood.Blood_Hit_Full_2, b, speed);
		blood.Blood_Hit_Full_3 = Mathf.Lerp(blood.Blood_Hit_Full_3, b, speed);
		blood.LightReflect = overallOverDmg.Evaluate(minimalDmg);
		grayScale._Fade = grayScaleOverDmg.Evaluate(minimalDmg);
	}
}
