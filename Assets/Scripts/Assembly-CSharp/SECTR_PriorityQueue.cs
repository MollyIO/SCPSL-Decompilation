using System;
using System.Collections.Generic;

public class SECTR_PriorityQueue<T> where T : IComparable<T>
{
	private List<T> data;

	public int Count
	{
		get
		{
			return data.Count;
		}
		set
		{
		}
	}

	public T this[int index]
	{
		get
		{
			return (index >= data.Count) ? default(T) : data[index];
		}
		set
		{
			if (index < data.Count)
			{
				data[index] = value;
				_Update(index);
			}
		}
	}

	public SECTR_PriorityQueue()
	{
		data = new List<T>(64);
	}

	public SECTR_PriorityQueue(int capacity)
	{
		data = new List<T>(capacity);
	}

	public void Enqueue(T item)
	{
		data.Add(item);
		int num = data.Count - 1;
		while (num > 0)
		{
			int num2 = (num - 1) / 2;
			if (data[num].CompareTo(data[num2]) < 0)
			{
				_SwapElements(num, num2);
				num = num2;
				continue;
			}
			break;
		}
	}

	public T Dequeue()
	{
		int num = data.Count - 1;
		T result = data[0];
		data[0] = data[num];
		data.RemoveAt(num);
		num--;
		int num2 = 0;
		while (true)
		{
			int num3 = num2 * 2 + 1;
			if (num3 > num)
			{
				break;
			}
			int num4 = num3 + 1;
			if (num4 <= num && data[num4].CompareTo(data[num3]) < 0)
			{
				num3 = num4;
			}
			if (data[num2].CompareTo(data[num3]) > 0)
			{
				_SwapElements(num2, num3);
				num2 = num3;
				continue;
			}
			break;
		}
		return result;
	}

	public T Peek()
	{
		return (data.Count <= 0) ? default(T) : data[0];
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < data.Count; i++)
		{
			text = text + data[i].ToString() + " ";
		}
		return text + "count = " + data.Count;
	}

	public bool IsConsistent()
	{
		if (data.Count > 0)
		{
			int num = data.Count - 1;
			for (int i = 0; i < data.Count; i++)
			{
				int num2 = 2 * i + 1;
				int num3 = 2 * i + 2;
				if ((num2 <= num && data[i].CompareTo(data[num2]) > 0) || (num3 <= num && data[i].CompareTo(data[num3]) > 0))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void Clear()
	{
		data.Clear();
	}

	private void _SwapElements(int i, int j)
	{
		T value = data[i];
		data[i] = data[j];
		data[j] = value;
	}

	private void _Update(int i)
	{
		int num = i;
		while (num > 0)
		{
			int num2 = (num - 1) / 2;
			if (data[num].CompareTo(data[num2]) < 0)
			{
				_SwapElements(num, num2);
				num = num2;
				continue;
			}
			break;
		}
		if (num < i)
		{
			return;
		}
		while (true)
		{
			int j = num;
			int num3 = 2 * num + 1;
			int num4 = 2 * num + 2;
			if (data.Count > num3 && data[num].CompareTo(data[num3]) > 0)
			{
				_SwapElements(num3, j);
				num = num3;
				continue;
			}
			if (data.Count > num4 && data[num].CompareTo(data[num4]) > 0)
			{
				_SwapElements(num4, j);
				num = num4;
				continue;
			}
			break;
		}
	}
}
