using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissonance.Networking
{
	internal sealed class ClientIdCollection : IReadonlyClientIdCollection
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(ClientIdCollection).Name);

		private readonly List<string> _items;

		private readonly List<ushort> _freeIds;

		private readonly IEnumerable<KeyValuePair<ushort, string>> _alive;

		[NotNull]
		public IEnumerable<KeyValuePair<ushort, string>> Items
		{
			get
			{
				return _alive;
			}
		}

		public ClientIdCollection()
		{
			_items = new List<string>();
			_freeIds = new List<ushort>();
			_alive = from x in _items.Select((string a, int i) => new KeyValuePair<ushort, string>((ushort)i, a))
				where x.Value != null
				select x;
		}

		private ushort GetFreeId()
		{
			if (_freeIds.Count == 0)
			{
				throw new InvalidOperationException("Cannot get a free ID, none available");
			}
			ushort result = _freeIds[_freeIds.Count - 1];
			_freeIds.RemoveAt(_freeIds.Count - 1);
			return result;
		}

		private void AddFreeId(ushort id)
		{
			int num = _freeIds.BinarySearch(id);
			Log.AssertAndThrowPossibleBug(num < 0, "371F58AD-CBB7-44FD-8503-0828496433F5", "Attempted to add free ID, but ID is already free");
			_freeIds.Insert(~num, id);
		}

		public string GetName(ushort id)
		{
			if (id >= _items.Count)
			{
				return null;
			}
			return _items[id];
		}

		public ushort? GetId(string name)
		{
			for (ushort num = 0; num < _items.Count; num++)
			{
				if (_items[num] == name)
				{
					return num;
				}
			}
			return null;
		}

		public ushort Register([NotNull] string name)
		{
			int num = _items.IndexOf(name);
			if (num != -1)
			{
				throw new InvalidOperationException(string.Format("Name is already in table with ID '{0}'", num));
			}
			if (_freeIds.Count > 0)
			{
				ushort freeId = GetFreeId();
				_items[freeId] = name;
				return freeId;
			}
			_items.Add(name);
			return (ushort)(_items.Count - 1);
		}

		public bool Unregister([NotNull] string name)
		{
			int num = _items.IndexOf(name);
			if (num == -1)
			{
				return false;
			}
			if (num == _items.Count - 1)
			{
				_items.RemoveAt(num);
				while (_freeIds.Contains((ushort)(_items.Count - 1)))
				{
					ushort num2 = (ushort)(_items.Count - 1);
					_freeIds.Remove(num2);
					_items.RemoveAt(num2);
				}
				return true;
			}
			_items[num] = null;
			AddFreeId((ushort)num);
			return true;
		}

		public void Clear()
		{
			_items.Clear();
			_freeIds.Clear();
		}

		public void Load([NotNull] List<ClientInfo> clients)
		{
			if (clients == null)
			{
				throw new ArgumentNullException("clients");
			}
			Clear();
			int num = 0;
			for (int i = 0; i < clients.Count; i++)
			{
				num = Math.Max(num, clients[i].PlayerId);
			}
			if (_items.Capacity < num + 1)
			{
				_items.Capacity = num + 1;
			}
			for (int j = 0; j < num + 1; j++)
			{
				_items.Add(null);
			}
			for (int k = 0; k < clients.Count; k++)
			{
				ClientInfo clientInfo = clients[k];
				_items[clientInfo.PlayerId] = clientInfo.PlayerName;
			}
			for (int l = 0; l < clients.Count; l++)
			{
				if (_items[l] == null)
				{
					AddFreeId((ushort)l);
				}
			}
		}
	}
}
