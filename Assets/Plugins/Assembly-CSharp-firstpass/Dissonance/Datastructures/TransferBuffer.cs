using System;
using System.Threading;

namespace Dissonance.Datastructures
{
	internal class TransferBuffer<T>
	{
		private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(TransferBuffer<T>).Name);

		private readonly T[] _buffer;

		private volatile int _readHead;

		private volatile int _unread;

		private volatile int _writeHead;

		private readonly T[] _singleReadItem = new T[1];

		private readonly T[] _singleWriteItem = new T[1];

		public int EstimatedUnreadCount
		{
			get
			{
				return _unread;
			}
		}

		public int Capacity
		{
			get
			{
				return _buffer.Length;
			}
		}

		public TransferBuffer(int capacity = 4096)
		{
			_buffer = new T[capacity];
		}

		public bool TryWrite(T item)
		{
			_singleWriteItem[0] = item;
			bool result = TryWriteAll(new ArraySegment<T>(_singleWriteItem));
			_singleWriteItem[0] = default(T);
			return result;
		}

		public bool TryWriteAll(ArraySegment<T> data)
		{
			if (_unread + data.Count > _buffer.Length)
			{
				return false;
			}
			if (_writeHead + data.Count > _buffer.Length)
			{
				int num = _buffer.Length - _writeHead;
				Array.Copy(data.Array, data.Offset, _buffer, _writeHead, num);
				Array.Copy(data.Array, data.Offset + num, _buffer, 0, data.Count - num);
				_writeHead = (_writeHead + data.Count) % _buffer.Length;
			}
			else
			{
				Array.Copy(data.Array, data.Offset, _buffer, _writeHead, data.Count);
				_writeHead += data.Count;
			}
			Interlocked.Add(ref _unread, data.Count);
			return true;
		}

		public int WriteSome(ArraySegment<T> data)
		{
			int num = Math.Min(_buffer.Length - _unread, data.Count);
			if (num == 0)
			{
				return 0;
			}
			Log.AssertAndThrowPossibleBug(TryWriteAll(new ArraySegment<T>(data.Array, data.Offset, num)), "A1E50AC5-27C5-4435-A792-3C80D5F629C0", "Failed to write expected number of samples into buffer");
			return num;
		}

		public bool Read([CanBeNull] out T item)
		{
			bool flag = Read(_singleReadItem);
			item = ((!flag) ? default(T) : _singleReadItem[0]);
			_singleReadItem[0] = default(T);
			return flag;
		}

		public bool Read([NotNull] T[] data)
		{
			return Read(new ArraySegment<T>(data, 0, data.Length));
		}

		public bool Read([NotNull] T[] data, int readCount)
		{
			if (readCount > data.Length)
			{
				throw new ArgumentException("Requested read amount is > size of supplied output buffer", "readCount");
			}
			return Read(new ArraySegment<T>(data, 0, readCount));
		}

		public bool Read(ArraySegment<T> data)
		{
			if (_unread < data.Count)
			{
				return false;
			}
			if (_readHead + data.Count > _buffer.Length)
			{
				int num = _buffer.Length - _readHead;
				Array.Copy(_buffer, _readHead, data.Array, data.Offset, num);
				Array.Copy(_buffer, 0, data.Array, data.Offset + num, data.Count - num);
				_readHead = (_readHead + data.Count) % _buffer.Length;
			}
			else
			{
				Array.Copy(_buffer, _readHead, data.Array, data.Offset, data.Count);
				_readHead += data.Count;
			}
			Interlocked.Add(ref _unread, -data.Count);
			return true;
		}

		public void Clear()
		{
			_readHead = 0;
			_writeHead = 0;
			_unread = 0;
		}
	}
}
