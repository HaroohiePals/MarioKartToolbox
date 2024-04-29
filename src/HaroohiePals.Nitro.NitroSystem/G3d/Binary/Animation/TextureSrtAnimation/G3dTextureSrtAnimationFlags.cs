using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;

[Flags]
public enum G3dTextureSrtAnimationFlags : byte
{
    Interpolation = 0x01,
    EndToStartInterpolation = 0x02
}
