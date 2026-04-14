namespace Dissonance
{
	public struct RoomMembership
	{
		private readonly string _name;

		private readonly ushort _roomId;

		internal int Count;

		[NotNull]
		public string RoomName
		{
			get
			{
				return _name;
			}
		}

		public ushort RoomId
		{
			get
			{
				return _roomId;
			}
		}

		internal RoomMembership([NotNull] string name, int count)
		{
			_name = name;
			_roomId = name.ToRoomId();
			Count = count;
		}
	}
}
