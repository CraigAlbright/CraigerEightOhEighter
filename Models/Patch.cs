using System;
using System.IO;
using System.Reflection;

namespace CraigerEightOhEighter.Models
{
	/// <summary>
	/// This is a "sound" in the drum machine. The sample start is the sample number that the sound will start on. Sample end is the 
	/// number of samples shorter that you want to make the sound. This will likely be a percentage of "shrink" to allow some DSP to "grow" the sample.
	/// </summary>
	public class Patch
	{
		public Patch(Stream stream, int sampleStart, int sampleEnd)
		{
		    if (sampleStart > stream.Length)
		        SampleStart = stream.Length - 1;
		    else
		    {
                SampleStart = sampleStart;    
		    }
		    
		    SampleEnd = stream.Length < SampleEnd ? stream.Length : sampleEnd;
			using(var s = new WaveStream(stream))
			{
				if (s.Format.wFormatTag != (short)WaveFormats.Pcm)
					throw new Exception("Invalid sample format");

				// read everything into memory to speed up things (CF optimization)
                
				var data = new byte[s.Length];
				s.Read(data, (int)SampleStart, data.Length - (int)SampleEnd);

				var samples = (int)s.Length / s.Format.nBlockAlign;
				var isStereo = s.Format.nChannels == 2;
				var isEightBit = s.Format.wBitsPerSample == 8; // assume 16 bit otherwise

				// we store the audio data always in mono
				_mAudioData = new int[samples];

				var r = new BinaryReader(new MemoryStream(data));
				try
				{
					var pos = 0;
					for (var i = 0; i < samples; i++)
					{
						_mAudioData[pos] = isEightBit ? 256 * (r.ReadByte() - 128) : r.ReadInt16();
						if (isStereo) // just add up channels to convert to mono
							_mAudioData[pos] += isEightBit ? 256 * (r.ReadByte() - 128) : r.ReadInt16();
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
			this(new FileStream(fileName, FileMode.Open), 0, 0)
		{
		}

		public Patch(Type type, string resourceName) :
			this(Assembly.GetExecutingAssembly().GetManifestResourceStream(type, resourceName), 0, 0)
		{
		}

	    public Patch(string name, Stream resStream) : this(resStream, 0,0)
	    {
	        
	    }

		public PatchReader GetReader()
		{
			return new PatchReader(_mAudioData);
		}
        public long SampleStart { get; set; }
        public long SampleEnd { get; set; }
		private readonly int[] _mAudioData;
	}
}
