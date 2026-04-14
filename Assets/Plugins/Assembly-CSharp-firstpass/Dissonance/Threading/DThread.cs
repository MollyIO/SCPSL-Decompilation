using System;
using System.Threading;

namespace Dissonance.Threading
{
	internal class DThread : IThread
	{
		private readonly Thread _thread;

		public bool IsStarted { get; private set; }

		public DThread([NotNull] Action action)
		{
			_thread = new Thread(action.Invoke);
		}

		public void Start()
		{
			_thread.Start();
			IsStarted = true;
		}

		public void Join()
		{
			if (_thread.ThreadState != ThreadState.Unstarted)
			{
				_thread.Join();
			}
		}
	}
}
