using CraigerEightOhEighter.DSP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CraigerEightOhEighterTests
{
    [TestClass]
    public class DspTests
    {
        [TestMethod]
        public void ResampleWaveTest()
        {
            //Arrange
            const string resampleTestFile = @"C:\Visual Studio Projects\CraigerEightOhEighter\media\808bd.wav";
            const string resampleOutFile = @"C:\Visual Studio Projects\CraigerEightOhEighter\media\808bd12khz.wav";
            //Act
            Resampler.ResampleWavFile(resampleTestFile,resampleOutFile,12000,ResampleQuality.Low);
            //Assert
            Assert.IsNotNull(resampleOutFile);
        }
    }
}
