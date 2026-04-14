using UnityEngine;

namespace Kino
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Kino Image Effects/Analog Glitch")]
	public class AnalogGlitch : MonoBehaviour
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _scanLineJitter;

		[SerializeField]
		[Range(0f, 1f)]
		private float _verticalJump;

		[SerializeField]
		[Range(0f, 1f)]
		private float _horizontalShake;

		[SerializeField]
		[Range(0f, 1f)]
		private float _colorDrift;

		[SerializeField]
		private Shader _shader;

		private Material _material;

		private float _verticalJumpTime;

		public float scanLineJitter
		{
			get
			{
				return _scanLineJitter;
			}
			set
			{
				_scanLineJitter = value;
			}
		}

		public float verticalJump
		{
			get
			{
				return _verticalJump;
			}
			set
			{
				_verticalJump = value;
			}
		}

		public float horizontalShake
		{
			get
			{
				return _horizontalShake;
			}
			set
			{
				_horizontalShake = value;
			}
		}

		public float colorDrift
		{
			get
			{
				return _colorDrift;
			}
			set
			{
				_colorDrift = value;
			}
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (_material == null)
			{
				_material = new Material(_shader);
				_material.hideFlags = HideFlags.DontSave;
			}
			_verticalJumpTime += Time.deltaTime * _verticalJump * 11.3f;
			float y = Mathf.Clamp01(1f - _scanLineJitter * 1.2f);
			float x = 0.002f + Mathf.Pow(_scanLineJitter, 3f) * 0.05f;
			_material.SetVector("_ScanLineJitter", new Vector2(x, y));
			Vector2 vector = new Vector2(_verticalJump, _verticalJumpTime);
			_material.SetVector("_VerticalJump", vector);
			_material.SetFloat("_HorizontalShake", _horizontalShake * 0.2f);
			Vector2 vector2 = new Vector2(_colorDrift * 0.04f, Time.time * 606.11f);
			_material.SetVector("_ColorDrift", vector2);
			Graphics.Blit(source, destination, _material);
		}
	}
}
