using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class VersionCheck : NetworkBehaviour
{
	[SyncVar(hook = "SyncVersion")]
	public string serverVersion = string.Empty;

	private string clientVersion = string.Empty;

	private bool isChecked;

	public string NetworkserverVersion
	{
		get
		{
			return serverVersion;
		}
		[param: In]
		set
		{
			VersionCheck versionCheck = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SyncVersion(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref versionCheck.serverVersion, 1u);
		}
	}

	private void Start()
	{
		clientVersion = CustomNetworkManager.CompatibleVersions[0];
		if (NetworkServer.active)
		{
			SyncVersion(clientVersion);
		}
	}

	private void Update()
	{
		if (!isChecked && base.name == "Host" && !string.IsNullOrEmpty(serverVersion))
		{
			isChecked = true;
			if (serverVersion != clientVersion)
			{
				CustomNetworkManager customNetworkManager = Object.FindObjectOfType<CustomNetworkManager>();
				customNetworkManager.StopClient();
				customNetworkManager.ShowLog(16, clientVersion, serverVersion);
			}
		}
	}

	public void SyncVersion(string s)
	{
		NetworkserverVersion = s;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(serverVersion);
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
			writer.Write(serverVersion);
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
			serverVersion = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SyncVersion(reader.ReadString());
		}
	}
}
