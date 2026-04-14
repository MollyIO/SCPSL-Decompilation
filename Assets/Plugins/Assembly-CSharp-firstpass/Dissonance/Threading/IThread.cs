namespace Dissonance.Threading
{
	internal interface IThread
	{
		bool IsStarted { get; }

		void Start();

		void Join();
	}
}
