namespace Dissonance.Networking.Client
{
	internal interface ISession
	{
		uint SessionId { get; }

		ushort? LocalId { get; }

		[NotNull]
		string LocalName { get; }
	}
}
