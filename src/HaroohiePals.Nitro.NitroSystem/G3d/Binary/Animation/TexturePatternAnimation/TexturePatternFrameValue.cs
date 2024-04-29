using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;

public sealed class TexturePatternFrameValue
{
    public TexturePatternFrameValue(EndianBinaryReaderEx er)
    {
        er.ReadObject(this);
    }

    public ushort FrameNumber;
    public byte TextureIndex;
    public byte PaletteIndex;
}