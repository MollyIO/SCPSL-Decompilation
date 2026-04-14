using System;
using System.Collections.Generic;
using Cryptography;

public class AuthenticatedMessage
{
	public readonly string Message;

	public readonly bool Administrator;

	public AuthenticatedMessage(string m, bool a)
	{
		Message = m;
		Administrator = a;
	}

	public static string GenerateAuthenticatedMessage(string message, long timestamp, string password)
	{
		if (message.Contains(":[:BR:]:"))
		{
			throw new MessageUnallowedCharsException("Message can't contain :[:BR:]:");
		}
		string text = message + ":[:BR:]:" + Convert.ToString(timestamp);
		return text + ":[:BR:]:" + Sha.HashToString(Sha.Sha512Hmac(Sha.Sha512(password), Utf8.GetBytes(text)));
	}

	public static string GenerateNonAuthenticatedMessage(string message)
	{
		if (message.Contains(":[:BR:]:"))
		{
			throw new MessageUnallowedCharsException("Message can't contain :[:BR:]:");
		}
		return message + ":[:BR:]:Guest";
	}

	public static AuthenticatedMessage AuthenticateMessage(string message, long timestamp, string password)
	{
		if (!message.Contains(":[:BR:]:"))
		{
			throw new MessageAuthenticationFailureException("Malformed message.");
		}
		string[] array = message.Split(new string[1] { ":[:BR:]:" }, StringSplitOptions.None);
		if (array.Length < 2 || array.Length > 3)
		{
			throw new MessageAuthenticationFailureException("Malformed message.");
		}
		if (array[1] == "Guest")
		{
			return new AuthenticatedMessage(array[0], false);
		}
		try
		{
			if (!TimeBehaviour.ValidateTimestamp(timestamp, Convert.ToInt64(array[1]), 1200000L))
			{
				throw new MessageExpiredException();
			}
		}
		catch (MessageExpiredException)
		{
			throw new MessageAuthenticationFailureException();
		}
		catch
		{
			throw new MessageAuthenticationFailureException("Malformed message - timestamp can't be converted to long.");
		}
		if (Sha.HashToString(Sha.Sha512Hmac(Sha.Sha512(password), Utf8.GetBytes(array[0] + ":[:BR:]:" + array[1]))) != array[2])
		{
			throw new MessageAuthenticationFailureException("Invalid authentication code.");
		}
		return (!string.IsNullOrEmpty(password) && !(password == "none")) ? new AuthenticatedMessage(array[0], true) : new AuthenticatedMessage(array[0], false);
	}

	public static byte[] Encode(byte[] data)
	{
		byte[] array = new byte[data.Length + 4];
		byte[] bytes = BitConverter.GetBytes(data.Length);
		Array.Reverse((Array)bytes);
		Array.Copy(bytes, 0, array, 0, bytes.Length);
		Array.Copy(data, 0, array, 4, data.Length);
		return array;
	}

	public static List<byte[]> Decode(byte[] data)
	{
		List<byte[]> list = new List<byte[]>();
		while (data.Length > 0)
		{
			byte[] array = new byte[4]
			{
				data[0],
				data[1],
				data[2],
				data[3]
			};
			Array.Reverse((Array)array);
			short num = BitConverter.ToInt16(array, 0);
			if (num == 0)
			{
				break;
			}
			byte[] array2 = new byte[num];
			Array.Copy(data, 4, array2, 0, num);
			list.Add(array2);
			array2 = new byte[data.Length - num - 4];
			Array.Copy(data, num + 4, array2, 0, data.Length - num - 4);
			data = array2;
		}
		return list;
	}
}
