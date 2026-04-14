using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BreakingCardSFX : MonoBehaviour
{
	public string[] texts;

	public float waitTime = 1.3f;

	private TextMeshProUGUI text;

	private Text txt;

	private void OnEnable()
	{
		Timing.KillCoroutines(base.gameObject);
		Timing.RunCoroutine(_DoAnimation(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _DoAnimation()
	{
		text = GetComponent<TextMeshProUGUI>();
		txt = GetComponent<Text>();
		while (this != null)
		{
			string[] array = texts;
			foreach (string item in array)
			{
				try
				{
					if (text != null)
					{
						text.text = item;
					}
					if (txt != null)
					{
						txt.text = item;
					}
				}
				catch
				{
					Debug.LogError("Iteration error in BC-SFX script");
				}
				yield return Timing.WaitForSeconds(waitTime);
			}
		}
	}
}
