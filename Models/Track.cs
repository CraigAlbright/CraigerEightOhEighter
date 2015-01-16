using System;
using System.Runtime.Serialization;

namespace CraigerEightOhEighter.Models
{
    /// <summary>
    /// Holds the patch and pattern for a specific drum track
    /// </summary>
    [Serializable]
    [DataContract]
    public class Track
    {
        public Track(string name, Patch patch, byte[] pattern) : 
            this(name, patch, pattern.Length)
        {
            Init(pattern);
        }
        public Track(string name, Patch patch, int length)
        {
            Name = name;
            if (patch == null)
                throw new ArgumentNullException("patch");
            if (length > Mixer.MaxTrackLength)
                throw new ArgumentException("length");
            Patch = patch;
            _mPattern = new bool[length];
        }
        public void Init(byte[] pattern)
        {
            lock(this)
            {
                for (var i = 0; i < pattern.Length; i++)
                    this[i] = pattern[i] != 0;
            }
        }
        public PatchReader GetBeat(int beat)
        {
            lock(this)
            {
                beat = beat % _mPattern.Length;
                PatchReader result = null;
                if (_mPattern[beat])
                    result = Patch.GetReader();
                return result;
            }
        }
        [DataMember]
        public int Length { get { return _mPattern.Length; } set { _mPattern = new bool[value]; } }
        
        public bool this[int ndx]
        {
            get { return _mPattern[ndx]; }
            set { lock(this) _mPattern[ndx] = value; }
        }
        private bool[] _mPattern;
        private decimal _volume = 0.5M;
        [DataMember]
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                if (value > 1) _volume = 1;
                if (value < 0) _volume = 0;
                if (value <= 1 && value >= 0)
                    _volume = value;
            }
        }
        [DataMember]
        public int SampleStart { get; set; }
        [DataMember]
        public int SampleEnd { get; set; }
        [DataMember]
        public int Pitch { get; set; }
        [DataMember]
        public int Attack { get; set; }
        [DataMember]
        public int Decay { get; set; }
        [DataMember]
        public int Sustain { get; set; }
        [DataMember]
        public int Release { get; set; }
        [DataMember]
        public readonly Patch Patch;
        [DataMember]
        public readonly string Name;
        [DataMember]
        public bool[] Pattern
        {
            get { return _mPattern; }
            set { _mPattern = value; }
        }
    }
}