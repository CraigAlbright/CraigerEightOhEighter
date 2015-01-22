using System.Collections.Generic;
using Autofac;

namespace CraigerEightOhEighter.Models
{
    public interface IMixer
    {
        int RequestedSampleRate { get; set; }

        IContainer Container { get; set; }

        /// <summary>
        /// Number of tracks
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets or sets the current BPM
        /// </summary>
        decimal Bpm { set; get; }

        int CurrentTick { get; }
        List<Track> Tracks { get; set; }
        void SetNewSampleRate(int requestedSampleRate);
        void Dispose();

        /// <summary>
        /// Delete all tracks
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds a track
        /// </summary>
        /// <param name="track">The track to add</param>
        Track Add(Track track);

        Track this[int ndx] { get; }
        Track this[string trackName] { get; }

        /// <summary>
        /// Starts the mixer
        /// </summary>
        void Play();

        /// <summary>
        /// Stops the mixer
        /// </summary>
        void Stop();
    }
}