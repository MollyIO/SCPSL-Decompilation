using System;

namespace Dissonance.Audio.Codecs
{
	public struct EncodedBuffer
	{
		public readonly ArraySegment<byte>? Encoded;

		public readonly bool PacketLost;

		public EncodedBuffer(ArraySegment<byte>? encoded, bool packetLost)
		{
			Encoded = encoded;
			PacketLost = packetLost;
		}
	}
}
