using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dissonance.Config
{
	public class ChatRoomSettings : ScriptableObject
	{
		private const string SettingsFileResourceName = "ChatRoomSettings";

		public static readonly string SettingsFilePath = Path.Combine("Assets/Plugins/Dissonance/Resources", "ChatRoomSettings.asset");

		private static readonly List<string> DefaultRooms = new List<string> { "Global", "Red Team", "Blue Team", "Room A", "Room B" };

		[SerializeField]
		internal List<string> Names;

		[NonSerialized]
		private Dictionary<ushort, string> _nameLookup;

		private static ChatRoomSettings _instance;

		[NotNull]
		public static ChatRoomSettings Instance
		{
			get
			{
				return _instance ?? (_instance = Load());
			}
		}

		public ChatRoomSettings()
		{
			Names = new List<string>(DefaultRooms);
		}

		[CanBeNull]
		public string FindRoomById(ushort id)
		{
			if (_nameLookup == null)
			{
				Dictionary<ushort, string> dictionary = new Dictionary<ushort, string>();
				for (int i = 0; i < Names.Count; i++)
				{
					dictionary[Names[i].ToRoomId()] = Names[i];
				}
				_nameLookup = dictionary;
			}
			string value;
			if (!_nameLookup.TryGetValue(id, out value))
			{
				return null;
			}
			return value;
		}

		public static ChatRoomSettings Load()
		{
			return Resources.Load<ChatRoomSettings>("ChatRoomSettings") ?? ScriptableObject.CreateInstance<ChatRoomSettings>();
		}

		public static void Preload()
		{
			if (_instance == null)
			{
				_instance = Load();
			}
		}
	}
}
