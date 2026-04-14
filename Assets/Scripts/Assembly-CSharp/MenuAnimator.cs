using System.Collections.Generic;
using MEC;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
	[SerializeField]
	private GameObject sceneCamera;

	[SerializeField]
	private GameObject focusedPosition;

	[SerializeField]
	private GameObject unfocusedPosition;

	[SerializeField]
	public static bool wasEverZoomed;

	private void Update()
	{
		sceneCamera.transform.position = Vector3.Lerp(sceneCamera.transform.position, (!wasEverZoomed) ? unfocusedPosition.transform.position : focusedPosition.transform.position, Time.deltaTime * 2f);
		sceneCamera.transform.rotation = Quaternion.Lerp(sceneCamera.transform.rotation, (!wasEverZoomed) ? unfocusedPosition.transform.rotation : focusedPosition.transform.rotation, Time.deltaTime);
	}

	private void Start()
	{
		wasEverZoomed = false;
		Timing.RunCoroutine(_Animate(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Animate()
	{
		while (this != null)
		{
			int t = Random.Range(2, 5);
			SignBlink[] array = Object.FindObjectsOfType<SignBlink>();
			foreach (SignBlink signBlink in array)
			{
				signBlink.Play(t);
			}
			yield return Timing.WaitForSeconds(Random.Range(3, 10));
		}
	}
}
