using System;
using System.Collections.Generic;

namespace Dissonance.Datastructures
{
	internal class Pool<T> : IRecycler<T> where T : class
	{
		private readonly int _maxSize;

		private readonly Func<T> _factory;

		private readonly Stack<T> _items;

		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		public int Capacity
		{
			get
			{
				return _maxSize;
			}
		}

		public Pool(int maxSize, Func<T> factory)
		{
			_maxSize = maxSize;
			_factory = factory;
			_items = new Stack<T>(maxSize);
		}

		public T Get()
		{
			if (_items.Count > 0)
			{
				return _items.Pop();
			}
			return _factory();
		}

		public void Put([NotNull] T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (_items.Count < _maxSize)
			{
				_items.Push(item);
			}
		}

		void IRecycler<T>.Recycle([NotNull] T item)
		{
			Put(item);
		}
	}
}
