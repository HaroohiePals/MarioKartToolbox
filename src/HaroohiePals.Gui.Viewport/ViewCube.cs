using ImGuiNET;
using OpenTK.Mathematics;
using System.Diagnostics;
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace HaroohiePals.Gui.Viewport;

// Improved port of the ImGuizmo view cube by Cedric Guillemet
// https://github.com/CedricGuillemet/ImGuizmo/blob/master/ImGuizmo.cpp
internal class ViewCube
{
    private static readonly uint[] DirectionColor = { 0xFF0000AA, 0xFF00AA00, 0xFFAA0000 };

    private bool _isDragging = false;
    private bool _isClicking = false;
    private bool _isInside = false;
    private float _interpolationTime = 0;

    private int _overBox = -1;

    private Matrix4 _interpolationStartMtx;
    private Matrix4 _interpolationEndMtx;

    private bool _wasMouseDown = false;

    private static readonly System.Numerics.Vector2[] PanelPosition =
    {
            new(0.75f, 0.75f), new(0.25f, 0.75f), new(0, 0.75f),
            new(0.75f, 0.25f), new(0.25f, 0.25f), new(0, 0.25f),
            new(0.75f, 0), new(0.25f, 0), new(0, 0)
        };

    private static readonly System.Numerics.Vector2[] PanelSize =
    {
            new(0.25f, 0.25f), new(0.5f, 0.25f), new(0.25f, 0.25f),
            new(0.25f, 0.5f), new(0.5f, 0.5f), new(0.25f, 0.5f),
            new(0.25f, 0.25f), new(0.5f, 0.25f), new(0.25f, 0.25f)
        };

    public void ViewManipulate(ref Matrix4 view, float length, Vector2 position, Vector2 size, uint backgroundColor)
    {
        var io = ImGui.GetIO();

        ImGui.GetWindowDrawList().AddRectFilled(new System.Numerics.Vector2(position.X, position.Y),
            new(position.X + size.X, position.Y + size.Y), backgroundColor);
        var viewInverse = view.Inverted();

        bool upsideDown = Vector3.Dot(viewInverse.Row1.Xyz, Vector3.UnitY) < 0;

        var camTarget = viewInverse.Row3.Xyz - viewInverse.Row2.Xyz * length;

        // view/projection matrices
        const float distance = 3.0f;
        float fov = MathF.Acos(distance / MathF.Sqrt(distance * distance + 3.0f));
        var cubeProjection =
            Matrix4.CreatePerspectiveFieldOfView(fov / MathF.Sqrt(2) * 2, size.X / size.Y, 0.01f, 1000.0f);

        var cubeView = Matrix4.LookAt(viewInverse.Row2.Xyz * distance, Vector3.Zero, viewInverse.Row1.Xyz);

        ImGuizmoUtils.ComputeCameraRay(out var mRayOrigin, out var mRayVector, position, size, cubeProjection, cubeView, false);

        var res = cubeView * cubeProjection;

        // tag faces
        var boxes = new bool[27];
        for (int iPass = 0; iPass < 2; iPass++)
        {
            for (int iFace = 0; iFace < 6; iFace++)
            {
                int normalIndex = iFace % 3;
                int perpXIndex = (normalIndex + 1) % 3;
                int perpYIndex = (normalIndex + 2) % 3;
                float invert = iFace > 2 ? -1f : 1f;
                var indexVectorX = ImGuizmoConsts.DirectionUnary[perpXIndex] * invert;
                var indexVectorY = ImGuizmoConsts.DirectionUnary[perpYIndex] * invert;
                var boxOrigin = ImGuizmoConsts.DirectionUnary[normalIndex] * -invert - indexVectorX - indexVectorY;

                // plan local space
                var n = ImGuizmoConsts.DirectionUnary[normalIndex] * invert;
                var viewSpaceNormal = n;
                var viewSpacePoint = n * 0.5f;
                viewSpaceNormal = Vector3.TransformVector(viewSpaceNormal, cubeView).Normalized();
                viewSpacePoint = Vector3.TransformPosition(viewSpacePoint, cubeView);
                var viewSpaceFacePlan = ImGuizmoUtils.BuildPlane(viewSpacePoint, viewSpaceNormal);

                // back face culling
                if (viewSpaceFacePlan.W > 0)
                    continue;

                var facePlan = ImGuizmoUtils.BuildPlane(n * 0.5f, n);

                float len = ImGuizmoUtils.IntersectRayPlane(mRayOrigin, mRayVector, facePlan);
                var posOnPlan = mRayOrigin + mRayVector * len - n * 0.5f;

                float localx = Vector3.Dot(ImGuizmoConsts.DirectionUnary[perpXIndex], posOnPlan) * invert + 0.5f;
                float localy = Vector3.Dot(ImGuizmoConsts.DirectionUnary[perpYIndex], posOnPlan) * invert + 0.5f;

                // panels
                var dx = ImGuizmoConsts.DirectionUnary[perpXIndex];
                var dy = ImGuizmoConsts.DirectionUnary[perpYIndex];
                var origin = ImGuizmoConsts.DirectionUnary[normalIndex] - dx - dy;
                for (int iPanel = 0; iPanel < 9; iPanel++)
                {
                    var boxCoord = boxOrigin + indexVectorX * (iPanel % 3) +
                                   indexVectorY * (iPanel / 3) + (1, 1, 1);
                    var p = PanelPosition[iPanel] * 2f;
                    var s = PanelSize[iPanel] * 2f;
                    var faceCoordsScreen = new System.Numerics.Vector2[4];
                    Vector3[] panelPos =
                    {
                            dx * p.X + dy * p.Y,
                            dx * p.X + dy * (p.Y + s.Y),
                            dx * (p.X + s.X) + dy * (p.Y + s.Y),
                            dx * (p.X + s.X) + dy * p.Y
                        };

                    for (uint iCoord = 0; iCoord < 4; iCoord++)
                    {
                        faceCoordsScreen[iCoord] = ImGuizmoUtils.WorldToPos((panelPos[iCoord] + origin) * 0.5f * invert, res,
                            position, size);
                    }

                    var panelCorners = new[]
                    {
                            PanelPosition[iPanel], PanelPosition[iPanel] + PanelSize[iPanel]
                        };
                    bool insidePanel = localx > panelCorners[0].X && localx < panelCorners[1].X &&
                                       localy > panelCorners[0].Y && localy < panelCorners[1].Y;
                    int boxCoordInt = (int)(boxCoord.X * 9 + boxCoord.Y * 3 + boxCoord.Z);
                    Debug.Assert(boxCoordInt < 27);
                    boxes[boxCoordInt] |= insidePanel && !_isDragging && ImGui.IsWindowHovered();

                    // draw face with lighter color
                    if (iPass != 0)
                    {
                        ImGui.GetWindowDrawList().AddConvexPolyFilled(ref faceCoordsScreen[0], 4,
                            DirectionColor[normalIndex] | 0x80808080 | (_isInside ? 0x00080808u : 0));
                        if (boxes[boxCoordInt])
                        {
                            ImGui.GetWindowDrawList().AddConvexPolyFilled(ref faceCoordsScreen[0], 4, 0x8060A0F0);

                            if (!_wasMouseDown && ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_isClicking && !_isDragging)
                            {
                                _overBox = boxCoordInt;
                                _isClicking = true;
                                _isDragging = true;
                            }
                        }
                    }
                }
            }
        }

        const float interpolationDuration = 20 * 1 / 60f;

        if (_interpolationTime > 0)
        {
            _interpolationTime -= ImGui.GetIO().DeltaTime;

            float t = 1 - _interpolationTime / interpolationDuration;
            if (t > 1)
                t = 1;
            t = (1 - MathF.Cos(t * MathF.PI)) / 2; //easing
            var quatA = ImGuizmoUtils.MtxToQuat(new Matrix3(_interpolationStartMtx)).Normalized();
            var quatB = ImGuizmoUtils.MtxToQuat(new Matrix3(_interpolationEndMtx)).Normalized();
            var newQuat = Quaternion.Slerp(quatA, quatB, t).Normalized();
            var newTrans = Vector3.Lerp(_interpolationStartMtx.Row3.Xyz, _interpolationEndMtx.Row3.Xyz, t);
            view = Matrix4.CreateFromQuaternion(newQuat);
            view.Row3.Xyz = newTrans;
            view.Invert();
        }

        _isInside = ImGui.IsWindowHovered() &&
                    new Box2(position, position + size).Contains((io.MousePos.X, io.MousePos.Y));

        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left, 5))
            _isClicking = false;

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (_isClicking)
            {
                // apply new view direction
                int cx = _overBox / 9;
                int cy = (_overBox - cx * 9) / 3;
                int cz = _overBox % 3;
                Vector3 interpolationDir = (1.0f - cx, 1.0f - cy, 1.0f - cz);
                interpolationDir.Normalize();

                Vector3 interpolationUp;

                if (MathF.Abs(Vector3.Dot(interpolationDir, Vector3.UnitY)) > 1.0f - 0.01f)
                {
                    var right = viewInverse.Row0.Xyz;
                    if (MathF.Abs(right.X) > MathF.Abs(right.Z))
                        right.Z = 0;
                    else
                        right.X = 0;
                    right.Normalize();
                    interpolationUp = Vector3.Cross(interpolationDir, right);
                    interpolationUp.Normalize();
                }
                else
                    interpolationUp = Vector3.UnitY;

                _interpolationStartMtx = view.Inverted();
                _interpolationEndMtx = Matrix4
                    .LookAt(camTarget + interpolationDir * length, camTarget, interpolationUp).Inverted();

                _interpolationTime = interpolationDuration;
            }

            _isClicking = false;
            _isDragging = false;
        }


        if (_isDragging && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 5))
        {
            float deltaX = ImGui.GetIO().MouseDelta.X * 0.01f;

            if (upsideDown)
                deltaX = -deltaX;

            var rx = Matrix4.CreateFromAxisAngle(view.Row1.Xyz, deltaX);
            var ry = Matrix4.CreateFromAxisAngle(Vector3.UnitX, ImGui.GetIO().MouseDelta.Y * 0.01f);
            view *= rx * ry;
        }

        _wasMouseDown = ImGui.IsMouseDown(ImGuiMouseButton.Left);
    }
}