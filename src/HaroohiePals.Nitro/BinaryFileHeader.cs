using HaroohiePals.IO;

namespace HaroohiePals.Nitro
{
    public class BinaryFileHeader
    {
        public BinaryFileHeader(uint signature, int nrBlocks)
        {
            Signature  = signature;
            ByteOrder  = 0xFEFF;
            Version    = 0x100;
            HeaderSize = 0x10;
            DataBlocks = (ushort)nrBlocks;
        }

        public BinaryFileHeader(EndianBinaryReader er)
        {
            Signature  = er.Read<uint>();
            ByteOrder  = er.Read<ushort>();
            Version    = er.Read<ushort>();
            FileSize   = er.Read<uint>();
            HeaderSize = er.Read<ushort>();
            DataBlocks = er.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx er)
        {
            er.Write(Signature);
            er.Write(ByteOrder);
            er.Write(Version);
            er.Write((uint)0);
            er.Write((ushort)0x10);
            er.Write(DataBlocks);
        }

        public uint   Signature;
        public ushort ByteOrder;
        public ushort Version;
        public uint   FileSize;
        public ushort HeaderSize;
        public ushort DataBlocks;
    }
}