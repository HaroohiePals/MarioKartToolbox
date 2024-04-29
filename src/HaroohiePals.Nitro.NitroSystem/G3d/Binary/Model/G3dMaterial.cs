using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.G3;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class G3dMaterial
{
    public G3dMaterial(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            er.ReadObject(this);
            if ((Flags & G3dMaterialFlags.TexMtxScaleOne) == 0)
            {
                ScaleS = er.ReadFx32();
                ScaleT = er.ReadFx32();
            }

            if ((Flags & G3dMaterialFlags.TexMtxRotZero) == 0)
            {
                RotationSin = er.ReadFx16();
                RotationCos = er.ReadFx16();
            }

            if ((Flags & G3dMaterialFlags.TexMtxTransZero) == 0)
            {
                TranslationS = er.ReadFx32();
                TranslationT = er.ReadFx32();
            }

            if ((Flags & G3dMaterialFlags.EffectMtx) == G3dMaterialFlags.EffectMtx)
            {
                EffectMtx = new Matrix4d();
                EffectMtx[0, 0] = er.ReadFx32();
                EffectMtx[0, 1] = er.ReadFx32();
                EffectMtx[0, 2] = er.ReadFx32();
                EffectMtx[0, 3] = er.ReadFx32();
                EffectMtx[1, 0] = er.ReadFx32();
                EffectMtx[1, 1] = er.ReadFx32();
                EffectMtx[1, 2] = er.ReadFx32();
                EffectMtx[1, 3] = er.ReadFx32();
                EffectMtx[2, 0] = er.ReadFx32();
                EffectMtx[2, 1] = er.ReadFx32();
                EffectMtx[2, 2] = er.ReadFx32();
                EffectMtx[2, 3] = er.ReadFx32();
                EffectMtx[3, 0] = er.ReadFx32();
                EffectMtx[3, 1] = er.ReadFx32();
                EffectMtx[3, 2] = er.ReadFx32();
                EffectMtx[3, 3] = er.ReadFx32();
            }
        }
        er.EndChunk(Size);
    }

    public G3dMaterial() { }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            er.WriteObject(this);

            if ((Flags & G3dMaterialFlags.TexMtxScaleOne) == 0)
            {
                er.WriteFx32(ScaleS);
                er.WriteFx32(ScaleT);
            }

            if ((Flags & G3dMaterialFlags.TexMtxRotZero) == 0)
            {
                er.WriteFx16(RotationSin);
                er.WriteFx16(RotationCos);
            }

            if ((Flags & G3dMaterialFlags.TexMtxTransZero) == 0)
            {
                er.WriteFx32(TranslationS);
                er.WriteFx32(TranslationT);
            }

            if ((Flags & G3dMaterialFlags.EffectMtx) != 0)
            {
                er.WriteFx32(EffectMtx[0, 0]);
                er.WriteFx32(EffectMtx[0, 1]);
                er.WriteFx32(EffectMtx[0, 2]);
                er.WriteFx32(EffectMtx[0, 3]);
                er.WriteFx32(EffectMtx[1, 0]);
                er.WriteFx32(EffectMtx[1, 1]);
                er.WriteFx32(EffectMtx[1, 2]);
                er.WriteFx32(EffectMtx[1, 3]);
                er.WriteFx32(EffectMtx[2, 0]);
                er.WriteFx32(EffectMtx[2, 1]);
                er.WriteFx32(EffectMtx[2, 2]);
                er.WriteFx32(EffectMtx[2, 3]);
                er.WriteFx32(EffectMtx[3, 0]);
                er.WriteFx32(EffectMtx[3, 1]);
                er.WriteFx32(EffectMtx[3, 2]);
                er.WriteFx32(EffectMtx[3, 3]);
            }
        }
        er.EndChunk();
    }

    public ushort ItemTag;

    [ChunkSize]
    public ushort Size;

    public uint DiffuseAmbient, SpecularEmission;

    [Type(FieldType.U32)]
    public GxPolygonAttr PolygonAttribute;

    public uint PolygonAttributeMask;

    [Type(FieldType.U32)]
    public GxTexImageParam TexImageParam;

    public uint TexImageParamMask;
    public ushort TexPlttBase;

    [Type(FieldType.U16)]
    public G3dMaterialFlags Flags;

    public ushort OriginalWidth, OriginalHeight;

    [Fx32]
    public double MagW, MagH;

    [Ignore]
    public double ScaleS, ScaleT;

    [Ignore]
    public double RotationSin, RotationCos;

    [Ignore]
    public double TranslationS, TranslationT;

    [Ignore]
    public Matrix4d EffectMtx;

    public void SetLightEnableFlags(uint lightMask)
    {
        PolygonAttribute.LightMask = lightMask;
    }

    public void SetTranslucentDepthUpdate(bool update)
    {
        PolygonAttribute.TranslucentDepthUpdate = update;
    }

    public void SetFogEnable(bool enable)
    {
        PolygonAttribute.FogEnable = enable;
    }

    public void SetPolygonId(byte id)
    {
        PolygonAttribute.PolygonId = id;
    }

    public void SetCullMode(GxCull mode)
    {
        PolygonAttribute.CullMode = mode;
    }

    public void SetPolygonMode(GxPolygonMode mode)
    {
        PolygonAttribute.PolygonMode = mode;
    }

    public void SetAlpha(byte alpha)
    {
        PolygonAttribute.Alpha = alpha;
    }

    public void SetDiffuse(ushort color)
    {
        DiffuseAmbient &= ~0x7FFFu;
        DiffuseAmbient |= color & 0x7FFFu;
    }

    public void SetAmbient(ushort color)
    {
        DiffuseAmbient &= ~0x7FFF0000u;
        DiffuseAmbient |= (color & 0x7FFFu) << 16;
    }

    public void SetSpecular(ushort color)
    {
        SpecularEmission &= ~0x7FFFu;
        SpecularEmission |= color & 0x7FFFu;
    }

    public void SetEmission(ushort color)
    {
        SpecularEmission &= ~0x7FFF0000u;
        SpecularEmission |= (color & 0x7FFFu) << 16;
    }
}
