using System;
using System.IO;
using System.Text;
using GameConsole;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using UnityEngine;

namespace Cryptography
{
	public class ECDSA
	{
		public static AsymmetricCipherKeyPair GenerateKeys(int size = 384)
		{
			ECKeyPairGenerator eCKeyPairGenerator = new ECKeyPairGenerator("ECDSA");
			SecureRandom random = new SecureRandom();
			KeyGenerationParameters parameters = new KeyGenerationParameters(random, size);
			eCKeyPairGenerator.Init(parameters);
			return eCKeyPairGenerator.GenerateKeyPair();
		}

		public static string Sign(string data, AsymmetricKeyParameter privKey)
		{
			return Convert.ToBase64String(SignBytes(data, privKey));
		}

		public static byte[] SignBytes(string data, AsymmetricKeyParameter privKey)
		{
			try
			{
				return SignBytes(Encoding.UTF8.GetBytes(data), privKey);
			}
			catch
			{
				return null;
			}
		}

		public static byte[] SignBytes(byte[] data, AsymmetricKeyParameter privKey)
		{
			try
			{
				ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
				signer.Init(true, privKey);
				signer.BlockUpdate(data, 0, data.Length);
				return signer.GenerateSignature();
			}
			catch
			{
				return null;
			}
		}

		public static bool Verify(string data, string signature, AsymmetricKeyParameter pubKey)
		{
			return VerifyBytes(data, Convert.FromBase64String(signature), pubKey);
		}

		public static bool VerifyBytes(string data, byte[] signature, AsymmetricKeyParameter pubKey)
		{
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");
				signer.Init(false, pubKey);
				signer.BlockUpdate(bytes, 0, data.Length);
				return signer.VerifySignature(signature);
			}
			catch (Exception ex)
			{
				GameConsole.Console.singleton.AddLog("ECDSA Verification Error (BouncyCastle): " + ex.Message + ", " + ex.StackTrace, Color.red);
				return false;
			}
		}

		public static AsymmetricKeyParameter PublicKeyFromString(string key)
		{
			using (TextReader reader = new StringReader(key))
			{
				return (AsymmetricKeyParameter)new PemReader(reader).ReadObject();
			}
		}

		public static string KeyToString(AsymmetricKeyParameter key)
		{
			using (TextWriter textWriter = new StringWriter())
			{
				PemWriter pemWriter = new PemWriter(textWriter);
				pemWriter.WriteObject(key);
				pemWriter.Writer.Flush();
				return textWriter.ToString();
			}
		}
	}
}
