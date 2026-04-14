using System;
using UnityEngine;

[Serializable]
public class UBER_MaterialProp
{
	[SerializeField]
	public string name;

	[SerializeField]
	public float floatValue;

	[SerializeField]
	public Color colorValue;

	[SerializeField]
	public Vector4 vectorValue;

	[SerializeField]
	public Texture textureValue;

	[SerializeField]
	public Vector2 textureOffset;

	[SerializeField]
	public Vector2 textureScale;
}
