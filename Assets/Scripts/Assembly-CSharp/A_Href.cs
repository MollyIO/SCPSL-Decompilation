using UnityEngine;

public class A_Href : MonoBehaviour
{
	public void Click(string url)
	{
		Application.OpenURL(url);
	}
}
