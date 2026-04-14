using System.Collections.Generic;

namespace Dissonance
{
	internal class RoomMembershipComparer : IComparer<RoomMembership>
	{
		public int Compare(RoomMembership x, RoomMembership y)
		{
			return x.RoomId.CompareTo(y.RoomId);
		}
	}
}
