using System;
using Dissonance.Extensions;

namespace Dissonance
{
	public struct PlayerChannel : IChannel<string>, IEquatable<PlayerChannel>, IDisposable
	{
		private readonly ushort _subscriptionId;

		private readonly string _playerId;

		private readonly ChannelProperties _properties;

		private readonly PlayerChannels _channels;

		ChannelProperties IChannel<string>.Properties
		{
			get
			{
				return _properties;
			}
		}

		public ushort SubscriptionId
		{
			get
			{
				return _subscriptionId;
			}
		}

		[NotNull]
		public string TargetId
		{
			get
			{
				return _playerId;
			}
		}

		[NotNull]
		internal ChannelProperties Properties
		{
			get
			{
				return _properties;
			}
		}

		public bool IsOpen
		{
			get
			{
				return _channels.Contains(this);
			}
		}

		public bool Positional
		{
			get
			{
				CheckValidProperties();
				return _properties.Positional;
			}
			set
			{
				CheckValidProperties();
				_properties.Positional = value;
			}
		}

		public ChannelPriority Priority
		{
			get
			{
				CheckValidProperties();
				return _properties.Priority;
			}
			set
			{
				CheckValidProperties();
				_properties.Priority = value;
			}
		}

		public float Volume
		{
			get
			{
				CheckValidProperties();
				return _properties.AmplitudeMultiplier;
			}
			set
			{
				CheckValidProperties();
				_properties.AmplitudeMultiplier = value;
			}
		}

		internal PlayerChannel(ushort subscriptionId, string playerId, PlayerChannels channels, ChannelProperties properties)
		{
			_subscriptionId = subscriptionId;
			_playerId = playerId;
			_channels = channels;
			_properties = properties;
		}

		public void Dispose()
		{
			_channels.Close(this);
		}

		private void CheckValidProperties()
		{
			if (_properties.Id != _subscriptionId)
			{
				throw new DissonanceException("Cannot access channel properties on a closed channel.");
			}
		}

		public bool Equals(PlayerChannel other)
		{
			return _subscriptionId == other._subscriptionId && string.Equals(_playerId, other._playerId);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is PlayerChannel && Equals((PlayerChannel)obj);
		}

		public override int GetHashCode()
		{
			return (_subscriptionId.GetHashCode() * 397) ^ _playerId.GetFnvHashCode();
		}
	}
}
