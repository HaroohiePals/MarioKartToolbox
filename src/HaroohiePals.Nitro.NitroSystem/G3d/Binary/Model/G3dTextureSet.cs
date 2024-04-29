using HaroohiePals.Graphics;
using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.Gx;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dTextureSet
{
    public const uint Tex0Signature = 0x30584554;

    public G3dTextureSet()
    {
        TextureInfo = new G3dTextureInfo();
        Texture4x4Info = new G3dTexture4x4Info();
        PaletteInfo = new G3dPaletteInfo();
        TextureDictionary = [];
        PaletteDictionary = [];
    }

    public G3dTextureSet(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            er.ReadObject(this);
        }
        er.EndChunk(SectionSize);
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            er.Write(Tex0Signature);
            er.WriteChunkSize();

            TextureInfo.Write(er);
            Texture4x4Info.Write(er);
            PaletteInfo.Write(er);

            er.WriteCurposRelativeU16(0xE);
            er.WriteCurposRelativeU16(0x1E);
            TextureDictionary.Write(er);

            er.WriteCurposRelativeU16(0x34);
            PaletteDictionary.Write(er);

            er.WriteCurposRelative(0x14);
            er.Write(TextureInfo.TextureData);
            er.WritePadding(4);

            er.WriteCurposRelative(0x24);
            er.Write(Texture4x4Info.TextureData);
            er.WritePadding(4);

            er.WriteCurposRelative(0x28);
            er.Write(Texture4x4Info.TexturePaletteIndexData);
            er.WritePadding(4);

            er.WriteCurposRelative(0x38);
            er.Write(PaletteInfo.PaletteData);
            er.WritePadding(4);
        }
        er.EndChunk();
    }

    [Constant(Tex0Signature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    public G3dTextureInfo TextureInfo;
    public G3dTexture4x4Info Texture4x4Info;
    public G3dPaletteInfo PaletteInfo;

    public G3dDictionary<TextureDictionaryData> TextureDictionary;
    public G3dDictionary<PaletteDictionaryData> PaletteDictionary;

    public Rgba8Bitmap ToBitmap(TextureDictionaryData tex, PaletteDictionaryData pltt)
    {
        int width = 8 << tex.TexImageParam.Width;
        int height = 8 << tex.TexImageParam.Height;
        if (tex.TexImageParam.Format == ImageFormat.Comp4x4)
        {
            return GxUtil.DecodeBmp(
                Texture4x4Info.TextureData.AsSpan(tex.TexImageParam.Address << 3),
                ImageFormat.Comp4x4, width, height,
                PaletteInfo.PaletteData.AsSpan(pltt.Offset << 3),
                tex.TexImageParam.Color0Transparent,
                Texture4x4Info.TexturePaletteIndexData.AsSpan(tex.TexImageParam.Address << 2));
        }
        else if (tex.TexImageParam.Format == ImageFormat.Direct)
        {
            return GxUtil.DecodeBmp(
                TextureInfo.TextureData.AsSpan(tex.TexImageParam.Address << 3),
                ImageFormat.Direct, width, height);
        }
        else
        {
            return GxUtil.DecodeBmp(
                TextureInfo.TextureData.AsSpan(tex.TexImageParam.Address << 3),
                tex.TexImageParam.Format, width, height,
                PaletteInfo.PaletteData.AsSpan(pltt.Offset << 3),
                tex.TexImageParam.Color0Transparent);
        }
    }
}