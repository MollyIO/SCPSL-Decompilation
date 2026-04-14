using UnityEngine;

public class ResolutionButton : MonoBehaviour
{
	public void Click(int id)
	{
		ResolutionManager.ChangeResolution(id);
	}
}
