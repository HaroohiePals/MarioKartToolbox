using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationRotation
{
    public const uint INFO_STEP_MASK = 0xC0000000;
    public const int INFO_STEP_SHIFT = 30;
    public const uint INFO_LAST_INTERPOLATION_FRAME_MASK = 0x1FFF0000;
    public const int INFO_LAST_INTERPOLATION_FRAME_SHIFT = 16;

    public const uint INDEX_PIVOT = 0x8000;
    public const uint INDEX_DATA_MASK = 0x7FFF;
    public const uint INDEX_DATA_SHIFT = 0;

    public G3dJointAnimationRotation(EndianBinaryReaderEx reader, bool isConst, int nrFrames)
    {
        if (isConst)
        {
            ConstIndex = reader.Read<uint>();
            return;
        }

        Info = reader.Read<uint>();
        Offset = reader.Read<uint>();
        int stepSize = Step.GetStepSize();
        long curpos = reader.JumpRelative(Offset);
        Indices = reader.Read<ushort>((int)(LastInterpolationFrame / stepSize + (nrFrames - LastInterpolationFrame)));

        reader.BaseStream.Position = curpos;
    }

    public uint ConstIndex;

    public uint Info;
    public uint Offset;

    public ushort[] Indices;

    public uint LastInterpolationFrame => (Info & INFO_LAST_INTERPOLATION_FRAME_MASK) >> INFO_LAST_INTERPOLATION_FRAME_SHIFT;
    public AnimationStep Step => (AnimationStep)((Info & INFO_STEP_MASK) >> INFO_STEP_SHIFT);
}
