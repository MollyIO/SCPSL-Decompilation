using UnityEngine;

public class MaterialLanguageReplacer : MonoBehaviour
{
	public Material englishVersion;

	private void Start()
	{
		GetComponent<Renderer>().material = englishVersion;
	}
}
