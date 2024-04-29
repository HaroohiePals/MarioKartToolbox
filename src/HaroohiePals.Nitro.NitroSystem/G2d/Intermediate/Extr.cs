using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d.Intermediate
{
    public class Extr
    {
        public const uint ExtrBlockType = 0x52545845;

        public Extr(EndianBinaryReaderEx er)
        {
            BlockType = er.ReadSignature(ExtrBlockType);
            BlockSize = er.Read<uint>();
            ExtraData = er.Read<byte>((int)BlockSize - 8);
        }

        public uint   BlockType;
        public uint   BlockSize;
        public byte[] ExtraData;
    }
}