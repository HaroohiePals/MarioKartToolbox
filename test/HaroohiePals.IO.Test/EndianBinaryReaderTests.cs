using NUnit.Framework;
using System.IO;

namespace HaroohiePals.IO.Test;

class EndianBinaryReaderTests
{
    public enum ByteEnum : byte
    {
        Red   = 0,
        Green = 1,
        Blue  = 2
    }

    public enum UShortEnum : ushort
    {
        Red   = 0,
        Green = 0x200,
        Blue  = 0xAA55
    }

    public enum UIntEnum : uint
    {
        Red   = 0,
        Green = 0x200,
        Blue  = 0xAA550102
    }

    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (byte)0xAA)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (byte)0xAA)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (sbyte)-86)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (sbyte)-86)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (ushort)0x55AA)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (ushort)0xAA55)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (short)0x55AA)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (short)-21931)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, 0x420155AAu)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, 0xAA550142u)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, 0x420155AA)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, -1437269694)]
    [TestCase(Endianness.LittleEndian,
        new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, 0x381290FF420155AAUL)]
    [TestCase(Endianness.BigEndian,
        new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, 0xAA550142FF901238UL)]
    [TestCase(Endianness.LittleEndian,
        new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, 0x381290FF420155AAL)]
    [TestCase(Endianness.BigEndian,
        new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, -6173026326974295496L)]
    [TestCase(Endianness.LittleEndian, new byte[] { 2 }, ByteEnum.Blue)]
    [TestCase(Endianness.BigEndian, new byte[] { 2 }, ByteEnum.Blue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x55, 0xAA }, UShortEnum.Blue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55 }, UShortEnum.Blue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x02, 0x01, 0x55, 0xAA }, UIntEnum.Blue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x02 }, UIntEnum.Blue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80, 0x3F }, 1.0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00 }, 1.0f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F }, 0.0078125)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0.0078125)]
    public void EndianBinaryReader_ReadSingle_ReadsValue<T>(Endianness endianness, byte[] data, T expected) where T : unmanaged
    {
        var er    = new EndianBinaryReader(new MemoryStream(data), endianness);
        T   value = er.Read<T>();
        Assert.That(value, Is.EqualTo(expected));
    }

    [Theory]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        3, new byte[] { 0xAA, 0x55, 0x01 }, (byte)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        3, new byte[] { 0xAA, 0x55, 0x01 }, (byte)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        3, new sbyte[] { -86, 0x55, 0x01 }, (sbyte)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        3, new sbyte[] { -86, 0x55, 0x01 }, (sbyte)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        2, new ushort[] { 0x55AA, 0x4201 }, (ushort)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        2, new ushort[] { 0xAA55, 0x0142 }, (ushort)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        2, new short[] { 0x55AA, 0x4201 }, (short)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
        2, new short[] { -21931, 0x0142 }, (short)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        2, new uint[] { 0x420155AAu, 0x381290FFu }, (uint)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        2, new uint[] { 0xAA550142u, 0xFF901238u }, (uint)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        2, new int[] { 0x420155AA, 0x381290FF }, (int)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        2, new int[] { -1437269694, -7335368 }, (int)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        1, new ulong[] { 0x381290FF420155AAUL }, (ulong)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        1, new ulong[] { 0xAA550142FF901238UL }, (ulong)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        1, new long[] { 0x381290FF420155AAL }, (long)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
        1, new long[] { -6173026326974295496L }, (long)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0, 1, 2 },
        3, new ByteEnum[] { ByteEnum.Red, ByteEnum.Green, ByteEnum.Blue }, (ByteEnum)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0, 1, 2 },
        3, new ByteEnum[] { ByteEnum.Red, ByteEnum.Green, ByteEnum.Blue }, (ByteEnum)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x02, 0x55, 0xAA },
        3, new UShortEnum[] { UShortEnum.Red, UShortEnum.Green, UShortEnum.Blue }, (UShortEnum)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x02, 0x00, 0xAA, 0x55 },
        3, new UShortEnum[] { UShortEnum.Red, UShortEnum.Green, UShortEnum.Blue }, (UShortEnum)0)]
    [TestCase(Endianness.LittleEndian,
        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x02, 0x01, 0x55, 0xAA },
        3, new UIntEnum[] { UIntEnum.Red, UIntEnum.Green, UIntEnum.Blue }, (UIntEnum)0)]
    [TestCase(Endianness.BigEndian,
        new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0xAA, 0x55, 0x01, 0x02 },
        3, new UIntEnum[] { UIntEnum.Red, UIntEnum.Green, UIntEnum.Blue }, (UIntEnum)0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80, 0x3F, 0x9A, 0x99, 0x79, 0x41 },
        2, new float[] { 1.0f, 15.6f }, (float)0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x41, 0x79, 0x99, 0x9A },
        2, new float[] { 1.0f, 15.6f }, (float)0)]
    [TestCase(Endianness.LittleEndian,
        new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x1f, 0x85, 0xeb, 0x51, 0xb8, 0x1e, 0x09, 0x40
        },
        2, new double[] { 0.0078125, 3.14 }, (double)0)]
    [TestCase(Endianness.BigEndian,
        new byte[]
        {
            0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x09, 0x1e, 0xb8, 0x51, 0xeb, 0x85, 0x1f
        },
        2, new double[] { 0.0078125, 3.14 }, (double)0)]
    public void EndianBinaryReader_ReadArray_ReadsArray<T>(Endianness endianness, byte[] data, int count, T[] expected, T dummy)
        where T : unmanaged
    {
        var er    = new EndianBinaryReader(new MemoryStream(data), endianness);
        var value = er.Read<T>(count);
        Assert.That(value, Is.EqualTo(expected));
    }
}