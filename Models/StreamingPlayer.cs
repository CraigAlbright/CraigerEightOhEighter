using System;
using System.IO;
using Microsoft.DirectX.DirectSound;

namespace CraigerEightOhEighter.Models
{
	/// <summary>
	/// An audio streaming player using DirectSound
	/// </summary>
	public class StreamingPlayer : IAudioPlayer
	{
	    public StreamingPlayer()
	    {
	        MaxLatencyMs = 30;
	    }

	    private int _latency = 300;

	    public int MaxLatencyMs
	    {
	        get { return _latency; } 
            set { _latency = value; }
	    }
		public int SampleRate { get; set; }

	    /// <summary>
		/// Helper function for creating WaveFormat instances
		/// </summary>
		/// <param name="sr">Sampling rate</param>
		/// <param name="bps">Bits per sample</param>
		/// <param name="ch">Channels</param>
		/// <returns></returns>
		public static Microsoft.DirectX.DirectSound.WaveFormat CreateWaveFormat(int sr, short bps, short ch)
		{
			var wfx = new Microsoft.DirectX.DirectSound.WaveFormat() {FormatTag = WaveFormatTag.Pcm, SamplesPerSecond = sr, BitsPerSample = bps, Channels = ch};

			wfx.BlockAlign = (short)(wfx.Channels * (wfx.BitsPerSample / 8));
			wfx.AverageBytesPerSecond = wfx.SamplesPerSecond * wfx.BlockAlign;

			return wfx;
		}

		public StreamingPlayer(IntPtr owner, int sr, short bps, short ch) : 
			this(owner, null, CreateWaveFormat(sr, bps, ch))
		{
		}

		public StreamingPlayer(IntPtr owner, Microsoft.DirectX.DirectSound.WaveFormat format) : 
			this(owner, null, format)
		{
		}

		public StreamingPlayer(IntPtr owner, Device device, int sr, short bps, short ch) : 
			this(owner, device, CreateWaveFormat(sr, bps, ch))
		{
		}

		public StreamingPlayer(IntPtr owner, Device device, Microsoft.DirectX.DirectSound.WaveFormat format)
		{
			Device = device;
			if (Device == null)
			{
				Device = new Device();
				Device.SetCooperativeLevel(owner, CooperativeLevel.Normal);
				_mOwnsDevice = true;
			}

			var desc = new BufferDescription(format)
			{
				BufferBytes = format.AverageBytesPerSecond,
				ControlVolume = true,
				GlobalFocus = true
			};

			_mBuffer = new SecondaryBuffer(desc, Device);
			_mBufferBytes = _mBuffer.Caps.BufferBytes;

			// ReSharper disable once PossibleLossOfFraction
			_mTimer = new System.Timers.Timer(BytesToMs(_mBufferBytes) / 0x6) {Enabled = false};
			_mTimer.Elapsed += Timer_Elapsed;
		}

		~StreamingPlayer()
		{
			Dispose();
		}

		public void Dispose()
		{
			Stop();
			if (_mTimer != null)
			{
				_mTimer.Dispose();
				_mTimer = null;
			}
			if (_mBuffer != null)
			{
				_mBuffer.Dispose();
				_mBuffer = null;
			}
			if (_mOwnsDevice && Device != null)
			{
				Device.Dispose();
				Device = null;
			}
			GC.SuppressFinalize(this);
		}

		// IAudioPlayer

		public int SamplingRate
		{
			get { return _mBuffer.Format.SamplesPerSecond; }
		}
		public int BitsPerSample { get { return _mBuffer.Format.BitsPerSample; } }
		public int Channels { get { return _mBuffer.Format.Channels; } }
		public SecondaryBuffer MBuffer { get { return _mBuffer; } set { _mBuffer = value; } }

		public void Play(PullAudioCallback pullAudio)
		{
			Stop();

			_mPullStream = new PullStream(pullAudio);
			_mBuffer.SetCurrentPosition(0);
			_mNextWrite = 0;
			Feed(_mBufferBytes);
			_mTimer.Enabled = true;
			_mBuffer.Play(0, BufferPlayFlags.Looping);
		}

		public void Stop()
		{
			if (_mTimer != null)
				_mTimer.Enabled = false;
			if (_mBuffer != null)
				_mBuffer.Stop();
		}

		public int GetBufferedSize()
		{
			var played = GetPlayedSize();
			return played > 0 && played < _mBufferBytes ? _mBufferBytes - played : 0;
		}

		private readonly bool _mOwnsDevice;
		private SecondaryBuffer _mBuffer;
		private System.Timers.Timer _mTimer;
		private int _mNextWrite;
		private readonly int _mBufferBytes;
		private Stream _mPullStream;

		public Device Device { get; private set; }

		private int BytesToMs(int bytes)
		{
			return bytes * 1000 / _mBuffer.Format.AverageBytesPerSecond;
		}

		private int MsToBytes(int ms)
		{
			var bytes = ms * _mBuffer.Format.AverageBytesPerSecond / 1000;
			bytes -= bytes % _mBuffer.Format.BlockAlign;
			return bytes;
		}

		private void Feed(int bytes)
		{
			// limit latency to some milliseconds
			var tocopy = Math.Min(bytes, MsToBytes(MaxLatencyMs));

			if (tocopy > 0)
			{
				// restore buffer
				if (_mBuffer.Status.BufferLost)
					_mBuffer.Restore();

				// copy data to the buffer
				_mBuffer.Write(_mNextWrite, _mPullStream, tocopy, LockFlag.None);

				_mNextWrite += tocopy;
				if (_mNextWrite >= _mBufferBytes)
					_mNextWrite -= _mBufferBytes;
			}
		}

		private int GetPlayedSize()
		{
			var pos = _mBuffer.PlayPosition;
			return pos < _mNextWrite ? pos + _mBufferBytes - _mNextWrite : pos - _mNextWrite;
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Feed(GetPlayedSize());
		}
	}
}
