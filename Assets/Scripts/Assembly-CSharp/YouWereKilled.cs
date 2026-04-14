using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class YouWereKilled : MonoBehaviour
{
	public Texture[] texturesRelatedToClass;

	[Space]
	public GameObject _root;

	public Text _info;

	public RawImage _imageClass;

	public void Play(PlayerStats.HitInfo hitInfo)
	{
		Timing.RunCoroutine(_Play(hitInfo), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Play(PlayerStats.HitInfo hitInfo)
	{
		_root.SetActive(true);
		CanvasRenderer[] renderers = _root.GetComponentsInChildren<CanvasRenderer>();
		string nick = GetNick(hitInfo);
		if (!string.IsNullOrEmpty(nick))
		{
			_imageClass.enabled = true;
			_info.text = string.Format("<size=20>{0}</size>\n{1}<size=20>\n{2}\n</size>{3}", TranslationReader.Get("Legancy_Interfaces", 15), nick, TranslationReader.Get("Legancy_Interfaces", 16), hitInfo.GetPlayerObject().GetComponent<CharacterClassManager>().klasy[GetClass(hitInfo)].fullName);
			_imageClass.texture = texturesRelatedToClass[GetClass(hitInfo)];
		}
		else
		{
			_info.text = RagdollManager.GetCause(hitInfo, false);
			_imageClass.enabled = false;
		}
		float time = 0f;
		while (time <= 1f)
		{
			CanvasRenderer[] array = renderers;
			foreach (CanvasRenderer canvasRenderer in array)
			{
				canvasRenderer.SetAlpha(time);
			}
			time += 0.03f;
			yield return 0f;
		}
		for (int j = 0; j < 300; j++)
		{
			yield return 0f;
		}
		while (time >= 0f)
		{
			time -= 0.02f;
			CanvasRenderer[] array2 = renderers;
			foreach (CanvasRenderer canvasRenderer2 in array2)
			{
				canvasRenderer2.SetAlpha(time);
			}
			yield return 0f;
		}
		CanvasRenderer[] array3 = renderers;
		foreach (CanvasRenderer canvasRenderer3 in array3)
		{
			canvasRenderer3.SetAlpha(0f);
		}
	}

	private string GetNick(PlayerStats.HitInfo hitInfo)
	{
		if (hitInfo.GetPlayerObject() == null)
		{
			return string.Empty;
		}
		return hitInfo.GetPlayerObject().GetComponent<NicknameSync>().myNick;
	}

	private int GetClass(PlayerStats.HitInfo hitInfo)
	{
		return hitInfo.GetPlayerObject().GetComponent<CharacterClassManager>().curClass;
	}
}
