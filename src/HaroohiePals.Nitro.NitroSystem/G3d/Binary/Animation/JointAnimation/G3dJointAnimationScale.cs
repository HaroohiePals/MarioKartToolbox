using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationScale
{
    public const uint INFO_STEP_MASK = 0xC0000000;
    public const int INFO_STEP_SHIFT = 30;
    public const uint INFO_IS_FX16 = 0x20000000;
    public const uint INFO_LAST_INTERPOLATION_FRAME_MASK = 0x1FFF0000;
    public const int INFO_LAST_INTERPOLATION_FRAME_SHIFT = 16;

    public G3dJointAnimationScale(EndianBinaryReaderEx reader, bool isConst, int nrFrames)
    {
        if (isConst)
        {
            ConstScale = reader.ReadFx32();
            ConstInverseScale = reader.ReadFx32();
            return;
        }

        Info = reader.Read<uint>();
        Offset = reader.Read<uint>();
        int stepSize = Step.GetStepSize();
        long curpos = reader.JumpRelative(Offset);
        int count = (int)(LastInterpolationFrame / stepSize + (nrFrames - LastInterpolationFrame));
        Scale = new double[count];
        InverseScale = new double[count];
        if ((Info & INFO_IS_FX16) != 0)
        {
            for (int i = 0; i < count; i++)
            {
                Scale[i] = reader.ReadFx16();
                InverseScale[i] = reader.ReadFx16();
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Scale[i] = reader.ReadFx32();
                InverseScale[i] = reader.ReadFx32();
            }
        }

        reader.BaseStream.Position = curpos;
    }

    public double ConstScale;
    public double ConstInverseScale;
    public uint Info;
    public uint Offset;
    public double[] Scale;
    public double[] InverseScale;

    public uint LastInterpolationFrame => (Info & INFO_LAST_INTERPOLATION_FRAME_MASK) >> INFO_LAST_INTERPOLATION_FRAME_SHIFT;
    public AnimationStep Step => (AnimationStep)((Info & INFO_STEP_MASK) >> INFO_STEP_SHIFT);
}
