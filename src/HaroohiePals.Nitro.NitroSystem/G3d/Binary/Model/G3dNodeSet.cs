using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dNodeSet
{
    public G3dNodeSet(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            NodeDictionary = new G3dDictionary<OffsetDictionaryData>(er);
            Data = new G3dNodeData[NodeDictionary.Count];
            long curpos = er.BaseStream.Position;
            for (int i = 0; i < NodeDictionary.Count; i++)
            {
                er.JumpRelative(NodeDictionary[i].Data.Offset);
                Data[i] = new G3dNodeData(er);
            }

            er.BaseStream.Position = curpos;
        }
        er.EndChunk();
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            NodeDictionary.Write(er);
            for (int i = 0; i < Data.Length; i++)
            {
                NodeDictionary[i].Data.Offset = (uint)er.GetCurposRelative();
                Data[i].Write(er);
            }

            long curpos = er.JumpRelative(0);
            NodeDictionary.Write(er);
            er.BaseStream.Position = curpos;
        }
        er.EndChunk();
    }

    public G3dNodeSet() { }

    public G3dDictionary<OffsetDictionaryData> NodeDictionary;

    public G3dNodeData[] Data;
}
