using System.Collections.Generic;
using GameConsole;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class MarkupTransceiver : NetworkBehaviour
{
	private static int kTargetRpcTargetRpcDownloadStyle;

	private static int kTargetRpcTargetRpcReceiveData;

	[ServerCallback]
	public void Transmit(string code, int[] playerIDs)
	{
		if (NetworkServer.active)
		{
			NetworkConnection[] targets = GetTargets(playerIDs);
			foreach (NetworkConnection target in targets)
			{
				CallTargetRpcReceiveData(target, code);
			}
		}
	}

	[ServerCallback]
	public void RequestStyleDownload(string url, int[] playerIDs)
	{
		if (NetworkServer.active)
		{
			NetworkConnection[] targets = GetTargets(playerIDs);
			foreach (NetworkConnection conn in targets)
			{
				CallTargetRpcDownloadStyle(conn, url);
			}
		}
	}

	[TargetRpc]
	private void TargetRpcDownloadStyle(NetworkConnection conn, string url)
	{
		if (Console.DisableSLML || Console.DisableRemoteSLML)
		{
			Console.singleton.AddLog("Rejected REMOTE SLML from the game server - disabled by user.", Color.gray);
		}
		else
		{
			MarkupReader.singleton.AddStyleFromURL(url);
		}
	}

	public NetworkConnection[] GetTargets(int[] playerIDs)
	{
		List<NetworkConnection> list = new List<NetworkConnection>();
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			QueryProcessor component = gameObject.GetComponent<QueryProcessor>();
			foreach (int num in playerIDs)
			{
				if (component.PlayerId == num)
				{
					list.Add(component.connectionToClient);
				}
			}
		}
		return list.ToArray();
	}

	[TargetRpc]
	private void TargetRpcReceiveData(NetworkConnection target, string code)
	{
		if (Console.DisableSLML)
		{
			Console.singleton.AddLog("Rejected SLML from the game server - disabled by user.", Color.gray);
		}
		else
		{
			MarkupWriter.singleton.ReadTag(code);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcTargetRpcDownloadStyle(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRpcDownloadStyle called on server.");
		}
		else
		{
			((MarkupTransceiver)obj).TargetRpcDownloadStyle(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetRpcReceiveData(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetRpcReceiveData called on server.");
		}
		else
		{
			((MarkupTransceiver)obj).TargetRpcReceiveData(ClientScene.readyConnection, reader.ReadString());
		}
	}

	public void CallTargetRpcDownloadStyle(NetworkConnection conn, string url)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetRpcDownloadStyle called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetRpcDownloadStyle called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetRpcDownloadStyle);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(url);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetRpcDownloadStyle");
	}

	public void CallTargetRpcReceiveData(NetworkConnection target, string code)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetRpcReceiveData called on client.");
			return;
		}
       if (target.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetRpcReceiveData called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetRpcReceiveData);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(code);
		SendTargetRPCInternal(target, networkWriter, 0, "TargetRpcReceiveData");
	}

	static MarkupTransceiver()
	{
		kTargetRpcTargetRpcDownloadStyle = 2037165113;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MarkupTransceiver), kTargetRpcTargetRpcDownloadStyle, InvokeRpcTargetRpcDownloadStyle);
		kTargetRpcTargetRpcReceiveData = -1873892515;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MarkupTransceiver), kTargetRpcTargetRpcReceiveData, InvokeRpcTargetRpcReceiveData);
		NetworkCRC.RegisterBehaviour("MarkupTransceiver", 0);
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
