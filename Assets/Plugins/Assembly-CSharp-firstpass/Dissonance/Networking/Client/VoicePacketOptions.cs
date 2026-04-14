namespace Dissonance.Networking.Client
{
	internal struct VoicePacketOptions
	{
		public const int ChannelSessionRange = 4;

		private readonly byte _bitfield;

		public byte ChannelSession
		{
			get
			{
				return (byte)(_bitfield & 3);
			}
		}

		public byte Bitfield
		{
			get
			{
				return _bitfield;
			}
		}

		private VoicePacketOptions(byte bitfield)
		{
			_bitfield = bitfield;
		}

		public static VoicePacketOptions Unpack(byte bitfield)
		{
			return new VoicePacketOptions(bitfield);
		}

		public static VoicePacketOptions Pack(byte channelSession)
		{
			byte bitfield = (byte)(0 | (channelSession % 4));
			return new VoicePacketOptions(bitfield);
		}
	}
}
