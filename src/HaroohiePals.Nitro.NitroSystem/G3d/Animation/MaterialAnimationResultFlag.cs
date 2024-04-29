using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

[Flags]
public enum MaterialAnimationResultFlag
{
    TextureMatrixScaleOne = 0x01,
    TextureMatrixRotationZero = 0x02,
    TextureMatrixTranslationZero = 0x04,
    TextureMatrixSet = 0x08,
    TextureMatrixMult = 0x10,
    Wireframe = 0x20
};
