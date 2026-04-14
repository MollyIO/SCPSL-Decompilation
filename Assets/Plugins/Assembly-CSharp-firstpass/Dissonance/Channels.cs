using System;
using System.Collections.Generic;
using Dissonance.Audio.Capture;
using Dissonance.Datastructures;

namespace Dissonance
{
	public abstract class Channels<T, TId> where T : IChannel<TId>, IEquatable<T> where TId : IEquatable<TId>
	{
		protected readonly Log Log;

		private readonly Dictionary<ushort, T> _openChannelsBySubId;

		private readonly Pool<ChannelProperties> _propertiesPool;

		private ushort _nextId;

		public int Count
		{
			get
			{
				return _openChannelsBySubId.Count;
			}
		}

		public event Action<TId, ChannelProperties> OpenedChannel;

		public event Action<TId, ChannelProperties> ClosedChannel;

		internal Channels([NotNull] IChannelPriorityProvider priorityProvider)
		{
			if (priorityProvider == null)
			{
				throw new ArgumentNullException("priorityProvider");
			}
			Log = Logs.Create(LogCategory.Core, GetType().Name);
			_openChannelsBySubId = new Dictionary<ushort, T>();
			_propertiesPool = new Pool<ChannelProperties>(64, () => new ChannelProperties(priorityProvider));
		}

		[NotNull]
		protected abstract T CreateChannel(ushort subscriptionId, TId channelId, ChannelProperties properties);

		public bool Contains([NotNull] T item)
		{
			return _openChannelsBySubId.ContainsKey(item.SubscriptionId);
		}

		[NotNull]
		public T Open([NotNull] TId id, bool positional = false, ChannelPriority priority = ChannelPriority.Default, float amplitudeMultiplier = 1f)
		{
			if (EqualityComparer<TId>.Default.Equals(id, default(TId)))
			{
				throw new ArgumentNullException("id", "Cannot open a channel with a null ID");
			}
			if (_openChannelsBySubId.Count >= 65535)
			{
				throw Log.CreateUserErrorException("Attempted to open 65535 channels", "Opening too many speech channels without closing them", "https://dissonance.readthedocs.io/en/latest/Tutorials/Script-Controlled-Speech/", "7564ECCA-73C2-4720-B4C0-B873E63216AD");
			}
			ushort num;
			do
			{
				num = _nextId++;
				if (num == 0)
				{
					num++;
				}
			}
			while (_openChannelsBySubId.ContainsKey(num));
			ChannelProperties channelProperties = _propertiesPool.Get();
			channelProperties.Id = num;
			channelProperties.Positional = positional;
			channelProperties.Priority = priority;
			channelProperties.AmplitudeMultiplier = amplitudeMultiplier;
			T val = CreateChannel(num, id, channelProperties);
			_openChannelsBySubId.Add(val.SubscriptionId, val);
			Action<TId, ChannelProperties> action = this.OpenedChannel;
			if (action != null)
			{
				action(val.TargetId, val.Properties);
			}
			return val;
		}

		public bool Close([NotNull] T channel)
		{
			if (EqualityComparer<T>.Default.Equals(channel, default(T)))
			{
				throw new ArgumentNullException("channel", "Cannot close a null channel");
			}
			bool flag = _openChannelsBySubId.Remove(channel.SubscriptionId);
			if (flag)
			{
				channel.Properties.Id = 0;
				_propertiesPool.Put(channel.Properties);
				Action<TId, ChannelProperties> action = this.ClosedChannel;
				if (action != null)
				{
					action(channel.TargetId, channel.Properties);
				}
			}
			return flag;
		}

		internal void Refresh()
		{
			using (Dictionary<ushort, T>.Enumerator enumerator = _openChannelsBySubId.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (this.ClosedChannel != null)
					{
						this.ClosedChannel(enumerator.Current.Value.TargetId, enumerator.Current.Value.Properties);
					}
				}
			}
			using (Dictionary<ushort, T>.Enumerator enumerator2 = _openChannelsBySubId.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (this.OpenedChannel != null)
					{
						this.OpenedChannel(enumerator2.Current.Value.TargetId, enumerator2.Current.Value.Properties);
					}
				}
			}
		}

		public Dictionary<ushort, T>.Enumerator GetEnumerator()
		{
			return _openChannelsBySubId.GetEnumerator();
		}
	}
}
