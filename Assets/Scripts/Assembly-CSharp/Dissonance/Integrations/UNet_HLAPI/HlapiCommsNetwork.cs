using System;
using System.Collections.Generic;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
	[HelpURL("https://placeholder-software.co.uk/dissonance/docs/Basics/Quick-Start-UNet-HLAPI/")]
	public class HlapiCommsNetwork : BaseCommsNetwork<HlapiServer, HlapiClient, HlapiConn, Unit, Unit>
	{
		public byte UnreliableChannel = 1;

		public byte ReliableSequencedChannel;

		public short TypeCode = 18385;

		private readonly ConcurrentPool<byte[]> _loopbackBuffers = new ConcurrentPool<byte[]>(8, () => new byte[1024]);

		private readonly List<ArraySegment<byte>> _loopbackQueue = new List<ArraySegment<byte>>();

		protected override HlapiServer CreateServer(Unit details)
		{
			return new HlapiServer(this);
		}

		protected override HlapiClient CreateClient(Unit details)
		{
			return new HlapiClient(this);
		}

		protected override void Update()
		{
			if (base.IsInitialized)
			{
				if (NetworkManager.singleton != null && NetworkManager.singleton.isNetworkActive && (NetworkServer.active || NetworkClient.active) && (!NetworkClient.active || (NetworkManager.singleton.client != null && NetworkManager.singleton.client.connection != null)))
				{
					bool active = NetworkServer.active;
					bool active2 = NetworkClient.active;
					if (base.Mode.IsServerEnabled() != active || (base.Mode.IsClientEnabled() != active2 && !ServerStatic.IsDedicated))
					{
						if (ServerStatic.IsDedicated)
						{
							RunAsDedicatedServer(Unit.None);
						}
						else if (active && active2)
						{
							RunAsHost(Unit.None, Unit.None);
						}
						else if (active)
						{
							RunAsDedicatedServer(Unit.None);
						}
						else if (active2)
						{
							RunAsClient(Unit.None);
						}
					}
				}
				else if (base.Mode != NetworkMode.None)
				{
					Stop();
					_loopbackQueue.Clear();
				}
				for (int i = 0; i < _loopbackQueue.Count; i++)
				{
					HlapiClient client = base.Client;
					if (client != null)
					{
						client.NetworkReceivedPacket(_loopbackQueue[i]);
					}
					_loopbackBuffers.Put(_loopbackQueue[i].Array);
				}
				_loopbackQueue.Clear();
			}
			base.Update();
		}

		protected override void Initialize()
		{
			if (UnreliableChannel >= NetworkManager.singleton.channels.Count)
			{
				throw Log.CreateUserErrorException("configured 'unreliable' channel is out of range", "set the wrong channel number in the HLAPI Comms Network component", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/", "B19B4916-8709-490B-8152-A646CCAD788E");
			}
			QosType qosType = NetworkManager.singleton.channels[UnreliableChannel];
			if (qosType != QosType.Unreliable)
			{
				throw Log.CreateUserErrorException(string.Format("configured 'unreliable' channel has QoS type '{0}'", qosType), "not creating the channel with the correct QoS type", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/", "24ee53b1-7517-4672-8a4a-64a3e3c87ef6");
			}
			if (ReliableSequencedChannel >= NetworkManager.singleton.channels.Count)
			{
				throw Log.CreateUserErrorException("configured 'reliable' channel is out of range", "set the wrong channel number in the HLAPI Comms Network component", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/", "5F5F2875-ECC8-433D-B0CB-97C151B8094D");
			}
			QosType qosType2 = NetworkManager.singleton.channels[ReliableSequencedChannel];
			if (qosType2 != QosType.ReliableSequenced)
			{
				throw Log.CreateUserErrorException(string.Format("configured 'reliable sequenced' channel has QoS type '{0}'", qosType2), "not creating the channel with the correct QoS type", "https://dissonance.readthedocs.io/en/latest/Basics/Quick-Start-UNet-HLAPI/", "035773ec-aef3-477a-8eeb-c234d416171c");
			}
			NetworkServer.RegisterHandler(TypeCode, NullMessageReceivedHandler);
			base.Initialize();
		}

		internal bool PreprocessPacketToClient(ArraySegment<byte> packet, HlapiConn destination)
		{
			if (base.Server == null)
			{
				throw Log.CreatePossibleBugException("server packet preprocessing running, but this peer is not a server", "8f9dc0a0-1b48-4a7f-9bb6-f767b2542ab1");
			}
			if (base.Client == null)
			{
				return false;
			}
			if (NetworkManager.singleton.client.connection != destination.Connection)
			{
				return false;
			}
			if (base.Client != null)
			{
				_loopbackQueue.Add(packet.CopyTo(_loopbackBuffers.Get()));
			}
			return true;
		}

		internal bool PreprocessPacketToServer(ArraySegment<byte> packet)
		{
			if (base.Client == null)
			{
				throw Log.CreatePossibleBugException("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");
			}
			if (base.Server == null)
			{
				return false;
			}
			base.Server.NetworkReceivedPacket(new HlapiConn(NetworkManager.singleton.client.connection), packet);
			return true;
		}

		internal static void NullMessageReceivedHandler([Dissonance.NotNull] NetworkMessage netmsg)
		{
			if (netmsg == null)
			{
				throw new ArgumentNullException("netmsg");
			}
			if (Logs.GetLogLevel(LogCategory.Network) <= LogLevel.Trace)
			{
				Debug.Log("Discarding Dissonance network message");
			}
			int num = (int)netmsg.reader.ReadPackedUInt32();
			for (int i = 0; i < num; i++)
			{
				netmsg.reader.ReadByte();
			}
		}

		internal ArraySegment<byte> CopyToArraySegment([Dissonance.NotNull] NetworkReader msg, ArraySegment<byte> segment)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			byte[] array = segment.Array;
			if (array == null)
			{
				throw new ArgumentNullException("segment");
			}
			int num = (int)msg.ReadPackedUInt32();
			if (num > segment.Count)
			{
				throw Log.CreatePossibleBugException("receive buffer is too small", "A7387195-BF3D-4796-A362-6C64BB546445");
			}
			for (int i = 0; i < num; i++)
			{
				array[segment.Offset + i] = msg.ReadByte();
			}
			return new ArraySegment<byte>(array, segment.Offset, num);
		}

		internal int CopyPacketToNetworkWriter(ArraySegment<byte> packet, [Dissonance.NotNull] NetworkWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			byte[] array = packet.Array;
			if (array == null)
			{
				throw new ArgumentNullException("packet");
			}
			writer.SeekZero();
			writer.StartMessage(TypeCode);
			writer.WritePackedUInt32((uint)packet.Count);
			for (int i = 0; i < packet.Count; i++)
			{
				writer.Write(array[packet.Offset + i]);
			}
			writer.FinishMessage();
			return writer.Position;
		}
	}
}
