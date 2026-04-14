using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("UBER/Deferred Params")]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class UBER_DeferredParams : MonoBehaviour
{
	[Header("Translucency setup 1")]
	[ColorUsage(false)]
	public Color TranslucencyColor1 = new Color(1f, 1f, 1f, 1f);

	[Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
	public float Strength1 = 4f;

	[Range(0f, 1f)]
	public float PointLightsDirectionality1 = 0.7f;

	[Range(0f, 0.5f)]
	public float Constant1 = 0.1f;

	[Range(0f, 0.3f)]
	public float Scattering1 = 0.05f;

	[Range(0f, 100f)]
	public float SpotExponent1 = 30f;

	[Range(0f, 20f)]
	public float SuppressShadows1 = 0.5f;

	[Range(0f, 1f)]
	public float NdotLReduction1;

	[Space]
	[Header("Translucency setup 2")]
	[ColorUsage(false)]
	public Color TranslucencyColor2 = new Color(1f, 1f, 1f, 1f);

	[Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
	public float Strength2 = 4f;

	[Range(0f, 1f)]
	public float PointLightsDirectionality2 = 0.7f;

	[Range(0f, 0.5f)]
	public float Constant2 = 0.1f;

	[Range(0f, 0.3f)]
	public float Scattering2 = 0.05f;

	[Range(0f, 100f)]
	public float SpotExponent2 = 30f;

	[Range(0f, 20f)]
	public float SuppressShadows2 = 0.5f;

	[Range(0f, 1f)]
	public float NdotLReduction2;

	[ColorUsage(false)]
	[Space]
	[Header("Translucency setup 3")]
	public Color TranslucencyColor3 = new Color(1f, 1f, 1f, 1f);

	[Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
	public float Strength3 = 4f;

	[Range(0f, 1f)]
	public float PointLightsDirectionality3 = 0.7f;

	[Range(0f, 0.5f)]
	public float Constant3 = 0.1f;

	[Range(0f, 0.3f)]
	public float Scattering3 = 0.05f;

	[Range(0f, 100f)]
	public float SpotExponent3 = 30f;

	[Range(0f, 20f)]
	public float SuppressShadows3 = 0.5f;

	[Range(0f, 1f)]
	public float NdotLReduction3;

	[Space]
	[Header("Translucency setup 4")]
	[ColorUsage(false)]
	public Color TranslucencyColor4 = new Color(1f, 1f, 1f, 1f);

	[Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
	public float Strength4 = 4f;

	[Range(0f, 1f)]
	public float PointLightsDirectionality4 = 0.7f;

	[Range(0f, 0.5f)]
	public float Constant4 = 0.1f;

	[Range(0f, 0.3f)]
	public float Scattering4 = 0.05f;

	[Range(0f, 100f)]
	public float SpotExponent4 = 30f;

	[Range(0f, 20f)]
	public float SuppressShadows4 = 0.5f;

	[Range(0f, 1f)]
	public float NdotLReduction4;

	private Camera mycam;

	private CommandBuffer combufPreLight;

	private CommandBuffer combufPostLight;

	private Material CopyPropsMat;

	private bool UBERPresenceChecked;

	private bool UBERPresent;

	[HideInInspector]
	public Texture2D TranslucencyPropsTex;

	private HashSet<Camera> sceneCamsWithBuffer = new HashSet<Camera>();

	private void Start()
	{
		SetupTranslucencyValues();
	}

	public void OnValidate()
	{
		SetupTranslucencyValues();
	}

	public void SetupTranslucencyValues()
	{
		if (TranslucencyPropsTex == null)
		{
			TranslucencyPropsTex = new Texture2D(4, 3, TextureFormat.RGBAFloat, false, true);
			TranslucencyPropsTex.anisoLevel = 0;
			TranslucencyPropsTex.filterMode = FilterMode.Point;
			TranslucencyPropsTex.wrapMode = TextureWrapMode.Clamp;
			TranslucencyPropsTex.hideFlags = HideFlags.HideAndDontSave;
		}
		Shader.SetGlobalTexture("_UBERTranslucencySetup", TranslucencyPropsTex);
		byte[] array = new byte[192];
		EncodeRGBAFloatTo16Bytes(TranslucencyColor1.r, TranslucencyColor1.g, TranslucencyColor1.b, Strength1, array, 0, 0);
		EncodeRGBAFloatTo16Bytes(PointLightsDirectionality1, Constant1, Scattering1, SpotExponent1, array, 0, 1);
		EncodeRGBAFloatTo16Bytes(SuppressShadows1, NdotLReduction1, 1f, 1f, array, 0, 2);
		EncodeRGBAFloatTo16Bytes(TranslucencyColor2.r, TranslucencyColor2.g, TranslucencyColor2.b, Strength2, array, 1, 0);
		EncodeRGBAFloatTo16Bytes(PointLightsDirectionality2, Constant2, Scattering2, SpotExponent2, array, 1, 1);
		EncodeRGBAFloatTo16Bytes(SuppressShadows2, NdotLReduction2, 1f, 1f, array, 1, 2);
		EncodeRGBAFloatTo16Bytes(TranslucencyColor3.r, TranslucencyColor3.g, TranslucencyColor3.b, Strength3, array, 2, 0);
		EncodeRGBAFloatTo16Bytes(PointLightsDirectionality3, Constant3, Scattering3, SpotExponent3, array, 2, 1);
		EncodeRGBAFloatTo16Bytes(SuppressShadows3, NdotLReduction3, 1f, 1f, array, 2, 2);
		EncodeRGBAFloatTo16Bytes(TranslucencyColor4.r, TranslucencyColor4.g, TranslucencyColor4.b, Strength4, array, 3, 0);
		EncodeRGBAFloatTo16Bytes(PointLightsDirectionality4, Constant4, Scattering4, SpotExponent4, array, 3, 1);
		EncodeRGBAFloatTo16Bytes(SuppressShadows4, NdotLReduction4, 1f, 1f, array, 3, 2);
		TranslucencyPropsTex.LoadRawTextureData(array);
		TranslucencyPropsTex.Apply();
	}

	private void EncodeRGBAFloatTo16Bytes(float r, float g, float b, float a, byte[] rawTexdata, int idx_u, int idx_v)
	{
		int num = idx_v * 4 * 16 + idx_u * 16;
		UBER_RGBA_ByteArray uBER_RGBA_ByteArray = new UBER_RGBA_ByteArray
		{
			R = r,
			G = g,
			B = b,
			A = a
		};
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte0;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte1;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte2;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte3;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte4;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte5;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte6;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte7;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte8;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte9;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte10;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte11;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte12;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte13;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte14;
		rawTexdata[num++] = uBER_RGBA_ByteArray.Byte15;
	}

	public void OnEnable()
	{
		SetupTranslucencyValues();
		if (NotifyDecals())
		{
			return;
		}
		if (mycam == null)
		{
			mycam = GetComponent<Camera>();
			if (mycam == null)
			{
				return;
			}
		}
		Initialize();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(SetupCam));
	}

	public void OnDisable()
	{
		NotifyDecals();
		Cleanup();
	}

	public void OnDestroy()
	{
		NotifyDecals();
		Cleanup();
	}

	private bool NotifyDecals()
	{
		Type type = Type.GetType("UBERDecalSystem.DecalManager");
		if (type != null)
		{
			bool flag = false;
			if (UnityEngine.Object.FindObjectOfType(type) != null && UnityEngine.Object.FindObjectOfType(type) is MonoBehaviour && (UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).enabled)
			{
				(UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnDisable", 0f);
				(UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnEnable", 0f);
				return true;
			}
		}
		return false;
	}

	private void Cleanup()
	{
		if ((bool)TranslucencyPropsTex)
		{
			UnityEngine.Object.DestroyImmediate(TranslucencyPropsTex);
			TranslucencyPropsTex = null;
		}
		if (combufPreLight != null)
		{
			if ((bool)mycam)
			{
				mycam.RemoveCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
				mycam.RemoveCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
			}
			foreach (Camera item in sceneCamsWithBuffer)
			{
				if ((bool)item)
				{
					item.RemoveCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
					item.RemoveCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
				}
			}
		}
		sceneCamsWithBuffer.Clear();
		Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(SetupCam));
	}

	private void SetupCam(Camera cam)
	{
		bool flag = false;
		if (cam == mycam || flag)
		{
			RefreshComBufs(cam, flag);
		}
	}

	public void RefreshComBufs(Camera cam, bool isSceneCam)
	{
		if (!cam || combufPreLight == null || combufPostLight == null)
		{
			return;
		}
		CommandBuffer[] commandBuffers = cam.GetCommandBuffers(CameraEvent.BeforeReflections);
		bool flag = false;
		CommandBuffer[] array = commandBuffers;
		foreach (CommandBuffer commandBuffer in array)
		{
			if (commandBuffer.name == combufPreLight.name)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			cam.AddCommandBuffer(CameraEvent.BeforeReflections, combufPreLight);
			cam.AddCommandBuffer(CameraEvent.AfterLighting, combufPostLight);
			if (isSceneCam)
			{
				sceneCamsWithBuffer.Add(cam);
			}
		}
	}

	public void Initialize()
	{
		if (combufPreLight != null)
		{
			return;
		}
		int num = Shader.PropertyToID("_UBERPropsBuffer");
		if (CopyPropsMat == null)
		{
			if (CopyPropsMat != null)
			{
				UnityEngine.Object.DestroyImmediate(CopyPropsMat);
			}
			CopyPropsMat = new Material(Shader.Find("Hidden/UBER_CopyPropsTexture"));
			CopyPropsMat.hideFlags = HideFlags.DontSave;
		}
		combufPreLight = new CommandBuffer();
		combufPreLight.name = "UBERPropsPrelight";
		combufPreLight.GetTemporaryRT(num, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RHalf);
		combufPreLight.Blit(BuiltinRenderTextureType.CameraTarget, num, CopyPropsMat);
		combufPostLight = new CommandBuffer();
		combufPostLight.name = "UBERPropsPostlight";
		combufPostLight.ReleaseTemporaryRT(num);
	}
}
