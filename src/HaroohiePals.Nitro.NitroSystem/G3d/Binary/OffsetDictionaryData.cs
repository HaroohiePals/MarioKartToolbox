using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

public class OffsetDictionaryData : IG3dDictionaryData
{
    public static ushort DataSize => 4;

    public void Read(EndianBinaryReaderEx reader)
    {
        Offset = reader.Read<uint>();
    }

    public void Write(EndianBinaryWriterEx writer)
    {
        writer.Write(Offset);
    }

    public uint Offset;
}