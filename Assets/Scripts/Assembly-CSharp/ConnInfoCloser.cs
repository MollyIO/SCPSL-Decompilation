using UnityEngine;

public class ConnInfoCloser : ConnInfoButton
{
	public GameObject objToClose;

	public override void UseButton()
	{
		objToClose.SetActive(false);
		base.UseButton();
	}
}
