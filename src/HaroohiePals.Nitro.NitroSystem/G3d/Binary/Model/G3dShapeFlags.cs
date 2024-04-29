using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

[Flags]
public enum G3dShapeFlags : uint
{
    UseNormal = 0x00000001,
    UseColor = 0x00000002,
    UseTexCoord = 0x00000004,
    UseRestoreMtx = 0x00000008
};