using System.IO;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Cryptography
{
	public class AES
	{
		public const int NonceSizeBytes = 32;

		public const int MacSizeBits = 128;

		public static byte[] AesGcmEncrypt(byte[] data, byte[] secret, SecureRandom secureRandom)
		{
			byte[] array = new byte[32];
			secureRandom.NextBytes(array, 0, array.Length);
			GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
			gcmBlockCipher.Init(true, new AeadParameters(new KeyParameter(secret), 128, array));
			byte[] array2 = new byte[gcmBlockCipher.GetOutputSize(data.Length)];
			int outOff = gcmBlockCipher.ProcessBytes(data, 0, data.Length, array2, 0);
			gcmBlockCipher.DoFinal(array2, outOff);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					binaryWriter.Write(array);
					binaryWriter.Write(array2);
				}
				return memoryStream.ToArray();
			}
		}

		public static byte[] AesGcmDecrypt(byte[] data, byte[] secret)
		{
			using (MemoryStream input = new MemoryStream(data))
			{
				using (BinaryReader binaryReader = new BinaryReader(input))
				{
					byte[] array = binaryReader.ReadBytes(32);
					GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
					gcmBlockCipher.Init(false, new AeadParameters(new KeyParameter(secret), 128, array));
					byte[] array2 = binaryReader.ReadBytes(data.Length - array.Length);
					byte[] array3 = new byte[gcmBlockCipher.GetOutputSize(array2.Length)];
					int outOff = gcmBlockCipher.ProcessBytes(array2, 0, array2.Length, array3, 0);
					gcmBlockCipher.DoFinal(array3, outOff);
					return array3;
				}
			}
		}
	}
}
