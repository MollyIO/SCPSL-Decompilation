using System;
using System.IO;
using UnityEngine;

namespace MiniEXR
{
	public static class MiniEXR
	{
		public static void MiniEXRWrite(string _filePath, uint _width, uint _height, uint _channels, float[] _rgbaArray)
		{
			File.WriteAllBytes(_filePath, MiniEXRWrite(_width, _height, _channels, _rgbaArray));
		}

		public static void MiniEXRWrite(string _filePath, uint _width, uint _height, Color[] _colorArray)
		{
			File.WriteAllBytes(_filePath, MiniEXRWrite(_width, _height, _colorArray));
		}

		public static byte[] MiniEXRWrite(uint _width, uint _height, Color[] _colorArray)
		{
			float[] array = new float[_colorArray.Length * 3];
			for (int i = 0; i < _colorArray.Length; i++)
			{
				array[i * 3] = _colorArray[i].r;
				array[i * 3 + 1] = _colorArray[i].g;
				array[i * 3 + 2] = _colorArray[i].b;
			}
			return MiniEXRWrite(_width, _height, 3u, array);
		}

		public static byte[] MiniEXRWrite(uint _width, uint _height, uint _channels, float[] _rgbaArray)
		{
			uint num = _width - 1;
			uint num2 = _height - 1;
			byte[] obj = new byte[313]
			{
				118, 47, 49, 1, 2, 0, 0, 0, 99, 104,
				97, 110, 110, 101, 108, 115, 0, 99, 104, 108,
				105, 115, 116, 0, 55, 0, 0, 0, 66, 0,
				1, 0, 0, 0, 0, 0, 0, 0, 1, 0,
				0, 0, 1, 0, 0, 0, 71, 0, 1, 0,
				0, 0, 0, 0, 0, 0, 1, 0, 0, 0,
				1, 0, 0, 0, 82, 0, 1, 0, 0, 0,
				0, 0, 0, 0, 1, 0, 0, 0, 1, 0,
				0, 0, 0, 99, 111, 109, 112, 114, 101, 115,
				115, 105, 111, 110, 0, 99, 111, 109, 112, 114,
				101, 115, 115, 105, 111, 110, 0, 1, 0, 0,
				0, 0, 100, 97, 116, 97, 87, 105, 110, 100,
				111, 119, 0, 98, 111, 120, 50, 105, 0, 16,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 100,
				105, 115, 112, 108, 97, 121, 87, 105, 110, 100,
				111, 119, 0, 98, 111, 120, 50, 105, 0, 16,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 108,
				105, 110, 101, 79, 114, 100, 101, 114, 0, 108,
				105, 110, 101, 79, 114, 100, 101, 114, 0, 1,
				0, 0, 0, 0, 112, 105, 120, 101, 108, 65,
				115, 112, 101, 99, 116, 82, 97, 116, 105, 111,
				0, 102, 108, 111, 97, 116, 0, 4, 0, 0,
				0, 0, 0, 128, 63, 115, 99, 114, 101, 101,
				110, 87, 105, 110, 100, 111, 119, 67, 101, 110,
				116, 101, 114, 0, 118, 50, 102, 0, 8, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				115, 99, 114, 101, 101, 110, 87, 105, 110, 100,
				111, 119, 87, 105, 100, 116, 104, 0, 102, 108,
				111, 97, 116, 0, 4, 0, 0, 0, 0, 0,
				128, 63, 0
			};
			obj[141] = (byte)(num & 0xFF);
			obj[142] = (byte)((num >> 8) & 0xFF);
			obj[143] = (byte)((num >> 16) & 0xFF);
			obj[144] = (byte)((num >> 24) & 0xFF);
			obj[145] = (byte)(num2 & 0xFF);
			obj[146] = (byte)((num2 >> 8) & 0xFF);
			obj[147] = (byte)((num2 >> 16) & 0xFF);
			obj[148] = (byte)((num2 >> 24) & 0xFF);
			obj[181] = (byte)(num & 0xFF);
			obj[182] = (byte)((num >> 8) & 0xFF);
			obj[183] = (byte)((num >> 16) & 0xFF);
			obj[184] = (byte)((num >> 24) & 0xFF);
			obj[185] = (byte)(num2 & 0xFF);
			obj[186] = (byte)((num2 >> 8) & 0xFF);
			obj[187] = (byte)((num2 >> 16) & 0xFF);
			obj[188] = (byte)((num2 >> 24) & 0xFF);
			byte[] array = obj;
			uint num3 = (uint)array.Length;
			uint num4 = 8 * _height;
			uint num5 = _width * 3 * 2;
			uint num6 = num5 + 8;
			uint num7 = num3 + num4 + _height * num6;
			byte[] array2 = new byte[num7];
			int num8 = 0;
			for (int i = 0; i < num3; i++)
			{
				array2[num8] = array[i];
				num8++;
			}
			uint num9 = num3 + num4;
			for (int j = 0; j < _height; j++)
			{
				array2[num8++] = (byte)(num9 & 0xFF);
				array2[num8++] = (byte)((num9 >> 8) & 0xFF);
				array2[num8++] = (byte)((num9 >> 16) & 0xFF);
				array2[num8++] = (byte)((num9 >> 24) & 0xFF);
				array2[num8++] = 0;
				array2[num8++] = 0;
				array2[num8++] = 0;
				array2[num8++] = 0;
				num9 += num6;
			}
			ushort[] array3 = new ushort[_rgbaArray.Length];
			for (int k = 0; k < _rgbaArray.Length; k++)
			{
				_rgbaArray[k] = Mathf.Pow(_rgbaArray[k], 2.2f);
				array3[k] = HalfHelper.SingleToHalf(_rgbaArray[k]);
			}
			uint num10 = 0u;
			for (int l = 0; l < _height; l++)
			{
				array2[num8++] = (byte)(l & 0xFF);
				array2[num8++] = (byte)((l >> 8) & 0xFF);
				array2[num8++] = (byte)((l >> 16) & 0xFF);
				array2[num8++] = (byte)((l >> 24) & 0xFF);
				array2[num8++] = (byte)(num5 & 0xFF);
				array2[num8++] = (byte)((num5 >> 8) & 0xFF);
				array2[num8++] = (byte)((num5 >> 16) & 0xFF);
				array2[num8++] = (byte)((num5 >> 24) & 0xFF);
				uint num11 = num10;
				for (int m = 0; m < _width; m++)
				{
					byte[] bytes = BitConverter.GetBytes(array3[num11 + 2]);
					array2[num8++] = bytes[0];
					array2[num8++] = bytes[1];
					num11 += _channels;
				}
				num11 = num10;
				for (int n = 0; n < _width; n++)
				{
					byte[] bytes2 = BitConverter.GetBytes(array3[num11 + 1]);
					array2[num8++] = bytes2[0];
					array2[num8++] = bytes2[1];
					num11 += _channels;
				}
				num11 = num10;
				for (int num12 = 0; num12 < _width; num12++)
				{
					byte[] bytes3 = BitConverter.GetBytes(array3[num11]);
					array2[num8++] = bytes3[0];
					array2[num8++] = bytes3[1];
					num11 += _channels;
				}
				num10 += _width * _channels;
			}
			return array2;
		}
	}
}
