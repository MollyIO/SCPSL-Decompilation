using Cryptography;
using GameConsole;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using UnityEngine;
using UnityEngine.Networking;

namespace RemoteAdmin
{
	public class RemoteAdminCryptographicManager : NetworkBehaviour
	{
		internal AsymmetricCipherKeyPair EcdhKeys;

		internal ECDHBasicAgreement Exchange;

		internal byte[] EcdhPublicKeySignature;

		internal bool ExchangeRequested;

		internal byte[] EncryptionKey;

		private static int kTargetRpcTargetDiffieHellmanExchange;

		private static int kCmdCmdDiffieHellmanExchange;

		public void Init()
		{
			EcdhKeys = ECDH.GenerateKeys();
			Exchange = ECDH.Init(EcdhKeys);
			ExchangeRequested = true;
		}

		[Server]
		public void StartExchange()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RemoteAdmin.RemoteAdminCryptographicManager::StartExchange()' called on client");
				return;
			}
			if (Exchange == null || EcdhKeys == null)
			{
				Init();
			}
			CallTargetDiffieHellmanExchange(base.connectionToClient, ECDSA.KeyToString(EcdhKeys.Public));
		}

		[TargetRpc(channel = 2)]
		public void TargetDiffieHellmanExchange(NetworkConnection conn, string publicKey)
		{
			if (EncryptionKey != null)
			{
				Console.singleton.AddLog("Rejected duplicated Elliptic-curve Diffie–Hellman (ECDH) parameters from server.", Color.magenta);
				return;
			}
			if (Exchange == null || EcdhKeys == null)
			{
				Init();
			}
			if (EcdhPublicKeySignature == null)
			{
				EcdhPublicKeySignature = ECDSA.SignBytes(ECDSA.KeyToString(EcdhKeys.Public), Console.SessionKeys.Private);
			}
			EncryptionKey = ECDH.DeriveKey(Exchange, ECDSA.PublicKeyFromString(publicKey));
			CallCmdDiffieHellmanExchange(ECDSA.KeyToString(EcdhKeys.Public), EcdhPublicKeySignature);
			Console.singleton.AddLog("Completed ECDHE exchange with server.", Color.grey);
		}

		[Command(channel = 2)]
		public void CmdDiffieHellmanExchange(string publicKey, byte[] signature)
		{
			if (EncryptionKey == null && Exchange != null && EcdhKeys != null)
			{
				bool onlineMode = GetComponent<CharacterClassManager>().OnlineMode;
				AsymmetricKeyParameter publicKey2 = GetComponent<ServerRoles>().PublicKey;
				string authToken = GetComponent<CharacterClassManager>().AuthToken;
				if (onlineMode && (publicKey == null || authToken == null))
				{
					GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Please complete authentication before requesting ECDHE exchange.", "magenta");
				}
				else if (onlineMode && publicKey2 != null && !ECDSA.VerifyBytes(publicKey, signature, publicKey2))
				{
					GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Exchange parameters signature is invalid!", "magenta");
				}
				else
				{
					EncryptionKey = ECDH.DeriveKey(Exchange, ECDSA.PublicKeyFromString(publicKey));
				}
			}
		}

		private void UNetVersion()
		{
		}

		protected static void InvokeCmdCmdDiffieHellmanExchange(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdDiffieHellmanExchange called on client.");
			}
			else
			{
				((RemoteAdminCryptographicManager)obj).CmdDiffieHellmanExchange(reader.ReadString(), reader.ReadBytesAndSize());
			}
		}

		public void CallCmdDiffieHellmanExchange(string publicKey, byte[] signature)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command function CmdDiffieHellmanExchange called on server.");
				return;
			}
			if (base.isServer)
			{
				CmdDiffieHellmanExchange(publicKey, signature);
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)5);
			networkWriter.WritePackedUInt32((uint)kCmdCmdDiffieHellmanExchange);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(publicKey);
			networkWriter.WriteBytesFull(signature);
			SendCommandInternal(networkWriter, 2, "CmdDiffieHellmanExchange");
		}

		protected static void InvokeRpcTargetDiffieHellmanExchange(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("TargetRPC TargetDiffieHellmanExchange called on server.");
			}
			else
			{
				((RemoteAdminCryptographicManager)obj).TargetDiffieHellmanExchange(ClientScene.readyConnection, reader.ReadString());
			}
		}

		public void CallTargetDiffieHellmanExchange(NetworkConnection conn, string publicKey)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function TargetDiffieHellmanExchange called on client.");
				return;
			}
           if (conn.connectionId == 0 && !NetworkServer.localClientActive)
			{
				Debug.LogError("TargetRPC Function TargetDiffieHellmanExchange called on connection to server");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kTargetRpcTargetDiffieHellmanExchange);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(publicKey);
			SendTargetRPCInternal(conn, networkWriter, 2, "TargetDiffieHellmanExchange");
		}

		static RemoteAdminCryptographicManager()
		{
			kCmdCmdDiffieHellmanExchange = 1617176144;
			NetworkBehaviour.RegisterCommandDelegate(typeof(RemoteAdminCryptographicManager), kCmdCmdDiffieHellmanExchange, InvokeCmdCmdDiffieHellmanExchange);
			kTargetRpcTargetDiffieHellmanExchange = 692156029;
			NetworkBehaviour.RegisterRpcDelegate(typeof(RemoteAdminCryptographicManager), kTargetRpcTargetDiffieHellmanExchange, InvokeRpcTargetDiffieHellmanExchange);
			NetworkCRC.RegisterBehaviour("RemoteAdminCryptographicManager", 0);
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			bool result = default(bool);
			return result;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
		}
	}
}
