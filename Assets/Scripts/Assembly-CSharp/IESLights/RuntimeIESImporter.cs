using System;
using System.IO;
using UnityEngine;

namespace IESLights
{
	public class RuntimeIESImporter : MonoBehaviour
	{
		public static void Import(string path, out Texture2D spotlightCookie, out Cubemap pointLightCookie, int resolution = 128, bool enhancedImport = false, bool applyVignette = true)
		{
			spotlightCookie = null;
			pointLightCookie = null;
			if (IsFileValid(path))
			{
				GameObject cubemapSphere;
				IESConverter iesConverter;
				GetIESConverterAndCubeSphere(enhancedImport, resolution, out cubemapSphere, out iesConverter);
				ImportIES(path, iesConverter, true, applyVignette, out spotlightCookie, out pointLightCookie);
				UnityEngine.Object.Destroy(cubemapSphere);
			}
		}

		public static Texture2D ImportSpotlightCookie(string path, int resolution = 128, bool enhancedImport = false, bool applyVignette = true)
		{
			if (!IsFileValid(path))
			{
				return null;
			}
			GameObject cubemapSphere;
			IESConverter iesConverter;
			GetIESConverterAndCubeSphere(enhancedImport, resolution, out cubemapSphere, out iesConverter);
			Texture2D spotlightCookie;
			Cubemap pointlightCookie;
			ImportIES(path, iesConverter, true, applyVignette, out spotlightCookie, out pointlightCookie);
			UnityEngine.Object.Destroy(cubemapSphere);
			return spotlightCookie;
		}

		public static Cubemap ImportPointLightCookie(string path, int resolution = 128, bool enhancedImport = false)
		{
			if (!IsFileValid(path))
			{
				return null;
			}
			GameObject cubemapSphere;
			IESConverter iesConverter;
			GetIESConverterAndCubeSphere(enhancedImport, resolution, out cubemapSphere, out iesConverter);
			Texture2D spotlightCookie;
			Cubemap pointlightCookie;
			ImportIES(path, iesConverter, false, false, out spotlightCookie, out pointlightCookie);
			UnityEngine.Object.Destroy(cubemapSphere);
			return pointlightCookie;
		}

		private static void GetIESConverterAndCubeSphere(bool logarithmicNormalization, int resolution, out GameObject cubemapSphere, out IESConverter iesConverter)
		{
			UnityEngine.Object original = Resources.Load("IES cubemap sphere");
			cubemapSphere = (GameObject)UnityEngine.Object.Instantiate(original);
			iesConverter = cubemapSphere.GetComponent<IESConverter>();
			iesConverter.NormalizationMode = (logarithmicNormalization ? NormalizationMode.Logarithmic : NormalizationMode.Linear);
			iesConverter.Resolution = resolution;
		}

		private static void ImportIES(string path, IESConverter iesConverter, bool allowSpotlightCookies, bool applyVignette, out Texture2D spotlightCookie, out Cubemap pointlightCookie)
		{
			string targetFilename = null;
			spotlightCookie = null;
			pointlightCookie = null;
			try
			{
				EXRData exrData;
				iesConverter.ConvertIES(path, string.Empty, allowSpotlightCookies, false, applyVignette, out pointlightCookie, out spotlightCookie, out exrData, out targetFilename);
			}
			catch (IESParseException ex)
			{
				Debug.LogError(string.Format("[IES] Encountered invalid IES data in {0}. Error message: {1}", path, ex.Message));
			}
			catch (Exception ex2)
			{
				Debug.LogError(string.Format("[IES] Error while parsing {0}. Please contact me through the forums or thomasmountainborn.com. Error message: {1}", path, ex2.Message));
			}
		}

		private static bool IsFileValid(string path)
		{
			if (!File.Exists(path))
			{
				Debug.LogWarningFormat("[IES] The file \"{0}\" does not exist.", path);
				return false;
			}
			if (Path.GetExtension(path).ToLower() != ".ies")
			{
				Debug.LogWarningFormat("[IES] The file \"{0}\" is not an IES file.", path);
				return false;
			}
			return true;
		}
	}
}
