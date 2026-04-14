using System;
using Dissonance.Extensions;

namespace Dissonance
{
	public static class RoomIdConversion
	{
		public static ushort ToRoomId([NotNull] this string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return Hash16(name);
		}

		private static ushort Hash16([NotNull] string str)
		{
			int fnvHashCode = str.GetFnvHashCode();
			ushort num = (ushort)(fnvHashCode >> 16);
			ushort num2 = (ushort)fnvHashCode;
			return (ushort)(num * 5791 + num2 * 7639);
		}
	}
}
