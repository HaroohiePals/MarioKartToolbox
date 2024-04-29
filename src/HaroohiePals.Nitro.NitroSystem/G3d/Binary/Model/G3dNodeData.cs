using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dNodeData
{
    public const ushort FLAGS_TRANSLATION_ZERO = 0x0001;
    public const ushort FLAGS_ROTATION_ZERO = 0x0002;
    public const ushort FLAGS_SCALE_ONE = 0x0004;
    public const ushort FLAGS_ROTATION_PIVOT = 0x0008;
    public const ushort FLAGS_ROTATION_PIVOT_INDEX_MASK = 0x00F0;
    public const ushort FLAGS_ROTATION_PIVOT_INDEX_SHIFT = 4;
    public const ushort FLAGS_ROTATION_PIVOT_NEGATIVE = 0x0100;
    public const ushort FLAGS_ROTATION_PIVOT_SIGN_REVERSE_C = 0x0200;
    public const ushort FLAGS_ROTATION_PIVOT_SIGN_REVERSE_D = 0x0400;
    public const ushort FLAGS_MATRIX_STACK_INDEX_MASK = 0xF800;
    public const ushort FLAGS_MATRIX_STACK_INDEX_SHIFT = 11;
    public const ushort FLAGS_IDENTITY = FLAGS_TRANSLATION_ZERO | FLAGS_ROTATION_ZERO | FLAGS_SCALE_ONE;

    public G3dNodeData(EndianBinaryReaderEx er)
    {
        Flags = er.Read<ushort>();
        _00 = er.ReadFx16();

        if ((Flags & FLAGS_TRANSLATION_ZERO) == 0)
        {
            Translation = er.ReadVecFx32();
        }

        if ((Flags & FLAGS_ROTATION_ZERO) == 0 && (Flags & FLAGS_ROTATION_PIVOT) == 0)
        {
            _01 = er.ReadFx16();
            _02 = er.ReadFx16();
            _10 = er.ReadFx16();
            _11 = er.ReadFx16();
            _12 = er.ReadFx16();
            _20 = er.ReadFx16();
            _21 = er.ReadFx16();
            _22 = er.ReadFx16();
        }

        if ((Flags & FLAGS_ROTATION_ZERO) == 0 && (Flags & FLAGS_ROTATION_PIVOT) != 0)
        {
            A = er.ReadFx16();
            B = er.ReadFx16();
        }

        if ((Flags & FLAGS_SCALE_ONE) == 0)
        {
            Scale = er.ReadVecFx32();
            InverseScale = er.ReadVecFx32();
        }
    }

    public G3dNodeData() { }

    public void Write(EndianBinaryWriterEx er)
    {
        er.Write(Flags);
        er.WriteFx16(_00);

        if ((Flags & FLAGS_TRANSLATION_ZERO) == 0)
        {
            er.WriteVecFx32(Translation);
        }

        if ((Flags & FLAGS_ROTATION_ZERO) == 0 && (Flags & FLAGS_ROTATION_PIVOT) == 0)
        {
            er.WriteFx16(_01);
            er.WriteFx16(_02);
            er.WriteFx16(_10);
            er.WriteFx16(_11);
            er.WriteFx16(_12);
            er.WriteFx16(_20);
            er.WriteFx16(_21);
            er.WriteFx16(_22);
        }

        if ((Flags & FLAGS_ROTATION_ZERO) == 0 && (Flags & FLAGS_ROTATION_PIVOT) != 0)
        {
            er.WriteFx16(A);
            er.WriteFx16(B);
        }

        if ((Flags & FLAGS_SCALE_ONE) == 0)
        {
            er.WriteVecFx32(Scale);
            er.WriteVecFx32(InverseScale);
        }
    }

    public ushort Flags;
    public double _00;

    public Vector3d Translation;

    public double _01, _02;
    public double _10, _11, _12;
    public double _20, _21, _22;

    public double A, B;

    public Vector3d Scale;
    public Vector3d InverseScale;

    public void GetTranslation(JointAnimationResult jointAnimationResult)
    {
        if ((Flags & FLAGS_TRANSLATION_ZERO) != 0)
        {
            jointAnimationResult.Flag |= JointAnimationResultFlag.TranslationZero;
        }
        else
        {
            jointAnimationResult.Translation = Translation;
        }
    }

    public void GetRotation(JointAnimationResult jointAnimationResult)
    {
        if ((Flags & FLAGS_ROTATION_ZERO) != 0)
        {
            jointAnimationResult.Flag |= JointAnimationResultFlag.RotationZero;
        }
        else
        {
            if ((Flags & FLAGS_ROTATION_PIVOT) != 0)
            {
                jointAnimationResult.Rotation = G3dUtil.DecodePivotRotation(
                    (uint)((Flags & FLAGS_ROTATION_PIVOT_INDEX_MASK) >> FLAGS_ROTATION_PIVOT_INDEX_SHIFT),
                    (Flags & FLAGS_ROTATION_PIVOT_NEGATIVE) != 0,
                    (Flags & FLAGS_ROTATION_PIVOT_SIGN_REVERSE_C) != 0,
                    (Flags & FLAGS_ROTATION_PIVOT_SIGN_REVERSE_D) != 0,
                    A, B);
            }
            else
            {
                jointAnimationResult.Rotation[0, 0] = _00;
                jointAnimationResult.Rotation[0, 1] = _01;
                jointAnimationResult.Rotation[0, 2] = _02;
                jointAnimationResult.Rotation[1, 0] = _10;
                jointAnimationResult.Rotation[1, 1] = _11;
                jointAnimationResult.Rotation[1, 2] = _12;
                jointAnimationResult.Rotation[2, 0] = _20;
                jointAnimationResult.Rotation[2, 1] = _21;
                jointAnimationResult.Rotation[2, 2] = _22;
            }
        }
    }
}

