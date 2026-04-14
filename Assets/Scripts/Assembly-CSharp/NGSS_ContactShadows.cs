using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class NGSS_ContactShadows : MonoBehaviour
{
	public Light mainDirectionalLight;

	public Shader contactShadowsShader;

	public bool noiseFilter;

	[Range(0f, 3f)]
	public float shadowsSoftness = 1f;

	[Range(1f, 4f)]
	public float shadowsDistance = 2f;

	[Range(0.1f, 4f)]
	public float shadowsFade = 1f;

	[Range(0f, 0.02f)]
	public float shadowsBias = 0.0065f;

	[Range(0f, 1f)]
	public float rayWidth = 0.1f;

	[Range(16f, 128f)]
	public int raySamples = 64;

	private CommandBuffer blendShadowsCB;

	private CommandBuffer computeShadowsCB;

	private bool isInitialized;

	private Camera _mCamera;

	private Material _mMaterial;

	private Camera mCamera
	{
		get
		{
			if (_mCamera == null)
			{
				_mCamera = GetComponent<Camera>();
				if (_mCamera == null)
				{
					_mCamera = Camera.main;
				}
				if (_mCamera == null)
				{
					Debug.LogError("NGSS Error: No MainCamera found, please provide one.", this);
				}
				else
				{
					_mCamera.depthTextureMode |= DepthTextureMode.Depth;
				}
			}
			return _mCamera;
		}
	}

	private Material mMaterial
	{
		get
		{
			if (_mMaterial == null)
			{
				if (contactShadowsShader == null)
				{
					Shader.Find("Hidden/NGSS_ContactShadows");
				}
				_mMaterial = new Material(contactShadowsShader);
				if (_mMaterial == null)
				{
					Debug.LogWarning("NGSS Warning: can't find NGSS_ContactShadows shader, make sure it's on your project.", this);
					base.enabled = false;
					return null;
				}
			}
			return _mMaterial;
		}
	}

	private void AddCommandBuffers()
	{
		computeShadowsCB = new CommandBuffer
		{
			name = "NGSS ContactShadows: Compute"
		};
		blendShadowsCB = new CommandBuffer
		{
			name = "NGSS ContactShadows: Mix"
		};
		bool flag = mCamera.actualRenderingPath == RenderingPath.Forward;
		if ((bool)mCamera)
		{
			CommandBuffer[] commandBuffers = mCamera.GetCommandBuffers(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting);
			foreach (CommandBuffer commandBuffer in commandBuffers)
			{
				if (commandBuffer.name == computeShadowsCB.name)
				{
					return;
				}
			}
			mCamera.AddCommandBuffer(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, computeShadowsCB);
		}
		if (!mainDirectionalLight)
		{
			return;
		}
		CommandBuffer[] commandBuffers2 = mainDirectionalLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask);
		foreach (CommandBuffer commandBuffer2 in commandBuffers2)
		{
			if (commandBuffer2.name == blendShadowsCB.name)
			{
				return;
			}
		}
		mainDirectionalLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, blendShadowsCB);
	}

	private void RemoveCommandBuffers()
	{
		_mMaterial = null;
		bool flag = mCamera.actualRenderingPath == RenderingPath.Forward;
		if ((bool)mCamera)
		{
			mCamera.RemoveCommandBuffer(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, computeShadowsCB);
		}
		if ((bool)mainDirectionalLight)
		{
			mainDirectionalLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, blendShadowsCB);
		}
		isInitialized = false;
	}

	private void Init()
	{
		if (!isInitialized && !(mainDirectionalLight == null))
		{
			if (mCamera.renderingPath == RenderingPath.UsePlayerSettings || mCamera.renderingPath == RenderingPath.VertexLit)
			{
				Debug.LogWarning("Please set your camera rendering path to either Forward or Deferred and re-enable this component.", this);
				base.enabled = false;
				return;
			}
			AddCommandBuffers();
			int num = Shader.PropertyToID("NGSS_ContactShadowRT");
			int num2 = Shader.PropertyToID("NGSS_DepthSourceRT");
			computeShadowsCB.GetTemporaryRT(num, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
			computeShadowsCB.GetTemporaryRT(num2, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
			computeShadowsCB.Blit(num, num2, mMaterial, 0);
			computeShadowsCB.Blit(num2, num, mMaterial, 1);
			computeShadowsCB.Blit(num, num2, mMaterial, 2);
			blendShadowsCB.Blit(BuiltinRenderTextureType.None, BuiltinRenderTextureType.CurrentActive, mMaterial, 3);
			computeShadowsCB.SetGlobalTexture("NGSS_ContactShadowsTexture", num2);
			isInitialized = true;
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		if (isInitialized)
		{
			RemoveCommandBuffers();
		}
	}

	private void OnApplicationQuit()
	{
		if (isInitialized)
		{
			RemoveCommandBuffers();
		}
	}

	private void OnPreRender()
	{
		Init();
		if (isInitialized && !(mainDirectionalLight == null))
		{
			mMaterial.SetVector("LightDir", mCamera.transform.InverseTransformDirection(mainDirectionalLight.transform.forward));
			mMaterial.SetFloat("ShadowsSoftness", shadowsSoftness);
			mMaterial.SetFloat("ShadowsDistance", shadowsDistance);
			mMaterial.SetFloat("ShadowsFade", shadowsFade);
			mMaterial.SetFloat("ShadowsBias", shadowsBias);
			mMaterial.SetFloat("RayWidth", rayWidth);
			mMaterial.SetInt("RaySamples", raySamples);
			if (noiseFilter)
			{
				mMaterial.EnableKeyword("NGSS_CONTACT_SHADOWS_USE_NOISE");
			}
			else
			{
				mMaterial.DisableKeyword("NGSS_CONTACT_SHADOWS_USE_NOISE");
			}
		}
	}
}
