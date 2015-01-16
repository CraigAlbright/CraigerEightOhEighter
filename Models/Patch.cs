using System;
using System.IO;
using System.Reflection;

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
}
