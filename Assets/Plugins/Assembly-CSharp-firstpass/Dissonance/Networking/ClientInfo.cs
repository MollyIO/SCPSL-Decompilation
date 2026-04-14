using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dissonance.Extensions;
using Dissonance.Networking.Client;

namespace Dissonance.Networking
{
	internal struct ClientInfo
	{
		public string PlayerName { get; private set; }

		public ushort PlayerId { get; private set; }

		public CodecSettings CodecSettings { get; private set; }

		public ClientInfo(string playerName, ushort playerId, CodecSettings codecSettings)
		{
			this = default(ClientInfo);
			PlayerName = playerName;
			PlayerId = playerId;
			CodecSettings = codecSettings;
		}
	}
	public class ClientInfo<TPeer> : IEquatable<ClientInfo<TPeer>>
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(ClientInfo<TPeer>).Name);

		private readonly string _playerName;

		private readonly ushort _playerId;

		private readonly CodecSettings _codecSettings;

		private readonly List<string> _rooms = new List<string>();

		private readonly ReadOnlyCollection<string> _roomsReadonly;

		[NotNull]
		public string PlayerName
		{
			get
			{
				return _playerName;
			}
		}

		public ushort PlayerId
		{
			get
			{
				return _playerId;
			}
		}

		public CodecSettings CodecSettings
		{
			get
			{
				return _codecSettings;
			}
		}

		[NotNull]
		internal ReadOnlyCollection<string> Rooms
		{
			get
			{
				return _roomsReadonly;
			}
		}

		[CanBeNull]
		public TPeer Connection { get; internal set; }

		public bool IsConnected { get; internal set; }

		internal PeerVoiceReceiver VoiceReceiver { get; set; }

		public ClientInfo(string playerName, ushort playerId, CodecSettings codecSettings, [CanBeNull] TPeer connection)
		{
			_roomsReadonly = new ReadOnlyCollection<string>(_rooms);
			_playerName = playerName;
			_playerId = playerId;
			_codecSettings = codecSettings;
			Connection = connection;
			IsConnected = true;
		}

		public override string ToString()
		{
			return string.Format("Client '{0}'/{1} {2}", PlayerName, PlayerId, Connection);
		}

		public bool Equals(ClientInfo<TPeer> other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			return string.Equals(_playerName, other._playerName) && _playerId == other._playerId;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((ClientInfo<TPeer>)obj);
		}

		public override int GetHashCode()
		{
			return (_playerName.GetFnvHashCode() * 397) ^ _playerId.GetHashCode();
		}

		public bool AddRoom([NotNull] string roomName)
		{
			if (roomName == null)
			{
				throw new ArgumentNullException("roomName");
			}
			int num = _rooms.BinarySearch(roomName);
			if (num < 0)
			{
				_rooms.Insert(~num, roomName);
				return true;
			}
			return false;
		}

		public bool RemoveRoom([NotNull] string roomName)
		{
			if (roomName == null)
			{
				throw new ArgumentNullException("roomName");
			}
			int num = _rooms.BinarySearch(roomName);
			if (num >= 0)
			{
				_rooms.RemoveAt(num);
				return true;
			}
			return false;
		}
	}
}
