using System;

namespace Dissonance.Networking
{
	internal struct ChannelBitField
	{
		private const ushort TypeMask = 1;

		private const ushort PositionalMask = 2;

		private const ushort ClosureMask = 4;

		private const ushort PriorityOffset = 3;

		private const ushort PriorityMask = 24;

		private const ushort SessionIdOffset = 5;

		private const ushort SessionIdMask = 97;

		private const ushort AmplitudeOffset = 8;

		private const ushort AmplitudeMask = 65280;

		private readonly ushort _bitfield;

		public ushort Bitfield
		{
			get
			{
				return _bitfield;
			}
		}

		public ChannelType Type
		{
			get
			{
				if ((_bitfield & 1) == 1)
				{
					return ChannelType.Room;
				}
				return ChannelType.Player;
			}
		}

		public bool IsClosing
		{
			get
			{
				return (_bitfield & 4) == 4;
			}
		}

		public bool IsPositional
		{
			get
			{
				return (_bitfield & 2) == 2;
			}
		}

		public ChannelPriority Priority
		{
			get
			{
				switch ((_bitfield & 0x18) >> 3)
				{
				default:
					return ChannelPriority.Default;
				case 1:
					return ChannelPriority.Low;
				case 2:
					return ChannelPriority.Medium;
				case 3:
					return ChannelPriority.High;
				}
			}
		}

		public float AmplitudeMultiplier
		{
			get
			{
				int num = (_bitfield & 0xFF00) >> 8;
				return (float)num / 255f * 2f;
			}
		}

		public int SessionId
		{
			get
			{
				return (_bitfield & 0x61) >> 5;
			}
		}

		public ChannelBitField(ushort bitfield)
		{
			_bitfield = bitfield;
		}

		public ChannelBitField(ChannelType type, int sessionId, ChannelPriority priority, float amplitudeMult, bool positional, bool closing)
		{
			this = default(ChannelBitField);
			_bitfield = 0;
			if (type == ChannelType.Room)
			{
				_bitfield |= 1;
			}
			if (positional)
			{
				_bitfield |= 2;
			}
			if (closing)
			{
				_bitfield |= 4;
			}
			_bitfield |= PackPriority(priority);
			_bitfield |= (ushort)(sessionId % 4 << 5);
			byte b = (byte)Math.Round(Math.Min(2f, Math.Max(0f, amplitudeMult)) / 2f * 255f);
			_bitfield |= (ushort)(b << 8);
		}

		private static ushort PackPriority(ChannelPriority priority)
		{
			switch (priority)
			{
			case ChannelPriority.Low:
				return 8;
			case ChannelPriority.Medium:
				return 16;
			case ChannelPriority.High:
				return 24;
			default:
				return 0;
			}
		}
	}
}
