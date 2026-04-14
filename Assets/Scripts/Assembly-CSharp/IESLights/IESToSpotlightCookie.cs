using System;
using System.Linq;
using UnityEngine;

namespace IESLights
{
	[ExecuteInEditMode]
	public class IESToSpotlightCookie : MonoBehaviour
	{
		private Material _spotlightMaterial;

		private Material _fadeSpotlightEdgesMaterial;

		private Material _verticalFlipMaterial;

		private void OnDestroy()
		{
			if (_spotlightMaterial != null)
			{
				UnityEngine.Object.Destroy(_spotlightMaterial);
			}
			if (_fadeSpotlightEdgesMaterial != null)
			{
				UnityEngine.Object.Destroy(_fadeSpotlightEdgesMaterial);
			}
			if (_verticalFlipMaterial != null)
			{
				UnityEngine.Object.Destroy(_verticalFlipMaterial);
			}
		}

		public void CreateSpotlightCookie(Texture2D iesTexture, IESData iesData, int resolution, bool applyVignette, bool flipVertically, out Texture2D cookie)
		{
			if (iesData.PhotometricType != PhotometricType.TypeA)
			{
				if (_spotlightMaterial == null)
				{
					_spotlightMaterial = new Material(Shader.Find("Hidden/IES/IESToSpotlightCookie"));
				}
				CalculateAndSetSpotHeight(iesData);
				SetShaderKeywords(iesData, applyVignette);
				cookie = CreateTexture(iesTexture, resolution, flipVertically);
			}
			else
			{
				if (_fadeSpotlightEdgesMaterial == null)
				{
					_fadeSpotlightEdgesMaterial = new Material(Shader.Find("Hidden/IES/FadeSpotlightCookieEdges"));
				}
				float verticalCenter = ((!applyVignette) ? 0f : CalculateCookieVerticalCenter(iesData));
				Vector2 vector = ((!applyVignette) ? Vector2.zero : CalculateCookieFadeEllipse(iesData));
				cookie = BlitToTargetSize(iesTexture, resolution, vector.x, vector.y, verticalCenter, applyVignette, flipVertically);
			}
		}

		private float CalculateCookieVerticalCenter(IESData iesData)
		{
			float num = 1f - (float)iesData.PadBeforeAmount / (float)iesData.NormalizedValues[0].Count;
			float num2 = (float)(iesData.NormalizedValues[0].Count - iesData.PadBeforeAmount - iesData.PadAfterAmount) / (float)iesData.NormalizedValues.Count / 2f;
			return num - num2;
		}

		private Vector2 CalculateCookieFadeEllipse(IESData iesData)
		{
			if (iesData.HorizontalAngles.Count > iesData.VerticalAngles.Count)
			{
				return new Vector2(0.5f, 0.5f * ((float)(iesData.NormalizedValues[0].Count - iesData.PadBeforeAmount - iesData.PadAfterAmount) / (float)iesData.NormalizedValues[0].Count));
			}
			if (iesData.HorizontalAngles.Count < iesData.VerticalAngles.Count)
			{
				return new Vector2(0.5f * (iesData.HorizontalAngles.Max() - iesData.HorizontalAngles.Min()) / (iesData.VerticalAngles.Max() - iesData.VerticalAngles.Min()), 0.5f);
			}
			return new Vector2(0.5f, 0.5f);
		}

		private Texture2D CreateTexture(Texture2D iesTexture, int resolution, bool flipVertically)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Trilinear;
			temporary.DiscardContents();
			RenderTexture.active = temporary;
			Graphics.Blit(iesTexture, _spotlightMaterial);
			if (flipVertically)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				Graphics.Blit(temporary, temporary2);
				FlipVertically(temporary2, temporary);
				RenderTexture.ReleaseTemporary(temporary2);
			}
			Texture2D texture2D = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
			texture2D.filterMode = FilterMode.Trilinear;
			Texture2D texture2D2 = texture2D;
			texture2D2.wrapMode = TextureWrapMode.Clamp;
			texture2D2.ReadPixels(new Rect(0f, 0f, resolution, resolution), 0, 0);
			texture2D2.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D2;
		}

		private Texture2D BlitToTargetSize(Texture2D iesTexture, int resolution, float horizontalFadeDistance, float verticalFadeDistance, float verticalCenter, bool applyVignette, bool flipVertically)
		{
			if (applyVignette)
			{
				_fadeSpotlightEdgesMaterial.SetFloat("_HorizontalFadeDistance", horizontalFadeDistance);
				_fadeSpotlightEdgesMaterial.SetFloat("_VerticalFadeDistance", verticalFadeDistance);
				_fadeSpotlightEdgesMaterial.SetFloat("_VerticalCenter", verticalCenter);
			}
			RenderTexture temporary = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Trilinear;
			temporary.DiscardContents();
			if (applyVignette)
			{
				RenderTexture.active = temporary;
				Graphics.Blit(iesTexture, _fadeSpotlightEdgesMaterial);
			}
			else if (flipVertically)
			{
				FlipVertically(iesTexture, temporary);
			}
			else
			{
				Graphics.Blit(iesTexture, temporary);
			}
			Texture2D texture2D = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
			texture2D.filterMode = FilterMode.Trilinear;
			Texture2D texture2D2 = texture2D;
			texture2D2.wrapMode = TextureWrapMode.Clamp;
			texture2D2.ReadPixels(new Rect(0f, 0f, resolution, resolution), 0, 0);
			texture2D2.Apply();
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D2;
		}

		private void FlipVertically(Texture iesTexture, RenderTexture renderTarget)
		{
			if (_verticalFlipMaterial == null)
			{
				_verticalFlipMaterial = new Material(Shader.Find("Hidden/IES/VerticalFlip"));
			}
			Graphics.Blit(iesTexture, renderTarget, _verticalFlipMaterial);
		}

		private void CalculateAndSetSpotHeight(IESData iesData)
		{
			float value = 0.5f / Mathf.Tan(iesData.HalfSpotlightFov * ((float)Math.PI / 180f));
			_spotlightMaterial.SetFloat("_SpotHeight", value);
		}

		private void SetShaderKeywords(IESData iesData, bool applyVignette)
		{
			if (applyVignette)
			{
				_spotlightMaterial.EnableKeyword("VIGNETTE");
			}
			else
			{
				_spotlightMaterial.DisableKeyword("VIGNETTE");
			}
			if (iesData.VerticalType == VerticalType.Top)
			{
				_spotlightMaterial.EnableKeyword("TOP_VERTICAL");
			}
			else
			{
				_spotlightMaterial.DisableKeyword("TOP_VERTICAL");
			}
			if (iesData.HorizontalType == HorizontalType.None)
			{
				_spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
				_spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
				_spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
			}
			else if (iesData.HorizontalType == HorizontalType.Quadrant)
			{
				_spotlightMaterial.EnableKeyword("QUAD_HORIZONTAL");
				_spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
				_spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
			}
			else if (iesData.HorizontalType == HorizontalType.Half)
			{
				_spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
				_spotlightMaterial.EnableKeyword("HALF_HORIZONTAL");
				_spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
			}
			else if (iesData.HorizontalType == HorizontalType.Full)
			{
				_spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
				_spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
				_spotlightMaterial.EnableKeyword("FULL_HORIZONTAL");
			}
		}
	}
}
