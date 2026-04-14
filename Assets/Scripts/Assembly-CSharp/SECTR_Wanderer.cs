using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("SECTR/Demos/SECTR Wanderer")]
public class SECTR_Wanderer : MonoBehaviour
{
	private List<SECTR_Graph.Node> path = new List<SECTR_Graph.Node>(16);

	private List<Vector3> waypoints = new List<Vector3>(16);

	private int currentWaypointIndex;

	[SECTR_ToolTip("The speed at which the wanderer moves throughout the world.")]
	public float MovementSpeed = 1f;

	private void Update()
	{
		if (waypoints.Count == 0 && SECTR_Sector.All.Count > 0 && MovementSpeed > 0f)
		{
			SECTR_Sector sECTR_Sector = SECTR_Sector.All[Random.Range(0, SECTR_Sector.All.Count)];
			SECTR_Graph.FindShortestPath(ref path, base.transform.position, sECTR_Sector.transform.position, SECTR_Portal.PortalFlags.Locked);
			Vector3 zero = Vector3.zero;
			Collider component = GetComponent<Collider>();
			if ((bool)component)
			{
				zero.y += component.bounds.extents.y;
			}
			waypoints.Clear();
			int count = path.Count;
			for (int i = 0; i < count; i++)
			{
				SECTR_Graph.Node node = path[i];
				waypoints.Add(node.Sector.transform.position + zero);
				if ((bool)node.Portal)
				{
					waypoints.Add(node.Portal.transform.position);
				}
			}
			waypoints.Add(sECTR_Sector.transform.position + zero);
			currentWaypointIndex = 0;
		}
		if (waypoints.Count <= 0 || !(MovementSpeed > 0f))
		{
			return;
		}
		Vector3 vector = waypoints[currentWaypointIndex];
		Vector3 vector2 = vector - base.transform.position;
		float sqrMagnitude = vector2.sqrMagnitude;
		if (sqrMagnitude > 0.001f)
		{
			float num = Mathf.Sqrt(sqrMagnitude);
			vector2 /= num;
			vector2 *= Mathf.Min(MovementSpeed * Time.deltaTime, num);
			base.transform.position += vector2;
		}
		else
		{
			currentWaypointIndex++;
			if (currentWaypointIndex >= waypoints.Count)
			{
				waypoints.Clear();
			}
		}
	}
}
