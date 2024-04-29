using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

[Flags]
public enum G3dRenderStateFlag
{
    NodeVisible = 0x00000001,
    MaterialTransparent = 0x00000002,
    CurrentNodeValid = 0x00000004,
    CurrentMaterialValid = 0x00000008,
    CurrentNodeDescriptionValid = 0x00000010,
    Return = 0x00000020,
    Skip = 0x00000040,

    OptRecord = 0x00000080,
    OptNoGeCmd = 0x00000100,
    OptSkipSbcDraw = 0x00000200,
    OptSkipSbcMtxCalc = 0x00000400
}
