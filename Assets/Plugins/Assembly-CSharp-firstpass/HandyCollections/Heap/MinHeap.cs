using System;
using System.Collections.Generic;

namespace HandyCollections.Heap
{
	internal class MinHeap<T> : IMinHeap<T>
	{
		private readonly List<T> _heap;

		private readonly IComparer<T> _comparer;

		private bool _allowResize = true;

		public int Count
		{
			get
			{
				return _heap.Count;
			}
		}

		public T Minimum
		{
			get
			{
				return _heap[0];
			}
		}

		public bool AllowHeapResize
		{
			get
			{
				return _allowResize;
			}
			set
			{
				_allowResize = value;
			}
		}

		public MinHeap()
			: this(64)
		{
		}

		public MinHeap(int capacity)
			: this(capacity, (IComparer<T>)Comparer<T>.Default)
		{
		}

		public MinHeap(int capacity, IComparer<T> comparer)
		{
			_heap = new List<T>(capacity);
			_comparer = comparer;
		}

		public MinHeap(IComparer<T> comparer)
		{
			_heap = new List<T>();
			_comparer = comparer;
		}

		public void Add(T item)
		{
			if (!_allowResize && _heap.Count == _heap.Capacity)
			{
				throw new InvalidOperationException("Heap is full and resizing is disabled");
			}
			_heap.Add(item);
			BubbleUp(_heap.Count - 1);
			DebugCheckHeapProperty();
		}

		public void Add(IEnumerable<T> items)
		{
			_heap.AddRange(items);
			Heapify();
			DebugCheckHeapProperty();
		}

		public void Heapify()
		{
			for (int num = _heap.Count - 1; num >= 0; num--)
			{
				TrickleDown(num);
			}
			DebugCheckHeapProperty();
		}

		public void Heapify(int mutated)
		{
			if (mutated < 0 || mutated >= Count)
			{
				throw new IndexOutOfRangeException("mutated");
			}
			if (TrickleDown(mutated) == mutated)
			{
				BubbleUp(mutated);
			}
		}

		public T RemoveMin()
		{
			return RemoveAt(0);
		}

		public T RemoveAt(int index)
		{
			if (index < 0 || index > _heap.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			T result = _heap[index];
			_heap[index] = _heap[_heap.Count - 1];
			_heap.RemoveAt(_heap.Count - 1);
			if (_heap.Count > 0 && index < _heap.Count)
			{
				Heapify(0);
			}
			DebugCheckHeapProperty();
			return result;
		}

		public void Clear()
		{
			_heap.Clear();
		}

		private void BubbleUp(int index)
		{
			while (index > 0)
			{
				int num = ParentIndex(index);
				if (IsLessThan(_heap[index], _heap[num]))
				{
					Swap(num, index);
					index = num;
					continue;
				}
				break;
			}
		}

		private int TrickleDown(int index)
		{
			while (true)
			{
				if (index >= _heap.Count)
				{
					throw new ArgumentException();
				}
				int num = SmallestChildSmallerThan(index, _heap[index]);
				if (num == -1)
				{
					break;
				}
				Swap(num, index);
				index = num;
			}
			return index;
		}

		private void DebugCheckHeapProperty()
		{
		}

		private bool IsLessThan(T a, T b)
		{
			return _comparer.Compare(a, b) < 0;
		}

		private static int ParentIndex(int i)
		{
			return (i - 1) / 2;
		}

		private void Swap(int a, int b)
		{
			T value = _heap[a];
			_heap[a] = _heap[b];
			_heap[b] = value;
		}

		private static int LeftChild(int i)
		{
			return 2 * i + 1;
		}

		private static int RightChild(int i)
		{
			return 2 * i + 2;
		}

		private int SmallestChildSmallerThan(int i, T item)
		{
			int num = LeftChild(i);
			int num2 = RightChild(i);
			int num3 = -1;
			if (num < _heap.Count)
			{
				num3 = num;
			}
			if (num2 < _heap.Count && IsLessThan(_heap[num2], _heap[num]))
			{
				num3 = num2;
			}
			if (num3 > -1 && IsLessThan(_heap[num3], item))
			{
				return num3;
			}
			return -1;
		}

		public int IndexOf(T item)
		{
			return _heap.IndexOf(item);
		}

		public int IndexOf(Predicate<T> predicate)
		{
			return _heap.FindIndex(predicate);
		}
	}
}
