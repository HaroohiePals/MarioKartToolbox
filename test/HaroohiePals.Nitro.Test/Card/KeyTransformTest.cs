using HaroohiePals.Nitro.Card;
using System.Security.Cryptography;
using Xunit;

namespace HaroohiePals.Nitro.Test.Card
{
    public class KeyTransformTest
    {
        [Fact]
        public void NtrRomTableTransformTest()
        {
            var table = KeyTransform.TransformTable(0x45434D41, 2, 8, Keys.NtrBlowfishTable);
            Assert.Equal(
                new byte[]
                {
                    0xD7, 0x21, 0x5D, 0xED, 0xE5, 0xA9, 0xBF, 0x97, 0xFC, 0x75, 0x33, 0x8B, 0x03, 0x7A, 0x68, 0x1F,
                    0x15, 0x8C, 0x3B, 0xBC
                }, SHA1.HashData(table));
        }
    }
}