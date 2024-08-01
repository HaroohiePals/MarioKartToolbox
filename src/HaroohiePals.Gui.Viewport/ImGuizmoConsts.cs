using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

static class ImGuizmoConsts
{
    private const float QUAD_UV_MIN = 0.5f;
    private const float QUAD_UV_MAX = 0.8f;

    public const float FLT_EPSILON = 1.19209290E-07F;
    public const float RAD_TO_DEG = 180f / MathF.PI;
    public const float DEG_TO_RAD = MathF.PI / 180f;

    public const float SNAP_TENSION = 0.5f;
    public const float SCREEN_ROTATE_SIZE = 0.06f;
    public const float ROTATION_DISPLAY_FACTOR = 1.2f;

    public const int HALF_CIRCLE_SEGMENT_COUNT = 64;

    /// <summary>
    /// Matches MT_MOVE_AB order
    /// </summary>
    public static readonly ImGuizmoOperation[] TranslatePlaneOperations =
    [
        ImGuizmoOperation.TranslateY | ImGuizmoOperation.TranslateZ,
        ImGuizmoOperation.TranslateX | ImGuizmoOperation.TranslateZ,
        ImGuizmoOperation.TranslateX | ImGuizmoOperation.TranslateY
    ];

    public static readonly Vector3[] DirectionUnary = [(1, 0, 0), (0, 1, 0), (0, 0, 1)];

    public static readonly string[] TranslationInfoMask =
    [
        "X : {0:0.000}", 
        "Y : {0:0.000}",
        "Z : {0:0.000}", 
        "Y : {0:0.000} Z : {1:0.000}",
        "X : {0:0.000} Z : {1:0.000}", 
        "X : {0:0.000} Y : {1:0.000}",
        "X : {0:0.000} Y : {1:0.000} Z : {2:0.000}"
    ];

    public static readonly string[] RotationInfoMask =
    [
        "X : {0:0.00} deg {1:0.00} rad",
        "Y : {0:0.00} deg {1:0.00} rad",
        "Z : {0:0.00} deg {1:0.00} rad",
        "Screen : {0:0.00} deg {1:0.00} rad"
    ];

    public static readonly string[] ScaleInfoMask =
    [
        "X : {0:0.00}", "Y : {0:0.00}",
        "Z : {0:0.00}", "XYZ : {0:0.00}"
    ];

    public static readonly int[] TranslationInfoIndex =
    [
        0, 0, 0,
        1, 0, 0,
        2, 0, 0,
        1, 2, 0,
        0, 2, 0,
        0, 1, 0,
        0, 1, 2
    ];

    public static readonly float[] QuadUVs =
    [
        QUAD_UV_MIN, QUAD_UV_MIN,
        QUAD_UV_MIN, QUAD_UV_MAX,
        QUAD_UV_MAX, QUAD_UV_MAX,
        QUAD_UV_MAX, QUAD_UV_MIN
    ];
}
