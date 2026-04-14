using UnityEngine;

public class TextureMaterialLanguage : MonoBehaviour
{
	public Texture englishVersion;

	public Material mat;

	private void Start()
	{
		mat.mainTexture = englishVersion;
	}
}
