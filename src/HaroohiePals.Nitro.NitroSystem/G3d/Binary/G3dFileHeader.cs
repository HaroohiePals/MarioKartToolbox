using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

public sealed class G3dFileHeader
{
    public G3dFileHeader(uint signature, ushort version)
    {
        Signature  = signature;
        ByteOrder  = 0xFEFF;
        Version    = version;
        HeaderSize = 16;
    }

    public G3dFileHeader(EndianBinaryReaderEx er, uint expectedSignature)
    {
        er.ReadObject(this);

        if (Signature != expectedSignature)
            throw new SignatureNotCorrectException(Signature, expectedSignature, er.BaseStream.Position - 16);

        BlockOffsets = er.Read<uint>(NrBlocks);
    }

    public void Write(EndianBinaryWriter er)
    {
        er.Write(Signature);
        er.Write(ByteOrder);
        er.Write(Version);
        er.Write((uint)0);
        er.Write(HeaderSize);
        er.Write(NrBlocks);
        er.Write(new uint[NrBlocks]);
    }

    public uint   Signature;
    public ushort ByteOrder;
    public ushort Version;
    public uint   FileSize;
    public ushort HeaderSize;
    public ushort NrBlocks;

    [Ignore]
    public uint[] BlockOffsets;
}