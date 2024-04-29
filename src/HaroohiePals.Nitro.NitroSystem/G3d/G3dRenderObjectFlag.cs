using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

[Flags]
public enum G3dRenderObjectFlag
{
    Record         = 0x00000001,
    NoGeCmd        = 0x00000002,
    SkipSbcDraw    = 0x00000004,
    SkipSbcMtxCalc = 0x00000008,
    HintObsolete   = 0x00000010
};
