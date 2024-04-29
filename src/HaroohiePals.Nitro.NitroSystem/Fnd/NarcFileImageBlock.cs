using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.Fnd;

public sealed class NarcFileImageBlock
{
    public const uint FimgSignature = 0x46494D47;

    public NarcFileImageBlock()
    {
        Signature = FimgSignature;
    }

    public NarcFileImageBlock(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        er.ReadObject(this);
        //Some programs seem to write the size wrong, which causes index out of range exceptions
        FileImage = er.Read<byte>((int)(er.BaseStream.Length - er.BaseStream.Position));
        er.EndChunk(SectionSize);
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        er.WriteObject(this);
        er.Write(FileImage);
        er.WritePadding(4, 0xFF);
        er.EndChunk();
    }

    [Constant(FimgSignature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    [Ignore]
    public byte[] FileImage;
}