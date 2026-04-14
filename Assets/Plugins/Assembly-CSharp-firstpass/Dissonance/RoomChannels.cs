using Dissonance.Audio.Capture;

namespace Dissonance
{
	public sealed class RoomChannels : Channels<RoomChannel, string>
	{
		internal RoomChannels([NotNull] IChannelPriorityProvider priorityProvider)
			: base(priorityProvider)
		{
			base.OpenedChannel += delegate
			{
			};
			base.ClosedChannel += delegate
			{
			};
		}

		protected override RoomChannel CreateChannel(ushort subscriptionId, string channelId, ChannelProperties properties)
		{
			return new RoomChannel(subscriptionId, channelId, this, properties);
		}
	}
}
