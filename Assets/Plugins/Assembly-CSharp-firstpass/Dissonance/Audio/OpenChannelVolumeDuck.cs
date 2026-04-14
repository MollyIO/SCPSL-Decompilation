using Dissonance.Audio.Playback;
using Dissonance.Config;

namespace Dissonance.Audio
{
	public class OpenChannelVolumeDuck : IVolumeProvider
	{
		private readonly RoomChannels _rooms;

		private readonly PlayerChannels _players;

		private volatile float _targetVolume = 1f;

		public float TargetVolume
		{
			get
			{
				return _targetVolume;
			}
		}

		public OpenChannelVolumeDuck(RoomChannels rooms, PlayerChannels players)
		{
			_rooms = rooms;
			_players = players;
		}

		public void Update(bool isMuted)
		{
			UpdateTargetVolume(isMuted);
		}

		private void UpdateTargetVolume(bool isMuted)
		{
			bool flag = !isMuted && (_rooms.Count > 0 || _players.Count > 0);
			_targetVolume = ((!flag) ? 1f : VoiceSettings.Instance.VoiceDuckLevel);
		}
	}
}
