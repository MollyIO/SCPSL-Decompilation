using System.Runtime.InteropServices;
using GameConsole;
using UnityEngine;
using UnityEngine.Networking;

public class ServerTime : NetworkBehaviour
{
	[SyncVar]
	public int timeFromStartup;

	public static int time;

	private const int allowedDeviation = 2;

	private static int kCmdCmdSetTime;

	public int NetworktimeFromStartup
	{
		get
		{
			return timeFromStartup;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref timeFromStartup, 1u);
		}
	}

	public static bool CheckSynchronization(int myTime)
	{
		int num = Mathf.Abs(myTime - time);
		if (num > 2)
		{
			Console.singleton.AddLog("Damage sync error.", new Color32(byte.MaxValue, 200, 0, byte.MaxValue));
		}
		return num <= 2;
	}

	private void Update()
	{
		if (base.name == "Host")
		{
			time = timeFromStartup;
		}
	}

	private void Start()
	{
		if (base.isLocalPlayer && base.isServer)
		{
			InvokeRepeating("IncreaseTime", 1f, 1f);
		}
	}

	private void IncreaseTime()
	{
		TransmitData(timeFromStartup + 1);
	}

	[ClientCallback]
	private void TransmitData(int timeFromStartup)
	{
		if (NetworkClient.active)
		{
			CallCmdSetTime(timeFromStartup);
		}
	}

	[Command(channel = 12)]
	private void CmdSetTime(int t)
	{
		NetworktimeFromStartup = t;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetTime(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTime called on client.");
		}
		else
		{
			((ServerTime)obj).CmdSetTime((int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdSetTime(int t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTime called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTime(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTime);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)t);
		SendCommandInternal(networkWriter, 12, "CmdSetTime");
	}

	static ServerTime()
	{
		kCmdCmdSetTime = 648282655;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerTime), kCmdCmdSetTime, InvokeCmdCmdSetTime);
		NetworkCRC.RegisterBehaviour("ServerTime", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)timeFromStartup);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)timeFromStartup);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			timeFromStartup = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			timeFromStartup = (int)reader.ReadPackedUInt32();
		}
	}
}
