using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System.IO;
using Xunit;

namespace HaroohiePals.Nitro.NitroSystem.Test.G3d.Binary
{
    public class NsbmdTest
    {
        [Theory]
        [InlineData("Assets/course_model.nsbmd")]
        [InlineData("Assets/ermii.nsbmd")]
        public void ReadWriteTest(string path)
        {
            var input = File.ReadAllBytes(path);
            var nsbmd = new Nsbmd(input);
            Assert.Equal(input, nsbmd.Write());
        }
    }
}