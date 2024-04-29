using HaroohiePals.IO;

namespace HaroohiePals.Nitro.Fs;

public sealed class DirectoryTableEntry
{
    public DirectoryTableEntry() { }

    public DirectoryTableEntry(EndianBinaryReaderEx er)
        => er.ReadObject(this);

    public void Write(EndianBinaryWriterEx er)
        => er.WriteObject(this);

    public uint   EntryStart;
    public ushort EntryFileId;
    public ushort ParentId;
}