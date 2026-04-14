using System;
using System.Threading;
using Dissonance.Config;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
	public class SamplePlaybackComponent : MonoBehaviour
	{
		private static readonly Log Log = Logs.Create(LogCategory.Playback, typeof(SamplePlaybackComponent).Name);

		private static readonly TimeSpan ResetDesync = TimeSpan.FromSeconds(1.0);

		private static readonly float[] DesyncFixBuffer = new float[1024];

		private DesyncCalculator _desync;

		private long _totalSamplesRead;

		private float[] _temp;

		[CanBeNull]
		private AudioFileWriter _diagnosticOutput;

		private SessionContext _lastPlayedSessionContext;

		private readonly ReaderWriterLockSlim _sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		private volatile float _arv;

		internal bool MultiplyBySource { get; set; }

		public bool HasActiveSession
		{
			get
			{
				return Session.HasValue;
			}
		}

		public SpeechSession? Session { get; private set; }

		public TimeSpan PlaybackPosition
		{
			get
			{
				SpeechSession? session = Session;
				if (!session.HasValue)
				{
					return TimeSpan.Zero;
				}
				return TimeSpan.FromSeconds((double)Interlocked.Read(ref _totalSamplesRead) / (double)session.Value.OutputWaveFormat.SampleRate);
			}
		}

		public TimeSpan IdealPlaybackPosition
		{
			get
			{
				SpeechSession? session = Session;
				if (!session.HasValue)
				{
					return TimeSpan.Zero;
				}
				return DateTime.UtcNow - session.Value.ActivationTime;
			}
		}

		public TimeSpan Desync
		{
			get
			{
				return TimeSpan.FromMilliseconds(_desync.DesyncMilliseconds);
			}
		}

		public float CorrectedPlaybackSpeed
		{
			get
			{
				return _desync.CorrectedPlaybackSpeed;
			}
		}

		public float ARV
		{
			get
			{
				return _arv;
			}
		}

		public void Play(SpeechSession session)
		{
			if (Session.HasValue)
			{
				throw Log.CreatePossibleBugException("Attempted to play a session when one is already playing", "C4F19272-994D-4025-AAEF-37BB62685C2E");
			}
			if (DebugSettings.Instance.EnablePlaybackDiagnostics && DebugSettings.Instance.RecordFinalAudio)
			{
				string filename = string.Format("Dissonance_Diagnostics/Output_{0}_{1}_{2}", session.Context.PlayerName, session.Context.Id, DateTime.UtcNow.ToFileTime());
				Interlocked.Exchange(ref _diagnosticOutput, new AudioFileWriter(filename, session.OutputWaveFormat));
			}
			_sessionLock.EnterWriteLock();
			try
			{
				ApplyReset();
				Session = session;
			}
			finally
			{
				_sessionLock.ExitWriteLock();
			}
		}

		public void Start()
		{
			_temp = new float[AudioSettings.outputSampleRate];
		}

		public void OnEnable()
		{
			Session = null;
			ApplyReset();
		}

		public void OnDisable()
		{
			Session = null;
			ApplyReset();
		}

		public void OnAudioFilterRead(float[] data, int channels)
		{
			if (!Session.HasValue)
			{
				Array.Clear(data, 0, data.Length);
				return;
			}
			_sessionLock.EnterUpgradeableReadLock();
			try
			{
				SpeechSession? session = Session;
				if (!session.HasValue)
				{
					Array.Clear(data, 0, data.Length);
					return;
				}
				SpeechSession value = session.Value;
				if (!value.Context.Equals(_lastPlayedSessionContext))
				{
					_lastPlayedSessionContext = session.Value.Context;
					ApplyReset();
				}
				_desync.Update(IdealPlaybackPosition, PlaybackPosition);
				int deltaSamples;
				int deltaDesyncMilliseconds;
				bool flag = Skip(value, _desync.DesyncMilliseconds, out deltaSamples, out deltaDesyncMilliseconds);
				Interlocked.Add(ref _totalSamplesRead, deltaSamples);
				_desync.Skip(deltaDesyncMilliseconds);
				if (!flag)
				{
					float arv;
					int samplesRead;
					flag = Filter(value, data, channels, _temp, _diagnosticOutput, out arv, out samplesRead, MultiplyBySource);
					_arv = arv;
					Interlocked.Add(ref _totalSamplesRead, samplesRead);
				}
				if (flag)
				{
					_sessionLock.EnterWriteLock();
					try
					{
						Session = null;
					}
					finally
					{
						_sessionLock.ExitWriteLock();
					}
					ApplyReset();
					if (_diagnosticOutput != null)
					{
						_diagnosticOutput.Dispose();
						_diagnosticOutput = null;
					}
				}
			}
			finally
			{
				_sessionLock.ExitUpgradeableReadLock();
			}
		}

		private void ApplyReset()
		{
			Interlocked.Exchange(ref _totalSamplesRead, 0L);
			_arv = 0f;
			_desync = default(DesyncCalculator);
		}

		internal static bool Skip(SpeechSession session, int desyncMilliseconds, out int deltaSamples, out int deltaDesyncMilliseconds)
		{
			if ((double)desyncMilliseconds > ResetDesync.TotalMilliseconds)
			{
				Log.Warn("Playback desync ({0}ms) beyond recoverable threshold; resetting stream to current time", desyncMilliseconds);
				deltaSamples = desyncMilliseconds * session.OutputWaveFormat.SampleRate / 1000;
				deltaDesyncMilliseconds = -desyncMilliseconds;
				int num = deltaSamples;
				while (num > 0)
				{
					int num2 = Math.Min(num, DesyncFixBuffer.Length);
					if (session.Read(new ArraySegment<float>(DesyncFixBuffer, 0, num2)))
					{
						return true;
					}
					num -= num2;
				}
				return false;
			}
			if ((double)desyncMilliseconds < 0.0 - ResetDesync.TotalMilliseconds)
			{
				Log.Error("Playback desync ({0}ms) AHEAD beyond recoverable threshold", desyncMilliseconds);
			}
			deltaSamples = 0;
			deltaDesyncMilliseconds = 0;
			return false;
		}

		internal static bool Filter(SpeechSession session, [NotNull] float[] output, int channels, [NotNull] float[] temp, [CanBeNull] AudioFileWriter diagnosticOutput, out float arv, out int samplesRead, bool multiply)
		{
			int num = output.Length / channels;
			bool result = session.Read(new ArraySegment<float>(temp, 0, num));
			if (diagnosticOutput != null)
			{
				diagnosticOutput.WriteSamples(new ArraySegment<float>(temp, 0, num));
			}
			float num2 = 0f;
			int num3 = 0;
			for (int i = 0; i < output.Length; i += channels)
			{
				float num4 = temp[num3++];
				num2 += Mathf.Abs(num4);
				for (int j = 0; j < channels; j++)
				{
					if (multiply)
					{
						output[i + j] *= num4;
					}
					else
					{
						output[i + j] = num4;
					}
				}
			}
			arv = num2 / (float)output.Length;
			samplesRead = num;
			return result;
		}
	}
}
