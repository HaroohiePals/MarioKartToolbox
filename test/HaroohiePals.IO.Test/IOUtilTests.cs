using NUnit.Framework;

namespace HaroohiePals.IO.Test;

class IOUtilTests
{
    [Test]
    public void IOUtil_ReadS16Le_ReadsValues()
    {
        // Arrange
        var testArray = new byte[] { 0xAA, 0xBB, 0x01, 0x00, 0x00, 0x01 };

        // Act
        var readData = IOUtil.ReadS16Le(testArray, 0, 3);

        // Assert
        Assert.That(readData, Is.EqualTo(new short[] { -17494, 0x0001, 0x0100 }));
    }
}