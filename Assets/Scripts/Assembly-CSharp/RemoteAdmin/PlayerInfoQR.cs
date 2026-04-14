using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace RemoteAdmin
{
	internal class PlayerInfoQR : MonoBehaviour
	{
		public RawImage QrDisplay;

		public static PlayerInfoQR Singleton;

		private static BarcodeWriter _barcodeWriter;

		private const int Size = 125;

		public void OnEnable()
		{
			Singleton = this;
			if (_barcodeWriter == null)
			{
				BarcodeWriter barcodeWriter = new BarcodeWriter();
				barcodeWriter.Format = BarcodeFormat.QR_CODE;
				barcodeWriter.Options = new QrCodeEncodingOptions
				{
					Height = 125,
					Width = 125
				};
				_barcodeWriter = barcodeWriter;
			}
		}

		public static void Display(string steamId)
		{
			BitMatrix bitMatrix = _barcodeWriter.Encode(steamId);
			Texture2D texture2D = new Texture2D(125, 125);
			for (int i = 0; i < 125; i++)
			{
				for (int j = 0; j < 125; j++)
				{
					texture2D.SetPixel(i, j, (!bitMatrix[i, j]) ? Color.white : Color.black);
				}
			}
			texture2D.Apply();
			Singleton.QrDisplay.enabled = true;
			Singleton.QrDisplay.texture = texture2D;
		}
	}
}
