using System;
using System.Collections.Generic;
using GameConsole;
using UnityEngine;
using UnityEngine.Networking;

public class Broadcast : NetworkBehaviour
{
	public static Queue<BroadcastMessage> Messages;

	private static int kTargetRpcTargetAddElement;

	private static int kRpcRpcAddElement;

	private static int kTargetRpcTargetClearElements;

	private static int kRpcRpcClearElements;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			Messages = new Queue<BroadcastMessage>();
		}
	}

	[TargetRpc(channel = 2)]
	public void TargetAddElement(NetworkConnection conn, string data, uint time, bool monospaced)
	{
		AddElement(data, time, monospaced);
	}

	[ClientRpc(channel = 2)]
	public void RpcAddElement(string data, uint time, bool monospaced)
	{
		AddElement(data, time, monospaced);
	}

	[TargetRpc(channel = 2)]
	public void TargetClearElements(NetworkConnection conn)
	{
		Messages.Clear();
		BroadcastAssigner.MessageDisplayed = false;
	}

	[ClientRpc(channel = 2)]
	public void RpcClearElements()
	{
		Messages.Clear();
		BroadcastAssigner.MessageDisplayed = false;
	}

	public static void AddElement(string data, uint time, bool monospaced)
	{
		if (time >= 1 && Messages.Count <= 25 && !string.IsNullOrEmpty(data) && data.Length <= 3072)
		{
			if (time > 300)
			{
				time = 10u;
			}
			Messages.Enqueue(new BroadcastMessage(data.Replace("\\n", Environment.NewLine), time, monospaced));
			BroadcastAssigner.Displaying = true;
			if (GameConsole.Console.singleton != null)
			{
				GameConsole.Console.singleton.AddLog("[BROADCAST FROM SERVER] " + data.Replace("<", "[").Replace(">", "]") + ", time: " + time + ", monospace: " + ((!monospaced) ? "NO" : "YES"), Color.grey);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcAddElement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddElement called on server.");
		}
		else
		{
			((Broadcast)obj).RpcAddElement(reader.ReadString(), reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcClearElements(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearElements called on server.");
		}
		else
		{
			((Broadcast)obj).RpcClearElements();
		}
	}

	protected static void InvokeRpcTargetAddElement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetAddElement called on server.");
		}
		else
		{
			((Broadcast)obj).TargetAddElement(ClientScene.readyConnection, reader.ReadString(), reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcTargetClearElements(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetClearElements called on server.");
		}
		else
		{
			((Broadcast)obj).TargetClearElements(ClientScene.readyConnection);
		}
	}

	public void CallRpcAddElement(string data, uint time, bool monospaced)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddElement called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddElement);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(data);
		networkWriter.WritePackedUInt32(time);
		networkWriter.Write(monospaced);
		SendRPCInternal(networkWriter, 2, "RpcAddElement");
	}

	public void CallRpcClearElements()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcClearElements called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcClearElements);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 2, "RpcClearElements");
	}

	public void CallTargetAddElement(NetworkConnection conn, string data, uint time, bool monospaced)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetAddElement called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetAddElement called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetAddElement);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(data);
		networkWriter.WritePackedUInt32(time);
		networkWriter.Write(monospaced);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetAddElement");
	}

	public void CallTargetClearElements(NetworkConnection conn)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetClearElements called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetClearElements called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetClearElements);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetClearElements");
	}

	static Broadcast()
	{
		kRpcRpcAddElement = 1176408882;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Broadcast), kRpcRpcAddElement, InvokeRpcRpcAddElement);
		kRpcRpcClearElements = -1878583571;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Broadcast), kRpcRpcClearElements, InvokeRpcRpcClearElements);
		kTargetRpcTargetAddElement = -841206165;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Broadcast), kTargetRpcTargetAddElement, InvokeRpcTargetAddElement);
		kTargetRpcTargetClearElements = -581141228;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Broadcast), kTargetRpcTargetClearElements, InvokeRpcTargetClearElements);
		NetworkCRC.RegisterBehaviour("Broadcast", 0);
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
