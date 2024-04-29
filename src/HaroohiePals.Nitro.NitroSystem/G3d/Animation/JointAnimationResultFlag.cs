using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

[Flags]
public enum JointAnimationResultFlag
{
    ScaleOne = 0x01,
    RotationZero = 0x02,
    TranslationZero = 0x04,
    ScaleEx0One = 0x08,
    ScaleEx1One = 0x10,
    MayaSsc = 0x20
}
