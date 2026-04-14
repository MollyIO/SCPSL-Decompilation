using System.Collections.Generic;

namespace Dissonance.Networking
{
	internal interface IClientCollection<TPeer>
	{
		[ContractAnnotation("=> true, info:notnull; => false, info:null")]
		bool TryGetClientInfoById(ushort clientId, out ClientInfo<TPeer> info);

		[ContractAnnotation("=> true, info:notnull; => false, info:null")]
		bool TryGetClientInfoByName([NotNull] string clientName, out ClientInfo<TPeer> info);

		[ContractAnnotation("=> true, clients:notnull; => false, clients:null")]
		bool TryGetClientsInRoom([NotNull] string room, out List<ClientInfo<TPeer>> clients);

		[ContractAnnotation("=> true, clients:notnull; => false, clients:null")]
		bool TryGetClientsInRoom(ushort roomId, out List<ClientInfo<TPeer>> clients);
	}
}
