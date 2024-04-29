using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

[Flags]
public enum G3dMaterialFlags : ushort
{
    TexMtxUse = 0x0001,
    TexMtxScaleOne = 0x0002,
    TexMtxRotZero = 0x0004,
    TexMtxTransZero = 0x0008,
    OrigWHSame = 0x0010,
    Wireframe = 0x0020,
    Diffuse = 0x0040,
    Ambient = 0x0080,
    VtxColor = 0x0100,
    Specular = 0x0200,
    Emission = 0x0400,
    Shininess = 0x0800,
    TexPlttBase = 0x1000,
    EffectMtx = 0x2000,
};
