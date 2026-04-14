using System.Collections.ObjectModel;

namespace Dissonance.Networking
{
	public struct RoomEvent
	{
		public string PlayerName;

		public string Room;

		public bool Joined;

		internal ReadOnlyCollection<string> Rooms;

		internal RoomEvent([NotNull] string name, [NotNull] string room, bool joined, [NotNull] ReadOnlyCollection<string> rooms)
		{
			PlayerName = name;
			Room = room;
			Joined = joined;
			Rooms = rooms;
		}
	}
}
