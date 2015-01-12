using System;

namespace CraigerEightOhEighter.Models
{
	/// <summary>
	/// Delegate used to fill in a buffer
	/// </summary>
	public delegate void PullAudioCallback(IntPtr data, int count);

	/// <summary>
	/// Audio player interface
	/// </summary>
	public interface IAudioPlayer : IDisposable
	{
		int SamplingRate { get; }
		int BitsPerSample { get; }
		int Channels { get; }

		int GetBufferedSize();
		void Play(PullAudioCallback onAudioData);
		void Stop();
	}
}