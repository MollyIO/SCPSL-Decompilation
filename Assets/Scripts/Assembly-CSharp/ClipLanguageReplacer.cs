using System.Collections;
using UnityEngine;

public class ClipLanguageReplacer : MonoBehaviour
{
	[SerializeField]
	public AudioClip englishVersion;

	private string file;

	private IEnumerator Start()
	{
		AudioSource asource = GetComponent<AudioSource>();
		asource.clip = englishVersion;
		string path = TranslationReader.path + "/Custom Audio/" + asource.clip.name + ".ogg";
		yield break;
	}
}
