using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using NAudio.Wave.Compression;

namespace CraigerEightOhEighter.Models
{
	/// <summary>
	/// This class holds the audio data for a drum patch
	/// </summary>
	public class Patch
	{
		public Patch(Stream stream)
		{
			using(var s = new WaveStream(stream))
			{
				if (s.Format.wFormatTag != (short)WaveFormats.Pcm)
					throw new Exception("Invalid sample format");

				// read everything into memory to speed up things (CF optimization)
				var data = new byte[s.Length];
				s.Read(data, 0, data.Length);

				var samples = (int)s.Length / s.Format.nBlockAlign;
				var stereo = s.Format.nChannels == 2;
				var eight = s.Format.wBitsPerSample == 8; // assume 16 bit otherwise

				// we store the audio data always in mono
				_mAudioData = new int[samples];

				var r = new BinaryReader(new MemoryStream(data));
				try
				{
					var pos = 0;
					for (var i = 0; i < samples; i++)
					{
						_mAudioData[pos] = eight ? 256 * (r.ReadByte() - 128) : r.ReadInt16();
						if (stereo) // just add up channels to convert to mono
							_mAudioData[pos] += eight ? 256 * (r.ReadByte() - 128) : r.ReadInt16();
						pos++;
					}
				}
				finally
				{
					r.Close();
				}
			}
		}

		public Patch(string fileName) :
			this(new FileStream(fileName, FileMode.Open))
		{
		}

		public Patch(Type type, string resourceName) :
			this(Assembly.GetExecutingAssembly().GetManifestResourceStream(type, resourceName))
		{
		}

	    public Patch(string name, Stream resStream) : this(resStream)
	    {
	        
	    }

		public PatchReader GetReader()
		{
			return new PatchReader(_mAudioData);
		}

		private readonly int[] _mAudioData;
	}

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
    [DataContract]
    public class TrackCollection<T> : ICollection<T> where T: Track
    {
        public TrackCollection()
        {
            InnerList = new List<Track>();
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new TrackEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TrackEnumerator<T>(this);
        }

        public void Add(T item)
        {
            InnerList.Add(item);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public bool Contains(T item)
        {
            return InnerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array.CopyTo(InnerList.ToArray(),arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = false;
            //loop through the inner array's indices
            for (var i = 0; i < InnerList.Count; i++)
            {
                //store current index being checked
                var obj = (T)InnerList[i];

                //compare the BusinessObjectBase UniqueId property
                if (obj.Name == item.Name)
                {
                    //remove item from inner ArrayList at index i
                    InnerList.RemoveAt(i);
                    result = true;
                    break;
                }
            }

            return result;

        }

        public virtual T this[int index]
        {
            get
            {
                return (T)InnerList[index];
            }
            set
            {
                InnerList[index] = value;
            }
        }
        

        protected List<Track> InnerList;

        public virtual int Count
        {
            get { return InnerList.Count; }
        }
        public bool IsReadOnly { get; private set; }
    }

    public class TrackEnumerator<T> : IEnumerator<T> where T : Track
    {

        protected TrackCollection<T> Collection; //enumerated collection
        protected int Index; //current index
        protected T current; //current enumerated object in the collection
        public TrackEnumerator(TrackCollection<T> trackCollection)
        {
            Collection = trackCollection;
            Index = -1;
            current = default(T);
        }

        public void Dispose()
        {
            Collection = null;
            current = default(T);
            Index = -1;
        }

        public bool MoveNext()
        {
            //make sure we are within the bounds of the collection
            if (++Index >= Collection.Count)
            {
                //if not return false
                return false;
            }
            //if we are, then set the current element
            //to the next object in the collection
            current = (T) Collection[Index];
            //return true
            return true;
        }

        public void Reset()
        {
            current = default(T); //reset current object
            Index = -1;
        }

        public T Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }

    /// <summary>
	/// Implements a basic rhythm machine
	/// </summary>
	public class Mixer : IDisposable
	{
		public const int MaxTrackLength = 128;
	    public AcmStream ResampleStream;
        public int RequestedSampleRate { get; set; }
		public Mixer(IAudioPlayer player, int ticksPerBeat)
		{
			if (player == null)
				throw new ArgumentNullException("player");
			//if (player.BitsPerSample != 16 || player.Channels != 1)
			//    throw new ArgumentException("player");
		    RequestedSampleRate = 44100;
		    ResampleStream = new AcmStream(new NAudio.Wave.WaveFormat(44100, 16, 2),
		        new NAudio.Wave.WaveFormat(RequestedSampleRate, 16, 2));
			_mPlayer = player;
			_mTicksPerBeat = ticksPerBeat;
			Bpm = 77;
		}
		~Mixer()
		{
			Dispose();
		}

	    public void SetNewSampleRate(int requestedSampleRate)
	    {
	        var oldRate = RequestedSampleRate;
            ResampleStream = new AcmStream(new NAudio.Wave.WaveFormat(oldRate,16,2),new NAudio.Wave.WaveFormat(requestedSampleRate,16,2));
	    }
		public void Dispose()
		{
			if (_mPlayer != null)
				_mPlayer.Dispose();
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Number of tracks
		/// </summary>
		public int Count
		{
			get 
			{ 
				lock(Tracks)
					return Tracks.Count;
			}
		}
		/// <summary>
		/// Delete all tracks
		/// </summary>
		public void Clear()
		{
			lock(Tracks)
				Tracks.Clear();
		}
		/// <summary>
		/// Adds a track
		/// </summary>
		/// <param name="track">The track to add</param>
		public Track Add(Track track)
		{
			if (track == null)
				throw new ArgumentNullException("track");
			lock(Tracks)
				Tracks.Add(track);
			return track;
		}
		public Track this[int ndx]
		{
			get 
			{ 
				lock(Tracks)
					return Tracks[ndx]; 
			}
		}
		public Track this[string trackName]
		{
			get
			{
				lock(Tracks)
					foreach (Track t in Tracks)
					{
						if (t.Name == trackName)
							return t;
					}
				return null;
			}
		}
		/// <summary>
		/// Gets or sets the current BPM
		/// </summary>
		public decimal Bpm
		{
			set
			{
				lock(_mBpmLock)
				{
					_mBpm = value;
					_mTickPeriod = (_mPlayer.SamplingRate * 60 / (int)_mBpm) / _mTicksPerBeat;
				}
			}
			get
			{
				return _mBpm;
			}
		}
		/// <summary>
		/// Starts the mixer
		/// </summary>
		public void Play()
		{
			Stop();

			_mTick = 0;
			_mTickLeft = 0;
			_mPlayer.Play(Mix16Mono);
		}
		/// <summary>
		/// Stops the mixer
		/// </summary>
		public void Stop()
		{
			_mPlayer.Stop();
		}
		public int CurrentTick
		{
			get 
			{ 
				var ticks = _mTick - GetBufferedTicks();
				while(ticks < 0)
					ticks += MaxTrackLength;
				return ticks % MaxTrackLength;
			}
		}
		// private stuff
		private void DoMix(int samples)
		{
			// grow mix buffer as necessary
			if (_mMixBuffer == null || _mMixBuffer.Length < samples)
				_mMixBuffer = new int[samples];

			// clear mix buffer
			Array.Clear(_mMixBuffer, 0, _mMixBuffer.Length);

			var pos = 0;
			while(pos < samples)
			{
				// load current patches
				if (_mTickLeft == 0)
				{
					DoTick();
					lock(_mBpmLock)
						_mTickLeft = _mTickPeriod;
				}

				var tomix = Math.Min(samples - pos, _mTickLeft);

				// mix current streams
				for (var i = _mReaders.Count - 1; i >= 0; i--)
				{
					var r = _mReaders[i];
					if (!r.Mix(_mMixBuffer, pos, tomix))
						_mReaders.RemoveAt(i);
				}

				_mTickLeft -= tomix;
				pos += tomix;
			}
		}
		private void Mix16Mono(IntPtr dest, int size)
		{
			var samples = size / 2;

			DoMix(samples);

			if (_mMixBuffer16 == null || _mMixBuffer16.Length < samples)
				_mMixBuffer16 = new short[samples];

			// clip to 16 bit
			for (var i = 0; i < samples; i++)
			{
				if (_mMixBuffer[i] > 32767)
					_mMixBuffer16[i] = 32767;
				else
				{
					if (_mMixBuffer[i] < -32768)
						_mMixBuffer16[i] = -32768;
					else
						_mMixBuffer16[i] = (short)_mMixBuffer[i];
				}
			}
            Buffer.BlockCopy(_mMixBuffer16,0,ResampleStream.SourceBuffer,0,_mMixBuffer.Length);
            Marshal.Copy(_mMixBuffer16, 0, dest, samples);
		}
		private void DoTick()
		{
			lock(Tracks)
			{
			    foreach (var r in Tracks.Select(t => t.GetBeat(_mTick)).Where(r => r != null))
			    {
			        _mReaders.Add(r);
			    }
			    _mTick = (_mTick + 1) % MaxTrackLength;
			}
		}
		private int GetBufferedTicks()
		{
			var samples = _mPlayer.GetBufferedSize() / (_mPlayer.Channels * _mPlayer.BitsPerSample / 8);
			return samples / _mTickPeriod;
		}

		private readonly IAudioPlayer _mPlayer;

		private readonly object _mBpmLock = new object();
		private decimal _mBpm;
		private readonly int _mTicksPerBeat;
		private int _mTick;
		private int _mTickPeriod;
		private int _mTickLeft;

		private int[] _mMixBuffer;
		private short[] _mMixBuffer16;
		private readonly List<PatchReader> _mReaders = new List<PatchReader>();

		public List<Track> Tracks = new List<Track>();
	}
}
