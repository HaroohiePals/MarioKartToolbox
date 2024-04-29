using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Animation;
using System.IO;
using Xunit;

namespace HaroohiePals.Nitro.NitroSystem.Test.G3d.Intermediate.Animation
{
    public class ItaTest
    {
        [Theory]
        [InlineData("BlackHole")]
        [InlineData("desert_gc")]
        [InlineData("dgc_sky")]
        [InlineData("patapata_gc")]
        public void ItaToNsbta(string testPath)
        {
            string basePath = Path.Join("Assets", "ita", testPath);
            var ita = new Ita(File.ReadAllBytes(basePath + ".ita"));
            var g3dcvtrNsbta = File.ReadAllBytes(basePath + ".nsbta");
            var g3dcvtrNsbtaData = new Nsbta(g3dcvtrNsbta);

            string name = Path.GetFileName(testPath);
            if (name.Length > 16)
                name = name[..16];

            var nsbta = ita.ToNsbta(name);
            //var itaNsbta = nsbta.Write();

            //Assert.Equal(g3dcvtrNsbta, itaNsbta);
        }
    }
}
