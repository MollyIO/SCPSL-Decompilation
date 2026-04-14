namespace Dissonance.Networking
{
	internal interface IReadonlyClientIdCollection
	{
		ushort? GetId([NotNull] string name);

		[CanBeNull]
		string GetName(ushort id);
	}
}
