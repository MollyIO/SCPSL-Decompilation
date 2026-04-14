using System;
using System.Threading;

namespace Dissonance.Threading
{
	internal class LockedValue<T>
	{
		public class Unlocker : IDisposable
		{
			private readonly LockedValue<T> _parent;

			[CanBeNull]
			public T Value
			{
				get
				{
					return _parent._value;
				}
				set
				{
					_parent._value = value;
				}
			}

			public Unlocker(LockedValue<T> parent)
			{
				_parent = parent;
			}

			public void Dispose()
			{
				_parent.Unlock();
			}
		}

		private T _value;

		private readonly object _lockObject;

		private readonly Unlocker _unlocker;

		public LockedValue(T value)
		{
			_value = value;
			_lockObject = new object();
			_unlocker = new Unlocker(this);
		}

		public Unlocker Lock()
		{
			Monitor.Enter(_lockObject);
			return _unlocker;
		}

		private void Unlock()
		{
			Monitor.Exit(_lockObject);
		}
	}
}
