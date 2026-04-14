using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
	[Serializable]
	public class Track
	{
		public string name;

		public AudioSource source;

		public bool playing;

		public bool restartOnPlay;

		public float enterFadeDuration;

		public float exitFadeDuration;

		public float maxVolume;

		public void Update(int speed = 1)
		{
			if (source != null)
			{
				if (restartOnPlay && source.volume == 0f && playing)
				{
					source.Stop();
					source.Play();
				}
				source.volume += 0.02f * (float)speed * ((!playing) ? (-1f / exitFadeDuration) : (1f / enterFadeDuration)) * maxVolume;
				source.volume = Mathf.Clamp(source.volume, 0f, maxVolume);
			}
		}
	}

	public Track[] overlayTracks;

	public Track[] mainTracks;

	public int overlayIndex;

	public int mainIndex;

	public bool overlayPlaying;

	public GameObject player;

	public LayerMask mask;

	private float nooneSawTime;

	private bool seeSomeone;

	public static SoundtrackManager singleton;

	private void FixedUpdate()
	{
		bool flag = false;
		if (AlphaWarheadController.host != null)
		{
			flag = AlphaWarheadController.host.inProgress;
		}
		if (nooneSawTime > 140f && !overlayPlaying)
		{
			for (int i = 0; i < mainTracks.Length; i++)
			{
				mainTracks[i].playing = i == 3 && !flag;
				mainTracks[i].Update();
			}
			for (int j = 0; j < overlayTracks.Length; j++)
			{
				overlayTracks[j].playing = overlayPlaying && j == overlayIndex && !flag;
				overlayTracks[j].Update();
			}
		}
		else
		{
			for (int k = 0; k < overlayTracks.Length; k++)
			{
				overlayTracks[k].playing = overlayPlaying && k == overlayIndex && !flag;
				overlayTracks[k].Update();
			}
			for (int l = 0; l < mainTracks.Length; l++)
			{
				mainTracks[l].playing = !overlayPlaying && l == mainIndex && !flag;
				mainTracks[l].Update();
			}
		}
	}

	private void Update()
	{
		if (!(player == null))
		{
			if (seeSomeone)
			{
				nooneSawTime = 0f;
			}
			else
			{
				nooneSawTime += Time.deltaTime;
			}
		}
	}

	private void Start()
	{
		Timing.RunCoroutine(_Start(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Start()
	{
		while (player == null)
		{
			player = PlayerManager.localPlayer;
			yield return 0f;
		}
		Transform _camera = player.GetComponent<Scp049PlayerScript>().plyCam.transform;
		CharacterClassManager ccm = player.GetComponent<CharacterClassManager>();
		while (this != null)
		{
			bool foundSomeone = false;
			Team team = ccm.klasy[Mathf.Clamp(ccm.curClass, 0, ccm.klasy.Length - 1)].team;
			if (team != Team.SCP && team != Team.RIP)
			{
				GameObject[] players = PlayerManager.singleton.players;
				foreach (GameObject item in players)
				{
					try
					{
						RaycastHit hitInfo;
						if (Physics.Raycast(new Ray(player.transform.position, (item.transform.position - _camera.position).normalized), out hitInfo, 20f, mask))
						{
							Transform root = hitInfo.collider.transform.root;
							if (root.tag == "Player")
							{
								int curClass = root.GetComponent<CharacterClassManager>().curClass;
								if (ccm.klasy[Mathf.Clamp(curClass, 0, ccm.klasy.Length - 1)].team != Team.SCP)
								{
									foundSomeone = true;
								}
							}
						}
					}
					catch
					{
					}
					yield return 0f;
				}
			}
			else
			{
				foundSomeone = true;
				StopOverlay(0);
			}
			seeSomeone = foundSomeone;
			yield return 0f;
		}
	}

	public void PlayOverlay(int id)
	{
		if (id != overlayIndex || !overlayPlaying)
		{
			overlayPlaying = true;
			overlayIndex = id;
			if (overlayTracks[id].restartOnPlay)
			{
				overlayTracks[id].source.Stop();
				overlayTracks[id].source.Play();
			}
		}
	}

	public void StopOverlay(int id)
	{
		if (overlayIndex == id)
		{
			overlayPlaying = false;
		}
	}

	private void Awake()
	{
		singleton = this;
	}
}
