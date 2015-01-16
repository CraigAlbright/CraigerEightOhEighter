using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NAudio.Wave.Compression;

namespace CraigerEightOhEighter.Models
{
    /// <summary>
    /// Implements a basic rhythm machine
    /// </summary>
    [System.Runtime.InteropServices.GuidAttribute("4BE447EA-4D19-4680-A8F1-976346F97BA7")]
    public class Mixer : IDisposable
    {
        private readonly IAudioPlayer _mPlayer;
        private readonly object _mBpmLock = new object();
        private decimal _mBpm;
        private readonly int _mTicksPerBeat;
        private int _mTick;
        private int _mTickPeriod;
        private int _mTickLeft;
        private int[] _mMixBuffer;
        private int[] _mMixBuffer16;
        private readonly List<PatchReader> _patchReaders = new List<PatchReader>();

        public List<Track> Tracks = new List<Track>();
        public const int MaxTrackLength = 128;
        public AcmStream ResampleStream;
        public int RequestedSampleRate { get; set; }
        public Mixer(IAudioPlayer player, int ticksPerBeat)
        {
            if (player == null)
                throw new ArgumentNullException("player");
            //if (player.BitsPerSample != 16 || player.Channels != 1)
            //    throw new ArgumentException("player");
            RequestedSampleRate = 12100;
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
                for (var i = _patchReaders.Count - 1; i >= 0; i--)
                {
                    var patchReader = _patchReaders[i];
                    if (!patchReader.Mix(_mMixBuffer, pos, tomix))
                        _patchReaders.RemoveAt(i);
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
                _mMixBuffer16 = new int[samples];

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
                        _mMixBuffer16[i] = _mMixBuffer[i];
                }
            }
            Buffer.BlockCopy(_mMixBuffer16,0,ResampleStream.SourceBuffer,0,_mMixBuffer.Length);
            //_mMixBuffer16.CopyTo(ResampleStream.SourceBuffer,0);
            Marshal.Copy(ResampleStream.SourceBuffer, 0, dest, samples);
            //Buffer.BlockCopy(ResampleStream.SourceBuffer, 0, _mMixBuffer16, samples,ResampleStream.SourceBuffer.Length);
        }
        private void DoTick()
        {
            lock(Tracks)
            {
                foreach (var reader in Tracks.Select(t => t.GetBeat(_mTick)).Where(reader => reader != null))
                {
                    _patchReaders.Add(reader);
                }
                _mTick = (_mTick + 1) % MaxTrackLength;
            }
        }
        private int GetBufferedTicks()
        {
            var samples = _mPlayer.GetBufferedSize() / (_mPlayer.Channels * _mPlayer.BitsPerSample / 8);
            return samples / _mTickPeriod;
        }

       
    }
}