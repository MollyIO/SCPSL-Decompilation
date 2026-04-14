using System;
using UnityEngine;

[Serializable]
public class UBER_MaterialPreset
{
	[SerializeField]
	public string name;

	[SerializeField]
	public Shader shader;

	[SerializeField]
	public UBER_MaterialProp[] props;
}
