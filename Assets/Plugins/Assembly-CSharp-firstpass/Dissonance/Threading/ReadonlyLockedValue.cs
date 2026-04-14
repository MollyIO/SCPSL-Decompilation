using System;
using System.Threading;

namespace Dissonance.Threading
{
	internal class ReadonlyLockedValue<T>
	{
		public class Unlocker : IDisposable
		{
			private readonly ReadonlyLockedValue<T> _parent;

			public T Value
			{
				get
				{
					return _parent._value;
				}
			}

			public Unlocker(ReadonlyLockedValue<T> parent)
			{
				_parent = parent;
			}

			public void Dispose()
			{
				_parent.Unlock();
			}
		}

		private readonly T _value;

		private readonly object _lockObject;

		private readonly Unlocker _unlocker;

		public ReadonlyLockedValue(T value)
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
