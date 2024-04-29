using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;

public sealed class TexturePatternAnimationDictionaryData : IG3dDictionaryData
{
    public static ushort DataSize => 8;

    public void Read(EndianBinaryReaderEx reader)
    {
        ushort frameValueCount = reader.Read<ushort>();
        UsePaletteAnimation = reader.Read<ushort>() == 0;
        RatioDataFrame = reader.ReadFx16();
        ushort offset = reader.Read<ushort>();

        long curPos = reader.BaseStream.Position;

        reader.JumpRelative(offset);

        FrameValues = new TexturePatternFrameValue[frameValueCount];
        for (int i = 0; i < frameValueCount; i++)
        {
            FrameValues[i] = new TexturePatternFrameValue(reader);
        }

        reader.BaseStream.Position = curPos;
    }

    public bool UsePaletteAnimation;
    public double RatioDataFrame;

    public TexturePatternFrameValue[] FrameValues;
}

