using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;
using System.IO;
using Xunit;

namespace HaroohiePals.Nitro.NitroSystem.Test.G3d.Binary
{
    public class NsbtaTest
    {
        [Theory]
        [InlineData("Assets/ita/BlackHole.nsbta")]
        [InlineData("Assets/ita/desert_gc.nsbta")]
        [InlineData("Assets/ita/dgc_sky.nsbta")]
        [InlineData("Assets/ita/patapata_gc.nsbta")]
        //[InlineData("Assets/ita/animtest_aligned.nsbta")]
        public void ReadWriteTest(string path)
        {
            var input = File.ReadAllBytes(path);
            var nsbta = new Nsbta(input);
            var rewritten = nsbta.Write();

            File.WriteAllBytes("Assets/original.nsbta", input);
            File.WriteAllBytes("Assets/rewritten.nsbta", rewritten);

            Assert.Equal(input, rewritten);
        }
    }
}
