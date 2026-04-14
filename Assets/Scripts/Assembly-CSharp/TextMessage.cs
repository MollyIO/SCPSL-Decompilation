using UnityEngine;

public class TextMessage : MonoBehaviour
{
	public float spacing = 15.5f;

	public float xOffset = 3f;

	public float lerpSpeed = 3f;

	public float position;

	public float remainingLife;

	private CanvasRenderer r;

	private Vector3 GetPosition()
	{
		return new Vector3(xOffset, spacing * position, 0f);
	}

	private void Start()
	{
		r = GetComponent<CanvasRenderer>();
		base.transform.localPosition = GetPosition() + Vector3.down * spacing;
	}

	private void Update()
	{
		remainingLife -= Time.deltaTime;
		r.SetAlpha(Mathf.Clamp01(remainingLife * 2f));
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, GetPosition(), Time.deltaTime * lerpSpeed);
	}
}
