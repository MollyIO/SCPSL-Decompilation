using UnityEngine;

public class MarkupCanvas : MonoBehaviour
{
	public static MarkupCanvas singleton;

	private void Awake()
	{
		singleton = this;
	}
}
