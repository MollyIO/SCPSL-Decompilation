using Dissonance;
using Dissonance.Audio.Playback;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SpeakerIcon : MonoBehaviour
{
	private Transform cam;

	private CharacterClassManager ccm;

	private RawImage img;

	private NetworkIdentity nid;

	private CanvasRenderer cr;

	private Radio r;

	private VoicePlayback vp;

	public Texture[] sprites;

	public static bool iAmHuman;

	public int id;

	private void Start()
	{
		img = GetComponentInChildren<RawImage>();
		ccm = GetComponentInParent<CharacterClassManager>();
		nid = GetComponentInParent<NetworkIdentity>();
		r = GetComponentInParent<Radio>();
		cr = GetComponent<CanvasRenderer>();
	}

	public void SetAlpha(float a)
	{
		cr.SetAlpha(Mathf.Clamp01(a));
	}

	private void Update()
	{
		if (nid.isLocalPlayer)
		{
			return;
		}
		if (vp == null)
		{
			if (r.mySource != null)
			{
				vp = r.mySource.GetComponent<VoicePlayback>();
			}
			return;
		}
		if (vp.Priority == ChannelPriority.None)
		{
			id = 0;
		}
		img.texture = sprites[id];
	}

	private void LateUpdate()
	{
		Class obj = null;
		if (ccm.curClass >= 0)
		{
			obj = ccm.klasy[ccm.curClass];
			if (nid.isLocalPlayer)
			{
				iAmHuman = obj.team != Team.SCP;
			}
		}
		if (cam == null)
		{
			cam = GameObject.Find("SpectatorCamera").transform;
		}
		else if (obj != null)
		{
			base.transform.localPosition = Vector3.up * 1.42f + Vector3.up * obj.iconHeightOffset;
			base.transform.LookAt(cam);
		}
	}
}
