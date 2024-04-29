using Xunit;

namespace HaroohiePals.IO.Test
{
    public class IOUtilTest
    {
        [Fact]
        public void ReadS16LeArray()
        {
            var testArray = new byte[] { 0xAA, 0xBB, 0x01, 0x00, 0x00, 0x01 };
            var readData  = IOUtil.ReadS16Le(testArray, 0, 3);
            Assert.Equal(new short[] { -17494, 0x0001, 0x0100 }, readData);
        }
    }
}