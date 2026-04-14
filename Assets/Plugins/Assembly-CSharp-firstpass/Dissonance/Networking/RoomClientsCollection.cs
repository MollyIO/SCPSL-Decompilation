using System.Collections.Generic;

namespace Dissonance.Networking
{
	internal class RoomClientsCollection<T>
	{
		private class ClientIdComparer : IComparer<ClientInfo<T>>
		{
			public int Compare(ClientInfo<T> x, ClientInfo<T> y)
			{
				bool flag = object.ReferenceEquals(x, null);
				bool flag2 = object.ReferenceEquals(y, null);
				if (flag && flag2)
				{
					return 0;
				}
				if (flag)
				{
					return -1;
				}
				if (flag2)
				{
					return 1;
				}
				return x.PlayerId.CompareTo(y.PlayerId);
			}
		}

		private static readonly IComparer<ClientInfo<T>> ClientComparer = new ClientIdComparer();

		private readonly Dictionary<string, List<ClientInfo<T>>> _clientByRoomName = new Dictionary<string, List<ClientInfo<T>>>();

		private readonly Dictionary<ushort, List<ClientInfo<T>>> _clientByRoomId = new Dictionary<ushort, List<ClientInfo<T>>>();

		public Dictionary<string, List<ClientInfo<T>>> ByName
		{
			get
			{
				return _clientByRoomName;
			}
		}

		public void Add([NotNull] string room, [NotNull] ClientInfo<T> client)
		{
			List<ClientInfo<T>> value;
			if (!_clientByRoomName.TryGetValue(room, out value))
			{
				value = new List<ClientInfo<T>>();
				_clientByRoomName.Add(room, value);
				_clientByRoomId.Add(room.ToRoomId(), value);
			}
			int num = value.BinarySearch(client, ClientComparer);
			if (num < 0)
			{
				value.Insert(~num, client);
			}
		}

		public bool Remove([NotNull] string room, [NotNull] ClientInfo<T> client)
		{
			List<ClientInfo<T>> value;
			if (!_clientByRoomName.TryGetValue(room, out value))
			{
				return false;
			}
			int num = value.BinarySearch(client, ClientComparer);
			if (num < 0)
			{
				return false;
			}
			value.RemoveAt(num);
			return true;
		}

		public void Clear()
		{
			_clientByRoomName.Clear();
			_clientByRoomId.Clear();
		}

		[ContractAnnotation("=> true, clients:notnull; => false, clients:null")]
		public bool TryGetClientsInRoom([NotNull] string room, out List<ClientInfo<T>> clients)
		{
			return _clientByRoomName.TryGetValue(room, out clients);
		}

		[ContractAnnotation("=> true, clients:notnull; => false, clients:null")]
		public bool TryGetClientsInRoom(ushort roomId, out List<ClientInfo<T>> clients)
		{
			return _clientByRoomId.TryGetValue(roomId, out clients);
		}

		public int ClientCount()
		{
			int num = 0;
			foreach (KeyValuePair<string, List<ClientInfo<T>>> item in _clientByRoomName)
			{
				num += item.Value.Count;
			}
			return num;
		}
	}
}
