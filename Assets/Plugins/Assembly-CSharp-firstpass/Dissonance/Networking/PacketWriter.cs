using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking.Client;

namespace Dissonance.Networking
{
	internal struct PacketWriter
	{
		private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(PacketWriter).Name);

		internal const ushort Magic = 35783;

		private readonly ArraySegment<byte> _array;

		private int _count;

		public ArraySegment<byte> Written
		{
			get
			{
				return new ArraySegment<byte>(_array.Array, _array.Offset, _count);
			}
		}

		public PacketWriter([NotNull] byte[] array)
			: this(new ArraySegment<byte>(array))
		{
		}

		public PacketWriter(ArraySegment<byte> array)
		{
			if (array.Array == null)
			{
				throw new ArgumentNullException("array");
			}
			_array = array;
			_count = 0;
		}

		private void Check(int count, string type)
		{
			if (_array.Count - count - _count < 0)
			{
				throw Log.CreatePossibleBugException(string.Format("Insufficient space in packet writer to write {0}", type), "ED58BC0A-CAD2-4AFB-BFDD-B5AF5BF7DDDB");
			}
		}

		public PacketWriter FastWriteByte(byte b)
		{
			_array.Array[_array.Offset + _count] = b;
			_count++;
			return this;
		}

		public PacketWriter Write(byte b)
		{
			Check(1, "byte");
			return FastWriteByte(b);
		}

		public PacketWriter Write(ushort u)
		{
			Check(2, "ushort");
			Union16 union = new Union16
			{
				UInt16 = u
			};
			FastWriteByte(union.MSB);
			FastWriteByte(union.LSB);
			return this;
		}

		public PacketWriter Write(uint u)
		{
			Check(4, "uint");
			Union32 union = new Union32
			{
				UInt32 = u
			};
			byte b;
			byte b2;
			byte b3;
			byte b4;
			union.GetBytesInNetworkOrder(out b, out b2, out b3, out b4);
			Write(b);
			Write(b2);
			Write(b3);
			Write(b4);
			return this;
		}

		public PacketWriter Write([CanBeNull] string s)
		{
			if (s == null)
			{
				Write((ushort)0);
				return this;
			}
			if (s.Length > 65535)
			{
				throw new ArgumentException("Cannot encode strings with more than 65535 characters");
			}
			Check(s.Length + 2, "string");
			int byteCount = Encoding.UTF8.GetByteCount(s);
			Check(byteCount + 2, "string");
			int bytes = Encoding.UTF8.GetBytes(s, 0, s.Length, _array.Array, _array.Offset + _count + 2);
			if (bytes > 65535)
			{
				throw new ArgumentException("Cannot encode strings which encode to more than 65535 UTF8 bytes");
			}
			Write((ushort)(bytes + 1));
			_count += bytes;
			return this;
		}

		public PacketWriter Write(ArraySegment<byte> data)
		{
			Check(data.Count + 2, "ArraySegment<byte>");
			Write((ushort)data.Count);
			data.CopyTo(_array.Array, _array.Offset + _count);
			_count += data.Count;
			return this;
		}

		public PacketWriter Write(CodecSettings codecSettings)
		{
			Write((byte)codecSettings.Codec);
			Write(codecSettings.FrameSize);
			Write((uint)codecSettings.SampleRate);
			return this;
		}

		public PacketWriter Write(string playerName, ushort playerId, CodecSettings codecSettings)
		{
			Write(playerName);
			Write(playerId);
			Write(codecSettings);
			return this;
		}

		internal PacketWriter WriteMagic()
		{
			return Write(35783);
		}

		public PacketWriter WriteHandshakeRequest([NotNull] string name, CodecSettings codecSettings)
		{
			WriteMagic();
			Write(4);
			Write(codecSettings);
			Write(name);
			return this;
		}

		public PacketWriter WriteHandshakeResponse<TPeer>(uint session, ushort clientId, [NotNull] List<ClientInfo<TPeer>> clients, [NotNull] Dictionary<string, List<ClientInfo<TPeer>>> peersByRoom)
		{
			if (clients == null)
			{
				throw new ArgumentNullException("clients");
			}
			if (peersByRoom == null)
			{
				throw new ArgumentNullException("peersByRoom");
			}
			WriteMagic();
			Write(5);
			Write(session);
			Write(clientId);
			Write((ushort)clients.Count);
			foreach (ClientInfo<TPeer> client in clients)
			{
				Write(client.PlayerName, client.PlayerId, client.CodecSettings);
			}
			Write((ushort)peersByRoom.Count);
			foreach (KeyValuePair<string, List<ClientInfo<TPeer>>> item in peersByRoom)
			{
				Write(item.Key);
			}
			Write((ushort)peersByRoom.Count);
			using (Dictionary<string, List<ClientInfo<TPeer>>>.Enumerator enumerator3 = peersByRoom.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					ushort u = enumerator3.Current.Key.ToRoomId();
					List<ClientInfo<TPeer>> value = enumerator3.Current.Value;
					Write(u);
					Write((byte)value.Count);
					for (int i = 0; i < value.Count; i++)
					{
						Write(value[i].PlayerId);
					}
				}
			}
			return this;
		}

		public PacketWriter WriteHandshakeP2P(uint session, ushort peerId)
		{
			WriteMagic();
			Write(11);
			Write(session);
			Write(peerId);
			return this;
		}

		public PacketWriter WriteClientState(uint session, [NotNull] string name, ushort clientId, CodecSettings codecSettings, [NotNull] Rooms rooms)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name", "Attempted to serialize ClientState with a null name");
			}
			if (rooms == null)
			{
				throw new ArgumentNullException("rooms");
			}
			return WriteClientState(session, name, clientId, codecSettings, rooms.Memberships);
		}

		public PacketWriter WriteClientState(uint session, [NotNull] string name, ushort clientId, CodecSettings codecSettings, [NotNull] ReadOnlyCollection<string> rooms)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name", "Attempted to serialize ClientState with a null name");
			}
			if (rooms == null)
			{
				throw new ArgumentNullException("rooms");
			}
			WriteMagic();
			Write(1);
			Write(session);
			Write(name, clientId, codecSettings);
			Write((ushort)rooms.Count);
			for (int i = 0; i < rooms.Count; i++)
			{
				Write(rooms[i]);
			}
			return this;
		}

		public PacketWriter WriteRemoveClient(uint session, ushort clientId)
		{
			WriteMagic();
			Write(10);
			Write(session);
			Write(clientId);
			return this;
		}

		internal PacketWriter WriteVoiceData(uint session, ushort senderId, ushort sequenceNumber, byte channelSession, [NotNull] IList<OpenChannel> channels, ArraySegment<byte> encodedAudio)
		{
			if (channels == null)
			{
				throw new ArgumentNullException("channels");
			}
			if (encodedAudio.Array == null)
			{
				throw new ArgumentNullException("encodedAudio");
			}
			WriteMagic();
			Write(2);
			Write(session);
			Write(senderId);
			Write(VoicePacketOptions.Pack(channelSession).Bitfield);
			Write(sequenceNumber);
			Write((ushort)channels.Count);
			for (int i = 0; i < channels.Count; i++)
			{
				OpenChannel openChannel = channels[i];
				Write(openChannel.Bitfield);
				Write(openChannel.Recipient);
			}
			Write(encodedAudio);
			return this;
		}

		internal PacketWriter WriteTextPacket(uint session, ushort senderId, ChannelType recipient, ushort target, string data)
		{
			WriteMagic();
			Write(3);
			Write(session);
			Write((byte)recipient);
			Write(senderId);
			Write(target);
			Write(data);
			return this;
		}

		public PacketWriter WriteErrorWrongSession(uint session)
		{
			WriteMagic();
			Write(6);
			Write(session);
			return this;
		}

		public PacketWriter WriteRelay<TPeer>(uint session, [NotNull] List<ClientInfo<TPeer>> destinations, ArraySegment<byte> segment, bool reliable)
		{
			if (destinations == null)
			{
				throw new ArgumentNullException("destinations");
			}
			if (segment.Array == null)
			{
				throw new ArgumentNullException("segment");
			}
			WriteMagic();
			Write((byte)((!reliable) ? 8 : 7));
			Write(session);
			Write((byte)destinations.Count);
			for (int i = 0; i < destinations.Count; i++)
			{
				Write(destinations[i].PlayerId);
			}
			Write(segment);
			return this;
		}

		public PacketWriter WriteDeltaChannelState(uint session, bool joined, ushort peer, [NotNull] string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			WriteMagic();
			Write(9);
			Write(session);
			Write((byte)(joined ? 1u : 0u));
			Write(peer);
			Write(name);
			return this;
		}
	}
}
