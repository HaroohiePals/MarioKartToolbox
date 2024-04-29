using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dShapeSet
{
    public G3dShapeSet(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        ShapeDictionary = new G3dDictionary<OffsetDictionaryData>(er);
        Shapes = new G3dShape[ShapeDictionary.Count];
        for (int i = 0; i < ShapeDictionary.Count; i++)
        {
            er.JumpRelative(ShapeDictionary[i].Data.Offset);
            Shapes[i] = new G3dShape(er);
        }

        er.EndChunk();
    }

    public G3dShapeSet() { }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        ShapeDictionary.Write(er);
        for (int i = 0; i < Shapes.Length; i++)
        {
            ShapeDictionary[i].Data.Offset = (uint)er.GetCurposRelative();
            Shapes[i].Write(er);
        }

        for (int i = 0; i < Shapes.Length; i++)
        {
            Shapes[i].DisplayListOffset = (uint)(er.GetCurposRelative() - ShapeDictionary[i].Data.Offset);
            er.Write(Shapes[i].DisplayList, 0, Shapes[i].DisplayList.Length);
        }

        long curpos = er.JumpRelative(0);
        ShapeDictionary.Write(er);
        for (int i = 0; i < Shapes.Length; i++)
            Shapes[i].Write(er);

        er.BaseStream.Position = curpos;
        er.EndChunk();
    }

    public G3dDictionary<OffsetDictionaryData> ShapeDictionary;

    public G3dShape[] Shapes;
}
