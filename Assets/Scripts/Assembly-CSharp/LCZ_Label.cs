using UnityEngine;

public class LCZ_Label : MonoBehaviour
{
	public MeshRenderer chRend;

	public MeshRenderer numRend;

	public void Refresh(Material ch, Material num, string err)
	{
		chRend.sharedMaterial = ch;
		if (chRend.sharedMaterial == null)
		{
			Debug.Log(err);
		}
		numRend.sharedMaterial = num;
	}
}
