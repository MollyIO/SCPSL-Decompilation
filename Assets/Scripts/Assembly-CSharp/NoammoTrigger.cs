using UnityEngine;

public class NoammoTrigger : MonoBehaviour
{
	public int filter = -1;

	public int triggerID;

	public string alias;

	public bool disableOnEnd = true;

	public int prioirty;

	public int[] optionalForcedID;

	public bool Trigger(int item)
	{
		bool flag = false;
		int[] array = optionalForcedID;
		foreach (int num in array)
		{
			if (TutorialManager.curlog == num)
			{
				flag = true;
			}
		}
		if (flag || optionalForcedID.Length == 0)
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
		return false;
	}
}
