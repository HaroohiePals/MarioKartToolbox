using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

[FieldAlignment(FieldAlignment.FieldSize)]
public sealed class G3dPaletteInfo
{
    public G3dPaletteInfo() { }

    public G3dPaletteInfo(EndianBinaryReaderEx er)
    {
        er.ReadObject(this);

        long curPos = er.JumpRelative(PaletteDataOffset);
        PaletteData = er.Read<byte>(PaletteDataSize << 3);

        er.BaseStream.Position = curPos;
    }

    public void Write(EndianBinaryWriterEx er)
    {
        PaletteDataSize = (ushort)((PaletteData.Length + 7) >> 3);
        er.WriteObject(this);
    }

    public uint VramKey;
    public ushort PaletteDataSize;
    public ushort Flag;
    public ushort DictionaryOffset;

    public uint PaletteDataOffset;

    [Ignore]
    public byte[] PaletteData;
}