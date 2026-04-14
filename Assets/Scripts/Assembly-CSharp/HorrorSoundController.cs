using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HorrorSoundController : NetworkBehaviour
{
	[Serializable]
	public struct DistanceSound
	{
		public float distance;

		public AudioClip clip;
	}

	private CharacterClassManager cmng;

	public LayerMask mask;

	[SerializeField]
	private Transform plyCamera;

	public AudioSource horrorSoundSource;

	[SerializeField]
	private DistanceSound[] sounds;

	private float cooldown = 20f;

	private PlayerManager pmng;

	public AudioClip blindedSoundClip;

	private float horrorSoundTriggerRange = 35f;

	private void Start()
	{
		pmng = PlayerManager.singleton;
		cmng = GetComponent<CharacterClassManager>();
	}

	public void BlindSFX()
	{
		horrorSoundSource.PlayOneShot(blindedSoundClip);
	}

	private void Update()
	{
		if (!base.isLocalPlayer || cmng.curClass < 0 || cmng.curClass == 2 || cmng.klasy[cmng.curClass].team == Team.SCP)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		GameObject[] players = pmng.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.curClass >= 0 && component.klasy[component.curClass].team == Team.SCP)
			{
				list.Add(gameObject);
			}
		}
		List<GameObject> list2 = new List<GameObject>();
		foreach (GameObject item in list)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(plyCamera.position, (item.transform.position - plyCamera.position).normalized, out hitInfo, horrorSoundTriggerRange, mask) && Mathf.Abs(Vector3.Angle(base.transform.forward, hitInfo.normal) + base.transform.rotation.y - 180f) < horrorSoundTriggerRange && hitInfo.transform.GetComponentInParent<CharacterClassManager>() != null && GetComponent<CharacterClassManager>().klasy[hitInfo.transform.GetComponentInParent<CharacterClassManager>().curClass].team == Team.SCP)
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0)
		{
			cooldown -= Time.deltaTime;
			if (cooldown < 0f && !TutorialManager.status)
			{
				SoundtrackManager.singleton.StopOverlay(0);
			}
			return;
		}
		if (cooldown < 0f)
		{
			float num = float.PositiveInfinity;
			foreach (GameObject item2 in list2)
			{
				float num2 = Vector3.Distance(base.transform.position, item2.transform.position);
				if (num2 < num)
				{
					num = num2;
				}
			}
			for (int j = 0; j < sounds.Length; j++)
			{
				if (sounds[j].distance > num)
				{
					horrorSoundSource.PlayOneShot(sounds[j].clip);
					cooldown = 20f;
					SoundtrackManager.singleton.PlayOverlay(0);
					return;
				}
			}
		}
		cooldown = 20f;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
