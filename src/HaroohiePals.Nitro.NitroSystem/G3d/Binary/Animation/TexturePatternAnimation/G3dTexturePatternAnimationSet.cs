using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;

public sealed class G3dTexturePatternAnimationSet
{
    public const uint Pat0Signature = 0x30544150;

    public G3dTexturePatternAnimationSet(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        er.ReadObject(this);

        TexturePatternAnimations = new G3dTexturePatternAnimation[Dictionary.Count];
        for (int i = 0; i < Dictionary.Count; i++)
        {
            er.JumpRelative(Dictionary[i].Data.Offset);
            TexturePatternAnimations[i] = new G3dTexturePatternAnimation(er);
        }

        er.EndChunk(SectionSize);
    }

    [Constant(Pat0Signature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    public G3dDictionary<OffsetDictionaryData> Dictionary;

    [Ignore]
    public G3dTexturePatternAnimation[] TexturePatternAnimations;
}