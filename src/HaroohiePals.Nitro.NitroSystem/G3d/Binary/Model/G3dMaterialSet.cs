using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dMaterialSet
{
    public G3dMaterialSet(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            ushort textureToMaterialListDictionaryOffset = er.Read<ushort>();
            ushort paletteToMaterialListDictionaryOffset = er.Read<ushort>();
            MaterialDictionary = new G3dDictionary<OffsetDictionaryData>(er);

            er.JumpRelative(textureToMaterialListDictionaryOffset);
            TextureToMaterialListDictionary = new G3dDictionary<TextureToMaterialDictionaryData>(er);

            er.JumpRelative(paletteToMaterialListDictionaryOffset);
            PaletteToMaterialListDictionary = new G3dDictionary<PaletteToMaterialDictionaryData>(er);

            Materials = new G3dMaterial[MaterialDictionary.Count];
            for (int i = 0; i < MaterialDictionary.Count; i++)
            {
                er.JumpRelative(MaterialDictionary[i].Data.Offset);
                Materials[i] = new G3dMaterial(er);
            }

            foreach (var item in TextureToMaterialListDictionary)
            {
                er.JumpRelative(item.Data.Offset);
                item.Data.Materials = er.Read<byte>(item.Data.MaterialCount);
            }

            foreach (var item in PaletteToMaterialListDictionary)
            {
                er.JumpRelative(item.Data.Offset);
                item.Data.Materials = er.Read<byte>(item.Data.MaterialCount);
            }
        }
        er.EndChunk();
    }

    public G3dMaterialSet() { }

    public void Write(EndianBinaryWriterEx er)
    {
        long offpos = er.BaseStream.Position;
        er.Write((ushort)0); // TextureToMaterialListDictionaryOffset
        er.Write((ushort)0); // PaletteToMaterialListDictionaryOffset
        MaterialDictionary.Write(er);

        // TextureToMaterialListDictionaryOffset
        long curpos = er.BaseStream.Position;
        er.BaseStream.Position = offpos;
        er.Write((ushort)(curpos - offpos));
        er.BaseStream.Position = curpos;

        TextureToMaterialListDictionary.Write(er);

        // PaletteToMaterialListDictionaryOffset
        curpos = er.BaseStream.Position;
        er.BaseStream.Position = offpos + 2;
        er.Write((ushort)(curpos - offpos));
        er.BaseStream.Position = curpos;

        PaletteToMaterialListDictionary.Write(er);

        foreach (var item in TextureToMaterialListDictionary)
        {
            item.Data.Offset = (ushort)(er.BaseStream.Position - offpos);
            foreach (int m in item.Data.Materials)
            {
                er.Write((byte)m);
            }
        }

        foreach (var item in PaletteToMaterialListDictionary)
        {
            item.Data.Offset = (ushort)(er.BaseStream.Position - offpos);
            foreach (int m in item.Data.Materials)
            {
                er.Write((byte)m);
            }
        }

        er.WritePadding(4);

        for (int i = 0; i < Materials.Length; i++)
        {
            MaterialDictionary[i].Data.Offset = (uint)(er.BaseStream.Position - offpos);
            Materials[i].Write(er);
        }

        curpos = er.BaseStream.Position;
        er.BaseStream.Position = offpos + 4;
        MaterialDictionary.Write(er);
        TextureToMaterialListDictionary.Write(er);
        PaletteToMaterialListDictionary.Write(er);
        er.BaseStream.Position = curpos;
    }

    public G3dDictionary<OffsetDictionaryData> MaterialDictionary;
    public G3dDictionary<TextureToMaterialDictionaryData> TextureToMaterialListDictionary;
    public G3dDictionary<PaletteToMaterialDictionaryData> PaletteToMaterialListDictionary;

    public G3dMaterial[] Materials;
}