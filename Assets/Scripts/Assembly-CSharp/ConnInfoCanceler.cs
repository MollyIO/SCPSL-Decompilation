using UnityEngine;

public class ConnInfoCanceler : ConnInfoButton
{
	public override void UseButton()
	{
		base.UseButton();
		Object.FindObjectOfType<CustomNetworkManager>().StopClient();
	}
}
