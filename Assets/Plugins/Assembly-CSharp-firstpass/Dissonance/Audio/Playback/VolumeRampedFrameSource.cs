using System;
using Dissonance.Extensions;
using NAudio.Wave;
using UnityEngine;

namespace Dissonance.Audio.Playback
{
	internal class VolumeRampedFrameSource : IFrameSource
	{
		private readonly IFrameSource _source;

		private readonly IVolumeProvider _volumeProvider;

		private float _targetVolume;

		private float _currentVolume;

		public uint FrameSize
		{
			get
			{
				return _source.FrameSize;
			}
		}

		public WaveFormat WaveFormat
		{
			get
			{
				return _source.WaveFormat;
			}
		}

		public VolumeRampedFrameSource(IFrameSource source, IVolumeProvider volumeProvider)
		{
			_source = source;
			_volumeProvider = volumeProvider;
		}

		public void Prepare(SessionContext context)
		{
			_source.Prepare(context);
		}

		public bool Read(ArraySegment<float> frame)
		{
			bool flag = _source.Read(frame);
			_targetVolume = ((!flag) ? _volumeProvider.TargetVolume : 0f);
			if (_targetVolume == _currentVolume)
			{
				ApplyFlatAttenuation(frame, _currentVolume);
			}
			else
			{
				ApplyRampedAttenuation(frame, _currentVolume, _targetVolume);
			}
			_currentVolume = _targetVolume;
			return flag;
		}

		private static void ApplyFlatAttenuation(ArraySegment<float> frame, float volume)
		{
			if (frame.Array == null)
			{
				throw new ArgumentNullException("frame");
			}
			if (volume == 1f)
			{
				return;
			}
			if (volume == 0f)
			{
				frame.Clear();
				return;
			}
			for (int i = 0; i < frame.Count; i++)
			{
				frame.Array[frame.Offset + i] *= volume;
			}
		}

		private static void ApplyRampedAttenuation(ArraySegment<float> frame, float start, float end)
		{
			if (frame.Array == null)
			{
				throw new ArgumentNullException("frame");
			}
			float num = (end - start) / (float)frame.Count;
			float num2 = start;
			for (int i = frame.Offset; i < frame.Offset + frame.Count; i++)
			{
				frame.Array[i] *= num2;
				num2 = Mathf.Clamp(num2 + num, 0f, 1f);
			}
		}

		public void Reset()
		{
			_source.Reset();
			_currentVolume = 0f;
		}
	}
}
