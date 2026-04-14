using System;
using System.Collections.Generic;

namespace Dissonance.Networking.Server
{
	internal interface IServer<TPeer>
	{
		uint SessionId { get; }

		void SendUnreliable([NotNull] List<TPeer> connections, ArraySegment<byte> packet);

		void SendReliable(TPeer connection, ArraySegment<byte> packet);

		void SendReliable([NotNull] List<TPeer> connections, ArraySegment<byte> packet);

		void AddClient([NotNull] ClientInfo<TPeer> client);
	}
}
