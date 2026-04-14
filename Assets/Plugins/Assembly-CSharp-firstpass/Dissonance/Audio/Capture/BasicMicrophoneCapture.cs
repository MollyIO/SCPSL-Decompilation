using System;
using System.Collections.Generic;
using System.Linq;
using Dissonance.Config;
using Dissonance.Datastructures;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Capture
{
	public class BasicMicrophoneCapture : MonoBehaviour, IMicrophoneCapture
	{
		private static readonly Log Log = Logs.Create(LogCategory.Recording, typeof(BasicMicrophoneCapture).Name);

		private byte _maxReadBufferPower;

		private readonly POTBuffer _readBuffer = new POTBuffer(10);

		private BufferedSampleProvider _rawMicSamples;

		private IFrameProvider _rawMicFrames;

		private float[] _frame;

		private WaveFormat _format;

		private AudioClip _clip;

		private int _readHead;

		private bool _started;

		private string _micName;

		private bool _audioDeviceChanged;

		private AudioFileWriter _microphoneDiagnosticOutput;

		private readonly List<IMicrophoneSubscriber> _subscribers = new List<IMicrophoneSubscriber>();

		public TimeSpan Latency { get; private set; }

		public bool IsRecording
		{
			get
			{
				return _clip != null;
			}
		}

		public WaveFormat StartCapture(string inputMicName)
		{
			Log.AssertAndThrowPossibleBug(_clip == null, "1BAD3E74-B451-4B7D-A9B9-35225BE55364", "Attempted to Start microphone capture, but capture is already running");
			if (Log.AssertAndLogWarn(Microphone.devices.Length > 0, "No microphone detected; disabling voice capture"))
			{
				return null;
			}
			_micName = ChooseMicName(inputMicName);
			int minFreq;
			int maxFreq;
			Microphone.GetDeviceCaps(_micName, out minFreq, out maxFreq);
			int frequency = ((minFreq != 0 || maxFreq != 0) ? Mathf.Clamp(48000, minFreq, maxFreq) : 48000);
			_clip = Microphone.Start(_micName, true, 10, frequency);
			if (_clip == null)
			{
				Log.Error("Failed to start microphone capture");
				return null;
			}
			_format = new WaveFormat(1, _clip.frequency);
			_maxReadBufferPower = (byte)Math.Ceiling(Math.Log(0.1f * (float)_clip.frequency, 2.0));
			int num = (int)(0.02 * (double)_clip.frequency);
			if (_rawMicSamples == null || _rawMicSamples.WaveFormat != _format || _rawMicSamples.Capacity != num || _rawMicFrames.FrameSize != num)
			{
				_rawMicSamples = new BufferedSampleProvider(_format, num * 4);
				_rawMicFrames = new SampleToFrameProvider(_rawMicSamples, (uint)num);
			}
			if (_frame == null || _frame.Length != num)
			{
				_frame = new float[num];
			}
			AudioSettings.OnAudioConfigurationChanged += OnAudioDeviceChanged;
			_audioDeviceChanged = false;
			for (int i = 0; i < _subscribers.Count; i++)
			{
				_subscribers[i].Reset();
			}
			Latency = TimeSpan.FromSeconds((float)num / (float)_format.SampleRate);
			Log.Info("Began mic capture (SampleRate:{0}Hz, FrameSize:{1}, Buffer Limit:2^{2}, Latency:{3}ms, Device:'{4}')", _clip.frequency, num, _maxReadBufferPower, Latency.TotalMilliseconds, _micName);
			return _format;
		}

		[CanBeNull]
		private static string ChooseMicName([CanBeNull] string micName)
		{
			if (string.IsNullOrEmpty(micName))
			{
				return null;
			}
			if (!Microphone.devices.Contains(micName))
			{
				Log.Warn("Cannot find microphone '{0}', using default mic", micName);
				return null;
			}
			return micName;
		}

		public void StopCapture()
		{
			Log.AssertAndThrowPossibleBug(_clip != null, "CDDAE69D-44DC-487F-9B69-4703B779400E", "Attempted to stop microphone capture, but it is already stopped");
			if (_microphoneDiagnosticOutput != null)
			{
				_microphoneDiagnosticOutput.Dispose();
				_microphoneDiagnosticOutput = null;
			}
			Microphone.End(_micName);
			_format = null;
			_clip = null;
			_readHead = 0;
			_started = false;
			_micName = null;
			_rawMicSamples.Reset();
			_rawMicFrames.Reset();
			AudioSettings.OnAudioConfigurationChanged -= OnAudioDeviceChanged;
			_audioDeviceChanged = false;
		}

		private void OnAudioDeviceChanged(bool deviceWasChanged)
		{
			_audioDeviceChanged |= deviceWasChanged;
		}

		public bool UpdateSubscribers()
		{
			if (!_started)
			{
				_readHead = Microphone.GetPosition(_micName);
				_started = _readHead > 0;
				if (!_started)
				{
					return false;
				}
			}
			if (_clip.samples == 0)
			{
				Log.Error("Unknown microphone capture error (zero length clip) - restarting mic");
				return true;
			}
			if (_audioDeviceChanged)
			{
				return true;
			}
			if (_subscribers.Count > 0)
			{
				DrainMicSamples();
			}
			else
			{
				_readHead = Microphone.GetPosition(_micName);
				_rawMicSamples.Reset();
				_rawMicFrames.Reset();
				if (_microphoneDiagnosticOutput != null)
				{
					_microphoneDiagnosticOutput.Dispose();
					_microphoneDiagnosticOutput = null;
				}
			}
			return false;
		}

		private void DrainMicSamples()
		{
			int position = Microphone.GetPosition(_micName);
			uint count = (uint)((_clip.samples + position - _readHead) % _clip.samples);
			if (count == 0)
			{
				return;
			}
			while (count > _readBuffer.MaxCount)
			{
				if (_readBuffer.Pow2 > _maxReadBufferPower || !_readBuffer.Expand())
				{
					Log.Warn("Insufficient buffer space, requested {0}, clamped to {1} (dropping samples)", count, _readBuffer.MaxCount);
					count = _readBuffer.MaxCount;
					uint num = count - _readBuffer.MaxCount;
					_readHead = (int)((_readHead + num) % _clip.samples);
					break;
				}
			}
			_readBuffer.Alloc(count);
			try
			{
				while (count != 0)
				{
					float[] buffer = _readBuffer.GetBuffer(ref count, true);
					_clip.GetData(buffer, _readHead);
					_readHead = (_readHead + buffer.Length) % _clip.samples;
					ConsumeSamples(new ArraySegment<float>(buffer, 0, buffer.Length));
				}
			}
			finally
			{
				_readBuffer.Free();
			}
		}

		private void ConsumeSamples(ArraySegment<float> samples)
		{
			if (samples.Array == null)
			{
				throw new ArgumentNullException("samples");
			}
			while (samples.Count > 0)
			{
				int num = _rawMicSamples.Write(samples);
				samples = new ArraySegment<float>(samples.Array, samples.Offset + num, samples.Count - num);
				SendFrame();
			}
		}

		private void SendFrame()
		{
			while (_rawMicSamples.Count > _rawMicFrames.FrameSize)
			{
				ArraySegment<float> arraySegment = new ArraySegment<float>(_frame);
				if (!_rawMicFrames.Read(arraySegment))
				{
					break;
				}
				if (DebugSettings.Instance.EnableRecordingDiagnostics && DebugSettings.Instance.RecordMicrophoneRawAudio)
				{
					if (_microphoneDiagnosticOutput == null)
					{
						string filename = string.Format("Dissonance_Diagnostics/MicrophoneRawAudio_{0}", DateTime.UtcNow.ToFileTime());
						_microphoneDiagnosticOutput = new AudioFileWriter(filename, _format);
					}
				}
				else if (_microphoneDiagnosticOutput != null)
				{
					_microphoneDiagnosticOutput.Dispose();
					_microphoneDiagnosticOutput = null;
				}
				if (_microphoneDiagnosticOutput != null)
				{
					_microphoneDiagnosticOutput.WriteSamples(arraySegment);
					_microphoneDiagnosticOutput.Flush();
				}
				for (int i = 0; i < _subscribers.Count; i++)
				{
					_subscribers[i].ReceiveMicrophoneData(arraySegment, _format);
				}
			}
		}

		public void Subscribe(IMicrophoneSubscriber listener)
		{
			if (listener == null)
			{
				throw new ArgumentNullException("listener");
			}
			_subscribers.Add(listener);
		}

		public bool Unsubscribe(IMicrophoneSubscriber listener)
		{
			if (listener == null)
			{
				throw new ArgumentNullException("listener");
			}
			return _subscribers.Remove(listener);
		}
	}
}
