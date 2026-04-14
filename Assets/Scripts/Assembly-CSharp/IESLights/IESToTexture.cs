using UnityEngine;

namespace IESLights
{
	public class IESToTexture : MonoBehaviour
	{
		public static Texture2D ConvertIesData(IESData data)
		{
			Texture2D texture2D = new Texture2D(data.NormalizedValues.Count, data.NormalizedValues[0].Count, TextureFormat.RGBAFloat, false, true);
			texture2D.filterMode = FilterMode.Trilinear;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			Texture2D texture2D2 = texture2D;
			Color[] array = new Color[texture2D2.width * texture2D2.height];
			for (int i = 0; i < texture2D2.width; i++)
			{
				for (int j = 0; j < texture2D2.height; j++)
				{
					float num = data.NormalizedValues[i][j];
					array[i + j * texture2D2.width] = new Color(num, num, num, num);
				}
			}
			texture2D2.SetPixels(array);
			texture2D2.Apply();
			return texture2D2;
		}
	}
}
