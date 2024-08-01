using ImGuiNET;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

sealed class ImGuizmoContext
{
    //Context
    public ImDrawListPtr DrawList;
    public ImGuizmoStyle Style = new();

    public ImGuizmoMode Mode;
    public Matrix4 ViewMat;
    public Matrix4 ProjectionMat;
    public Matrix4 Model;
    public Matrix4 ModelLocal; // orthonormalized model
    public Matrix4 ModelInverse;
    public Matrix4 ModelSource;
    public Matrix4 ModelSourceInverse;
    public Matrix4 MVP;
    public Matrix4 MVPLocal; // MVP with full model matrix whereas MVP's model matrix might only be translation in case of World space edition
    public Matrix4 ViewProjection;

    public Vector3 ModelScaleOrigin;
    public Vector3 CameraEye;
    public Vector3 CameraRight;
    public Vector3 CameraDir;
    public Vector3 CameraUp;
    public Vector3 RayOrigin;
    public Vector3 RayVector;

    public float RadiusSquareCenter;
    public Vector2 ScreenSquareCenter;
    public Vector2 ScreenSquareMin;
    public Vector2 ScreenSquareMax;

    public float ScreenFactor;
    public Vector3 RelativeOrigin;

    public bool IsUsing = false;
    public bool Enabled = true;
    public bool MouseOver;
    public bool Reversed; // reversed projection matrix

    // translation
    public Vector4 TranslationPlane;
    public Vector3 TranslationPlaneOrigin;
    public Vector3 MatrixOrigin;
    public Vector3 TranslationLastDelta;

    // rotation
    public Vector3 RotationVectorSource;
    public float RotationAngle;
    public float RotationAngleOrigin;

    // scale
    public Vector3 Scale;
    public Vector3 ScaleValueOrigin;
    public Vector3 ScaleLast;
    public float SaveMousePosX;

    // save axis factor when using gizmo
    public bool[] BelowAxisLimit = new bool[3];
    public bool[] BelowPlaneLimit = new bool[3];
    public float[] AxisFactor = new float[3];

    // bounds stretching
    public Vector3 BoundsPivot;
    public Vector3 BoundsAnchor;
    public Vector4 BoundsPlane;
    public Vector3 BoundsLocalPivot;
    public int BoundsBestAxis;
    public int[] BoundsAxis = new int[2];
    public bool IsUsingBounds = false;
    public Matrix4 BoundsMatrix;

    public ImGuizmoMoveType CurrentOperation;

    public float X = 0.0f;
    public float Y = 0.0f;
    public float Width = 0.0f;
    public float Height = 0.0f;
    public float XMax = 0.0f;
    public float YMax = 0.0f;
    public float DisplayRatio = 1.0f;

    public bool IsOrthographic = false;

    public int ActualID = -1;
    public int EditingID = -1;
    public ImGuizmoOperation Operation;

    public bool AllowAxisFlip = true;
    public float GizmoSizeClipSpace = 0.1f;
}
