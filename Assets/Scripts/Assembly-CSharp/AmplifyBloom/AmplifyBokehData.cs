using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public class AmplifyBokehData
	{
		internal RenderTexture BokehRenderTexture;

		internal Vector4[] Offsets;

		public AmplifyBokehData(Vector4[] offsets)
		{
			Offsets = offsets;
		}

		public void Destroy()
		{
			if (BokehRenderTexture != null)
			{
				AmplifyUtils.ReleaseTempRenderTarget(BokehRenderTexture);
				BokehRenderTexture = null;
			}
			Offsets = null;
		}
	}
}
