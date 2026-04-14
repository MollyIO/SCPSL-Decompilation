namespace Dissonance.Extensions
{
	internal static class UShortExtensions
	{
		internal static int WrappedDelta(this ushort a, ushort b)
		{
			return Int32Extensions.WrappedDelta(a, b, 65535);
		}
	}
}
