using System;
using System.Runtime.InteropServices;

namespace Dissonance.Extensions
{
	public static class ArraySegmentExtensions
	{
		internal struct DisposableHandle : IDisposable
		{
			private readonly IntPtr _ptr;

			private GCHandle _handle;

			public IntPtr Ptr
			{
				get
				{
					if (!_handle.IsAllocated)
					{
						throw new ObjectDisposedException("GC Handle has already been freed");
					}
					return _ptr;
				}
			}

			internal DisposableHandle(IntPtr ptr, GCHandle handle)
			{
				_ptr = ptr;
				_handle = handle;
			}

			public void Dispose()
			{
				_handle.Free();
			}
		}

		public static ArraySegment<T> CopyTo<T>(this ArraySegment<T> source, [NotNull] T[] destination, int destinationOffset = 0) where T : struct
		{
			if (destination == null)
			{
				throw new ArgumentNullException("destination");
			}
			if (source.Count > destination.Length - destinationOffset)
			{
				throw new ArgumentException("Insufficient space in destination array", "destination");
			}
			Array.Copy(source.Array, source.Offset, destination, destinationOffset, source.Count);
			return new ArraySegment<T>(destination, destinationOffset, source.Count);
		}

		internal static int CopyFrom<T>(this ArraySegment<T> destination, [NotNull] T[] source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			int num = Math.Min(destination.Count, source.Length);
			Array.Copy(source, 0, destination.Array, destination.Offset, num);
			return num;
		}

		[NotNull]
		internal static T[] ToArray<T>(this ArraySegment<T> segment) where T : struct
		{
			T[] array = new T[segment.Count];
			segment.CopyTo(array);
			return array;
		}

		internal static void Clear<T>(this ArraySegment<T> segment)
		{
			Array.Clear(segment.Array, segment.Offset, segment.Count);
		}

		internal static DisposableHandle Pin<T>(this ArraySegment<T> segment) where T : struct
		{
			GCHandle handle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
			int num = Marshal.SizeOf(typeof(T));
			IntPtr ptr = new IntPtr(handle.AddrOfPinnedObject().ToInt64() + segment.Offset * num);
			return new DisposableHandle(ptr, handle);
		}
	}
}
