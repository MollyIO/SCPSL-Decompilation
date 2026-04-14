using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HintManager : MonoBehaviour
{
	[Serializable]
	public class Hint
	{
		[Multiline]
		public string content_en;

		public string keyName;

		public float duration;
	}

	public static HintManager singleton;

	[SerializeField]
	private Image box;

	public Hint[] hints;

	public List<Hint> hintQueue = new List<Hint>();

	private void Awake()
	{
		box.canvasRenderer.SetAlpha(0f);
		singleton = this;
		for (int i = 0; i < hints.Length; i++)
		{
			hints[i].content_en = TranslationReader.Get("Hints", i);
		}
	}

	private void Start()
	{
		box.canvasRenderer.SetAlpha(0f);
	}

	private IEnumerator<float> _ShowHints()
	{
		yield return 0f;
	}

	public void AddHint(int hintID)
	{
		if (!TutorialManager.status && PlayerPrefs.GetInt(hints[hintID].keyName, 0) == 0)
		{
			hintQueue.Add(hints[hintID]);
			PlayerPrefs.SetInt(hints[hintID].keyName, 1);
		}
	}
}
