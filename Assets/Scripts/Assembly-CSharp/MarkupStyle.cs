using System;
using UnityEngine;

[Serializable]
public class MarkupStyle
{
	[Header("Transform")]
	public Vector2 position = Vector2.zero;

	public Vector2 size = new Vector2(100f, 100f);

	public float rotation;

	[Header("Main Styles")]
	public Color mainColor = Color.clear;

	public Color outlineColor = Color.white;

	public float outlineSize;

	public bool raycast;

	[Header("Text")]
	public TextAnchor textAlignment = TextAnchor.MiddleCenter;

	public string textContent = string.Empty;

	public Color textColor = Color.white;

	public Color textOutlineColor = Color.black;

	public float textOutlineSize;

	public int fontID;

	public int fontSize = 20;

	[Header("Background Image")]
	public string imageUrl = string.Empty;

	public Color imageColor = Color.white;
}
