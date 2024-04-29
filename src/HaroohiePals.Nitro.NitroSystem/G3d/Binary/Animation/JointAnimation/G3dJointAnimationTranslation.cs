using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationTranslation
{
    public const uint INFO_STEP_MASK = 0xC0000000;
    public const int INFO_STEP_SHIFT = 30;
    public const uint INFO_IS_FX16 = 0x20000000;
    public const uint INFO_LAST_INTERPOLATION_FRAME_MASK = 0x1FFF0000;
    public const int INFO_LAST_INTERPOLATION_FRAME_SHIFT = 16;

    public G3dJointAnimationTranslation(EndianBinaryReaderEx reader, bool isConst, int nrFrames)
    {
        if (isConst)
        {
            ConstTranslation = reader.ReadFx32();
            return;
        }

        Info = reader.Read<uint>();
        Offset = reader.Read<uint>();
        int stepSize = Step.GetStepSize();
        long curpos = reader.JumpRelative(Offset);
        int valueCount = (int)(LastInterpolationFrame / stepSize + (nrFrames - LastInterpolationFrame));
        if ((Info & INFO_IS_FX16) != 0)
        {
            Translation = reader.ReadFx16s(valueCount);
        }
        else
        {
            Translation = reader.ReadFx32s(valueCount);
        }

        reader.BaseStream.Position = curpos;
    }

    public double ConstTranslation;
    public uint Info;
    public uint Offset;
    public double[] Translation;

    public uint LastInterpolationFrame => (Info & INFO_LAST_INTERPOLATION_FRAME_MASK) >> INFO_LAST_INTERPOLATION_FRAME_SHIFT;
    public AnimationStep Step => (AnimationStep)((Info & INFO_STEP_MASK) >> INFO_STEP_SHIFT);
}
