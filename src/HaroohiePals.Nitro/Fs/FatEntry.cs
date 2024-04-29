using HaroohiePals.IO;

namespace HaroohiePals.Nitro.Fs;

public sealed class FatEntry
{
    public FatEntry(uint offset, uint size)
    {
        FileTop    = offset;
        FileBottom = offset + size;
    }

    public FatEntry(EndianBinaryReaderEx er)
        => er.ReadObject(this);

    public void Write(EndianBinaryWriterEx er)
        => er.WriteObject(this);

    public uint FileTop;
    public uint FileBottom;

    public uint FileSize => FileBottom - FileTop;
}