using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class HeadlessCallbacks : Attribute
{
	private static IEnumerable callbackRegistry;

	public static void FindCallbacks()
	{
		if (callbackRegistry == null)
		{
			callbackRegistry = from a in AppDomain.CurrentDomain.GetAssemblies()
				from t in a.GetTypes()
				let attributes = t.GetCustomAttributes(typeof(HeadlessCallbacks), true)
				where attributes != null && attributes.Length > 0
				select t;
		}
	}

	public static void InvokeCallbacks(string callbackName)
	{
		FindCallbacks();
		foreach (Type item in callbackRegistry)
		{
			MethodInfo method = item.GetMethod(callbackName);
			if (method != null)
			{
				try
				{
					method.Invoke(item, null);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
	}
}
