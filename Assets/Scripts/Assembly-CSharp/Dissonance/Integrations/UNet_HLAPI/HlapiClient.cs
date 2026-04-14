using System;
using Dissonance.Networking;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
	public class HlapiClient : BaseClient<HlapiServer, HlapiClient, HlapiConn>
	{
		private readonly HlapiCommsNetwork _network;

		private readonly NetworkWriter _sendWriter;

		private readonly byte[] _receiveBuffer = new byte[1024];

		public HlapiClient([Dissonance.NotNull] HlapiCommsNetwork network)
			: base((ICommsNetworkState)network)
		{
			if (network == null)
			{
				throw new ArgumentNullException("network");
			}
			_network = network;
			_sendWriter = new NetworkWriter(new byte[1024]);
		}

		public override void Connect()
		{
			if (!_network.Mode.IsServerEnabled())
			{
				NetworkManager.singleton.client.RegisterHandler(_network.TypeCode, OnMessageReceivedHandler);
			}
			Connected();
		}

		public override void Disconnect()
		{
			if (!_network.Mode.IsServerEnabled() && NetworkManager.singleton.client != null)
			{
				NetworkManager.singleton.client.RegisterHandler(_network.TypeCode, HlapiCommsNetwork.NullMessageReceivedHandler);
			}
			base.Disconnect();
		}

		private void OnMessageReceivedHandler([Dissonance.CanBeNull] NetworkMessage netMsg)
		{
			if (netMsg != null)
			{
				NetworkReceivedPacket(_network.CopyToArraySegment(netMsg.reader, new ArraySegment<byte>(_receiveBuffer)));
			}
		}

		protected override void ReadMessages()
		{
		}

		protected override void SendReliable(ArraySegment<byte> packet)
		{
			if (!Send(packet, _network.ReliableSequencedChannel))
			{
				FatalError("Failed to send reliable packet (unknown HLAPI error)");
			}
		}

		protected override void SendUnreliable(ArraySegment<byte> packet)
		{
			Send(packet, _network.UnreliableChannel);
		}

		private bool Send(ArraySegment<byte> packet, byte channel)
		{
			if (_network.PreprocessPacketToServer(packet))
			{
				return true;
			}
			int numBytes = _network.CopyPacketToNetworkWriter(packet, _sendWriter);
			if (!NetworkManager.singleton.client.connection.SendBytes(_sendWriter.AsArray(), numBytes, channel))
			{
				return false;
			}
			return true;
		}
	}
}
