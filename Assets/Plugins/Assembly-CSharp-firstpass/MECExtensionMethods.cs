using System;
using System.Collections.Generic;
using System.Threading;
using MEC;
using UnityEngine;

public static class MECExtensionMethods
{
	public static IEnumerator<float> Delay(this IEnumerator<float> coroutine, float timeToDelay)
	{
		yield return Timing.WaitForSeconds(timeToDelay);
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> Delay(this IEnumerator<float> coroutine, Func<bool> condition)
	{
		while (!condition())
		{
			yield return 0f;
		}
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> Delay<T>(this IEnumerator<float> coroutine, T data, Func<T, bool> condition)
	{
		while (!condition(data))
		{
			yield return 0f;
		}
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> DelayFrames(this IEnumerator<float> coroutine, int framesToDelay)
	{
		while (framesToDelay-- > 0)
		{
			yield return 0f;
		}
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject)
	{
		while (Timing.MainThread != Thread.CurrentThread || ((bool)gameObject && gameObject.activeInHierarchy && coroutine.MoveNext()))
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2)
	{
		while (Timing.MainThread != Thread.CurrentThread || ((bool)gameObject1 && gameObject1.activeInHierarchy && (bool)gameObject2 && gameObject2.activeInHierarchy && coroutine.MoveNext()))
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> CancelWith(this IEnumerator<float> coroutine, Func<bool> condition)
	{
		if (condition != null)
		{
			while (Timing.MainThread != Thread.CurrentThread || (condition() && coroutine.MoveNext()))
			{
				yield return coroutine.Current;
			}
		}
	}

	public static IEnumerator<float> PauseWith(this IEnumerator<float> coroutine, GameObject gameObject)
	{
		while (Timing.MainThread != Thread.CurrentThread || (bool)gameObject)
		{
			if (gameObject.activeInHierarchy)
			{
				if (!coroutine.MoveNext())
				{
					break;
				}
				yield return coroutine.Current;
			}
			else
			{
				yield return 0f;
			}
		}
	}

	public static IEnumerator<float> PauseWith(this IEnumerator<float> coroutine, GameObject gameObject1, GameObject gameObject2)
	{
		while (Timing.MainThread != Thread.CurrentThread || ((bool)gameObject1 && (bool)gameObject2))
		{
			if (gameObject1.activeInHierarchy && gameObject2.activeInHierarchy)
			{
				if (!coroutine.MoveNext())
				{
					break;
				}
				yield return coroutine.Current;
			}
			else
			{
				yield return 0f;
			}
		}
	}

	public static IEnumerator<float> PauseWith(this IEnumerator<float> coroutine, Func<bool> condition)
	{
		if (condition != null)
		{
			while (Timing.MainThread != Thread.CurrentThread || (condition() && coroutine.MoveNext()))
			{
				yield return coroutine.Current;
			}
		}
	}

	public static IEnumerator<float> Append(this IEnumerator<float> coroutine, IEnumerator<float> nextCoroutine)
	{
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
		if (nextCoroutine != null)
		{
			while (nextCoroutine.MoveNext())
			{
				yield return nextCoroutine.Current;
			}
		}
	}

	public static IEnumerator<float> Append(this IEnumerator<float> coroutine, Action onDone)
	{
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
		if (onDone != null)
		{
			onDone();
		}
	}

	public static IEnumerator<float> Prepend(this IEnumerator<float> coroutine, IEnumerator<float> lastCoroutine)
	{
		if (lastCoroutine != null)
		{
			while (lastCoroutine.MoveNext())
			{
				yield return lastCoroutine.Current;
			}
		}
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> Prepend(this IEnumerator<float> coroutine, Action onStart)
	{
		if (onStart != null)
		{
			onStart();
		}
		while (coroutine.MoveNext())
		{
			yield return coroutine.Current;
		}
	}

	public static IEnumerator<float> Superimpose(this IEnumerator<float> coroutineA, IEnumerator<float> coroutineB)
	{
		return coroutineA.Superimpose(coroutineB, Timing.Instance);
	}

	public static IEnumerator<float> Superimpose(this IEnumerator<float> coroutineA, IEnumerator<float> coroutineB, Timing instance)
	{
		while (coroutineA != null || coroutineB != null)
		{
			if (coroutineA != null && !(instance.localTime < coroutineA.Current) && !coroutineA.MoveNext())
			{
				coroutineA = null;
			}
			if (coroutineB != null && !(instance.localTime < coroutineB.Current) && !coroutineB.MoveNext())
			{
				coroutineB = null;
			}
			if ((coroutineA != null && float.IsNaN(coroutineA.Current)) || (coroutineB != null && float.IsNaN(coroutineB.Current)))
			{
				yield return float.NaN;
			}
			else if (coroutineA != null && coroutineB != null)
			{
				yield return (!(coroutineA.Current < coroutineB.Current)) ? coroutineB.Current : coroutineA.Current;
			}
			else if (coroutineA == null && coroutineB != null)
			{
				yield return coroutineB.Current;
			}
			else if (coroutineA != null)
			{
				yield return coroutineA.Current;
			}
		}
	}

	public static IEnumerator<float> Hijack(this IEnumerator<float> coroutine, Func<float, float> newReturn)
	{
		if (newReturn != null)
		{
			while (coroutine.MoveNext())
			{
				yield return newReturn(coroutine.Current);
			}
		}
	}
}
