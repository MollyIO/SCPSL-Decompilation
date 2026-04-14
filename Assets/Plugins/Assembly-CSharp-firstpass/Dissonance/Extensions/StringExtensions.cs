namespace Dissonance.Extensions
{
	internal static class StringExtensions
	{
		public static int GetFnvHashCode([CanBeNull] this string str)
		{
			if (object.ReferenceEquals(str, null))
			{
				return 0;
			}
			uint num = 2166136261u;
			foreach (char c in str)
			{
				byte b = (byte)((int)c >> 8);
				byte b2 = (byte)c;
				num ^= b;
				num *= 16777619;
				num ^= b2;
				num *= 16777619;
			}
			return (int)num;
		}
	}
}
