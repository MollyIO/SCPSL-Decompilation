using System;
using System.Collections.Generic;
using Dissonance.Datastructures;

namespace Dissonance.Networking.Client
{
	internal class SendQueue<TPeer> : ISendQueue<TPeer> where TPeer : struct
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(SendQueue<TPeer>).Name);

		private readonly IClient<TPeer> _client;

		private readonly List<ArraySegment<byte>> _serverReliableQueue = new List<ArraySegment<byte>>();

		private readonly List<ArraySegment<byte>> _serverUnreliableQueue = new List<ArraySegment<byte>>();

		private readonly List<KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>>> _reliableP2PQueue = new List<KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>>>();

		private readonly List<KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>>> _unreliableP2PQueue = new List<KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>>>();

		private readonly ConcurrentPool<byte[]> _sendBufferPool;

		private readonly Pool<List<ClientInfo<TPeer?>>> _listPool = new Pool<List<ClientInfo<TPeer?>>>(32, () => new List<ClientInfo<TPeer?>>());

		public ConcurrentPool<byte[]> SendBufferPool
		{
			get
			{
				return _sendBufferPool;
			}
		}

		public SendQueue([NotNull] IClient<TPeer> client, [NotNull] ConcurrentPool<byte[]> bytePool)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			if (bytePool == null)
			{
				throw new ArgumentNullException("bytePool");
			}
			_client = client;
			_sendBufferPool = bytePool;
		}

		public void Update()
		{
			for (int i = 0; i < _serverReliableQueue.Count; i++)
			{
				ArraySegment<byte> arraySegment = _serverReliableQueue[i];
				_client.SendReliable(arraySegment);
				Recycle(arraySegment.Array);
			}
			_serverReliableQueue.Clear();
			for (int j = 0; j < _serverUnreliableQueue.Count; j++)
			{
				ArraySegment<byte> arraySegment2 = _serverUnreliableQueue[j];
				_client.SendUnreliable(arraySegment2);
				Recycle(arraySegment2.Array);
			}
			_serverUnreliableQueue.Clear();
			for (int k = 0; k < _reliableP2PQueue.Count; k++)
			{
				KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>> keyValuePair = _reliableP2PQueue[k];
				_client.SendReliableP2P(keyValuePair.Key, keyValuePair.Value);
				Recycle(keyValuePair.Value.Array);
				keyValuePair.Key.Clear();
				_listPool.Put(keyValuePair.Key);
			}
			_reliableP2PQueue.Clear();
			for (int l = 0; l < _unreliableP2PQueue.Count; l++)
			{
				KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>> keyValuePair2 = _unreliableP2PQueue[l];
				_client.SendUnreliableP2P(keyValuePair2.Key, keyValuePair2.Value);
				Recycle(keyValuePair2.Value.Array);
				keyValuePair2.Key.Clear();
				_listPool.Put(keyValuePair2.Key);
			}
			_unreliableP2PQueue.Clear();
		}

		private void Recycle([NotNull] byte[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			_sendBufferPool.Put(array);
		}

		public void Stop()
		{
			int num = _serverReliableQueue.Count + _serverUnreliableQueue.Count + _reliableP2PQueue.Count + _unreliableP2PQueue.Count;
			_serverReliableQueue.Clear();
			_serverUnreliableQueue.Clear();
			_reliableP2PQueue.Clear();
			_unreliableP2PQueue.Clear();
		}

		public void EnqueueReliable(ArraySegment<byte> packet)
		{
			if (packet.Array == null)
			{
				throw new ArgumentNullException("packet");
			}
			_serverReliableQueue.Add(packet);
		}

		public void EnqeueUnreliable(ArraySegment<byte> packet)
		{
			if (packet.Array == null)
			{
				throw new ArgumentNullException("packet");
			}
			_serverUnreliableQueue.Add(packet);
		}

		public void EnqueueReliableP2P(ushort localId, IList<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet)
		{
			if (destinations == null)
			{
				throw new ArgumentNullException("destinations");
			}
			if (packet.Array == null)
			{
				throw new ArgumentNullException("packet");
			}
			EnqueueP2P(localId, destinations, _reliableP2PQueue, packet);
		}

		public void EnqueueUnreliableP2P(ushort localId, IList<ClientInfo<TPeer?>> destinations, ArraySegment<byte> packet)
		{
			if (destinations == null)
			{
				throw new ArgumentNullException("destinations");
			}
			if (packet.Array == null)
			{
				throw new ArgumentNullException("packet");
			}
			EnqueueP2P(localId, destinations, _unreliableP2PQueue, packet);
		}

		private void EnqueueP2P(ushort localId, [NotNull] ICollection<ClientInfo<TPeer?>> destinations, [NotNull] ICollection<KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>>> queue, ArraySegment<byte> packet)
		{
			if (packet.Array == null)
			{
				throw new ArgumentNullException("packet");
			}
			if (destinations == null)
			{
				throw new ArgumentNullException("destinations");
			}
			if (queue == null)
			{
				throw new ArgumentNullException("queue");
			}
			if (destinations.Count == 0)
			{
				return;
			}
			List<ClientInfo<TPeer?>> list = _listPool.Get();
			list.Clear();
			list.AddRange(destinations);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].PlayerId == localId)
				{
					list.RemoveAt(i);
					break;
				}
			}
			if (list.Count == 0)
			{
				_listPool.Put(list);
			}
			else
			{
				queue.Add(new KeyValuePair<List<ClientInfo<TPeer?>>, ArraySegment<byte>>(list, packet));
			}
		}
	}
}
