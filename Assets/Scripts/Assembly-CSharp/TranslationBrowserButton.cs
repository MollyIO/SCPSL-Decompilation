using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TranslationBrowserButton : MonoBehaviour
{
	public void OnClick()
	{
		PlayerPrefs.SetString("translation_path", GetComponent<Text>().text);
		SceneManager.LoadScene(0);
	}
}
