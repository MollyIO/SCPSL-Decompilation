using UnityEngine;

public class DoorOpenTrigger : MonoBehaviour
{
	public Door door;

	public bool stageToTrigger = true;

	public int id;

	public string alias;

	private void Update()
	{
		if (door.isOpen == stageToTrigger)
		{
			if (alias != string.Empty)
			{
				Object.FindObjectOfType<TutorialManager>().Trigger(alias);
			}
			else
			{
				Object.FindObjectOfType<TutorialManager>().Trigger(id);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
