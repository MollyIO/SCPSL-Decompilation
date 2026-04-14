using System;
using System.Runtime.InteropServices;

namespace Dissonance.Datastructures
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct Union32
	{
		[FieldOffset(0)]
		private uint _uint;

		[FieldOffset(0)]
		private byte _byte1;

		[FieldOffset(1)]
		private byte _byte2;

		[FieldOffset(2)]
		private byte _byte3;

		[FieldOffset(3)]
		private byte _byte4;

		public uint UInt32
		{
			get
			{
				return _uint;
			}
			set
			{
				_uint = value;
			}
		}

		public void SetBytesFromNetworkOrder(byte b1, byte b2, byte b3, byte b4)
		{
			if (BitConverter.IsLittleEndian)
			{
				_byte1 = b4;
				_byte2 = b3;
				_byte3 = b2;
				_byte4 = b1;
			}
			else
			{
				_byte1 = b1;
				_byte2 = b2;
				_byte3 = b3;
				_byte4 = b4;
			}
		}

		public void GetBytesInNetworkOrder(out byte b1, out byte b2, out byte b3, out byte b4)
		{
			if (BitConverter.IsLittleEndian)
			{
				b4 = _byte1;
				b3 = _byte2;
				b2 = _byte3;
				b1 = _byte4;
			}
			else
			{
				b1 = _byte1;
				b2 = _byte2;
				b3 = _byte3;
				b4 = _byte4;
			}
		}
	}
}
