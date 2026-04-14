using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace RemoteAdmin
{
	public class LargeDataPrinter : MonoBehaviour
	{
		private const int Size = 500;

		internal static LargeDataPrinter Singleton;

		private static BarcodeWriter _barcodeWriter;

		public TextMeshProUGUI Header;

		public TextMeshProUGUI Content;

		public GameObject Panel;

		public RawImage QrDisplay;

		public void OnEnable()
		{
			Singleton = this;
			if (_barcodeWriter == null)
			{
				BarcodeWriter barcodeWriter = new BarcodeWriter();
				barcodeWriter.Format = BarcodeFormat.QR_CODE;
				barcodeWriter.Options = new QrCodeEncodingOptions
				{
					Height = 500,
					Width = 500
				};
				_barcodeWriter = barcodeWriter;
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape) && Panel.activeSelf)
			{
				Panel.SetActive(false);
			}
		}

		public static void Display(string content, bool replaceToBr)
		{
			BitMatrix bitMatrix = _barcodeWriter.Encode((!replaceToBr) ? content : content.Replace("\n", "<br>"));
			Texture2D texture2D = new Texture2D(500, 500, TextureFormat.RGBA32, false);
			for (int i = 0; i < 500; i++)
			{
				for (int j = 0; j < 500; j++)
				{
					texture2D.SetPixel(i, j, (!bitMatrix[i, j]) ? Color.white : Color.black);
				}
			}
			texture2D.Apply();
			Singleton.Panel.SetActive(true);
			Singleton.Content.SetText(content);
			Singleton.QrDisplay.texture = texture2D;
		}

		public static void Hide()
		{
			Singleton.Panel.SetActive(false);
		}
	}
}
