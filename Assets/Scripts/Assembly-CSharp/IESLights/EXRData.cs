using UnityEngine;

namespace IESLights
{
	public struct EXRData
	{
		public Color[] Pixels;

		public uint Width;

		public uint Height;

		public EXRData(Color[] pixels, int width, int height)
		{
			Pixels = pixels;
			Width = (uint)width;
			Height = (uint)height;
		}
	}
}
