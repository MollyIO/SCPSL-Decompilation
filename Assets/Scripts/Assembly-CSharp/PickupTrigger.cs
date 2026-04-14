using UnityEngine;

public class PickupTrigger : MonoBehaviour
{
	public int filter = -1;

	public int triggerID;

	public string alias;

	public bool disableOnEnd = true;

	public int prioirty;

	public bool Trigger(int item)
	{
		if (triggerID == -1)
		{
			Object.Destroy(base.gameObject);
			return true;
		}
		if (filter == -1 || item == filter)
		{
			if (alias != string.Empty)
			{
				Object.FindObjectOfType<TutorialManager>().Trigger(alias);
			}
			else
			{
				Object.FindObjectOfType<TutorialManager>().Trigger(triggerID);
			}
			if (disableOnEnd)
			{
				Object.Destroy(base.gameObject);
			}
			return true;
		}
		return false;
	}
}
