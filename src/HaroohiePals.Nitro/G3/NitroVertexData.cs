using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace HaroohiePals.Nitro.G3;

[StructLayout(LayoutKind.Sequential)]
public struct NitroVertexData
{
    public const uint MtxIdMask     = 0x1F;
    public const uint CurMtxId      = 0x1F;
    public const uint HasNormalFlag = 1 << 5;

    public const int PosIdx      = 0;
    public const int NrmClrIdx   = 1;
    public const int TexCoordIdx = 2;
    public const int MtxIdIdx    = 3;

    public Vector3 Position;
    public Vector3 NormalOrColor;
    public Vector2 TexCoord;
    public uint    MtxId;

    public static int Size => Marshal.SizeOf<NitroVertexData>();

    public override string ToString() => $"{Position}";
}