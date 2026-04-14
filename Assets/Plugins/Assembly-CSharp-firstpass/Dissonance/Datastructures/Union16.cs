using System;
using System.Runtime.InteropServices;

namespace Dissonance.Datastructures
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct Union16
	{
		[FieldOffset(0)]
		private ushort _ushort;

		[FieldOffset(0)]
		private byte _byte1;

		[FieldOffset(1)]
		private byte _byte2;

		public ushort UInt16
		{
			get
			{
				return _ushort;
			}
			set
			{
				_ushort = value;
			}
		}

		public byte LSB
		{
			get
			{
				return (!BitConverter.IsLittleEndian) ? _byte2 : _byte1;
			}
			set
			{
				if (BitConverter.IsLittleEndian)
				{
					_byte1 = value;
				}
				else
				{
					_byte2 = value;
				}
			}
		}

		public byte MSB
		{
			get
			{
				return (!BitConverter.IsLittleEndian) ? _byte1 : _byte2;
			}
			set
			{
				if (BitConverter.IsLittleEndian)
				{
					_byte2 = value;
				}
				else
				{
					_byte1 = value;
				}
			}
		}
	}
}
