using System;
using System.Collections.Generic;
using Dissonance.Audio.Codecs;
using Dissonance.Datastructures;
using Dissonance.Networking;

namespace Dissonance.Audio.Playback
{
	internal class SpeechSessionStream : IJitterEstimator
	{
		private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof(SpeechSessionStream).Name);

		private readonly Queue<SpeechSession> _awaitingActivation;

		private readonly IVolumeProvider _volumeProvider;

		private DateTime? _queueHeadFirstDequeueAttempt;

		private DecoderPipeline _active;

		private uint _currentId;

		private string _playerName;

		private readonly WindowDeviationCalculator _arrivalJitterMeter = new WindowDeviationCalculator(128u);

		private static readonly Dictionary<FrameFormat, ConcurrentPool<DecoderPipeline>> FreePipelines = new Dictionary<FrameFormat, ConcurrentPool<DecoderPipeline>>();

		float IJitterEstimator.Jitter
		{
			get
			{
				return _arrivalJitterMeter.StdDev;
			}
		}

		float IJitterEstimator.Confidence
		{
			get
			{
				return _arrivalJitterMeter.Confidence;
			}
		}

		public string PlayerName
		{
			get
			{
				return _playerName;
			}
			set
			{
				if (_playerName != value)
				{
					_playerName = value;
					_arrivalJitterMeter.Clear();
				}
			}
		}

		public SpeechSessionStream(IVolumeProvider volumeProvider)
		{
			_volumeProvider = volumeProvider;
			_awaitingActivation = new Queue<SpeechSession>();
		}

		public void StartSession(FrameFormat format, DateTime? now = null, [CanBeNull] IJitterEstimator jitter = null)
		{
			if (PlayerName == null)
			{
				throw Log.CreatePossibleBugException("Attempted to `StartSession` but `PlayerName` is null", "0C0F3731-8D6B-43F6-87C1-33CEC7A26804");
			}
			_active = GetOrCreateDecoderPipeline(format, _volumeProvider);
			SpeechSession item = SpeechSession.Create(new SessionContext(PlayerName, _currentId++), jitter ?? this, _active, _active, (!now.HasValue) ? DateTime.UtcNow : now.Value);
			_awaitingActivation.Enqueue(item);
		}

		public SpeechSession? TryDequeueSession(DateTime? now = null)
		{
			DateTime dateTime = ((!now.HasValue) ? DateTime.UtcNow : now.Value);
			if (_awaitingActivation.Count > 0)
			{
				if (!_queueHeadFirstDequeueAttempt.HasValue)
				{
					_queueHeadFirstDequeueAttempt = dateTime;
				}
				SpeechSession value = _awaitingActivation.Peek();
				if (value.TargetActivationTime < dateTime)
				{
					value.Prepare(dateTime, _queueHeadFirstDequeueAttempt.Value);
					_awaitingActivation.Dequeue();
					_queueHeadFirstDequeueAttempt = null;
					return value;
				}
			}
			return null;
		}

		public void ReceiveFrame(VoicePacket packet, DateTime? now = null)
		{
			if (packet.SenderPlayerId != PlayerName)
			{
				throw Log.CreatePossibleBugException(string.Format("Attempted to deliver voice from player {0} to playback queue for player {1}", packet.SenderPlayerId, PlayerName), "F55DB7D5-621B-4F5B-8C19-700B1FBC9871");
			}
			float added = _active.Push(packet, (!now.HasValue) ? DateTime.UtcNow : now.Value);
			_arrivalJitterMeter.Update(added);
		}

		public void StopSession(bool logNoSessionError = true)
		{
			if (_active != null)
			{
				_active.Stop();
			}
			else if (logNoSessionError)
			{
				Log.Warn(Log.PossibleBugMessage("Attempted to stop a session, but there is no active session", "6DB702AA-D683-47AA-9544-BE4857EF8160"));
			}
		}

		[NotNull]
		private static DecoderPipeline GetOrCreateDecoderPipeline(FrameFormat format, [NotNull] IVolumeProvider volume)
		{
			if (volume == null)
			{
				throw new ArgumentNullException("volume");
			}
			ConcurrentPool<DecoderPipeline> value;
			if (!FreePipelines.TryGetValue(format, out value))
			{
				value = new ConcurrentPool<DecoderPipeline>(3, delegate
				{
					IVoiceDecoder decoder = DecoderFactory.Create(format);
					return new DecoderPipeline(decoder, format.FrameSize, delegate(DecoderPipeline p)
					{
						p.Reset();
						Recycle(format, p);
					});
				});
				FreePipelines[format] = value;
			}
			DecoderPipeline decoderPipeline = value.Get();
			decoderPipeline.Reset();
			decoderPipeline.VolumeProvider = volume;
			return decoderPipeline;
		}

		private static void Recycle(FrameFormat format, DecoderPipeline pipeline)
		{
			ConcurrentPool<DecoderPipeline> value;
			if (!FreePipelines.TryGetValue(format, out value))
			{
				Log.Warn(Log.PossibleBugMessage("Tried to recycle a pipeline but the pool for this pipeline format does not exist", "A6212BCF-9318-4224-B69F-BA4B5A651785"));
			}
			else
			{
				value.Put(pipeline);
			}
		}
	}
}
