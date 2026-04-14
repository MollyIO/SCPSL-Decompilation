using System;
using System.Collections.Generic;
using System.Text;
using Dissonance.Audio.Codecs;
using Dissonance.Datastructures;
using Dissonance.Networking.Client;

namespace Dissonance.Networking
{
	internal struct PacketReader
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PacketReader).Name);

		private readonly ArraySegment<byte> _array;

		private int _count;

		public ArraySegment<byte> Read
		{
			get
			{
				return new ArraySegment<byte>(_array.Array, _array.Offset, _count);
			}
		}

		public ArraySegment<byte> Unread
		{
			get
			{
				return new ArraySegment<byte>(_array.Array, _array.Offset + _count, _array.Count - _count);
			}
		}

		public ArraySegment<byte> All
		{
			get
			{
				return _array;
			}
		}

		public PacketReader(ArraySegment<byte> array)
		{
			if (array.Array == null)
			{
				throw new ArgumentNullException("array");
			}
			_array = array;
			_count = 0;
		}

		public PacketReader([NotNull] byte[] array)
			: this(new ArraySegment<byte>(array))
		{
		}

		private void Check(int count, string type)
		{
			if (_array.Count - count - _count < 0)
			{
				throw Log.CreatePossibleBugException(string.Format("Insufficient space in packet reader to read {0}", type), "4AFBC61A-77D4-43B8-878F-796F0D921184");
			}
		}

		private byte FastReadByte()
		{
			_count++;
			return _array.Array[_array.Offset + _count - 1];
		}

		public byte ReadByte()
		{
			Check(1, "byte");
			return FastReadByte();
		}

		public ushort ReadUInt16()
		{
			Check(2, "ushort");
			Union16 union = new Union16
			{
				MSB = FastReadByte(),
				LSB = FastReadByte()
			};
			return union.UInt16;
		}

		public uint ReadUInt32()
		{
			Check(4, "uint");
			Union32 union = default(Union32);
			union.SetBytesFromNetworkOrder(FastReadByte(), FastReadByte(), FastReadByte(), FastReadByte());
			return union.UInt32;
		}

		public ArraySegment<byte> ReadByteSegment()
		{
			ushort num = ReadUInt16();
			Check(num, "byte[]");
			ArraySegment<byte> result = new ArraySegment<byte>(_array.Array, Unread.Offset, num);
			_count += num;
			return result;
		}

		[CanBeNull]
		public string ReadString()
		{
			ushort num = ReadUInt16();
			if (num == 0)
			{
				return null;
			}
			num--;
			Check(num, "string");
			ArraySegment<byte> unread = Unread;
			string result = Encoding.UTF8.GetString(unread.Array, unread.Offset, num);
			_count += num;
			return result;
		}

		public CodecSettings ReadCodecSettings()
		{
			Codec codec = (Codec)ReadByte();
			uint frameSize = ReadUInt32();
			int sampleRate = (int)ReadUInt32();
			return new CodecSettings(codec, frameSize, sampleRate);
		}

		public ClientInfo ReadClientInfo()
		{
			string playerName = ReadString();
			ushort playerId = ReadUInt16();
			CodecSettings codecSettings = ReadCodecSettings();
			return new ClientInfo(playerName, playerId, codecSettings);
		}

		public bool ReadPacketHeader(out MessageTypes messageType)
		{
			bool flag = ReadUInt16() == 35783;
			if (flag)
			{
				messageType = (MessageTypes)ReadByte();
			}
			else
			{
				messageType = (MessageTypes)0;
			}
			return flag;
		}

		public void ReadHandshakeRequest([CanBeNull] out string name, out CodecSettings codecSettings)
		{
			codecSettings = ReadCodecSettings();
			name = ReadString();
		}

		public void ReadHandshakeResponseHeader(out uint session, out ushort clientId)
		{
			session = ReadUInt32();
			clientId = ReadUInt16();
		}

		public void ReadHandshakeResponseBody([NotNull] List<ClientInfo> clients, [NotNull] Dictionary<string, List<ushort>> outputRoomsToPeerId)
		{
			if (clients == null)
			{
				throw new ArgumentNullException("clients");
			}
			if (outputRoomsToPeerId == null)
			{
				throw new ArgumentNullException("outputRoomsToPeerId");
			}
			ushort num = ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				ClientInfo item = ReadClientInfo();
				clients.Add(item);
			}
			Dictionary<ushort, string> dictionary = new Dictionary<ushort, string>();
			ushort num2 = ReadUInt16();
			for (int j = 0; j < num2; j++)
			{
				string text = ReadString();
				if (!Log.AssertAndLogWarn(text != null, "Read a null room name in handshake packet (potentially corrupt packet)"))
				{
					dictionary[text.ToRoomId()] = text;
				}
			}
			foreach (KeyValuePair<string, List<ushort>> item2 in outputRoomsToPeerId)
			{
				item2.Value.Clear();
			}
			ushort num3 = ReadUInt16();
			for (int k = 0; k < num3; k++)
			{
				ushort num4 = ReadUInt16();
				byte b = ReadByte();
				string value;
				Log.AssertAndThrowPossibleBug(dictionary.TryGetValue(num4, out value), "C8E9EBED-2A46-4207-A050-0ABFE00BA9E8", "Could not find room name in handshake for ID:{0}", num4);
				List<ushort> value2;
				if (!outputRoomsToPeerId.TryGetValue(value, out value2))
				{
					value2 = (outputRoomsToPeerId[value] = new List<ushort>());
				}
				for (int l = 0; l < b; l++)
				{
					value2.Add(ReadUInt16());
				}
			}
		}

		public void ReadhandshakeP2P(out ushort peerId)
		{
			peerId = ReadUInt16();
		}

		public ClientInfo ReadClientStateHeader()
		{
			ClientInfo result = ReadClientInfo();
			if (result.PlayerName == null)
			{
				throw Log.CreatePossibleBugException("Deserialized a ClientState packet with a null client Name", "5F77FC4F-4110-4A6F-8F96-97B393AD7324");
			}
			return result;
		}

		public void ReadClientStateRooms([NotNull][ItemNotNull] List<string> rooms)
		{
			if (rooms == null)
			{
				throw new ArgumentNullException("rooms");
			}
			rooms.Clear();
			ushort num = ReadUInt16();
			for (int i = 0; i < num; i++)
			{
				string text = ReadString();
				if (text == null)
				{
				}
				rooms.Add(text);
			}
		}

		public void ReadRemoveClient(out ushort clientId)
		{
			clientId = ReadUInt16();
		}

		public void ReadVoicePacketHeader1(out ushort senderId)
		{
			senderId = ReadUInt16();
		}

		public void ReadVoicePacketHeader2(out VoicePacketOptions options, out ushort sequenceNumber, out ushort numChannels)
		{
			options = VoicePacketOptions.Unpack(ReadByte());
			sequenceNumber = ReadUInt16();
			numChannels = ReadUInt16();
		}

		public void ReadVoicePacketChannel(out ChannelBitField bitfield, out ushort recipient)
		{
			bitfield = new ChannelBitField(ReadUInt16());
			recipient = ReadUInt16();
		}

		public TextPacket ReadTextPacket()
		{
			byte recipientType = ReadByte();
			ushort sender = ReadUInt16();
			ushort recipient = ReadUInt16();
			string text = ReadString();
			return new TextPacket(sender, (ChannelType)recipientType, recipient, text);
		}

		public uint ReadErrorWrongSession()
		{
			return ReadUInt32();
		}

		public void ReadRelay(List<ushort> destinations, out ArraySegment<byte> data)
		{
			byte b = ReadByte();
			for (int i = 0; i < b; i++)
			{
				destinations.Add(ReadUInt16());
			}
			data = ReadByteSegment();
		}

		public void ReadDeltaChannelState(out bool joined, out ushort peer, [NotNull] out string name)
		{
			joined = ReadByte() != 0;
			peer = ReadUInt16();
			name = ReadString();
			Log.AssertAndThrowPossibleBug(name != null, "51C30A8D-C665-4AFD-A88F-BC9060A4DDB9", "Read a null string from a DeltaChannelState packet");
		}
	}
}
