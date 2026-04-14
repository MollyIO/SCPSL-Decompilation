using UnityEngine;

public class KeyBindElement : MonoBehaviour
{
	public string axis;

	public void Click()
	{
		GetComponentInParent<ChangeKeyBinding>().ChangeKey(axis);
	}
}
