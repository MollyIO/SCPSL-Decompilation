using System;
using System.Collections.Generic;
using Dissonance.Datastructures;

namespace Dissonance.Networking.Client
{
	internal interface ISendQueue<TPeer> where TPeer : struct
	{
		[NotNull]
		ConcurrentPool<byte[]> SendBufferPool { get; }

		void EnqueueReliable(ArraySegment<byte> packet);

		void EnqeueUnreliable(ArraySegment<byte> packet);

		void EnqueueReliableP2P(ushort localId, [NotNull] IList<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet);

		void EnqueueUnreliableP2P(ushort localId, [NotNull] IList<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet);
	}
}
