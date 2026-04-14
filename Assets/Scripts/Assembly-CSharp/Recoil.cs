using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class Recoil : MonoBehaviour
{
	public static Recoil singleton;

	public GameObject plyCam;

	private float recoil;

	private float backSpeed = 1f;

	private float lerpSpeed = 1f;

	private Vector3 target;

	private MouseLook mlook;

	public float positionOffset;

	public void DoRecoil(RecoilProperties r, float multip)
	{
		if (mlook != null)
		{
			backSpeed = r.backSpeed;
			lerpSpeed = r.lerpSpeed;
			recoil += r.shockSize * multip;
			target = new Vector3
			{
				x = Random.Range(-70, -50),
				y = Random.Range(-20, 20),
				z = Random.Range(-20, 20)
			};
			Vector3 vector = multip * target.normalized * 13f * r.upSize;
			mlook.Recoil(0f - vector.x, vector.y);
		}
	}

	private void Start()
	{
		Timing.RunCoroutine(_Start(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Start()
	{
		if (GetComponentInParent<NetworkIdentity>().isLocalPlayer)
		{
			singleton = this;
			yield return 0f;
			mlook = GetComponentInParent<FirstPersonController>().m_MouseLook;
			while (this != null)
			{
				recoil -= backSpeed / 50f;
				recoil = Mathf.Clamp01(recoil);
				Vector3 t = Vector3.Lerp(Vector3.zero, target, recoil);
				base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, Quaternion.Euler(t), (lerpSpeed * recoil + 1f) / 10f);
				base.transform.localPosition = Vector3.up * (positionOffset + 0.8380203f);
				yield return 0f;
			}
		}
	}

	public static void StaticDoRecoil(RecoilProperties r, float multip)
	{
		singleton.DoRecoil(r, multip);
	}
}
