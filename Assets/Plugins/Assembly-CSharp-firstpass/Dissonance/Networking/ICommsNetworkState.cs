namespace Dissonance.Networking
{
	public interface ICommsNetworkState
	{
		string PlayerName { get; }

		Rooms Rooms { get; }

		PlayerChannels PlayerChannels { get; }

		RoomChannels RoomChannels { get; }

		CodecSettings CodecSettings { get; }
	}
}
