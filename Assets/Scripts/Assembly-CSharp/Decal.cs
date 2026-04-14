using UnityEngine;

public class Decal : MonoBehaviour
{
	private MeshFilter filter;

	private MeshRenderer renderer;

	private void Start()
	{
		renderer = GetComponent<MeshRenderer>();
		filter = GetComponent<MeshFilter>();
		Mesh sharedMesh = filter.sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			MonoBehaviour.print(i);
			Debug.DrawRay(base.transform.TransformPoint(vertices[i]), -base.transform.forward, Color.red, 10f);
			RaycastHit hitInfo;
			if (Physics.Raycast(base.transform.TransformPoint(vertices[i]), -base.transform.forward, out hitInfo))
			{
				vertices[i] = base.transform.InverseTransformPoint(hitInfo.point);
			}
			else
			{
				vertices[i] = Vector3.zero;
			}
		}
		sharedMesh.vertices = vertices;
		sharedMesh.RecalculateNormals();
	}
}
