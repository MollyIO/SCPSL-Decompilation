using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Escape : NetworkBehaviour
{
	private CharacterClassManager ccm;

	private Text respawnText;

	private bool escaped;

	public Vector3 worldPosition;

	public int radius = 10;

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		respawnText = GameObject.Find("Respawn Text").GetComponent<Text>();
	}

	private void Update()
	{
		if (base.isLocalPlayer && Vector3.Distance(base.transform.position, worldPosition) < (float)radius)
		{
			EscapeFromFacility();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(worldPosition, radius);
	}

	private void EscapeFromFacility()
	{
		if (!escaped)
		{
			if (ccm.klasy[ccm.curClass].team == Team.RSC)
			{
				escaped = true;
				ccm.RegisterEscape();
				Timing.RunCoroutine(_EscapeAnim(TranslationReader.Get("Facility", 29) + "\n" + TranslationReader.Get("Facility", 32)), Segment.Update);
				AchievementManager.Achieve("forscience");
			}
			if (ccm.klasy[ccm.curClass].team == Team.CDP)
			{
				escaped = true;
				ccm.RegisterEscape();
				Timing.RunCoroutine(_EscapeAnim(TranslationReader.Get("Facility", 30) + "\n" + TranslationReader.Get("Facility", 32)), Segment.Update);
				AchievementManager.Achieve("awayout");
			}
		}
	}

	private IEnumerator<float> _EscapeAnim(string txt)
	{
		int seconds = (int)Time.realtimeSinceStartup - ccm.EscapeStartTime;
		if (seconds <= 180)
		{
			AchievementManager.Achieve("escapeartist");
		}
		int minutes = 0;
		while (seconds >= 60)
		{
			seconds -= 60;
			minutes++;
		}
		CanvasRenderer cr = respawnText.GetComponent<CanvasRenderer>();
		cr.SetAlpha(0f);
		respawnText.text = txt.Replace("[escape_minutes]", minutes.ToString()).Replace("[escape_seconds]", seconds.ToString());
		while (cr.GetAlpha() < 1f)
		{
			cr.SetAlpha(cr.GetAlpha() + 0.1f);
			yield return Timing.WaitForSeconds(0.1f);
		}
		yield return Timing.WaitForSeconds(2f);
		escaped = false;
		yield return Timing.WaitForSeconds(3f);
		while (cr.GetAlpha() > 0f)
		{
			cr.SetAlpha(cr.GetAlpha() - 0.1f);
			yield return Timing.WaitForSeconds(0.1f);
		}
		cr.SetAlpha(0f);
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
