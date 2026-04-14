using UnityEngine;

public class GameMenuButton : MonoBehaviour
{
	public Vector3 normalPos;

	public Vector3 focusedPos;

	public AnimationCurve anim;

	private bool isFocused;

	private float status;

	private RectTransform rectTransform;

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void Focus(bool b)
	{
		isFocused = b;
	}

	private void Update()
	{
		status += Time.deltaTime * (float)(isFocused ? 1 : (-1));
		status = Mathf.Clamp01(status);
		Vector3 vector = focusedPos - normalPos;
		rectTransform.localPosition = normalPos + vector * anim.Evaluate(status);
	}
}
