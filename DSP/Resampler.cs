using NAudio.Wave;

namespace CraigerEightOhEighter.DSP
{
    public class Resampler
    {
        public static void ResampleWavFile(string inputFileName, string outputFileName, int destinationSampleRate, ResampleQuality quality)
        {
            using (var reader = new WaveFileReader(inputFileName))
            {
                var outFormat = new WaveFormat(destinationSampleRate, reader.WaveFormat.Channels);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    resampler.ResamplerQuality = (int)quality;
                    WaveFileWriter.CreateWaveFile(outputFileName, resampler);
                }
            }
        }
    }

    public enum ResampleQuality
    {
        Poor = 1,
        Low = 15,
        Medium = 30,
        Good = 45,
        High = 60,
    }
}
