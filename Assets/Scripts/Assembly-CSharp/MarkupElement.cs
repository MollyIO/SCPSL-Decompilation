using UnityEngine;
using UnityEngine.UI;

public class MarkupElement : MonoBehaviour
{
	public MarkupStyle markupStyle;

	private string curTag;

	[Space]
	public Image targetPlane;

	public Text targetText;

	private GameObject targetWWW;

	public GameObject templateWWW;

	public Font[] fonts;

	private void Update()
	{
	}

	public void RefreshStyle(string tagName)
	{
		curTag = tagName;
		MarkupStyle style = markupStyle;
		foreach (MarkupReader.TagStyleRelation relation in MarkupReader.singleton.relations)
		{
			if (relation.tag == tagName.ToLower())
			{
				style = relation.style;
			}
		}
		style.position = markupStyle.position;
		style.rotation = markupStyle.rotation;
		style.size = markupStyle.size;
		markupStyle = style;
		RectTransform component = GetComponent<RectTransform>();
		component.localScale = Vector3.one;
		component.sizeDelta = markupStyle.size;
		component.localPosition = markupStyle.position;
		component.localRotation = Quaternion.Euler(Vector3.forward * markupStyle.rotation);
		if (markupStyle.mainColor != Color.clear)
		{
			targetPlane.enabled = true;
			targetPlane.color = markupStyle.mainColor;
			targetPlane.raycastTarget = markupStyle.raycast;
			Outline component2 = targetPlane.GetComponent<Outline>();
			if (markupStyle.outlineSize > 0f)
			{
				component2.enabled = true;
				component2.effectColor = markupStyle.outlineColor;
				component2.effectDistance = Vector2.one * markupStyle.outlineSize;
			}
			else
			{
				component2.enabled = false;
			}
		}
		else
		{
			targetPlane.enabled = false;
		}
		if (!string.IsNullOrEmpty(markupStyle.textContent))
		{
			targetText.enabled = true;
			targetText.color = markupStyle.textColor;
			targetText.raycastTarget = markupStyle.raycast;
			targetText.text = markupStyle.textContent;
			targetText.alignment = markupStyle.textAlignment;
			targetText.fontSize = markupStyle.fontSize;
			targetText.font = fonts[markupStyle.fontID];
			Outline component3 = targetText.GetComponent<Outline>();
			if (markupStyle.textOutlineSize > 0f)
			{
				component3.enabled = true;
				component3.effectColor = markupStyle.textOutlineColor;
				component3.effectDistance = Vector2.one * markupStyle.textOutlineSize;
			}
			else
			{
				component3.enabled = false;
			}
		}
		else
		{
			targetText.enabled = false;
		}
		if (!string.IsNullOrEmpty(markupStyle.imageUrl))
		{
			if (targetWWW != null)
			{
				Object.DestroyImmediate(targetWWW);
			}
			targetWWW = Object.Instantiate(templateWWW, base.transform);
			component = targetWWW.GetComponent<RectTransform>();
			component.localScale = Vector3.one;
			component.sizeDelta = Vector2.zero;
			component.localPosition = Vector2.zero;
			component.localRotation = Quaternion.Euler(Vector3.zero);
			component.SetSiblingIndex(1);
			targetWWW.GetComponent<MarkupImageRequest>().DownloadImage(markupStyle.imageUrl, markupStyle.imageColor);
		}
		else if (targetWWW != null)
		{
			Object.Destroy(targetWWW);
		}
	}
}
