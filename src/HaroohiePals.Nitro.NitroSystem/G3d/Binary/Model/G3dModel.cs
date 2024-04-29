using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dModel
{
    public G3dModel() { }

    public G3dModel(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            Size = er.Read<uint>();
            SbcOffset = er.Read<uint>();
            MaterialsOffset = er.Read<uint>();
            ShapesOffset = er.Read<uint>();
            EnvelopeMatricesOffset = er.Read<uint>();
            Info = new G3dModelInfo(er);
            Nodes = new G3dNodeSet(er);
            er.JumpRelative(SbcOffset);
            Sbc = er.Read<byte>((int)(MaterialsOffset - SbcOffset));
            er.JumpRelative(MaterialsOffset);
            Materials = new G3dMaterialSet(er);
            er.JumpRelative(ShapesOffset);
            Shapes = new G3dShapeSet(er);
            if (EnvelopeMatricesOffset != Size && EnvelopeMatricesOffset != 0)
            {
                er.JumpRelative(EnvelopeMatricesOffset);
                EnvelopeMatrices = new G3dEnvelopeMatrices(er, Nodes.NodeDictionary.Count);
            }
        }
        er.EndChunk(Size);
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            er.WriteChunkSize();
            er.Write((uint)0); // SbcOffset
            er.Write((uint)0); // MaterialsOffset
            er.Write((uint)0); // ShapesOffset
            er.Write((uint)0); // EnvelopeMatricesOffset
            Info.Write(er);
            Nodes.Write(er);

            er.WriteCurposRelative(4);
            er.Write(Sbc, 0, Sbc.Length);
            er.WritePadding(4);

            er.WriteCurposRelative(8);
            Materials.Write(er);

            er.WriteCurposRelative(12);
            Shapes.Write(er);

            er.WriteCurposRelative(16);
            EnvelopeMatrices?.Write(er);
        }
        er.EndChunk();
    }

    public uint Size;
    public uint SbcOffset;
    public uint MaterialsOffset;
    public uint ShapesOffset;
    public uint EnvelopeMatricesOffset;

    public G3dModelInfo Info;
    public G3dNodeSet Nodes;
    public byte[] Sbc;
    public G3dMaterialSet Materials;
    public G3dShapeSet Shapes;
    public G3dEnvelopeMatrices EnvelopeMatrices;

    public void SetAllLightEnableFlags(uint lightMask)
    {
        foreach (var m in Materials.Materials)
            m.SetLightEnableFlags(lightMask);
    }

    public void SetAllTranslucentDepthUpdate(bool update)
    {
        foreach (var m in Materials.Materials)
            m.SetTranslucentDepthUpdate(update);
    }

    public void SetAllFogEnable(bool enable)
    {
        foreach (var m in Materials.Materials)
            m.SetFogEnable(enable);
    }

    public void SetAllPolygonId(byte id)
    {
        foreach (var m in Materials.Materials)
            m.SetPolygonId(id);
    }

    public void SetAllCullMode(GxCull mode)
    {
        foreach (var m in Materials.Materials)
            m.SetCullMode(mode);
    }

    public void SetAllPolygonMode(GxPolygonMode mode)
    {
        foreach (var m in Materials.Materials)
            m.SetPolygonMode(mode);
    }

    public void SetAllAlpha(byte alpha)
    {
        foreach (var m in Materials.Materials)
            m.SetAlpha(alpha);
    }

    public void SetAllDiffuse(ushort color)
    {
        foreach (var m in Materials.Materials)
            m.SetDiffuse(color);
    }

    public void SetAllAmbient(ushort color)
    {
        foreach (var m in Materials.Materials)
            m.SetAmbient(color);
    }

    public void SetAllSpecular(ushort color)
    {
        foreach (var m in Materials.Materials)
            m.SetSpecular(color);
    }

    public void SetAllEmission(ushort color)
    {
        foreach (var m in Materials.Materials)
            m.SetEmission(color);
    }
}