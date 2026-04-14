namespace Dissonance.Extensions
{
	internal static class Int32Extensions
	{
		internal static int WrappedDelta(this int a, int b)
		{
			return a.WrappedDelta(b, int.MaxValue);
		}

		internal static int WrappedDelta(this int a, int b, int max)
		{
			int num = b - a;
			if (num < -max / 2)
			{
				return b + (max - a) + 1;
			}
			if (num > max / 2)
			{
				return b - max - a - 1;
			}
			return num;
		}
	}
}
