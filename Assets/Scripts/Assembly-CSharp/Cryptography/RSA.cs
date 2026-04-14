using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Cryptography
{
	public class RSA
	{
		public static bool Verify(string data, string signature, string key)
		{
			using (TextReader reader = new StringReader(key))
			{
				PemReader pemReader = new PemReader(reader);
				AsymmetricKeyParameter parameters = (AsymmetricKeyParameter)pemReader.ReadObject();
				ISigner signer = SignerUtilities.GetSigner("SHA256withRSA");
				signer.Init(false, parameters);
				byte[] signature2 = Convert.FromBase64String(signature);
				byte[] bytes = Encoding.UTF8.GetBytes(data);
				signer.BlockUpdate(bytes, 0, bytes.Length);
				return signer.VerifySignature(signature2);
			}
		}
	}
}
