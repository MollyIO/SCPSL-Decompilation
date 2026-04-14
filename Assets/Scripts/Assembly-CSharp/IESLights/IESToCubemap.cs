using UnityEngine;

namespace IESLights
{
	[ExecuteInEditMode]
	public class IESToCubemap : MonoBehaviour
	{
		private Material _iesMaterial;

		private Material _horizontalMirrorMaterial;

		private void OnDestroy()
		{
			if (_horizontalMirrorMaterial != null)
			{
				Object.DestroyImmediate(_horizontalMirrorMaterial);
			}
		}

		public void CreateCubemap(Texture2D iesTexture, IESData iesData, int resolution, out Cubemap cubemap)
		{
			PrepMaterial(iesTexture, iesData);
			CreateCubemap(resolution, out cubemap);
		}

		public Color[] CreateRawCubemap(Texture2D iesTexture, IESData iesData, int resolution)
		{
			PrepMaterial(iesTexture, iesData);
			RenderTexture[] array = new RenderTexture[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				array[i].filterMode = FilterMode.Trilinear;
			}
			Camera[] componentsInChildren = base.transform.GetChild(0).GetComponentsInChildren<Camera>();
			for (int j = 0; j < 6; j++)
			{
				componentsInChildren[j].targetTexture = array[j];
				componentsInChildren[j].Render();
				componentsInChildren[j].targetTexture = null;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(resolution * 6, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Trilinear;
			if (_horizontalMirrorMaterial == null)
			{
				_horizontalMirrorMaterial = new Material(Shader.Find("Hidden/IES/HorizontalFlip"));
			}
			RenderTexture.active = temporary;
			for (int k = 0; k < 6; k++)
			{
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, resolution * 6, 0f, resolution);
				Graphics.DrawTexture(new Rect(k * resolution, 0f, resolution, resolution), array[k], _horizontalMirrorMaterial);
				GL.PopMatrix();
			}
			Texture2D texture2D = new Texture2D(resolution * 6, resolution, TextureFormat.RGBAFloat, false, true);
			texture2D.filterMode = FilterMode.Trilinear;
			Texture2D texture2D2 = texture2D;
			texture2D2.ReadPixels(new Rect(0f, 0f, texture2D2.width, texture2D2.height), 0, 0);
			Color[] pixels = texture2D2.GetPixels();
			RenderTexture.active = null;
			RenderTexture[] array2 = array;
			foreach (RenderTexture temp in array2)
			{
				RenderTexture.ReleaseTemporary(temp);
			}
			RenderTexture.ReleaseTemporary(temporary);
			Object.DestroyImmediate(texture2D2);
			return pixels;
		}

		private void PrepMaterial(Texture2D iesTexture, IESData iesData)
		{
			if (_iesMaterial == null)
			{
				_iesMaterial = GetComponent<Renderer>().sharedMaterial;
			}
			_iesMaterial.mainTexture = iesTexture;
			SetShaderKeywords(iesData, _iesMaterial);
		}

		private void SetShaderKeywords(IESData iesData, Material iesMaterial)
		{
			if (iesData.VerticalType == VerticalType.Bottom)
			{
				iesMaterial.EnableKeyword("BOTTOM_VERTICAL");
				iesMaterial.DisableKeyword("TOP_VERTICAL");
				iesMaterial.DisableKeyword("FULL_VERTICAL");
			}
			else if (iesData.VerticalType == VerticalType.Top)
			{
				iesMaterial.EnableKeyword("TOP_VERTICAL");
				iesMaterial.DisableKeyword("BOTTOM_VERTICAL");
				iesMaterial.DisableKeyword("FULL_VERTICAL");
			}
			else
			{
				iesMaterial.DisableKeyword("TOP_VERTICAL");
				iesMaterial.DisableKeyword("BOTTOM_VERTICAL");
				iesMaterial.EnableKeyword("FULL_VERTICAL");
			}
			if (iesData.HorizontalType == HorizontalType.None)
			{
				iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
				iesMaterial.DisableKeyword("HALF_HORIZONTAL");
				iesMaterial.DisableKeyword("FULL_HORIZONTAL");
			}
			else if (iesData.HorizontalType == HorizontalType.Quadrant)
			{
				iesMaterial.EnableKeyword("QUAD_HORIZONTAL");
				iesMaterial.DisableKeyword("HALF_HORIZONTAL");
				iesMaterial.DisableKeyword("FULL_HORIZONTAL");
			}
			else if (iesData.HorizontalType == HorizontalType.Half)
			{
				iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
				iesMaterial.EnableKeyword("HALF_HORIZONTAL");
				iesMaterial.DisableKeyword("FULL_HORIZONTAL");
			}
			else if (iesData.HorizontalType == HorizontalType.Full)
			{
				iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
				iesMaterial.DisableKeyword("HALF_HORIZONTAL");
				iesMaterial.EnableKeyword("FULL_HORIZONTAL");
			}
		}

		private void CreateCubemap(int resolution, out Cubemap cubemap)
		{
			cubemap = new Cubemap(resolution, TextureFormat.ARGB32, false)
			{
				filterMode = FilterMode.Trilinear
			};
			GetComponent<Camera>().RenderToCubemap(cubemap);
		}
	}
}
