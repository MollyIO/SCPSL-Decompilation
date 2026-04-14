using System;
using System.IO;
using Dissonance.Threading;
using NAudio.Wave;

namespace Dissonance.Audio
{
	internal class AudioFileWriter : IDisposable
	{
		private readonly LockedValue<WaveFileWriter> _lock;

		public AudioFileWriter(string filename, [NotNull] WaveFormat format)
		{
			if (filename == null)
			{
				throw new ArgumentNullException("filename");
			}
			if (format == null)
			{
				throw new ArgumentNullException("format");
			}
			if (string.IsNullOrEmpty(Path.GetExtension(filename)))
			{
				filename += ".wav";
			}
			string directoryName = Path.GetDirectoryName(filename);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			_lock = new LockedValue<WaveFileWriter>(new WaveFileWriter(File.Open(filename, FileMode.CreateNew), format));
		}

		public void Dispose()
		{
			using (LockedValue<WaveFileWriter>.Unlocker unlocker = _lock.Lock())
			{
				WaveFileWriter value = unlocker.Value;
				if (value != null)
				{
					value.Dispose();
				}
				unlocker.Value = null;
			}
		}

		public void Flush()
		{
			using (LockedValue<WaveFileWriter>.Unlocker unlocker = _lock.Lock())
			{
				WaveFileWriter value = unlocker.Value;
				if (value != null)
				{
					value.Flush();
				}
			}
		}

		public void WriteSamples(ArraySegment<float> samples)
		{
			using (LockedValue<WaveFileWriter>.Unlocker unlocker = _lock.Lock())
			{
				WaveFileWriter value = unlocker.Value;
				if (value != null)
				{
					value.WriteSamples(samples.Array, samples.Offset, samples.Count);
				}
			}
		}
	}
}
