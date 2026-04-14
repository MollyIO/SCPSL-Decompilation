namespace Dissonance.Datastructures
{
	public interface IRecycler<in T> where T : class
	{
		void Recycle(T item);
	}
}
