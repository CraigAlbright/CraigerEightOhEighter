using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CraigerEightOhEighter.Models
{
    public class PullStream : Stream 
    {
        public PullStream(PullAudioCallback pullAudio)
        {
            _mPullAudio = pullAudio;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { return 0; } }
        public override long Position { get { return 0; } set {} }
        public override void Close() {}
        public override void Flush() {}
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_mPullAudio != null)
            {
                var h = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    _mPullAudio(new IntPtr(h.AddrOfPinnedObject().ToInt64() + offset), count);
                }
                finally
                {
                    h.Free();
                }
            }
            else
            {
                for (var i = offset; i < offset + count; i++)
                    buffer[i] = 0;
            }
            return count;
        }
        public override long Seek(long offset, SeekOrigin origin) { return 0; }
        public override void SetLength(long length) {}
        public override void Write(byte[] buffer, int offset, int count) {}
        public override void WriteByte(byte value) {}

        private readonly PullAudioCallback _mPullAudio;
    }
}