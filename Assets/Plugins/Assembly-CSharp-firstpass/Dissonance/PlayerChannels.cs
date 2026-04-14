using Dissonance.Audio.Capture;

namespace Dissonance
{
	public sealed class PlayerChannels : Channels<PlayerChannel, string>
	{
		internal PlayerChannels(IChannelPriorityProvider priorityProvider)
			: base(priorityProvider)
		{
			base.OpenedChannel += delegate
			{
			};
			base.ClosedChannel += delegate
			{
			};
		}

		protected override PlayerChannel CreateChannel(ushort subscriptionId, string channelId, ChannelProperties properties)
		{
			return new PlayerChannel(subscriptionId, channelId, this, properties);
		}
	}
}
