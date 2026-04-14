using System;
using Dissonance.Extensions;

namespace Dissonance.Audio.Playback
{
	public struct SessionContext : IEquatable<SessionContext>
	{
		public readonly string PlayerName;

		public readonly uint Id;

		public SessionContext([NotNull] string playerName, uint id)
		{
			if (playerName == null)
			{
				throw new ArgumentNullException("playerName", "Cannot create a session context with a null player name");
			}
			PlayerName = playerName;
			Id = id;
		}

		public bool Equals(SessionContext other)
		{
			return string.Equals(PlayerName, other.PlayerName) && Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is SessionContext && Equals((SessionContext)obj);
		}

		public override int GetHashCode()
		{
			return (PlayerName.GetFnvHashCode() * 397) ^ (int)Id;
		}
	}
}
