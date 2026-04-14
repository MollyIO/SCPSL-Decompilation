using System.Runtime.CompilerServices;

namespace UnityEngine.PostProcessing
{
	public class PostProcessingContext
	{
		public Camera camera;

		public MaterialFactory materialFactory;

		public PostProcessingProfile profile;

		public RenderTextureFactory renderTextureFactory;

		public bool interrupted { get; private set; }

		public bool isGBufferAvailable
		{
			[CompilerGenerated]
			get
			{
				return camera.actualRenderingPath == RenderingPath.DeferredShading;
			}
		}

		public bool isHdr
		{
			get
			{
				return camera.allowHDR;
			}
		}

		public int width
		{
			[CompilerGenerated]
			get
			{
				return camera.pixelWidth;
			}
		}

		public int height
		{
			[CompilerGenerated]
			get
			{
				return camera.pixelHeight;
			}
		}

		public Rect viewport
		{
			[CompilerGenerated]
			get
			{
				return camera.rect;
			}
		}

		public void Interrupt()
		{
			interrupted = true;
		}

		public PostProcessingContext Reset()
		{
			profile = null;
			camera = null;
			materialFactory = null;
			renderTextureFactory = null;
			interrupted = false;
			return this;
		}
	}
}
