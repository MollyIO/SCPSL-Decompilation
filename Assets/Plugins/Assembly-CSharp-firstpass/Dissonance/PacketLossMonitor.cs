using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dissonance
{
	internal class PacketLossMonitor
	{
		private readonly ReadOnlyCollection<VoicePlayerState> _players;

		private DateTime _lastUpdatedPacketLoss = DateTime.MinValue;

		private int _lastUpdatedPlayerCount = -1;

		private readonly List<float> _tmpLossValues = new List<float>();

		public float PacketLoss { get; private set; }

		public PacketLossMonitor(ReadOnlyCollection<VoicePlayerState> players)
		{
			_players = players;
		}

		public void Update(DateTime? utcNow = null)
		{
			DateTime dateTime = ((!utcNow.HasValue) ? DateTime.UtcNow : utcNow.Value);
			if (CheckTime(dateTime) || CheckCount())
			{
				_lastUpdatedPacketLoss = dateTime;
				_lastUpdatedPlayerCount = _players.Count;
				float? num = CalculatePacketLoss();
				PacketLoss = ((!num.HasValue) ? PacketLoss : num.Value);
			}
		}

		private bool CheckTime(DateTime now)
		{
			return now - _lastUpdatedPacketLoss > TimeSpan.FromSeconds(0.5);
		}

		private bool CheckCount()
		{
			return _lastUpdatedPlayerCount != _players.Count;
		}

		private float? CalculatePacketLoss()
		{
			_tmpLossValues.Clear();
			for (int i = 0; i < _players.Count; i++)
			{
				float? packetLoss = _players[i].PacketLoss;
				if (packetLoss.HasValue)
				{
					_tmpLossValues.Add(packetLoss.Value);
				}
			}
			if (_tmpLossValues.Count == 0)
			{
				return null;
			}
			_tmpLossValues.Sort();
			int val = (int)Math.Ceiling((float)(_tmpLossValues.Count - 1) / 2f);
			return Math.Min(1f, Math.Max(0f, _tmpLossValues[Math.Min(_tmpLossValues.Count - 1, val)]));
		}
	}
}
