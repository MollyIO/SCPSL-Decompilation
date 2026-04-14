namespace Dissonance.Datastructures
{
	internal abstract class BaseWindowCalculator<T> where T : struct
	{
		private readonly RingBuffer<T> _buffer;

		protected int Count
		{
			get
			{
				return _buffer.Count;
			}
		}

		protected int Capacity
		{
			get
			{
				return _buffer.Capacity;
			}
		}

		protected BaseWindowCalculator(uint size)
		{
			_buffer = new RingBuffer<T>(size);
		}

		public void Update(T added)
		{
			T? removed = _buffer.Add(added);
			Updated(removed, added);
		}

		protected abstract void Updated(T? removed, T added);

		public virtual void Clear()
		{
			_buffer.Clear();
		}
	}
}
