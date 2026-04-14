using System.IO;
using System.Linq;
using UnityEngine;

namespace IESLights
{
	[RequireComponent(typeof(IESToCubemap))]
	[RequireComponent(typeof(IESToSpotlightCookie))]
	public class IESConverter : MonoBehaviour
	{
		public int Resolution = 512;

		public NormalizationMode NormalizationMode;

		private Texture2D _iesTexture;

		public void ConvertIES(string filePath, string targetPath, bool createSpotlightCookies, bool rawImport, bool applyVignette, out Cubemap pointLightCookie, out Texture2D spotlightCookie, out EXRData exrData, out string targetFilename)
		{
			IESData iESData = ParseIES.Parse(filePath, (!rawImport) ? NormalizationMode : NormalizationMode.Linear);
			_iesTexture = IESToTexture.ConvertIesData(iESData);
			if (!rawImport)
			{
				exrData = default(EXRData);
				RegularImport(filePath, targetPath, createSpotlightCookies, applyVignette, out pointLightCookie, out spotlightCookie, out targetFilename, iESData);
			}
			else
			{
				pointLightCookie = null;
				spotlightCookie = null;
				RawImport(iESData, filePath, targetPath, createSpotlightCookies, out exrData, out targetFilename);
			}
			if (_iesTexture != null)
			{
				Object.Destroy(_iesTexture);
			}
		}

		private void RegularImport(string filePath, string targetPath, bool createSpotlightCookies, bool applyVignette, out Cubemap pointLightCookie, out Texture2D spotlightCookie, out string targetFilename, IESData iesData)
		{
			if ((createSpotlightCookies && iesData.VerticalType != VerticalType.Full) || iesData.PhotometricType == PhotometricType.TypeA)
			{
				pointLightCookie = null;
				GetComponent<IESToSpotlightCookie>().CreateSpotlightCookie(_iesTexture, iesData, Resolution, applyVignette, false, out spotlightCookie);
			}
			else
			{
				spotlightCookie = null;
				GetComponent<IESToCubemap>().CreateCubemap(_iesTexture, iesData, Resolution, out pointLightCookie);
			}
			BuildTargetFilename(Path.GetFileNameWithoutExtension(filePath), targetPath, pointLightCookie != null, false, NormalizationMode, iesData, out targetFilename);
		}

		private void RawImport(IESData iesData, string filePath, string targetPath, bool createSpotlightCookie, out EXRData exrData, out string targetFilename)
		{
			if ((createSpotlightCookie && iesData.VerticalType != VerticalType.Full) || iesData.PhotometricType == PhotometricType.TypeA)
			{
				Texture2D cookie = null;
				GetComponent<IESToSpotlightCookie>().CreateSpotlightCookie(_iesTexture, iesData, Resolution, false, true, out cookie);
				exrData = new EXRData(cookie.GetPixels(), Resolution, Resolution);
				Object.DestroyImmediate(cookie);
			}
			else
			{
				exrData = new EXRData(GetComponent<IESToCubemap>().CreateRawCubemap(_iesTexture, iesData, Resolution), Resolution * 6, Resolution);
			}
			BuildTargetFilename(Path.GetFileNameWithoutExtension(filePath), targetPath, false, true, NormalizationMode.Linear, iesData, out targetFilename);
		}

		private void BuildTargetFilename(string name, string folderHierarchy, bool isCubemap, bool isRaw, NormalizationMode normalizationMode, IESData iesData, out string targetFilePath)
		{
			if (!Directory.Exists(Path.Combine(Application.dataPath, string.Format("IES/Imports/{0}", folderHierarchy))))
			{
				Directory.CreateDirectory(Path.Combine(Application.dataPath, string.Format("IES/Imports/{0}", folderHierarchy)));
			}
			float num = 0f;
			if (iesData.PhotometricType == PhotometricType.TypeA)
			{
				num = iesData.HorizontalAngles.Max() - iesData.HorizontalAngles.Min();
			}
			string text = string.Empty;
			switch (normalizationMode)
			{
			case NormalizationMode.EqualizeHistogram:
				text = "[H] ";
				break;
			case NormalizationMode.Logarithmic:
				text = "[E] ";
				break;
			}
			string empty = string.Empty;
			empty = ((!isRaw) ? ((!isCubemap) ? "asset" : "cubemap") : "exr");
			targetFilePath = Path.Combine(Path.Combine("Assets/IES/Imports/", folderHierarchy), string.Format("{0}{1}{2}.{3}", text, (iesData.PhotometricType != PhotometricType.TypeA) ? string.Empty : ("[FOV " + num + "] "), name, empty));
			if (File.Exists(targetFilePath))
			{
				File.Delete(targetFilePath);
			}
		}
	}
}
