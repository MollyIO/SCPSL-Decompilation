using System;
using System.Collections.Generic;

namespace Dissonance.Datastructures
{
	internal class POTBuffer
	{
		private readonly List<float[]> _buffers;

		public uint MaxCount { get; private set; }

		public uint Pow2
		{
			get
			{
				return (uint)_buffers.Count;
			}
		}

		public uint Count { get; private set; }

		public POTBuffer(byte initialMaxPow)
		{
			_buffers = new List<float[]>(initialMaxPow);
			for (int i = 0; i < initialMaxPow; i++)
			{
				_buffers.Add(new float[1 << i]);
			}
			MaxCount = (uint)((1 << (int)initialMaxPow) - 1);
		}

		public void Free()
		{
			Count = 0u;
		}

		public void Alloc(uint count)
		{
			if (count > MaxCount)
			{
				throw new ArgumentOutOfRangeException("count", "count is larger than buffer capacity");
			}
			Count = count;
		}

		public bool Expand(int limit = int.MaxValue)
		{
			if (Count != 0)
			{
				throw new InvalidOperationException("Cannot expand buffer while it is in use");
			}
			uint num = (uint)((1 << _buffers.Count + 1) - 1);
			if (num > limit)
			{
				return false;
			}
			_buffers.Add(new float[1 << _buffers.Count]);
			MaxCount = num;
			return true;
		}

		[NotNull]
		public float[] GetBuffer(ref uint count, bool zeroed = false)
		{
			if (count > Count)
			{
				throw new ArgumentOutOfRangeException("count", "count must be <= the total allocated size (set with Alloc(count))");
			}
			if (count == 0)
			{
				throw new ArgumentOutOfRangeException("count", "count must be > 0");
			}
			for (int num = _buffers.Count - 1; num >= 0; num--)
			{
				float[] array = _buffers[num];
				if (array.Length <= count)
				{
					count = checked((uint)(count - array.Length));
					if (zeroed)
					{
						Array.Clear(array, 0, array.Length);
					}
					return array;
				}
			}
			throw new InvalidOperationException("Failed to find a correctly sized buffer to service request");
		}
	}
}
