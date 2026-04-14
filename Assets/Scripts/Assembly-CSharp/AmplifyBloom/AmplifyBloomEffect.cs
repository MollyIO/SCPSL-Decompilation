using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Image Effects/Amplify Bloom")]
	public sealed class AmplifyBloomEffect : AmplifyBloomBase
	{
	}
}
