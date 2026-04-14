using UnityEngine;

public class KillTrigger : MonoBehaviour
{
	public int killsToTrigger;

	public int triggerID;

	public string alias;

	public bool disableOnEnd = true;

	public int prioirty;

	public void Trigger(int amount)
	{
		if (amount == killsToTrigger)
		{
			if (triggerID == -1)
			{
				Object.FindObjectOfType<TutorialManager>().Tutorial2_Result();
			}
			else if (alias != string.Empty)
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
		}
	}
}
