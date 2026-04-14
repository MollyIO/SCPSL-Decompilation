using System.Collections.Generic;

namespace Dissonance.Audio.Playback
{
	internal interface IRemoteChannelProvider
	{
		void GetRemoteChannels([NotNull] List<RemoteChannel> output);
	}
}
