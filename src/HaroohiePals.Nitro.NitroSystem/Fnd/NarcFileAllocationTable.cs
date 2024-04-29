using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.Fs;

namespace HaroohiePals.Nitro.NitroSystem.Fnd;

public sealed class NarcFileAllocationTable
{
    public const uint FatbSignature = 0x46415442;

    public NarcFileAllocationTable()
    {
        Signature = FatbSignature;
    }

    public NarcFileAllocationTable(EndianBinaryReaderEx reader)
    {
        reader.BeginChunk();
        reader.ReadObject(this);
        reader.EndChunk(SectionSize);
    }

    public void Write(EndianBinaryWriterEx writer)
    {
        FileCount = (ushort)Entries.Length;
        writer.BeginChunk();
        writer.WriteObject(this);
        writer.EndChunk();
    }

    [Constant(FatbSignature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    public ushort FileCount;

    [ArraySize(nameof(FileCount))]
    [Align(4)]
    public FatEntry[] Entries;
}