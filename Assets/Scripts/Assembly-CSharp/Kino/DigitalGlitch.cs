using UnityEngine;

namespace Kino
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Kino Image Effects/Digital Glitch")]
	public class DigitalGlitch : MonoBehaviour
	{
		[Range(0f, 1f)]
		[SerializeField]
		private float _intensity;

		[SerializeField]
		private Shader _shader;

		private Material _material;

		private Texture2D _noiseTexture;

		private RenderTexture _trashFrame1;

		private RenderTexture _trashFrame2;

		public float intensity
		{
			get
			{
				return _intensity;
			}
			set
			{
				_intensity = value;
			}
		}

		private static Color RandomColor()
		{
			return new Color(Random.value, Random.value, Random.value, Random.value);
		}

		private void SetUpResources()
		{
			if (!(_material != null))
			{
				_material = new Material(_shader);
				_material.hideFlags = HideFlags.DontSave;
				_noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
				_noiseTexture.hideFlags = HideFlags.DontSave;
				_noiseTexture.wrapMode = TextureWrapMode.Clamp;
				_noiseTexture.filterMode = FilterMode.Point;
				_trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
				_trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
				_trashFrame1.hideFlags = HideFlags.DontSave;
				_trashFrame2.hideFlags = HideFlags.DontSave;
				UpdateNoiseTexture();
			}
		}

		private void UpdateNoiseTexture()
		{
			Color color = RandomColor();
			for (int i = 0; i < _noiseTexture.height; i++)
			{
				for (int j = 0; j < _noiseTexture.width; j++)
				{
					if (Random.value > 0.89f)
					{
						color = RandomColor();
					}
					_noiseTexture.SetPixel(j, i, color);
				}
			}
			_noiseTexture.Apply();
		}

		private void Update()
		{
			if (Random.value > Mathf.Lerp(0.9f, 0.5f, _intensity))
			{
				SetUpResources();
				UpdateNoiseTexture();
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			SetUpResources();
			int frameCount = Time.frameCount;
			if (frameCount % 13 == 0)
			{
				Graphics.Blit(source, _trashFrame1);
			}
			if (frameCount % 73 == 0)
			{
				Graphics.Blit(source, _trashFrame2);
			}
			_material.SetFloat("_Intensity", _intensity);
			_material.SetTexture("_NoiseTex", _noiseTexture);
			RenderTexture value = ((!(Random.value > 0.5f)) ? _trashFrame2 : _trashFrame1);
			_material.SetTexture("_TrashTex", value);
			Graphics.Blit(source, destination, _material);
		}
	}
}
