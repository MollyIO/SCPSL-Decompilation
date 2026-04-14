using System.Collections;
using UnityEngine;

public class DecalDestroyer : MonoBehaviour
{
	public float lifeTime = 5f;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(lifeTime);
		Object.Destroy(base.gameObject);
	}
}
