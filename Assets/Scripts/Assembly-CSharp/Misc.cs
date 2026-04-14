using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

public class Misc
{
	public static string LeadingZeroes(int integer, uint len, bool plusSign = false)
	{
		bool flag = integer < 0;
		if (flag)
		{
			integer *= -1;
		}
		string text = integer.ToString();
		while (text.Length < len)
		{
			text = "0" + text;
		}
		return (flag ? "-" : ((!plusSign) ? string.Empty : "+")) + text;
	}

	public static void ShuffleList<T>(IList<T> list)
	{
		Random random = new Random();
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = random.Next(num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static void ShuffleListSecure<T>(IList<T> list)
	{
		using (RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider())
		{
			int num = list.Count;
			while (num > 1)
			{
				byte[] array = new byte[1];
				do
				{
					rNGCryptoServiceProvider.GetBytes(array);
				}
				while (array[0] >= num * (255 / num));
				int index = array[0] % num;
				num--;
				T value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
		}
	}

	public static string RemoveSpecialCharacters(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in str)
		{
			if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ' || c == '-' || c == '.' || c == ',' || c == '_')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string Base64Encode(string plainText)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(bytes);
	}

	public static string Base64Decode(string base64EncodedData)
	{
		byte[] bytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(bytes);
	}

	public static string GetRuntimeVersion()
	{
		try
		{
			return RuntimeInformation.FrameworkDescription;
		}
		catch
		{
			return "Not supported!";
		}
	}
}
