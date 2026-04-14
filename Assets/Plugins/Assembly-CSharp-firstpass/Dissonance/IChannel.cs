using System;

namespace Dissonance
{
	public interface IChannel<T> : IDisposable
	{
		T TargetId { get; }

		ushort SubscriptionId { get; }

		[NotNull]
		ChannelProperties Properties { get; }
	}
}
