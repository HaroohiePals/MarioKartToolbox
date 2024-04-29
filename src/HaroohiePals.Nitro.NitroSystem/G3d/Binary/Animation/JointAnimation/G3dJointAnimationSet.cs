using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationSet
{
    public const uint Jnt0Signature = 0x30544E4A;

    public G3dJointAnimationSet(EndianBinaryReaderEx reader)
    {
        reader.BeginChunk();
        reader.ReadObject(this);
        JointAnimations = new G3dJointAnimation[Dictionary.Count];
        for (int i = 0; i < Dictionary.Count; i++)
        {
            reader.JumpRelative(Dictionary[i].Data.Offset);
            JointAnimations[i] = new G3dJointAnimation(reader);
        }

        reader.EndChunk(SectionSize);
    }

    [Constant(Jnt0Signature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    public G3dDictionary<OffsetDictionaryData> Dictionary;

    [Ignore]
    public G3dJointAnimation[] JointAnimations;
}