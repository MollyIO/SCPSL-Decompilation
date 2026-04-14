using UnityEngine;

public class Rid : MonoBehaviour
{
	public string id;

	private void Start()
	{
		if (string.IsNullOrEmpty(id))
		{
			id = GetComponentInChildren<MeshRenderer>().material.mainTexture.name;
		}
	}
}
