using System.Collections;
using UnityEngine;

[AddComponentMenu("Camera Filter Pack/EXTRA/SHOWFPS")]
[ExecuteInEditMode]
public class CameraFilterPack_EXTRA_SHOWFPS : MonoBehaviour
{
	private float accum;

	private int frames;

	public float frequency = 0.5f;

	public Shader SCShader;

	private float TimeX = 1f;

	private Material SCMaterial;

	[Range(8f, 42f)]
	public float Size = 12f;

	[Range(0f, 100f)]
	private int FPS = 1;

	[Range(0f, 10f)]
	private readonly float Value3 = 1f;

	[Range(0f, 10f)]
	private readonly float Value4 = 1f;

	private Material material
	{
		get
		{
			if (SCMaterial == null)
			{
				SCMaterial = new Material(SCShader);
				SCMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return SCMaterial;
		}
	}

	private void Start()
	{
		FPS = 0;
		StartCoroutine(FPSX());
		SCShader = Shader.Find("CameraFilterPack/EXTRA_SHOWFPS");
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
	}

	private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if (SCShader != null)
		{
			TimeX += Time.deltaTime;
			if (TimeX > 100f)
			{
				TimeX = 0f;
			}
			material.SetFloat("_TimeX", TimeX);
			material.SetFloat("_Value", Size);
			material.SetFloat("_Value2", FPS);
			material.SetFloat("_Value3", Value3);
			material.SetFloat("_Value4", Value4);
			material.SetVector("_ScreenResolution", new Vector4(sourceTexture.width, sourceTexture.height, 0f, 0f));
			Graphics.Blit(sourceTexture, destTexture, material);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}

	private IEnumerator FPSX()
	{
		while (true)
		{
			float fps = accum / (float)frames;
			FPS = (int)fps;
			accum = 0f;
			frames = 0;
			yield return new WaitForSeconds(frequency);
		}
	}

	private void Update()
	{
		accum += Time.timeScale / Time.deltaTime;
		frames++;
	}

	private void OnDisable()
	{
		if ((bool)SCMaterial)
		{
			Object.DestroyImmediate(SCMaterial);
		}
	}
}
