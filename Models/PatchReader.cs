using System;

namespace CraigerEightOhEighter.Models
{
    /// <summary>
    /// This class implements an audio data reader/mixer for a given patch
    /// </summary>
    public class PatchReader
    {
        internal PatchReader(int[] data)
        {
            _mData = data;
            _mDataPos = 0;
        }

        public bool Mix(int[] dest, int offset, int samples)
        {
            var toget = Math.Min(samples, _mData.Length - _mDataPos);
            if (toget > 0)
            {
                for (var i = offset; i < offset + toget; i++)
                    dest[i] += _mData[_mDataPos++]; // mix into destination buffer
            }
            return _mDataPos < _mData.Length;
        }

        private int _mDataPos;
        private readonly int[] _mData;
    }
}