namespace Dissonance.Datastructures
{
	internal class RingBuffer<T> where T : struct
	{
		private readonly T[] _items;

		private int _end;

		public int Count { get; private set; }

		public int Capacity
		{
			get
			{
				return _items.Length;
			}
		}

		public RingBuffer(uint size)
		{
			_items = new T[size];
		}

		public T? Add(T item)
		{
			T? result = null;
			if (Count == Capacity)
			{
				result = _items[_end];
			}
			_items[_end] = item;
			_end = (_end + 1) % _items.Length;
			if (Count < _items.Length)
			{
				Count++;
			}
			return result;
		}

		public void Clear()
		{
			Count = 0;
			_end = 0;
		}
	}
}
