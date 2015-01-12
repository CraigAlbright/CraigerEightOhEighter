using System;
using System.IO;

namespace CraigerEightOhEighter.Models
{
	public class WaveStream : Stream, IDisposable
	{
		private readonly Stream _mStream;
		private long _mDataPos;
		private long _mLength;

		private WaveFormat _mFormat;

		public WaveFormat Format
		{
            get { return _mFormat; }
		}

		private string ReadChunk(BinaryReader reader)
		{
			var ch = new byte[4];
			reader.Read(ch, 0, ch.Length);
			return System.Text.Encoding.ASCII.GetString(ch, 0, ch.Length);
		}

		private void ReadHeader()
		{
            var reader = new BinaryReader(_mStream);
			if (ReadChunk(reader) != "RIFF")
				throw new Exception("Invalid file format");

			reader.ReadInt32(); // File length minus first 8 bytes of RIFF description, we don't use it

			if (ReadChunk(reader) != "WAVE")
				throw new Exception("Invalid file format");

			if (ReadChunk(reader) != "fmt ")
				throw new Exception("Invalid file format");

			if (reader.ReadInt32() != 16) // bad format chunk length
				throw new Exception("Invalid file format");

            _mFormat = new WaveFormat(22050, 16, 2)
            {
                wFormatTag = reader.ReadInt16(),
                nChannels = reader.ReadInt16(),
                nSamplesPerSec = reader.ReadInt32(),
                nAvgBytesPerSec = reader.ReadInt32(),
                nBlockAlign = reader.ReadInt16(),
                wBitsPerSample = reader.ReadInt16()
            }; // initialize to any format

		    // assume the data chunk is aligned
            while (_mStream.Position < _mStream.Length && ReadChunk(reader) != "data")
            {
            }

		    if (_mStream.Position >= _mStream.Length)
				throw new Exception("Invalid file format");

            _mLength = reader.ReadInt32();
            _mDataPos = _mStream.Position;

			Position = 0;
		}

		public WaveStream(string fileName) : this(new FileStream(fileName, FileMode.Open))
		{
		}
		public WaveStream(Stream s)
		{
            _mStream = s;
			ReadHeader();
		}
		~WaveStream()
		{
			Dispose();
		}
		public new void Dispose()
		{
            if (_mStream != null)
                _mStream.Close();
			GC.SuppressFinalize(this);
		}

		public override bool CanRead
		{
			get { return true; }
		}
		public override bool CanSeek
		{
			get { return true; }
		}
		public override bool CanWrite
		{
			get { return false; }
		}
		public override long Length
		{
            get { return _mLength; }
		}
		public override long Position
		{
            get { return _mStream.Position - _mDataPos; }
			set { Seek(value, SeekOrigin.Begin); }
		}
		public override void Close()
		{
			Dispose();
		}
		public override void Flush()
		{
		}
        public override void SetLength(long len)
        {
            _mStream.SetLength(len);
        }
		public override long Seek(long pos, SeekOrigin o)
		{
			switch(o)
			{
				case SeekOrigin.Begin:
                    _mStream.Position = pos + _mDataPos;
					break;
				case SeekOrigin.Current:
                    _mStream.Seek(pos, SeekOrigin.Current);
					break;
				case SeekOrigin.End:
                    _mStream.Position = _mDataPos + _mLength - pos;
					break;
			}
			return Position;
		}
		public override int ReadByte()
		{
            return Position < _mLength ? _mStream.ReadByte() : -1;
		}
		public override int Read(byte[] buf, int ofs, int count)
		{
            var toread = (int)Math.Min(count, _mLength - Position);
            return _mStream.Read(buf, ofs, toread);
		}
		public override void Write(byte[] buf, int ofs, int count)
		{
			throw new InvalidOperationException();
		}
	}
}
