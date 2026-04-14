using System;
using System.IO;
using System.Text;

namespace NAudio.Wave
{
	internal class WaveFileWriter : Stream
	{
		private Stream _outStream;

		private BinaryWriter _writer;

		private long _dataSizePos;

		private long _factSampleCountPos;

		private int _dataChunkSize;

		public string Filename { get; private set; }

		public override long Length
		{
			get
			{
				return _dataChunkSize;
			}
		}

		public WaveFormat WaveFormat { get; private set; }

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override long Position
		{
			get
			{
				return _dataChunkSize;
			}
			set
			{
				throw new InvalidOperationException("Repositioning a WaveFileWriter is not supported");
			}
		}

		public WaveFileWriter(Stream outStream, WaveFormat format)
		{
			_outStream = outStream;
			WaveFormat = format;
			_writer = new BinaryWriter(outStream, Encoding.ASCII);
			_writer.Write(Encoding.ASCII.GetBytes("RIFF"));
			_writer.Write(0);
			_writer.Write(Encoding.ASCII.GetBytes("WAVE"));
			_writer.Write(Encoding.ASCII.GetBytes("fmt "));
			_writer.Write(18);
			_writer.Write((short)3);
			_writer.Write((short)format.Channels);
			_writer.Write(format.SampleRate);
			_writer.Write(format.SampleRate * 4);
			_writer.Write((short)4);
			_writer.Write((short)32);
			_writer.Write((short)0);
			CreateFactChunk();
			WriteDataChunkHeader();
		}

		public WaveFileWriter(string filename, WaveFormat format)
			: this(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read), format)
		{
			Filename = filename;
		}

		private void WriteDataChunkHeader()
		{
			_writer.Write(Encoding.ASCII.GetBytes("data"));
			_dataSizePos = _outStream.Position;
			_writer.Write(0);
		}

		private void CreateFactChunk()
		{
			if (HasFactChunk())
			{
				_writer.Write(Encoding.ASCII.GetBytes("fact"));
				_writer.Write(4);
				_factSampleCountPos = _outStream.Position;
				_writer.Write(0);
			}
		}

		private bool HasFactChunk()
		{
			return true;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException("Cannot read from a WaveFileWriter");
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException("Cannot seek within a WaveFileWriter");
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException("Cannot set length of a WaveFileWriter");
		}

		public override void Write(byte[] data, int offset, int count)
		{
			_outStream.Write(data, offset, count);
			_dataChunkSize += count;
		}

		public void WriteSample(float sample)
		{
			_writer.Write(sample);
			_dataChunkSize += 4;
		}

		public void WriteSamples(float[] samples, int offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				WriteSample(samples[offset + i]);
			}
		}

		public override void Flush()
		{
			_writer.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _outStream != null)
			{
				try
				{
					UpdateHeader(_writer);
				}
				finally
				{
					_outStream.Close();
					_outStream = null;
				}
			}
		}

		protected virtual void UpdateHeader(BinaryWriter writer)
		{
			Flush();
			UpdateRiffChunk(writer);
			UpdateFactChunk(writer);
			UpdateDataChunk(writer);
		}

		private void UpdateDataChunk(BinaryWriter writer)
		{
			writer.Seek((int)_dataSizePos, SeekOrigin.Begin);
			writer.Write(_dataChunkSize);
		}

		private void UpdateRiffChunk(BinaryWriter writer)
		{
			writer.Seek(4, SeekOrigin.Begin);
			writer.Write((int)(_outStream.Length - 8));
		}

		private void UpdateFactChunk(BinaryWriter writer)
		{
			if (HasFactChunk())
			{
				int num = 32 * WaveFormat.Channels;
				if (num != 0)
				{
					writer.Seek((int)_factSampleCountPos, SeekOrigin.Begin);
					writer.Write(_dataChunkSize * 8 / num);
				}
			}
		}

		~WaveFileWriter()
		{
			Dispose(false);
		}
	}
}
