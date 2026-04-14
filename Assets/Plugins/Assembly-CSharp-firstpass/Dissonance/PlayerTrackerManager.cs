using System;
using System.Collections.Generic;

namespace Dissonance
{
	internal class PlayerTrackerManager
	{
		private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(PlayerTrackerManager).Name);

		private readonly Dictionary<string, IDissonancePlayer> _unlinkedPlayerTrackers = new Dictionary<string, IDissonancePlayer>();

		private readonly PlayerCollection _players;

		public PlayerTrackerManager([NotNull] PlayerCollection players)
		{
			if (players == null)
			{
				throw new ArgumentNullException("players");
			}
			_players = players;
		}

		public void AddPlayer([NotNull] VoicePlayerState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state", "Cannot start tracking a null player");
			}
			IDissonancePlayer value;
			if (_unlinkedPlayerTrackers.TryGetValue(state.Name, out value))
			{
				state.Tracker = value;
				_unlinkedPlayerTrackers.Remove(state.Name);
			}
		}

		public void RemovePlayer([NotNull] VoicePlayerState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state", "Cannot stop tracking a null player");
			}
			IDissonancePlayer tracker = state.Tracker;
			if (tracker != null)
			{
				_unlinkedPlayerTrackers.Add(tracker.PlayerId, tracker);
			}
			state.Tracker = null;
		}

		public void AddTracker([NotNull] IDissonancePlayer player)
		{
			if (player == null)
			{
				throw new ArgumentNullException("player", "Cannot track a null player");
			}
			VoicePlayerState state;
			if (_players.TryGet(player.PlayerId, out state))
			{
				state.Tracker = player;
			}
			else
			{
				_unlinkedPlayerTrackers.Add(player.PlayerId, player);
			}
		}

		public void RemoveTracker([NotNull] IDissonancePlayer player)
		{
			if (player == null)
			{
				throw new ArgumentNullException("player", "Cannot stop tracking a null player");
			}
			VoicePlayerState state;
			if (!_unlinkedPlayerTrackers.Remove(player.PlayerId) && _players.TryGet(player.PlayerId, out state))
			{
				state.Tracker = null;
			}
		}
	}
}
