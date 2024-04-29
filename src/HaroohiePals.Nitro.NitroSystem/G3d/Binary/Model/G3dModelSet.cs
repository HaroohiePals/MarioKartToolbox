using HaroohiePals.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dModelSet
{
    public const uint Mdl0Signature = 0x304C444D;

    public G3dModelSet()
    {
        Dictionary = [];
    }

    public G3dModelSet(EndianBinaryReaderEx er)
    {
        uint sectionSize;
        er.BeginChunk();
        {
            er.ReadSignature(Mdl0Signature);
            sectionSize = er.Read<uint>();
            Dictionary = new G3dDictionary<OffsetDictionaryData>(er);
            Models = new G3dModel[Dictionary.Count];
            for (int i = 0; i < Dictionary.Count; i++)
            {
                er.JumpRelative(Dictionary[i].Data.Offset);
                Models[i] = new G3dModel(er);
            }
        }
        er.EndChunk(sectionSize);
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            er.Write(Mdl0Signature);
            er.WriteChunkSize();

            Dictionary.Write(er);
            for (int i = 0; i < Models.Length; i++)
            {
                Dictionary[i].Data.Offset = (uint)er.GetCurposRelative();
                Models[i].Write(er);
            }

            long curpos = er.JumpRelative(8);
            Dictionary.Write(er);
            er.BaseStream.Position = curpos;
        }
        er.EndChunk();
    }

    public G3dDictionary<OffsetDictionaryData> Dictionary;

    public G3dModel[] Models;
}