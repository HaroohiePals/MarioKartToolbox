using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class TextureToMaterialDictionaryData : IG3dDictionaryData
{
    public static ushort DataSize => 4;

    public void Read(EndianBinaryReaderEx reader)
    {
        uint flags = reader.Read<uint>();
        Offset = (ushort)(flags & 0xFFFF);
        MaterialCount = (byte)(flags >> 16 & 0x7F);
        Bound = (byte)(flags >> 24 & 0xFF);
    }

    public void Write(EndianBinaryWriterEx writer)
    {
        writer.Write((uint)((Bound & 0xFF) << 24 | (MaterialCount & 0xFF) << 16 | (Offset & 0xFFFF) << 0));
    }

    public ushort Offset;
    public byte MaterialCount;
    public byte Bound;

    public byte[] Materials;
}
