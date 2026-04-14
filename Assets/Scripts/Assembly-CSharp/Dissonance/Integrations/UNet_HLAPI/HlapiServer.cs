using System;
using System.Collections.Generic;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
	public class HlapiServer : BaseServer<HlapiServer, HlapiClient, HlapiConn>
	{
		public static HlapiServer _instance;

		[Dissonance.NotNull]
		private readonly HlapiCommsNetwork _network;

		private readonly NetworkWriter _sendWriter = new NetworkWriter(new byte[1024]);

		private readonly byte[] _receiveBuffer = new byte[1024];

		private readonly List<NetworkConnection> _addedConnections = new List<NetworkConnection>();

		public HlapiServer([Dissonance.NotNull] HlapiCommsNetwork network)
		{
			if (network == null)
			{
				throw new ArgumentNullException("network");
			}
			_network = network;
			_instance = this;
		}

		public override void Connect()
		{
			NetworkServer.RegisterHandler(_network.TypeCode, OnMessageReceivedHandler);
			base.Connect();
		}

		private void OnMessageReceivedHandler([Dissonance.NotNull] NetworkMessage netmsg)
		{
			NetworkReceivedPacket(new HlapiConn(netmsg.conn), _network.CopyToArraySegment(netmsg.reader, new ArraySegment<byte>(_receiveBuffer)));
		}

		protected override void AddClient([Dissonance.NotNull] ClientInfo<HlapiConn> client)
		{
			base.AddClient(client);
			if (client.PlayerName != _network.PlayerName)
			{
				_addedConnections.Add(client.Connection.Connection);
			}
		}

		public override void Disconnect()
		{
			base.Disconnect();
			NetworkServer.RegisterHandler(_network.TypeCode, HlapiCommsNetwork.NullMessageReceivedHandler);
		}

		protected override void ReadMessages()
		{
		}

		public static void OnServerDisconnect(NetworkConnection connection)
		{
			if (_instance != null)
			{
				_instance.OnServerDisconnect(new HlapiConn(connection));
			}
		}

		private void OnServerDisconnect(HlapiConn conn)
		{
			int num = _addedConnections.IndexOf(conn.Connection);
			if (num >= 0)
			{
				_addedConnections.RemoveAt(num);
				ClientDisconnected(conn);
			}
		}

		public override ServerState Update()
		{
			for (int num = _addedConnections.Count - 1; num >= 0; num--)
			{
				NetworkConnection networkConnection = _addedConnections[num];
				if (!networkConnection.isConnected || networkConnection.lastError == NetworkError.Timeout || !NetworkServer.connections.Contains(_addedConnections[num]))
				{
					ClientDisconnected(new HlapiConn(_addedConnections[num]));
					_addedConnections.RemoveAt(num);
				}
			}
			return base.Update();
		}

		protected override void SendReliable(HlapiConn connection, ArraySegment<byte> packet)
		{
			if (!Send(packet, connection, _network.ReliableSequencedChannel))
			{
				FatalError("Failed to send reliable packet (unknown HLAPI error)");
			}
		}

		protected override void SendUnreliable(HlapiConn connection, ArraySegment<byte> packet)
		{
			Send(packet, connection, _network.UnreliableChannel);
		}

		private bool Send(ArraySegment<byte> packet, HlapiConn connection, byte channel)
		{
			if (_network.PreprocessPacketToClient(packet, connection))
			{
				return true;
			}
			if (!connection.Connection.isConnected || connection.Connection.lastError == NetworkError.Timeout)
			{
				return true;
			}
			int numBytes = _network.CopyPacketToNetworkWriter(packet, _sendWriter);
			if (connection.Connection == null)
			{
				Log.Error("Cannot send to a null destination");
				return false;
			}
			if (!connection.Connection.SendBytes(_sendWriter.AsArray(), numBytes, channel))
			{
				return false;
			}
			return true;
		}
	}
}
