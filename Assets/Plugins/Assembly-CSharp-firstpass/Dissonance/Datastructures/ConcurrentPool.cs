using System;
using Dissonance.Threading;

namespace Dissonance.Datastructures
{
	public class ConcurrentPool<T> : IRecycler<T> where T : class
	{
		private readonly Func<T> _factory;

		private readonly TransferBuffer<T> _items;

		private readonly ReadonlyLockedValue<int> _getter = new ReadonlyLockedValue<int>(1);

		private readonly ReadonlyLockedValue<int> _putter = new ReadonlyLockedValue<int>(2);

		public ConcurrentPool(int maxSize, Func<T> factory)
		{
			_factory = factory;
			_items = new TransferBuffer<T>(maxSize);
		}

		[NotNull]
		public T Get()
		{
			using (_getter.Lock())
			{
				T item;
				if (_items.Read(out item) && !object.ReferenceEquals(item, null))
				{
					return item;
				}
				return _factory();
			}
		}

		public void Put([NotNull] T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			using (_putter.Lock())
			{
				_items.TryWrite(item);
			}
		}

		void IRecycler<T>.Recycle([NotNull] T item)
		{
			Put(item);
		}
	}
}
