using System;
using System.Runtime.InteropServices;
using Dissonance.Extensions;
using Dissonance.Threading;

namespace Dissonance.Audio.Codecs.Opus
{
	internal class OpusNative
	{
		private static class OpusNativeMethods
		{
			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr opus_encoder_create(int samplingRate, int channels, int application, out int error);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern void opus_encoder_destroy(IntPtr encoder);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int opus_encode_float(IntPtr encoder, IntPtr floatPcm, int frameSize, IntPtr byteEncoded, int maxEncodedLength);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr opus_decoder_create(int samplingRate, int channels, out int error);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr opus_decoder_destroy(IntPtr decoder);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int opus_decode_float(IntPtr decoder, IntPtr byteData, int dataLength, IntPtr floatPcm, int frameSize, bool decodeFEC);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int dissonance_opus_decoder_ctl_out(IntPtr st, Ctl request, out int value);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int dissonance_opus_decoder_ctl_in(IntPtr st, Ctl request, int value);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int dissonance_opus_encoder_ctl_out(IntPtr st, Ctl request, out int value);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern int dissonance_opus_encoder_ctl_in(IntPtr st, Ctl request, int value);

			[DllImport("opus", CallingConvention = CallingConvention.Cdecl)]
			internal static extern void opus_pcm_soft_clip(IntPtr pcm, int frameSize, int channels, float[] softClipMem);
		}

		private enum Ctl
		{
			SetBitrateRequest = 4002,
			GetBitrateRequest = 4003,
			SetInbandFECRequest = 4012,
			GetInbandFECRequest = 4013,
			SetPacketLossPercRequest = 4014,
			GetPacketLossPercRequest = 4015,
			ResetState = 4028
		}

		public enum Bandwidth
		{
			Narrowband = 1101,
			Mediumband = 1102,
			Wideband = 1103,
			SuperWideband = 1104,
			Fullband = 1105
		}

		private enum Application
		{
			Voip = 2048,
			Audio = 2049,
			RestrictedLowLatency = 2051
		}

		private enum OpusErrors
		{
			Ok = 0,
			BadArg = -1,
			BufferToSmall = -2,
			InternalError = -3,
			InvalidPacket = -4,
			Unimplemented = -5,
			InvalidState = -6,
			AllocFail = -7
		}

		public class OpusException : Exception
		{
			public OpusException(string message)
				: base(message)
			{
			}
		}

		public sealed class OpusEncoder : IDisposable
		{
			private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof(OpusEncoder).Name);

			private readonly LockedValue<IntPtr> _encoder;

			private static readonly float[] PermittedFrameSizesMs = new float[6] { 2.5f, 5f, 10f, 20f, 40f, 60f };

			private int _packetLoss;

			private bool _disposed;

			public int[] PermittedFrameSizes { get; private set; }

			public int Bitrate
			{
				get
				{
					int value;
					OpusCtlOut(Ctl.GetBitrateRequest, out value);
					return value;
				}
				set
				{
					OpusCtlIn(Ctl.SetBitrateRequest, value);
				}
			}

			public bool EnableForwardErrorCorrection
			{
				get
				{
					int value;
					OpusCtlOut(Ctl.GetInbandFECRequest, out value);
					return value > 0;
				}
				set
				{
					OpusCtlIn(Ctl.SetInbandFECRequest, Convert.ToInt32(value));
				}
			}

			public float PacketLoss
			{
				get
				{
					int value;
					OpusCtlOut(Ctl.GetPacketLossPercRequest, out value);
					return (float)value / 100f;
				}
				set
				{
					if (value < 0f || value > 1f)
					{
						throw new ArgumentOutOfRangeException("value", Log.PossibleBugMessage(string.Format("Packet loss percentage must be 0 <= {0} <= 1", value), "CFDF590D-C61A-4BB4-BB2D-1FAC1E59C114"));
					}
					int num = (int)(value * 100f);
					if (_packetLoss != num)
					{
						_packetLoss = num;
						OpusCtlIn(Ctl.SetPacketLossPercRequest, _packetLoss);
					}
				}
			}

			public OpusEncoder(int srcSamplingRate, int srcChannelCount)
			{
				if (srcSamplingRate != 8000 && srcSamplingRate != 12000 && srcSamplingRate != 16000 && srcSamplingRate != 24000 && srcSamplingRate != 48000)
				{
					throw new ArgumentOutOfRangeException("srcSamplingRate", Log.PossibleBugMessage("sample rate must be one of the valid values", "3F2C6D2D-338E-495E-8970-42A3C98243A5"));
				}
				if (srcChannelCount != 1 && srcChannelCount != 2)
				{
					throw new ArgumentOutOfRangeException("srcChannelCount", Log.PossibleBugMessage("channel count must be 1 or 2", "8FE1EC0F-09E0-4CE6-AFD7-04199202D45D"));
				}
				int error;
				IntPtr value = OpusNativeMethods.opus_encoder_create(srcSamplingRate, srcChannelCount, 2048, out error);
				if (error != 0)
				{
					throw new OpusException(Log.PossibleBugMessage(string.Format("Exception occured while creating encoder: {0}", (OpusErrors)error), "D77ECA73-413F-40D1-8427-CFD8A59CD5F6"));
				}
				_encoder = new LockedValue<IntPtr>(value);
				PermittedFrameSizes = new int[PermittedFrameSizesMs.Length];
				for (int i = 0; i < PermittedFrameSizesMs.Length; i++)
				{
					PermittedFrameSizes[i] = (int)((float)srcSamplingRate / 1000f * PermittedFrameSizesMs[i]);
				}
			}

			public int EncodeFloats(ArraySegment<float> sourcePcm, ArraySegment<byte> dstEncoded)
			{
				if (sourcePcm.Array == null)
				{
					throw new ArgumentNullException("sourcePcm", Log.PossibleBugMessage("source pcm must not be null", "58AE3110-8F9A-4C36-9520-B7F3383096EC"));
				}
				if (dstEncoded.Array == null)
				{
					throw new ArgumentNullException("dstEncoded", Log.PossibleBugMessage("destination must not be null", "36C327BB-A128-400D-AFB3-FF760A1562C1"));
				}
				if (Array.IndexOf(PermittedFrameSizes, sourcePcm.Count) == -1)
				{
					throw new ArgumentException(Log.PossibleBugMessage(string.Format("Incorrect frame size '{0}'", sourcePcm.Count), "6AFD9ADF-1D15-4197-99E9-5A19ECB8CD20"), "sourcePcm");
				}
				int num;
				using (LockedValue<IntPtr>.Unlocker unlocker = _encoder.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw new DissonanceException(Log.PossibleBugMessage("Attempted to access a null Opus encoder", "647001C3-39BB-418D-99EF-1D66B8EA633C"));
					}
					using (ArraySegmentExtensions.DisposableHandle disposableHandle = sourcePcm.Pin())
					{
						using (ArraySegmentExtensions.DisposableHandle disposableHandle2 = dstEncoded.Pin())
						{
							num = OpusNativeMethods.opus_encode_float(unlocker.Value, disposableHandle.Ptr, sourcePcm.Count, disposableHandle2.Ptr, dstEncoded.Count);
						}
					}
				}
				if (num < 0)
				{
					throw new OpusException(Log.PossibleBugMessage(string.Format("Encoding failed: {0}", (OpusErrors)num), "9C923F57-146B-47CB-8EEE-5BF129FA3124"));
				}
				return num;
			}

			public void Reset()
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _encoder.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw Log.CreatePossibleBugException("Attempted to access a null Opus encoder", "A86D13A5-FC58-446C-9522-DDD9D199DFA6");
					}
					OpusNativeMethods.dissonance_opus_encoder_ctl_in(unlocker.Value, Ctl.ResetState, 0);
				}
			}

			private int OpusCtlIn(Ctl ctl, int value)
			{
				int num;
				using (LockedValue<IntPtr>.Unlocker unlocker = _encoder.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "10A3BFFB-EC3B-4664-B06C-D5D42F75FE42"));
					}
					num = OpusNativeMethods.dissonance_opus_encoder_ctl_in(unlocker.Value, ctl, value);
				}
				if (num < 0)
				{
					throw new Exception(Log.PossibleBugMessage(string.Format("Encoder error (Ctl {0}): {1}", ctl, (OpusErrors)num), "4AAA9AA6-8429-4346-B939-D113206FFBA8"));
				}
				return num;
			}

			private int OpusCtlOut(Ctl ctl, out int value)
			{
				int num;
				using (LockedValue<IntPtr>.Unlocker unlocker = _encoder.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw new ObjectDisposedException("OpusEncoder", Log.PossibleBugMessage("trying to use decoder after is has been disposed", "10A3BFFB-EC3B-4664-B06C-D5D42F75FE42"));
					}
					num = OpusNativeMethods.dissonance_opus_encoder_ctl_out(unlocker.Value, ctl, out value);
				}
				if (num < 0)
				{
					throw new Exception(Log.PossibleBugMessage(string.Format("Encoder error (Ctl {0}): {1}", ctl, (OpusErrors)num), "4AAA9AA6-8429-4346-B939-D113206FFBA8"));
				}
				return num;
			}

			~OpusEncoder()
			{
				Dispose();
			}

			public void Dispose()
			{
				if (_disposed)
				{
					return;
				}
				GC.SuppressFinalize(this);
				using (LockedValue<IntPtr>.Unlocker unlocker = _encoder.Lock())
				{
					if (unlocker.Value != IntPtr.Zero)
					{
						OpusNativeMethods.opus_encoder_destroy(unlocker.Value);
						unlocker.Value = IntPtr.Zero;
					}
				}
				_disposed = true;
			}
		}

		public sealed class OpusDecoder : IDisposable
		{
			private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(OpusDecoder).Name);

			private readonly LockedValue<IntPtr> _decoder;

			private bool _disposed;

			public bool EnableForwardErrorCorrection { get; set; }

			public OpusDecoder(int outputSampleRate, int outputChannelCount)
			{
				if (outputSampleRate != 8000 && outputSampleRate != 12000 && outputSampleRate != 16000 && outputSampleRate != 24000 && outputSampleRate != 48000)
				{
					throw new ArgumentOutOfRangeException("outputSampleRate", Log.PossibleBugMessage("sample rate must be one of the valid values", "548757DF-DC64-40C9-BEAD-9826B8245A7D"));
				}
				if (outputChannelCount != 1 && outputChannelCount != 2)
				{
					throw new ArgumentOutOfRangeException("outputChannelCount", Log.PossibleBugMessage("channel count must be 1 or 2", "BA56610F-1FA3-4D68-9507-7B0DFA0E28AB"));
				}
				int error;
				_decoder = new LockedValue<IntPtr>(OpusNativeMethods.opus_decoder_create(outputSampleRate, outputChannelCount, out error));
				if (error != 0)
				{
					throw new OpusException(Log.PossibleBugMessage(string.Format("Exception occured while creating decoder: {0}", (OpusErrors)error), "6E09F275-99A1-4CD6-A36A-FA093B146B29"));
				}
			}

			~OpusDecoder()
			{
				Dispose();
			}

			public void Dispose()
			{
				if (_disposed)
				{
					return;
				}
				GC.SuppressFinalize(this);
				using (LockedValue<IntPtr>.Unlocker unlocker = _decoder.Lock())
				{
					if (unlocker.Value != IntPtr.Zero)
					{
						OpusNativeMethods.opus_decoder_destroy(unlocker.Value);
						unlocker.Value = IntPtr.Zero;
					}
				}
				_disposed = true;
			}

			public int DecodeFloats(EncodedBuffer srcEncodedBuffer, ArraySegment<float> dstBuffer)
			{
				int num;
				using (LockedValue<IntPtr>.Unlocker unlocker = _decoder.Lock())
				{
					if (unlocker.Value == IntPtr.Zero)
					{
						throw new DissonanceException(Log.PossibleBugMessage("Attempted to access a null Opus decoder", "16261551-968B-44A8-80A1-E8DFB0109469"));
					}
					using (ArraySegmentExtensions.DisposableHandle disposableHandle = dstBuffer.Pin())
					{
						if (!srcEncodedBuffer.Encoded.HasValue || (srcEncodedBuffer.PacketLost && !EnableForwardErrorCorrection))
						{
							num = OpusNativeMethods.opus_decode_float(unlocker.Value, IntPtr.Zero, 0, disposableHandle.Ptr, dstBuffer.Count, false);
						}
						else
						{
							using (ArraySegmentExtensions.DisposableHandle disposableHandle2 = srcEncodedBuffer.Encoded.Value.Pin())
							{
								num = OpusNativeMethods.opus_decode_float(unlocker.Value, disposableHandle2.Ptr, srcEncodedBuffer.Encoded.Value.Count, disposableHandle.Ptr, dstBuffer.Count, srcEncodedBuffer.PacketLost);
							}
						}
					}
				}
				if (num < 0)
				{
					if (num == -4)
					{
						if (!srcEncodedBuffer.Encoded.HasValue)
						{
							throw new OpusException(Log.PossibleBugMessage("Decoding failed: InvalidPacket. 'null' ", "03BE7561-3BCC-4F41-A7CB-C80F03981267"));
						}
						ArraySegment<byte> value = srcEncodedBuffer.Encoded.Value;
						throw new OpusException(Log.PossibleBugMessage(string.Format("Decoding failed: InvalidPacket. '{0}'", Convert.ToBase64String(value.Array, value.Offset, value.Count)), "EF4BC24C-491E-45D9-974C-FE5CB61BD54E"));
					}
					throw new OpusException(Log.PossibleBugMessage(string.Format("Decoding failed: {0} ", (OpusErrors)num), "A9C8EF2C-7830-4D8E-9D6E-EF0B9827E0A8"));
				}
				return num;
			}

			public void Reset()
			{
				using (LockedValue<IntPtr>.Unlocker unlocker = _decoder.Lock())
				{
					OpusNativeMethods.dissonance_opus_decoder_ctl_in(unlocker.Value, Ctl.ResetState, 0);
				}
			}
		}

		public sealed class OpusSoftClip
		{
			private readonly float[] _memory;

			public OpusSoftClip(int channels = 1)
			{
				if (channels <= 0)
				{
					throw new ArgumentOutOfRangeException("channels", "Channels must be > 0");
				}
				_memory = new float[channels];
			}

			public void Clip(ArraySegment<float> samples)
			{
				using (ArraySegmentExtensions.DisposableHandle disposableHandle = samples.Pin())
				{
					OpusNativeMethods.opus_pcm_soft_clip(disposableHandle.Ptr, samples.Count / _memory.Length, _memory.Length, _memory);
				}
			}

			public void Reset()
			{
				Array.Clear(_memory, 0, _memory.Length);
			}
		}
	}
}
