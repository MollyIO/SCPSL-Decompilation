using System.Collections.Generic;
using UnityEngine;

public class ContentFitter : MonoBehaviour
{
	public bool continuousUpdate;

	public RectTransform targetTransform;

	private void LateUpdate()
	{
		if (continuousUpdate)
		{
			continuousUpdate = false;
			Fit();
		}
	}

	public void Fit()
	{
		List<RectTransform> list = new List<RectTransform>();
		RectTransform[] componentsInChildren = GetComponentsInChildren<RectTransform>();
		foreach (RectTransform rectTransform in componentsInChildren)
		{
			if (rectTransform != GetComponent<RectTransform>())
			{
				list.Add(rectTransform);
			}
		}
		Vector2 vector = new Vector2(1E+09f, -1E+09f);
		Vector2 vector2 = new Vector2(-1E+09f, 1E+09f);
		foreach (RectTransform item in list)
		{
			Vector2 vector3 = new Vector2(item.localPosition.x - item.sizeDelta.x * item.pivot.x, item.localPosition.y + item.sizeDelta.y * item.pivot.y);
			Vector2 vector4 = new Vector2(item.localPosition.x + item.sizeDelta.x * (1f - item.pivot.x), item.localPosition.y - item.sizeDelta.y * (1f - item.pivot.y));
			if (vector3.x < vector.x)
			{
				vector.x = vector3.x;
			}
			if (vector3.y > vector.y)
			{
				vector.y = vector3.y;
			}
			if (vector4.y < vector2.y)
			{
				vector2.y = vector4.y;
			}
			if (vector4.x > vector2.x)
			{
				vector2.x = vector4.x;
			}
		}
		Vector2 sizeDelta = new Vector2(Mathf.Abs(vector.x - vector2.x), Mathf.Abs(vector.y - vector2.y));
		targetTransform.localPosition = vector;
		targetTransform.sizeDelta = sizeDelta;
	}
}
