using System;
using System.Collections.Generic;

namespace Dissonance.Extensions
{
	public static class IEnumerableExtensions
	{
		[NotNull]
		public static IEnumerable<T> Concat<T>([NotNull] this IEnumerable<T> enumerable, T tail)
		{
			if (enumerable == null)
			{
				throw new ArgumentNullException("enumerable");
			}
			return enumerable.ConcatUnsafe(tail);
		}

		[NotNull]
		private static IEnumerable<T> ConcatUnsafe<T>([NotNull] this IEnumerable<T> enumerable, T tail)
		{
			foreach (T item in enumerable)
			{
				yield return item;
			}
			yield return tail;
		}
	}
}
