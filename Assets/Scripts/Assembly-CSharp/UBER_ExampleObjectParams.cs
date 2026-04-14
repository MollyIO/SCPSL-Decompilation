using System;
using UnityEngine;

[Serializable]
public class UBER_ExampleObjectParams
{
	public GameObject target;

	public string materialProperty;

	public MeshRenderer renderer;

	public int submeshIndex;

	public Vector2 SliderRange;

	public string Title;

	public string MatParamName;

	[TextArea(2, 5)]
	public string Description;
}
