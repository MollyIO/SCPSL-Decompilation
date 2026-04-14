using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dissonance
{
	public sealed class Rooms
	{
		private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(Rooms).Name);

		private static readonly RoomMembershipComparer Comparer = new RoomMembershipComparer();

		private readonly List<RoomMembership> _rooms;

		private readonly List<string> _roomNames;

		private readonly ReadOnlyCollection<string> _roomNamesReadonly;

		internal ReadOnlyCollection<string> Memberships
		{
			get
			{
				return _roomNamesReadonly;
			}
		}

		public int Count
		{
			get
			{
				return _rooms.Count;
			}
		}

		internal RoomMembership this[int i]
		{
			get
			{
				return _rooms[i];
			}
		}

		public event Action<string> JoinedRoom;

		public event Action<string> LeftRoom;

		internal Rooms()
		{
			_rooms = new List<RoomMembership>();
			_roomNames = new List<string>();
			_roomNamesReadonly = new ReadOnlyCollection<string>(_roomNames);
		}

		public bool Contains([NotNull] string roomName)
		{
			if (roomName == null)
			{
				throw new ArgumentNullException("roomName");
			}
			return FindById(roomName.ToRoomId()).HasValue && _rooms.Count > 0;
		}

		public RoomMembership Join([NotNull] string roomName)
		{
			if (roomName == null)
			{
				throw new ArgumentNullException("roomName", "Cannot join a null room");
			}
			RoomMembership roomMembership = new RoomMembership(roomName, 1);
			int num = _rooms.BinarySearch(roomMembership, Comparer);
			if (_rooms.Count == 0 || num < 0)
			{
				int num2 = ~num;
				if (num2 == _rooms.Count)
				{
					_rooms.Add(roomMembership);
				}
				else
				{
					_rooms.Insert(num2, roomMembership);
				}
				OnJoinedRoom(roomMembership);
			}
			else
			{
				RoomMembership value = _rooms[num];
				value.Count++;
				_rooms[num] = value;
			}
			return roomMembership;
		}

		public bool Leave(RoomMembership membership)
		{
			int? num = FindById(membership.RoomId);
			if (!num.HasValue)
			{
				return false;
			}
			RoomMembership value = _rooms[num.Value];
			value.Count--;
			_rooms[num.Value] = value;
			if (value.Count <= 0)
			{
				_rooms.RemoveAt(num.Value);
				OnLeftRoom(membership);
				return true;
			}
			return false;
		}

		private void OnJoinedRoom(RoomMembership membership)
		{
			int num = _roomNames.BinarySearch(membership.RoomName);
			if (num < 0)
			{
				_roomNames.Insert(~num, membership.RoomName);
			}
			Action<string> action = this.JoinedRoom;
			if (action != null)
			{
				action(membership.RoomName);
			}
		}

		private void OnLeftRoom(RoomMembership membership)
		{
			int num = _roomNames.BinarySearch(membership.RoomName);
			if (num >= 0)
			{
				_roomNames.RemoveAt(num);
			}
			Action<string> action = this.LeftRoom;
			if (action != null)
			{
				action(membership.RoomName);
			}
		}

		[CanBeNull]
		internal string Name(ushort roomId)
		{
			if (_rooms.Count == 0)
			{
				return null;
			}
			int? num = FindById(roomId);
			if (!num.HasValue)
			{
				return null;
			}
			return _rooms[num.Value].RoomName;
		}

		private int? FindById(ushort id)
		{
			int num = _rooms.Count - 1;
			int num2 = 0;
			while (num >= num2)
			{
				int num3 = num2 + (num - num2) / 2;
				int num4 = id.CompareTo(_rooms[num3].RoomId);
				if (num4 == 0)
				{
					return num3;
				}
				if (num4 > 0)
				{
					num2 = num3 + 1;
				}
				else
				{
					num = num3 - 1;
				}
			}
			return null;
		}
	}
}
