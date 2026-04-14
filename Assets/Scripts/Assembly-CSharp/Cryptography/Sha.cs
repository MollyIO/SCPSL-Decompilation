using System.Security.Cryptography;
using System.Text;

namespace Cryptography
{
	public class Sha
	{
		public static byte[] Sha256(byte[] message)
		{
			using (SHA256 sHA = SHA256.Create())
			{
				return sHA.ComputeHash(message);
			}
		}

		public static byte[] Sha256(string message)
		{
			return Sha256(Utf8.GetBytes(message));
		}

		public static byte[] Sha256Hmac(byte[] key, byte[] message)
		{
			using (HMACSHA256 hMACSHA = new HMACSHA256(key))
			{
				return hMACSHA.ComputeHash(message);
			}
		}

		public static byte[] Sha512(string message)
		{
			return Sha512(Utf8.GetBytes(message));
		}

		public static byte[] Sha512(byte[] message)
		{
			using (SHA512 sHA = SHA512.Create())
			{
				return sHA.ComputeHash(message);
			}
		}

		public static byte[] Sha512Hmac(byte[] key, byte[] message)
		{
			using (HMACSHA512 hMACSHA = new HMACSHA512(key))
			{
				return hMACSHA.ComputeHash(message);
			}
		}

		public static string HashToString(byte[] hash)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in hash)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}
	}
}
