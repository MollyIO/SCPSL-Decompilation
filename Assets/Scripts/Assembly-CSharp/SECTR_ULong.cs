using System;
using UnityEngine;

[Serializable]
public class SECTR_ULong
{
	[SerializeField]
	private int first;

	[SerializeField]
	private int second;

	public ulong value
	{
		get
		{
			ulong num = (ulong)second;
			num <<= 32;
			return num | (ulong)first;
		}
		set
		{
			first = (int)(value & 0xFFFFFFFFu);
			second = (int)(value >> 32);
		}
	}

	public SECTR_ULong(ulong newValue)
	{
		value = newValue;
	}

	public SECTR_ULong()
	{
		value = 0uL;
	}

	public override string ToString()
	{
		return string.Format("[ULong: value={0}, firstHalf={1}, secondHalf={2}]", value, first, second);
	}

	public static bool operator >(SECTR_ULong a, ulong b)
	{
		return a.value > b;
	}

	public static bool operator >(ulong a, SECTR_ULong b)
	{
		return a > b.value;
	}

	public static bool operator <(SECTR_ULong a, ulong b)
	{
		return a.value < b;
	}

	public static bool operator <(ulong a, SECTR_ULong b)
	{
		return a < b.value;
	}
}
