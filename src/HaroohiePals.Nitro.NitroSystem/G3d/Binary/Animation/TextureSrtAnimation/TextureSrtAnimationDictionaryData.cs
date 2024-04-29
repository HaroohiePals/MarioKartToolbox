using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;

public sealed class TextureSrtAnimationDictionaryData : IG3dDictionaryData
{
    public static ushort DataSize => 40;

    public const uint ELEM_FX16 = 0x10000000;
    public const uint ELEM_CONST = 0x20000000;

    public const uint ELEM_STEP_MASK = 0xc0000000;
    public const uint ELEM_LAST_INTERP_MASK = 0x0000ffff;

    public const int ELEM_LAST_INTERP_SHIFT = 0;
    public const int ELEM_STEP_SHIFT = 30;

    public void Read(EndianBinaryReaderEx er)
    {
        er.ReadObject(this);
        long curpos = er.BaseStream.Position;
        if ((ScaleS & ELEM_CONST) == 0)
        {
            er.JumpRelative(ScaleSEx);
            int stepSize = ((AnimationStep)((ScaleS & ELEM_STEP_MASK) >> ELEM_STEP_SHIFT)).GetStepSize();
            if ((ScaleS & ELEM_FX16) != 0)
                ScaleSValues = er.ReadFx16s((int)(ScaleS & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
            else
                ScaleSValues = er.ReadFx32s((int)(ScaleS & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
        }

        if ((ScaleT & ELEM_CONST) == 0)
        {
            er.JumpRelative(ScaleTEx);
            int stepSize = ((AnimationStep)((ScaleT & ELEM_STEP_MASK) >> ELEM_STEP_SHIFT)).GetStepSize();
            if ((ScaleT & ELEM_FX16) != 0)
                ScaleTValues = er.ReadFx16s((int)(ScaleT & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
            else
                ScaleTValues = er.ReadFx32s((int)(ScaleT & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
        }

        if ((Rotation & ELEM_CONST) == 0)
        {
            er.JumpRelative(RotationEx);
            int stepSize = ((AnimationStep)((Rotation & ELEM_STEP_MASK) >> ELEM_STEP_SHIFT)).GetStepSize();
            int count = (int)(Rotation & ELEM_LAST_INTERP_MASK) / stepSize + stepSize;

            RotationSinValues = new double[count];
            RotationCosValues = new double[count];
            for (int i = 0; i < count; i++)
            {
                RotationSinValues[i] = er.ReadFx16();
                RotationCosValues[i] = er.ReadFx16();
            }
        }

        if ((TranslationS & ELEM_CONST) == 0)
        {
            er.JumpRelative(TranslationSEx);
            int stepSize = ((AnimationStep)((TranslationS & ELEM_STEP_MASK) >> ELEM_STEP_SHIFT)).GetStepSize();
            if ((TranslationS & ELEM_FX16) != 0)
                TranslationSValues = er.ReadFx16s((int)(TranslationS & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
            else
                TranslationSValues = er.ReadFx32s((int)(TranslationS & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
        }

        if ((TranslationT & ELEM_CONST) == 0)
        {
            er.JumpRelative(TranslationTEx);
            int stepSize = ((AnimationStep)((TranslationT & ELEM_STEP_MASK) >> ELEM_STEP_SHIFT)).GetStepSize();
            if ((TranslationT & ELEM_FX16) != 0)
                TranslationTValues = er.ReadFx16s((int)(TranslationT & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
            else
                TranslationTValues = er.ReadFx32s((int)(TranslationT & ELEM_LAST_INTERP_MASK) / stepSize + stepSize);
        }

        er.BaseStream.Position = curpos;
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.WriteObject(this);
    }

    public void WriteData(EndianBinaryWriterEx er)
    {
        if ((ScaleS & ELEM_CONST) == 0)
        {
            er.JumpRelative(ScaleSEx);
            if ((ScaleS & ELEM_FX16) != 0)
                er.WriteFx16s(ScaleSValues);
            else
                er.WriteFx32s(ScaleSValues);
        }
        else
        {
            er.Write(ScaleSEx);
        }

        if ((ScaleT & ELEM_CONST) == 0)
        {
            er.JumpRelative(ScaleTEx);
            if ((ScaleT & ELEM_FX16) != 0)
                er.WriteFx16s(ScaleTValues);
            else
                er.WriteFx32s(ScaleTValues);
        }
        else
        {
            er.Write(ScaleTEx);
        }

        if ((Rotation & ELEM_CONST) == 0)
        {
            er.JumpRelative(RotationEx);
            int stepSize = ((AnimationStep)((Rotation & ELEM_STEP_MASK) >> ELEM_STEP_SHIFT)).GetStepSize();
            int count = (int)(Rotation & ELEM_LAST_INTERP_MASK) / stepSize + stepSize;

            for (int i = 0; i < count; i++)
            {
                er.WriteFx16(RotationSinValues[i]);
                er.WriteFx16(RotationCosValues[i]);
            }
        }
        else
        {
            er.Write(RotationEx);
        }

        if ((TranslationS & ELEM_CONST) == 0)
        {
            er.JumpRelative(TranslationSEx);
            if ((TranslationS & ELEM_FX16) != 0)
                er.WriteFx16s(TranslationSValues);
            else
                er.WriteFx32s(TranslationSValues);
        }
        else
        {
            er.Write(TranslationSEx);
        }

        if ((TranslationT & ELEM_CONST) == 0)
        {
            er.JumpRelative(TranslationTEx);
            if ((TranslationT & ELEM_FX16) != 0)
                er.WriteFx16s(TranslationTValues);
            else
                er.WriteFx32s(TranslationTValues);
        }
        else
        {
            er.Write(TranslationTEx);
        }
    }

    public uint ScaleS;
    public uint ScaleSEx;

    public uint ScaleT;
    public uint ScaleTEx;

    public uint Rotation;
    public uint RotationEx;

    public uint TranslationS;
    public uint TranslationSEx;

    public uint TranslationT;
    public uint TranslationTEx;

    [Ignore]
    public double[] ScaleSValues, ScaleTValues;

    [Ignore]
    public double[] RotationSinValues, RotationCosValues;

    [Ignore]
    public double[] TranslationSValues, TranslationTValues;
}
