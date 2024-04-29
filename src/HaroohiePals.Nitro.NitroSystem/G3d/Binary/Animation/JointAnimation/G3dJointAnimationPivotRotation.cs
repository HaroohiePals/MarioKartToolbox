using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class G3dJointAnimationPivotRotation
{
    public const uint INFO_PIVOT_INDEX_MASK = 0x000F;
    public const int INFO_PIVOT_INDEX_SHIFT = 0;
    public const uint INFO_PIVOT_NEGATIVE = 0x0010;
    public const uint INFO_SIGN_REVERSE_C = 0x0020;
    public const uint INFO_SIGN_REVERSE_D = 0x0040;

    public G3dJointAnimationPivotRotation(EndianBinaryReaderEx reader)
    {
        Info = reader.Read<ushort>();
        A = reader.ReadFx16();
        B = reader.ReadFx16();
    }

    public ushort Info;
    public double A;
    public double B;

    public Matrix3d GetRotationMatrix()
    {
        return G3dUtil.DecodePivotRotation(
            (Info & INFO_PIVOT_INDEX_MASK) >> INFO_PIVOT_INDEX_SHIFT,
            (Info & INFO_PIVOT_NEGATIVE) != 0,
            (Info & INFO_SIGN_REVERSE_C) != 0,
            (Info & INFO_SIGN_REVERSE_D) != 0,
            A, B);
    }
}
