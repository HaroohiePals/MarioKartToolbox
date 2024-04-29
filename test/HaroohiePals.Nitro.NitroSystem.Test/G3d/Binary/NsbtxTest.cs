using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System.IO;
using Xunit;

namespace HaroohiePals.Nitro.NitroSystem.Test.G3d.Binary
{
    public class NsbtxTest
    {
        [Fact]
        public void ReadWriteTest()
        {
            var input = File.ReadAllBytes("Assets/course_model.nsbtx");
            var nsbtx = new Nsbtx(input);
            Assert.Equal(input, nsbtx.Write());
        }
    }
}
