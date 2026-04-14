using System;
using Dissonance.Config;

namespace Dissonance.Networking.Client
{
	internal class PacketDelaySimulator
	{
		private readonly Random _rnd = new Random();

		private static bool IsOrderedReliable(MessageTypes header)
		{
			return header != MessageTypes.VoiceData;
		}

		public bool ShouldLose(ArraySegment<byte> packet)
		{
			if (DebugSettings.Instance.EnableNetworkSimulation)
			{
				MessageTypes messageType;
				if (!new PacketReader(packet).ReadPacketHeader(out messageType))
				{
					return false;
				}
				if (!IsOrderedReliable(messageType) && _rnd.NextDouble() < (double)DebugSettings.Instance.PacketLoss)
				{
					return true;
				}
			}
			return false;
		}
	}
}
