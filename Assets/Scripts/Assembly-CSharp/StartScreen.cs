using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
	public GameObject popup;

	public Image black;

	public Text youare;

	public Text wmi;

	public Text wihtd;

	private CoroutineHandle handle;

	public void PlayAnimation(int classID)
	{
		Timing.KillCoroutines(handle);
		handle = Timing.RunCoroutine(_Animate(classID), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Animate(int classID)
	{
		black.gameObject.SetActive(true);
		GameObject host = GameObject.Find("Host");
		CharacterClassManager ccm = host.GetComponent<CharacterClassManager>();
		Class klasa = ccm.klasy[classID];
		youare.text = ((!TutorialManager.status) ? TranslationReader.Get("Facility", 31) : string.Empty);
		wmi.text = klasa.fullName;
		wmi.GetComponent<Outline>().effectColor = ((!(klasa.classColor.r < 0.24f) || !(klasa.classColor.g < 0.24f) || !(klasa.classColor.b < 0.24f)) ? Color.black : new Color(0.35f, 0.35f, 0.35f));
		wmi.color = klasa.classColor;
		wihtd.text = klasa.description;
		while (popup.transform.localScale.x < 1f)
		{
			popup.transform.localScale += Vector3.one * 0.02f * 2f;
			if (popup.transform.localScale.x > 1f)
			{
				popup.transform.localScale = Vector3.one;
			}
			yield return 0f;
		}
		while (black.color.a > 0f)
		{
			black.color = new Color(0f, 0f, 0f, black.color.a - 0.02f);
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(1f);
		CanvasRenderer c1 = youare.GetComponent<CanvasRenderer>();
		CanvasRenderer c2 = wmi.GetComponent<CanvasRenderer>();
		CanvasRenderer c3 = wihtd.GetComponent<CanvasRenderer>();
		HintManager.singleton.AddHint(0);
		while (c1.GetAlpha() > 0.2f)
		{
			c1.SetAlpha(c1.GetAlpha() - 0.0039999997f);
			c2.SetAlpha(c2.GetAlpha() - 0.0039999997f);
			c3.SetAlpha(c3.GetAlpha() - 0.0039999997f);
			yield return 0f;
		}
		yield return Timing.WaitForSeconds(6f);
		while (c1.GetAlpha() > 0f)
		{
			c1.SetAlpha(c1.GetAlpha() - 0.0039999997f);
			c2.SetAlpha(c2.GetAlpha() - 0.0039999997f);
			c3.SetAlpha(c3.GetAlpha() - 0.0039999997f);
			yield return 0f;
		}
		black.gameObject.SetActive(false);
	}
}
