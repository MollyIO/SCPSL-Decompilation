using System;
using System.Security.Cryptography;

namespace Cryptography
{
	public class PBKDF2
	{
		public static string Pbkdf2HashString(string password, byte[] salt, int iterations, int outputBytes)
		{
			return Convert.ToBase64String(Pbkdf2HashBytes(password, salt, iterations, outputBytes));
		}

		public static byte[] Pbkdf2HashBytes(string password, byte[] salt, int iterations, int outputBytes)
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt);
			rfc2898DeriveBytes.IterationCount = iterations;
			using (Rfc2898DeriveBytes rfc2898DeriveBytes2 = rfc2898DeriveBytes)
			{
				return rfc2898DeriveBytes2.GetBytes(outputBytes);
			}
		}
	}
}
