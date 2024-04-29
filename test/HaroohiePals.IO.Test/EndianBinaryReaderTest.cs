using System.IO;
using Xunit;

namespace HaroohiePals.IO.Test
{
    public class EndianBinaryReaderTest
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

        [Theory]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (byte)0xAA)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (byte)0xAA)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (sbyte)-86)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (sbyte)-86)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (ushort)0x55AA)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (ushort)0xAA55)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (short)0x55AA)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, (short)-21931)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, 0x420155AAu)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, 0xAA550142u)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, 0x420155AA)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 }, -1437269694)]
        [InlineData(Endianness.LittleEndian,
            new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, 0x381290FF420155AAUL)]
        [InlineData(Endianness.BigEndian,
            new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, 0xAA550142FF901238UL)]
        [InlineData(Endianness.LittleEndian,
            new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, 0x381290FF420155AAL)]
        [InlineData(Endianness.BigEndian,
            new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 }, -6173026326974295496L)]
        [InlineData(Endianness.LittleEndian, new byte[] { 2 }, ByteEnum.Blue)]
        [InlineData(Endianness.BigEndian, new byte[] { 2 }, ByteEnum.Blue)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0x55, 0xAA }, UShortEnum.Blue)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55 }, UShortEnum.Blue)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0x02, 0x01, 0x55, 0xAA }, UIntEnum.Blue)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x02 }, UIntEnum.Blue)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80, 0x3F }, 1.0f)]
        [InlineData(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00 }, 1.0f)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F }, 0.0078125)]
        [InlineData(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0.0078125)]
        public void ReadTest<T>(Endianness endianness, byte[] data, T expected) where T : unmanaged
        {
            var er    = new EndianBinaryReader(new MemoryStream(data), endianness);
            T   value = er.Read<T>();
            Assert.Equal(expected, value);
        }

        [Theory]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            3, new byte[] { 0xAA, 0x55, 0x01 }, (byte)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            3, new byte[] { 0xAA, 0x55, 0x01 }, (byte)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            3, new sbyte[] { -86, 0x55, 0x01 }, (sbyte)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            3, new sbyte[] { -86, 0x55, 0x01 }, (sbyte)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            2, new ushort[] { 0x55AA, 0x4201 }, (ushort)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            2, new ushort[] { 0xAA55, 0x0142 }, (ushort)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            2, new short[] { 0x55AA, 0x4201 }, (short)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42 },
            2, new short[] { -21931, 0x0142 }, (short)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            2, new uint[] { 0x420155AAu, 0x381290FFu }, (uint)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            2, new uint[] { 0xAA550142u, 0xFF901238u }, (uint)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            2, new int[] { 0x420155AA, 0x381290FF }, (int)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            2, new int[] { -1437269694, -7335368 }, (int)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            1, new ulong[] { 0x381290FF420155AAUL }, (ulong)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            1, new ulong[] { 0xAA550142FF901238UL }, (ulong)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            1, new long[] { 0x381290FF420155AAL }, (long)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0xAA, 0x55, 0x01, 0x42, 0xFF, 0x90, 0x12, 0x38 },
            1, new long[] { -6173026326974295496L }, (long)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0, 1, 2 },
            3, new ByteEnum[] { ByteEnum.Red, ByteEnum.Green, ByteEnum.Blue }, (ByteEnum)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0, 1, 2 },
            3, new ByteEnum[] { ByteEnum.Red, ByteEnum.Green, ByteEnum.Blue }, (ByteEnum)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x02, 0x55, 0xAA },
            3, new UShortEnum[] { UShortEnum.Red, UShortEnum.Green, UShortEnum.Blue }, (UShortEnum)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x02, 0x00, 0xAA, 0x55 },
            3, new UShortEnum[] { UShortEnum.Red, UShortEnum.Green, UShortEnum.Blue }, (UShortEnum)0)]
        [InlineData(Endianness.LittleEndian,
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x02, 0x01, 0x55, 0xAA },
            3, new UIntEnum[] { UIntEnum.Red, UIntEnum.Green, UIntEnum.Blue }, (UIntEnum)0)]
        [InlineData(Endianness.BigEndian,
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0xAA, 0x55, 0x01, 0x02 },
            3, new UIntEnum[] { UIntEnum.Red, UIntEnum.Green, UIntEnum.Blue }, (UIntEnum)0)]
        [InlineData(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80, 0x3F, 0x9A, 0x99, 0x79, 0x41 },
            2, new float[] { 1.0f, 15.6f }, (float)0)]
        [InlineData(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x41, 0x79, 0x99, 0x9A },
            2, new float[] { 1.0f, 15.6f }, (float)0)]
        [InlineData(Endianness.LittleEndian,
            new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x1f, 0x85, 0xeb, 0x51, 0xb8, 0x1e, 0x09, 0x40
            },
            2, new double[] { 0.0078125, 3.14 }, (double)0)]
        [InlineData(Endianness.BigEndian,
            new byte[]
            {
                0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x09, 0x1e, 0xb8, 0x51, 0xeb, 0x85, 0x1f
            },
            2, new double[] { 0.0078125, 3.14 }, (double)0)]
        public void ReadArrayTest<T>(Endianness endianness, byte[] data, int count, object expected, T dummy)
            where T : unmanaged
        {
            var er    = new EndianBinaryReader(new MemoryStream(data), endianness);
            var value = er.Read<T>(count);
            Assert.Equal((T[])expected, value);
        }
    }
}