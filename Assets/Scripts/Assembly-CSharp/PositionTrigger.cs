using UnityEngine;

public class PositionTrigger : MonoBehaviour
{
	public bool disableOnEnd = true;

	public int id;

	public float range;

	private GameObject ply;

	private void Update()
	{
		if (ply == null)
		{
			ply = GameObject.FindGameObjectWithTag("Player");
		}
		else if (Vector3.Distance(ply.transform.position, base.transform.position) <= range)
		{
			Object.FindObjectOfType<TutorialManager>().Trigger(id);
			if (disableOnEnd)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0f, 0.1f, 0.2f, 0.2f);
		Gizmos.DrawSphere(base.transform.position, range);
	}
}
