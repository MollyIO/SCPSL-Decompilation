using System;
using Cryptography;
using GameConsole;
using Org.BouncyCastle.Security;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class GameConsoleTransmission : NetworkBehaviour
{
	public RemoteAdminCryptographicManager CryptoManager;

	public QueryProcessor Processor;

	public GameConsole.Console Console;

	public static SecureRandom SecureRandom;

	private static int kTargetRpcTargetPrintOnConsole;

	private static int kCmdCmdCommandToServer;

	private void Start()
	{
		CryptoManager = GetComponent<RemoteAdminCryptographicManager>();
		Processor = GetComponent<QueryProcessor>();
		if (SecureRandom == null)
		{
			SecureRandom = new SecureRandom();
		}
		if (base.isLocalPlayer)
		{
			Console = GameConsole.Console.singleton;
		}
	}

	[Server]
	public void SendToClient(NetworkConnection connection, string text, string color)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameConsoleTransmission::SendToClient(UnityEngine.Networking.NetworkConnection,System.String,System.String)' called on client");
			return;
		}
		byte[] bytes = Utf8.GetBytes(color + "#" + text);
		if (CryptoManager.EncryptionKey == null)
		{
			CallTargetPrintOnConsole(connection, bytes, false);
		}
		else
		{
			CallTargetPrintOnConsole(connection, AES.AesGcmEncrypt(bytes, CryptoManager.EncryptionKey, SecureRandom), true);
		}
	}

	[TargetRpc(channel = 15)]
	public void TargetPrintOnConsole(NetworkConnection connection, byte[] data, bool encrypted)
	{
		string empty = string.Empty;
		if (!encrypted)
		{
			empty = Utf8.GetString(data);
		}
		else
		{
			if (CryptoManager.EncryptionKey == null)
			{
				Console.AddLog("Can't process encrypted message from server before completing ECDHE exchange.", Color.magenta);
				return;
			}
			try
			{
				byte[] data2 = AES.AesGcmDecrypt(data, CryptoManager.EncryptionKey);
				empty = Utf8.GetString(data2);
			}
			catch
			{
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Decryption or verification of encrypted message failed.", "magenta");
				return;
			}
		}
		string text = empty.Remove(empty.IndexOf("#", StringComparison.Ordinal));
		empty = empty.Remove(0, empty.IndexOf("#", StringComparison.Ordinal) + 1);
		Console.AddLog(((!encrypted) ? "[UNENCRYPTED FROM SERVER] " : "[FROM SERVER] ") + empty, ProcessColor(text));
	}

	[Client]
	public void SendToServer(string command)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void GameConsoleTransmission::SendToServer(System.String)' called on server");
			return;
		}
		byte[] bytes = Utf8.GetBytes(command);
		if (CryptoManager.EncryptionKey == null)
		{
			CallCmdCommandToServer(bytes, false);
		}
		else
		{
			CallCmdCommandToServer(AES.AesGcmEncrypt(bytes, CryptoManager.EncryptionKey, SecureRandom), true);
		}
	}

	[Command(channel = 15)]
	public void CmdCommandToServer(byte[] data, bool encrypted)
	{
		string empty = string.Empty;
		if (!encrypted)
		{
			if (CryptoManager.EncryptionKey != null || CryptoManager.ExchangeRequested)
			{
				SendToClient(base.connectionToClient, "Please use encrypted connection to send commands.", "magenta");
				return;
			}
			empty = Utf8.GetString(data);
		}
		else
		{
			if (CryptoManager.EncryptionKey == null)
			{
				SendToClient(base.connectionToClient, "Can't process encrypted message from server before completing ECDHE exchange.", "magenta");
				return;
			}
			try
			{
				byte[] data2 = AES.AesGcmDecrypt(data, CryptoManager.EncryptionKey);
				empty = Utf8.GetString(data2);
			}
			catch
			{
				SendToClient(base.connectionToClient, "Decryption or verification of encrypted message failed.", "magenta");
				return;
			}
		}
		Processor.ProcessGameConsoleQuery(empty, encrypted);
	}

	public Color ProcessColor(string name)
	{
		Color grey = Color.grey;
		switch (name)
		{
		case "red":
			return Color.red;
		case "cyan":
			return Color.cyan;
		case "blue":
			return Color.blue;
		case "magenta":
			return Color.magenta;
		case "white":
			return Color.white;
		case "green":
			return Color.green;
		case "yellow":
			return Color.yellow;
		case "black":
			return Color.black;
		default:
			return Color.grey;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdCommandToServer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCommandToServer called on client.");
		}
		else
		{
			((GameConsoleTransmission)obj).CmdCommandToServer(reader.ReadBytesAndSize(), reader.ReadBoolean());
		}
	}

	public void CallCmdCommandToServer(byte[] data, bool encrypted)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCommandToServer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCommandToServer(data, encrypted);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCommandToServer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WriteBytesFull(data);
		networkWriter.Write(encrypted);
		SendCommandInternal(networkWriter, 15, "CmdCommandToServer");
	}

	protected static void InvokeRpcTargetPrintOnConsole(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetPrintOnConsole called on server.");
		}
		else
		{
			((GameConsoleTransmission)obj).TargetPrintOnConsole(ClientScene.readyConnection, reader.ReadBytesAndSize(), reader.ReadBoolean());
		}
	}

	public void CallTargetPrintOnConsole(NetworkConnection connection, byte[] data, bool encrypted)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetPrintOnConsole called on client.");
			return;
		}
       if (connection.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetPrintOnConsole called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetPrintOnConsole);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WriteBytesFull(data);
		networkWriter.Write(encrypted);
		SendTargetRPCInternal(connection, networkWriter, 15, "TargetPrintOnConsole");
	}

	static GameConsoleTransmission()
	{
		kCmdCmdCommandToServer = 348220192;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GameConsoleTransmission), kCmdCmdCommandToServer, InvokeCmdCmdCommandToServer);
		kTargetRpcTargetPrintOnConsole = -1796898349;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameConsoleTransmission), kTargetRpcTargetPrintOnConsole, InvokeRpcTargetPrintOnConsole);
		NetworkCRC.RegisterBehaviour("GameConsoleTransmission", 0);
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
