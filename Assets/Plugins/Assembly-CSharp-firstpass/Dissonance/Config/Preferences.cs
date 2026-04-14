using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dissonance.Config
{
	internal static class Preferences
	{
		public static void Get<T>(string key, ref T output, Func<string, T, T> get, Log log)
		{
			if (PlayerPrefs.HasKey(key))
			{
				output = get(key, output);
				log.Info("Loaded Pref {0} = {1}", key, output);
			}
		}

		public static void Set<T>(string key, ref T field, T value, Action<string, T> save, Log log, IEqualityComparer<T> equality = null, bool setAtRuntime = true)
		{
			if (!setAtRuntime && Application.isPlaying)
			{
				throw log.CreatePossibleBugException(string.Format("Attempted to set pref '{0}' but this cannot be set at runtime", key), "28579FE7-72D7-4516-BF04-BE96B11BB0C7");
			}
			if (equality == null)
			{
				equality = EqualityComparer<T>.Default;
			}
			if (!equality.Equals(field, value))
			{
				field = value;
				save(key, value);
				log.Info("Saved Pref {0} = {1}", key, value);
				PlayerPrefs.Save();
			}
		}

		internal static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, Convert.ToInt32(value));
		}

		internal static bool GetBool(string key, bool defaultValue)
		{
			return Convert.ToBoolean(PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue)));
		}
	}
}
