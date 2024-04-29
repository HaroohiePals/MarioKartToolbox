using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;

public sealed class G3dTextureSrtAnimationSet
{
    public const uint Srt0Signature = 0x30545253;

    public G3dTextureSrtAnimationSet(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        er.ReadObject(this);

        TextureSrtAnimations = new G3dTextureSrtAnimation[Dictionary.Count];
        for (int i = 0; i < Dictionary.Count; i++)
        {
            er.JumpRelative(Dictionary[i].Data.Offset);
            TextureSrtAnimations[i] = new G3dTextureSrtAnimation(er);
        }

        er.EndChunk(SectionSize);
    }

    [Constant(Srt0Signature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    public G3dDictionary<OffsetDictionaryData> Dictionary;

    [Ignore]
    public G3dTextureSrtAnimation[] TextureSrtAnimations;

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            er.Write(Srt0Signature);
            er.WriteChunkSize();

            Dictionary.Write(er);
            for (int i = 0; i < TextureSrtAnimations.Length; i++)
            {
                Dictionary[i].Data.Offset = (uint)er.GetCurposRelative();
                TextureSrtAnimations[i].Write(er);
            }

            long curpos = er.JumpRelative(8);
            Dictionary.Write(er);
            er.BaseStream.Position = curpos;

            // To do: Investigate this later:
            // The SRT0 End position is lost since tex srt dictionary write function changes its position to rewrite the dictionary's name array
            //er.BaseStream.Seek(0, System.IO.SeekOrigin.End);
            er.WritePadding(4);
        }
        er.EndChunk();
    }
}