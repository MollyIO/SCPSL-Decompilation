using System;
using Dissonance.Audio.Codecs;
using Dissonance.Networking;
using Dissonance.Threading;
using NAudio.Wave;

namespace Dissonance.Audio.Capture
{
	internal class EncoderPipeline : IMicrophoneSubscriber, IDisposable
	{
		private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(EncoderPipeline).Name);

		private readonly byte[] _encodedBytes;

		private readonly float[] _plainSamples;

		private readonly ReadonlyLockedValue<IVoiceEncoder> _encoder;

		private readonly ICommsNetwork _net;

		private readonly BufferedSampleProvider _input;

		private readonly Resampler _resampler;

		private readonly IFrameProvider _output;

		private readonly WaveFormat _inputFormat;

		private volatile bool _stopped;

		private volatile bool _stopping;

		private volatile bool _disposed;

		public bool Stopped
		{
			get
			{
				return _stopped;
			}
		}

		public bool Stopping
		{
			get
			{
				return _stopping;
			}
		}

		public float TransmissionPacketLoss { get; set; }

		public EncoderPipeline([NotNull] WaveFormat inputFormat, [NotNull] IVoiceEncoder encoder, [NotNull] ICommsNetwork net)
		{
			if (inputFormat == null)
			{
				throw new ArgumentNullException("inputFormat");
			}
			if (encoder == null)
			{
				throw new ArgumentNullException("encoder");
			}
			if (net == null)
			{
				throw new ArgumentNullException("net");
			}
			_net = net;
			_inputFormat = inputFormat;
			_encoder = new ReadonlyLockedValue<IVoiceEncoder>(encoder);
			_plainSamples = new float[encoder.FrameSize];
			_encodedBytes = new byte[encoder.FrameSize * 4 * 2];
			_input = new BufferedSampleProvider(_inputFormat, encoder.FrameSize * 2);
			_resampler = new Resampler(_input, encoder.SampleRate);
			_output = new SampleToFrameProvider(_resampler, (uint)encoder.FrameSize);
		}

		public void ReceiveMicrophoneData(ArraySegment<float> inputSamples, [NotNull] WaveFormat format)
		{
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			if (!format.Equals(_inputFormat))
			{
				throw new ArgumentException(string.Format("Samples expected in format {0}, but supplied with format {1}", _inputFormat, format), "format");
			}
			using (ReadonlyLockedValue<IVoiceEncoder>.Unlocker unlocker = _encoder.Lock())
			{
				IVoiceEncoder value = unlocker.Value;
				if (_disposed || _stopped)
				{
					return;
				}
				value.PacketLoss = TransmissionPacketLoss;
				int num = 0;
				while (num != inputSamples.Count)
				{
					num += _input.Write(new ArraySegment<float>(inputSamples.Array, inputSamples.Offset + num, inputSamples.Count - num));
					int num2 = EncodeFrames(value, _stopping ? 1 : int.MaxValue);
					if (num2 > 0 && _stopping)
					{
						_stopped = true;
						break;
					}
				}
			}
		}

		private int EncodeFrames([NotNull] IVoiceEncoder encoder, int maxCount)
		{
			int num = 0;
			ArraySegment<float> arraySegment = new ArraySegment<float>(_plainSamples, 0, encoder.FrameSize);
			while (_output.Read(arraySegment) && num < maxCount)
			{
				ArraySegment<byte> data = encoder.Encode(arraySegment, new ArraySegment<byte>(_encodedBytes));
				_net.SendVoice(data);
				num++;
			}
			return num;
		}

		public void Reset()
		{
			if (_disposed)
			{
				return;
			}
			using (_encoder.Lock())
			{
				_resampler.Reset();
				_input.Reset();
				_output.Reset();
				_stopping = false;
				_stopped = false;
			}
		}

		public void Stop()
		{
			using (_encoder.Lock())
			{
				_stopping = true;
			}
		}

		public void Dispose()
		{
			using (ReadonlyLockedValue<IVoiceEncoder>.Unlocker unlocker = _encoder.Lock())
			{
				_disposed = true;
				_stopping = true;
				_stopped = true;
				unlocker.Value.Dispose();
			}
		}
	}
}
