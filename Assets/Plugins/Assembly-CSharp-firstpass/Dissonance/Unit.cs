using System.Runtime.InteropServices;

namespace Dissonance
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Unit
	{
		public static readonly Unit None = default(Unit);
	}
}
