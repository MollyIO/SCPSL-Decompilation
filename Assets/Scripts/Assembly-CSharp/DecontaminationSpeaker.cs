using UnityEngine;

public class DecontaminationSpeaker : MonoBehaviour
{
	private static AudioSource source;

	private static bool isGlobal;

	private static DecontaminationSpeaker singleton;

	public Door[] doorsToOpen;

	private Transform lplayer;

	private void Awake()
	{
		singleton = this;
		isGlobal = false;
		source = GetComponent<AudioSource>();
	}

	public static void OpenDoors()
	{
		Door[] array = singleton.doorsToOpen;
		foreach (Door door in array)
		{
			if (door.curCooldown <= 0f && !door.isOpen)
			{
				door.OpenDecontamination();
			}
		}
	}

	private void Start()
	{
		lplayer = Object.FindObjectOfType<SpectatorCamera>().cam.transform;
	}

	private void Update()
	{
		if (!source.isPlaying)
		{
			isGlobal = false;
		}
		float y = lplayer.position.y;
		int num = ((isGlobal || (y > -100f && y < 100f)) ? 1 : 0);
		if (num != 0 || !(source.volume > 0.85f) || !source.isPlaying)
		{
			source.volume = Mathf.Lerp(source.volume, num, Time.deltaTime * 2f);
		}
	}

	public static void PlaySound(AudioClip clip, bool global)
	{
		isGlobal = global;
		source.PlayOneShot(clip);
		if (isGlobal)
		{
			source.volume = 1f;
		}
	}
}
