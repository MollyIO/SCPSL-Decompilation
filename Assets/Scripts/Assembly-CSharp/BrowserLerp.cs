using UnityEngine;

public class BrowserLerp : MonoBehaviour
{
	private Vector3 prevPos;

	private RectTransform rectTransform;

	private Vector3 targetPos;

	public float speed = 2f;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		targetPos += rectTransform.localPosition - prevPos;
		rectTransform.localPosition = prevPos;
		rectTransform.localPosition = Vector3.Lerp(rectTransform.localPosition, targetPos, Time.deltaTime * speed * 4f);
		prevPos = rectTransform.localPosition;
	}
}
