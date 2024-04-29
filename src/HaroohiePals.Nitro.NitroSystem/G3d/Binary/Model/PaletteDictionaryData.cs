using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class PaletteDictionaryData : IG3dDictionaryData
{
    public static ushort DataSize => 4;

    public void Read(EndianBinaryReaderEx er) => er.ReadObject(this);
    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

    public ushort Offset;
    public ushort Flags;
}