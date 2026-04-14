using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
	[RequireComponent(typeof(NetworkIdentity))]
	public class HlapiPlayer : NetworkBehaviour, IDissonancePlayer
	{
		private static readonly Log Log;

		private DissonanceComms _comms;

		[SyncVar]
		private string _playerId;

		private static int kCmdCmdSetPlayerName;

		private static int kRpcRpcSetPlayerName;

		public bool IsTracking { get; private set; }

		public string PlayerId
		{
			get
			{
				return _playerId;
			}
		}

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return base.transform.rotation;
			}
		}

		public NetworkPlayerType Type
		{
			get
			{
				return base.isLocalPlayer ? NetworkPlayerType.Local : NetworkPlayerType.Remote;
			}
		}

		public string Network_playerId
		{
			get
			{
				return _playerId;
			}
			[param: In]
			set
			{
				SetSyncVar(value, ref _playerId, 1u);
			}
		}

		public void OnDestroy()
		{
			if (_comms != null)
			{
				_comms.LocalPlayerNameChanged -= SetPlayerName;
			}
		}

		public void OnEnable()
		{
			_comms = Object.FindObjectOfType<DissonanceComms>();
		}

		public void OnDisable()
		{
			if (IsTracking)
			{
				StopTracking();
			}
		}

		public override void OnStartAuthority()
		{
			base.OnStartAuthority();
			DissonanceComms dissonanceComms = Object.FindObjectOfType<DissonanceComms>();
			if (dissonanceComms == null)
			{
				throw Log.CreateUserErrorException("cannot find DissonanceComms component in scene", "not placing a DissonanceComms component on a game object in the scene", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/", "9A79FDCB-263E-4124-B54D-67EDA39C09A5");
			}
			if (dissonanceComms.LocalPlayerName != null)
			{
				SetPlayerName(dissonanceComms.LocalPlayerName);
			}
			dissonanceComms.LocalPlayerNameChanged += SetPlayerName;
		}

		private void SetPlayerName(string playerName)
		{
			if (IsTracking)
			{
				StopTracking();
			}
			Network_playerId = playerName;
			StartTracking();
			if (base.hasAuthority)
			{
				CallCmdSetPlayerName(playerName);
			}
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			if (!string.IsNullOrEmpty(PlayerId))
			{
				StartTracking();
			}
		}

		[Command]
		private void CmdSetPlayerName(string playerName)
		{
			Network_playerId = playerName;
			CallRpcSetPlayerName(playerName);
		}

		[ClientRpc]
		private void RpcSetPlayerName(string playerName)
		{
			if (!base.hasAuthority)
			{
				SetPlayerName(playerName);
			}
		}

		private void StartTracking()
		{
			if (IsTracking)
			{
				throw Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "B7D1F25E-72AF-4E93-8CFF-90CEBEAC68CF");
			}
			if (_comms != null)
			{
				_comms.TrackPlayerPosition(this);
				IsTracking = true;
			}
		}

		private void StopTracking()
		{
			if (!IsTracking)
			{
				throw Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "EC5C395D-B544-49DC-B33C-7D7533349134");
			}
			if (_comms != null)
			{
				_comms.StopTracking(this);
				IsTracking = false;
			}
		}

		static HlapiPlayer()
		{
			Log = Logs.Create(LogCategory.Network, "HLAPI Player Component");
			kCmdCmdSetPlayerName = -1254064873;
			NetworkBehaviour.RegisterCommandDelegate(typeof(HlapiPlayer), kCmdCmdSetPlayerName, InvokeCmdCmdSetPlayerName);
			kRpcRpcSetPlayerName = 1332331777;
			NetworkBehaviour.RegisterRpcDelegate(typeof(HlapiPlayer), kRpcRpcSetPlayerName, InvokeRpcRpcSetPlayerName);
			NetworkCRC.RegisterBehaviour("HlapiPlayer", 0);
		}

		private void UNetVersion()
		{
		}

		protected static void InvokeCmdCmdSetPlayerName(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSetPlayerName called on client.");
			}
			else
			{
				((HlapiPlayer)obj).CmdSetPlayerName(reader.ReadString());
			}
		}

		public void CallCmdSetPlayerName(string playerName)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command function CmdSetPlayerName called on server.");
				return;
			}
			if (base.isServer)
			{
				CmdSetPlayerName(playerName);
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)5);
			networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerName);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(playerName);
			SendCommandInternal(networkWriter, 0, "CmdSetPlayerName");
		}

		protected static void InvokeRpcRpcSetPlayerName(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcSetPlayerName called on server.");
			}
			else
			{
				((HlapiPlayer)obj).RpcSetPlayerName(reader.ReadString());
			}
		}

		public void CallRpcSetPlayerName(string playerName)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("RPC Function RpcSetPlayerName called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kRpcRpcSetPlayerName);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(playerName);
			SendRPCInternal(networkWriter, 0, "RpcSetPlayerName");
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.Write(_playerId);
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
				writer.Write(_playerId);
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
				_playerId = reader.ReadString();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if ((num & 1) != 0)
			{
				_playerId = reader.ReadString();
			}
		}
	}
}
