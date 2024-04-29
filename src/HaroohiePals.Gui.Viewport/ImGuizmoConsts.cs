using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

internal static class ImGuizmoConsts
{
    public const float FltEpsilon = 1.19209290E-07F;
    public const float RadToDeg = 180f / MathF.PI;
    public const float DegToRad = MathF.PI / 180f;

    public static readonly Vector3[] DirectionUnary = { (1, 0, 0), (0, 1, 0), (0, 0, 1) };
}
