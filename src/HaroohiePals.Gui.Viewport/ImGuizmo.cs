using ImGuiNET;
using OpenTK.Mathematics;
using System.Drawing;
using System.Security.Principal;

namespace HaroohiePals.Gui.Viewport;

//Based on ImGuizmo: https://github.com/CedricGuillemet/ImGuizmo/blob/master/ImGuizmo.cpp
sealed class ImGuizmo
{
    public ImGuizmoConfirmAction ConfirmAction = ImGuizmoConfirmAction.MouseUp;
    public float? OverrideValue = null;
    public bool IsUsing =>
        (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID))
        || _context.IsUsingBounds;

    private ImGuizmoContext _context = new();
    private bool _forceActivate = false;
    private bool _canActivate
        => _forceActivate || (ImGui.IsMouseClicked(ImGuiMouseButton.Left)
            && !ImGui.IsAnyItemHovered() && !ImGui.IsAnyItemActive());

    public void SetDrawlist(ImDrawListPtr? drawList = null)
        => _context.DrawList = drawList ?? ImGui.GetWindowDrawList();

    public void BeginFrame()
    {
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus;

        ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

        var io = ImGui.GetIO();
        ImGui.SetNextWindowSize(io.DisplaySize);
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));

        ImGui.PushStyleColor(ImGuiCol.WindowBg, 0);
        ImGui.PushStyleColor(ImGuiCol.Border, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);

        bool open = true;
        ImGui.Begin($"gizmo_{GetHashCode()}", ref open, flags);
        _context.DrawList = ImGui.GetWindowDrawList();
        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
    }

    public void SetImGuiContext(IntPtr context) =>
        ImGui.SetCurrentContext(context);

    public bool IsOver()
        => (Intersects(_context.Operation, ImGuizmoOperation.Translate) && GetMoveType(_context.Operation, out _) != ImGuizmoMoveType.None) ||
         (Intersects(_context.Operation, ImGuizmoOperation.Rotate) && GetRotateType(_context.Operation) != ImGuizmoMoveType.None) ||
         (Intersects(_context.Operation, ImGuizmoOperation.Scale) && GetScaleType(_context.Operation) != ImGuizmoMoveType.None) || IsUsing;

    public bool IsOver(ImGuizmoOperation op)
    {
        if (IsUsing)
        {
            return true;
        }
        if (Intersects(op, ImGuizmoOperation.Scale) && GetScaleType(op) != ImGuizmoMoveType.None)
        {
            return true;
        }
        if (Intersects(op, ImGuizmoOperation.Rotate) && GetRotateType(op) != ImGuizmoMoveType.None)
        {
            return true;
        }
        if (Intersects(op, ImGuizmoOperation.Translate) && GetMoveType(op, out _) != ImGuizmoMoveType.None)
        {
            return true;
        }
        return false;
    }

    //New function
    public void StartMoveType(ImGuizmoMoveType moveType)
    {
        _context.CurrentOperation = moveType;
        _forceActivate = true;
    }

    public void StopUsing()
    {
        _context.IsUsing = false;
        _context.IsUsingBounds = false;
    }

    public void Enable(bool enable)
    {
        _context.Enabled = enable;
        if (!enable)
            StopUsing();
    }

    public void SetRect(float x, float y, float width, float height)
    {
        _context.X = x;
        _context.Y = y;
        _context.Width = width;
        _context.Height = height;
        _context.XMax = _context.X + _context.Width;
        _context.YMax = _context.Y + _context.Height;
        _context.DisplayRatio = width / height;
    }

    public void SetOrthographic(bool isOrthographic)
        => _context.IsOrthographic = isOrthographic;

    public bool Manipulate(Matrix4 view, Matrix4 projection, ImGuizmoOperation operation, ImGuizmoMode mode,
        ref Matrix4 matrix, out Matrix4 deltaMatrix, Vector3? snap = null, Box3d? localBounds = null, Vector3? boundsSnap = null)
    {
        // set delta to identity
        deltaMatrix = Matrix4.Identity;

        try
        {
            // Scale is always local or matrix will be skewed when applying world scale or oriented matrix
            ComputeContext(view, projection, matrix, Intersects(operation, ImGuizmoOperation.Scale) ? ImGuizmoMode.Local : mode);

            // behind camera
            var camSpacePosition = Vector3.Zero.TransformPoint(_context.MVP);
            if (!_context.IsOrthographic && camSpacePosition.Z < 0.001f && !_context.IsUsing)
            {
                return false;
            }

            // --
            ImGuizmoMoveType type = ImGuizmoMoveType.None;
            bool manipulated = false;
            if (_context.Enabled)
            {
                if (!_context.IsUsingBounds)
                {
                    manipulated = HandleTranslation(ref matrix, ref deltaMatrix, operation, ref type, snap) ||
                                  HandleScale(ref matrix, ref deltaMatrix, operation, ref type, snap) ||
                                  HandleRotation(ref matrix, ref deltaMatrix, operation, ref type, snap);
                }
            }

            if (localBounds is not null && !_context.IsUsing)
            {
                manipulated = HandleAndDrawLocalBounds(localBounds.Value, ref matrix, boundsSnap, operation);
            }

            _context.Operation = operation;
            if (!_context.IsUsingBounds)
            {
                DrawRotationGizmo(operation, type);
                DrawTranslationGizmo(operation, type);
                DrawScaleGizmo(operation, type);
                DrawScaleUniversalGizmo(operation, type);
            }
            return manipulated;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Manipulate error: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    public Matrix4 ComputeRotation(Matrix4 matrix, ImGuizmoMode mode)
    {
        if (!IsRotateType(_context.CurrentOperation))
            return Matrix4.Identity;

        bool local = mode == ImGuizmoMode.Local || _context.CurrentOperation == ImGuizmoMoveType.RotateScreen;
        var type = _context.CurrentOperation;

        var modelLocal = matrix;
        modelLocal.OrthoNormalize();

        var model = local ? matrix : matrix.ClearRotation();

        Vector4 rotatePlane;

        var rotatePlanNormal = new[] { model.Row0.Xyz, model.Row1.Xyz, model.Row2.Xyz, -_context.CameraDir };

        rotatePlane = local ?
            ImGuizmoUtils.BuildPlane(model.Row3.Xyz, rotatePlanNormal[type - ImGuizmoMoveType.RotateX]) :
            ImGuizmoUtils.BuildPlane(matrix.Row3.Xyz, ImGuizmoConsts.DirectionUnary[type - ImGuizmoMoveType.RotateX]);

        var rotationAxisLocalSpace = Vector3.TransformVector(rotatePlane.Xyz, model.Inverted());
        rotationAxisLocalSpace.Normalize();

        var deltaRotation = Matrix4.CreateFromAxisAngle(rotationAxisLocalSpace, _context.RotationAngle);

        var scaleOrigin = Matrix4.CreateScale(1);


        if (local)
            return scaleOrigin * deltaRotation * modelLocal;

        Matrix4 res = matrix;
        res = res.ClearTranslation();

        res = res * deltaRotation;
        res.Row3 = matrix.Row3;

        return res;
    }

    public void SetId(int id)
    {

    }

    public void SetGizmoSizeClipSpace(float value)
    {

    }

    public void AllowAxisFlip(bool value)
    {
        _context.AllowAxisFlip = value;
    }

    //?? TODO
    private bool IsHoveringWindow()
    {
        return ImGui.IsWindowHovered();
        //var g = ImGui.GetCurrentContext();

        //var window = ImGui.FindWindowByName(_context.DrawList._OwnerName);
        //if (g.HoveredWindow == window)   // Mouse hovering drawlist window
        //    return true;
        //if (g.HoveredWindow != NULL)     // Any other window is hovered
        //    return false;
        //if (ImGui.IsMouseHoveringRect(window.InnerRect.Min, window.InnerRect.Max, false))   // Hovering drawlist window rect, while no other window is hovered (for _NoInputs windows)
        //    return true;
        //return false;
    }

    private void ComputeContext(Matrix4 view, Matrix4 projection, Matrix4 matrix, ImGuizmoMode mode)
    {
        _context.Mode = mode;
        _context.ViewMat = view;
        _context.ProjectionMat = projection;
        _context.MouseOver = IsHoveringWindow();

        _context.ModelLocal = matrix;
        _context.ModelLocal.OrthoNormalize();

        if (mode == ImGuizmoMode.Local)
        {
            _context.Model = _context.ModelLocal;
        }
        else
        {
            _context.Model = Matrix4.CreateTranslation(matrix.Row3.Xyz);
        }
        _context.ModelSource = matrix;
        _context.ModelScaleOrigin = new Vector3(_context.ModelSource.Row0.Length, _context.ModelSource.Row1.Length, _context.ModelSource.Row2.Length);

        _context.ModelInverse = _context.Model.Inverted();
        _context.ModelSourceInverse = _context.ModelSource.Inverted();
        _context.ViewProjection = _context.ViewMat * _context.ProjectionMat;
        _context.MVP = _context.Model * _context.ViewProjection;
        _context.MVPLocal = _context.ModelLocal * _context.ViewProjection;

        var viewInverse = _context.ViewMat.Inverted();
        _context.CameraDir = viewInverse.Row2.Xyz;
        _context.CameraEye = viewInverse.Row3.Xyz;
        _context.CameraRight = viewInverse.Row0.Xyz;
        _context.CameraUp = viewInverse.Row1.Xyz;

        // Fix for orthographic projection view
        if (_context.IsOrthographic)
            _context.CameraEye = _context.ModelSource.Row3.Xyz + _context.CameraDir;

        // projection reverse
        var nearVec = new Vector4(0, 0, 1f, 1f);
        var farVec = new Vector4(0, 0, 2f, 1f);

        var nearPos = nearVec.Transform(_context.ProjectionMat);
        var farPos = farVec.Transform(_context.ProjectionMat);

        _context.Reversed = (nearPos.Z / nearPos.W) > (farPos.Z / farPos.W);

        // compute scale from the size of camera right vector projected on screen at the matrix position
        var pointRight = viewInverse.Row0;
        pointRight.TransformPoint(_context.ViewProjection);
        _context.ScreenFactor = _context.GizmoSizeClipSpace / (pointRight.X / pointRight.W - _context.MVP.Row3.X / _context.MVP.Row3.W);

        var rightViewInverse = Vector3.TransformVector(viewInverse.Row0.Xyz, _context.ModelInverse);
        float rightLength = GetSegmentLengthClipSpace(Vector3.Zero, rightViewInverse);
        _context.ScreenFactor = _context.GizmoSizeClipSpace / rightLength;

        var centerSSpace = ImGuizmoUtils.WorldToPos(Vector3.Zero, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
        _context.ScreenSquareCenter = new Vector2(centerSSpace.X, centerSSpace.Y);

        _context.ScreenSquareMin = new Vector2(centerSSpace.X - 5f, centerSSpace.Y - 5f);
        _context.ScreenSquareMax = new Vector2(centerSSpace.X + 5f, centerSSpace.Y + 5f);

        ImGuizmoUtils.ComputeCameraRay(out _context.RayOrigin, out _context.RayVector, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height), _context.ProjectionMat, _context.ViewMat, _context.Reversed);
    }

    private uint ComputeDirectionColor(int index) => index switch
    {
        0 => GetColorU32(_context.Style.Colors.DirectionX),
        1 => GetColorU32(_context.Style.Colors.DirectionY),
        2 => GetColorU32(_context.Style.Colors.DirectionZ),
        _ => GetColorU32(_context.Style.Colors.DirectionX),
    };

    private uint ComputePlaneColor(int index) => index switch
    {
        0 => GetColorU32(_context.Style.Colors.PlaneX),
        1 => GetColorU32(_context.Style.Colors.PlaneY),
        2 => GetColorU32(_context.Style.Colors.PlaneZ),
        _ => GetColorU32(_context.Style.Colors.PlaneX),
    };

    private uint[] ComputeColors(ImGuizmoMoveType type, ImGuizmoOperation operation)
    {
        uint[] colors = new uint[7];

        if (_context.Enabled)
        {
            uint selectionColor = GetColorU32(_context.Style.Colors.Selection);
            uint unselectedColor = GetColorU32(_context.Style.Colors.Unselected);

            switch (operation)
            {
                case ImGuizmoOperation.Translate:
                    colors[0] = (type == ImGuizmoMoveType.MoveScreen) ? selectionColor : unselectedColor;
                    for (int i = 0; i < 3; i++)
                    {
                        colors[i + 1] = (type == (ImGuizmoMoveType.MoveX + i)) ? selectionColor : ComputeDirectionColor(i);
                        colors[i + 4] = (type == (ImGuizmoMoveType.MoveYZ + i)) ? selectionColor : ComputePlaneColor(i);
                        colors[i + 4] = (type == ImGuizmoMoveType.MoveScreen) ? selectionColor : colors[i + 4];
                    }
                    break;
                case ImGuizmoOperation.Rotate:
                    colors[0] = (type == ImGuizmoMoveType.RotateScreen) ? selectionColor : unselectedColor;
                    for (int i = 0; i < 3; i++)
                    {
                        colors[i + 1] = (type == (ImGuizmoMoveType.RotateX + i)) ? selectionColor : ComputeDirectionColor(i);
                    }
                    break;
                case ImGuizmoOperation.ScaleUniversal:
                case ImGuizmoOperation.Scale:
                    colors[0] = (type == ImGuizmoMoveType.ScaleXYZ) ? selectionColor : unselectedColor;
                    for (int i = 0; i < 3; i++)
                    {
                        colors[i + 1] = (type == (ImGuizmoMoveType.ScaleX + i)) ? selectionColor : ComputeDirectionColor(i);
                    }
                    break;
                // note: this internal function is only called with three possible values for operation
                default:
                    break;
            }
        }
        else
        {
            uint inactiveColor = GetColorU32(_context.Style.Colors.Inactive);
            for (int i = 0; i < 7; i++)
            {
                colors[i] = inactiveColor;
            }
        }

        return colors;
    }

    private uint GetColorU32(Color color)
        => ImGuiEx.ColorConvertColorToU32(color);

    private ImGuizmoMoveType GetMoveType(ImGuizmoOperation op, out Vector3 gizmoHitProportion)
    {
        gizmoHitProportion = Vector3.Zero;

        if (!Intersects(op, ImGuizmoOperation.Translate) || _context.IsUsing || !_context.MouseOver)
            return ImGuizmoMoveType.None;

        var io = ImGui.GetIO();
        ImGuizmoMoveType type = ImGuizmoMoveType.None;

        // screen
        if ((_forceActivate || (
            io.MousePos.X >= _context.ScreenSquareMin.X && io.MousePos.X <= _context.ScreenSquareMax.X &&
            io.MousePos.Y >= _context.ScreenSquareMin.Y && io.MousePos.Y <= _context.ScreenSquareMax.Y)) &&
           Contains(op, ImGuizmoOperation.Translate))
        {
            type = ImGuizmoMoveType.MoveScreen;
        }

        var screenCoordVec2 = io.MousePos - new System.Numerics.Vector2(_context.X, _context.Y);

        var screenCoord = new Vector3(screenCoordVec2.X, screenCoordVec2.Y, 0);

        // compute
        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            ComputeTripodAxisAndVisibility(i, out Vector3 dirAxis, out Vector3 dirPlaneX, out Vector3 dirPlaneY,
                out bool belowAxisLimit, out bool belowPlaneLimit);

            dirAxis = Vector3.TransformVector(dirAxis, _context.Model);
            dirPlaneX = Vector3.TransformVector(dirPlaneX, _context.Model);
            dirPlaneY = Vector3.TransformVector(dirPlaneY, _context.Model);

            var modelPos = _context.Model.Row3.Xyz;

            float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, ImGuizmoUtils.BuildPlane(modelPos, dirAxis));
            Vector3 posOnPlan = _context.RayOrigin + _context.RayVector * len;

            var axisStartOnScreen = ImGuizmoUtils.WorldToPos(modelPos + dirAxis * _context.ScreenFactor * 0.1f, _context.ViewProjection,
                new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height)) - new System.Numerics.Vector2(_context.X, _context.Y);
            var axisEndOnScreen = ImGuizmoUtils.WorldToPos(modelPos + dirAxis * _context.ScreenFactor, _context.ViewProjection,
                new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height)) - new System.Numerics.Vector2(_context.X, _context.Y);

            var closestPointOnAxis = ImGuizmoUtils.PointOnSegment(screenCoord, new Vector3(axisStartOnScreen.X, axisStartOnScreen.Y, 0), new Vector3(axisEndOnScreen.X, axisEndOnScreen.Y, 0));
            if ((closestPointOnAxis - screenCoord).Length < 12.0f && Intersects(op, (ImGuizmoOperation)(((uint)ImGuizmoOperation.TranslateX) << i))) // pixel size
            {
                type = ImGuizmoMoveType.MoveX + i;
            }

            float dx = Vector3.Dot(dirPlaneX, (posOnPlan - modelPos) * (1f / _context.ScreenFactor));
            float dy = Vector3.Dot(dirPlaneY, (posOnPlan - modelPos) * (1f / _context.ScreenFactor));
            if (belowPlaneLimit && dx >= ImGuizmoConsts.QuadUVs[0] && dx <= ImGuizmoConsts.QuadUVs[4]
                && dy >= ImGuizmoConsts.QuadUVs[1] && dy <= ImGuizmoConsts.QuadUVs[3] && Contains(op, ImGuizmoConsts.TranslatePlaneOperations[i]))
            {
                type = ImGuizmoMoveType.MoveYZ + i;
            }

            gizmoHitProportion = new Vector3(dx, dy, 0.0f);
        }

        return type;
    }

    private ImGuizmoMoveType GetRotateType(ImGuizmoOperation op)
    {
        if (_forceActivate)
            return _context.CurrentOperation;

        if (_context.IsUsing)
        {
            return ImGuizmoMoveType.None;
        }
        var io = ImGui.GetIO();
        ImGuizmoMoveType type = ImGuizmoMoveType.None;

        var deltaScreen = new Vector3(io.MousePos.X - _context.ScreenSquareCenter.X, io.MousePos.Y - _context.ScreenSquareCenter.Y, 0f);
        float dist = deltaScreen.Length;
        if (Intersects(op, ImGuizmoOperation.RotateScreen) && (_forceActivate || dist >= (_context.RadiusSquareCenter - 4.0f) && dist < (_context.RadiusSquareCenter + 4.0f)))
        {
            type = ImGuizmoMoveType.RotateScreen;
        }

        //right, up, dir, position;
        var planNormals = new[] { _context.Model.Row0.Xyz, _context.Model.Row1.Xyz, _context.Model.Row2.Xyz };
        var modelPos = _context.Model.Row3.Xyz;

        var modelViewPos = _context.Model.Row3.TransformPoint(_context.ViewMat);

        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            if (!Intersects(op, (ImGuizmoOperation)((int)ImGuizmoOperation.RotateX << i)))
            {
                continue;
            }
            // pickup plan
            var pickupPlan = ImGuizmoUtils.BuildPlane(modelPos, planNormals[i]);

            float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, pickupPlan);
            var intersectWorldPos = _context.RayOrigin + _context.RayVector * len;
            var intersectViewPos = intersectWorldPos.TransformPoint(_context.ViewMat);

            if (MathF.Abs(modelViewPos.Z) - MathF.Abs(intersectViewPos.Z) < -ImGuizmoConsts.FLT_EPSILON)
            {
                continue;
            }

            var localPos = intersectWorldPos - modelPos;
            var idealPosOnCircle = localPos.Normalized();
            idealPosOnCircle = Vector3.TransformVector(idealPosOnCircle, _context.ModelInverse);
            var idealPosOnCircleScreen = ImGuizmoUtils.WorldToPos(idealPosOnCircle * ImGuizmoConsts.ROTATION_DISPLAY_FACTOR * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

            //_context.DrawList.AddCircle(idealPosOnCircleScreen, 5.f, IM_COL32_WHITE);
            var distanceOnScreen = idealPosOnCircleScreen - io.MousePos;

            if (distanceOnScreen.Length() < 8.0f) // pixel size
            {
                type = ImGuizmoMoveType.RotateX + i;
            }
        }

        return type;
    }

    private ImGuizmoMoveType GetScaleType(ImGuizmoOperation op)
    {
        if (_forceActivate)
            return _context.CurrentOperation;

        if (_context.IsUsing)
        {
            return ImGuizmoMoveType.None;
        }
        var io = ImGui.GetIO();
        ImGuizmoMoveType type = ImGuizmoMoveType.None;

        // screen
        if ((_forceActivate || (
            io.MousePos.X >= _context.ScreenSquareMin.X && io.MousePos.X <= _context.ScreenSquareMax.X &&
            io.MousePos.Y >= _context.ScreenSquareMin.Y && io.MousePos.Y <= _context.ScreenSquareMax.Y)) &&
           Contains(op, ImGuizmoOperation.Scale))
        {
            type = ImGuizmoMoveType.ScaleXYZ;
        }

        // compute
        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            if (!Intersects(op, (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleX << i)))
            {
                continue;
            }
            Vector3 dirPlaneX, dirPlaneY, dirAxis;
            bool belowAxisLimit, belowPlaneLimit;
            ComputeTripodAxisAndVisibility(i, out dirAxis, out dirPlaneX, out dirPlaneY, out belowAxisLimit, out belowPlaneLimit, true);

            dirAxis = Vector3.TransformVector(dirAxis, _context.ModelLocal);
            dirPlaneX = Vector3.TransformVector(dirPlaneX, _context.ModelLocal);
            dirPlaneY = Vector3.TransformVector(dirPlaneX, _context.ModelLocal);

            float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, ImGuizmoUtils.BuildPlane(_context.ModelLocal.Row3.Xyz, dirAxis));
            var posOnPlan = _context.RayOrigin + _context.RayVector * len;

            float startOffset = Contains(op, (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i)) ? 1.0f : 0.1f;
            float endOffset = Contains(op, (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i)) ? 1.4f : 1.0f;
            var posOnPlanScreen = ImGuizmoUtils.WorldToPos(posOnPlan, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            var axisStartOnScreen = ImGuizmoUtils.WorldToPos(_context.ModelLocal.Row3.Xyz + dirAxis * _context.ScreenFactor * startOffset, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            var axisEndOnScreen = ImGuizmoUtils.WorldToPos(_context.ModelLocal.Row3.Xyz + dirAxis * _context.ScreenFactor * endOffset, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

            var closestPointOnAxis = ImGuizmoUtils.PointOnSegment(new Vector3(posOnPlanScreen.X, posOnPlanScreen.Y, 0),
                new Vector3(axisStartOnScreen.X, axisStartOnScreen.Y, 0), new Vector3(axisEndOnScreen.X, axisEndOnScreen.Y, 0));

            if ((closestPointOnAxis - new Vector3(posOnPlanScreen.X, posOnPlanScreen.Y, 0)).Length < 12.0f) // pixel size
            {
                type = ImGuizmoMoveType.ScaleX + i;
            }
        }

        // universal

        var deltaScreen = new Vector3(io.MousePos.X - _context.ScreenSquareCenter.X, io.MousePos.Y - _context.ScreenSquareCenter.Y, 0f);
        float dist = deltaScreen.Length;
        if (Contains(op, ImGuizmoOperation.ScaleUniversal) && dist >= 17.0f && dist < 23.0f)
        {
            type = ImGuizmoMoveType.ScaleXYZ;
        }

        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            if (!Intersects(op, (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleXUniversal << i)))
            {
                continue;
            }

            Vector3 dirPlaneX, dirPlaneY, dirAxis;
            bool belowAxisLimit, belowPlaneLimit;
            ComputeTripodAxisAndVisibility(i, out dirAxis, out dirPlaneX, out dirPlaneY, out belowAxisLimit, out belowPlaneLimit, true);

            // draw axis
            if (belowAxisLimit)
            {
                bool hasTranslateOnAxis = Contains(op, (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i));
                float markerScale = hasTranslateOnAxis ? 1.4f : 1.0f;

                var worldDirSSpace = ImGuizmoUtils.WorldToPos((dirAxis * markerScale) * _context.ScreenFactor, _context.MVPLocal, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

                float distance = MathF.Sqrt((worldDirSSpace - io.MousePos).LengthSquared());
                if (distance < 12.0f)
                {
                    type = ImGuizmoMoveType.ScaleX + i;
                }
            }
        }

        return type;
    }

    private void ComputeTripodAxisAndVisibility(int axisIndex, out Vector3 dirAxis, out Vector3 dirPlaneX, out Vector3 dirPlaneY, out bool belowAxisLimit, out bool belowPlaneLimit, bool localCoordinates = false)
    {
        dirAxis = ImGuizmoConsts.DirectionUnary[axisIndex];
        dirPlaneX = ImGuizmoConsts.DirectionUnary[(axisIndex + 1) % 3];
        dirPlaneY = ImGuizmoConsts.DirectionUnary[(axisIndex + 2) % 3];

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID))
        {
            // when using, use stored factors so the gizmo doesn't flip when we translate
            belowAxisLimit = _context.BelowAxisLimit[axisIndex];
            belowPlaneLimit = _context.BelowPlaneLimit[axisIndex];

            dirAxis *= _context.AxisFactor[axisIndex];
            dirPlaneX *= _context.AxisFactor[(axisIndex + 1) % 3];
            dirPlaneY *= _context.AxisFactor[(axisIndex + 2) % 3];
        }
        else
        {
            // new method
            float lenDir = GetSegmentLengthClipSpace(Vector3.Zero, dirAxis, localCoordinates);
            float lenDirMinus = GetSegmentLengthClipSpace(Vector3.Zero, -dirAxis, localCoordinates);

            float lenDirPlaneX = GetSegmentLengthClipSpace(Vector3.Zero, dirPlaneX, localCoordinates);
            float lenDirMinusPlaneX = GetSegmentLengthClipSpace(Vector3.Zero, -dirPlaneX, localCoordinates);

            float lenDirPlaneY = GetSegmentLengthClipSpace(Vector3.Zero, dirPlaneY, localCoordinates);
            float lenDirMinusPlaneY = GetSegmentLengthClipSpace(Vector3.Zero, -dirPlaneY, localCoordinates);

            // For readability
            bool allowFlip = _context.AllowAxisFlip;
            float mulAxis = (allowFlip && lenDir < lenDirMinus && MathF.Abs(lenDir - lenDirMinus) > ImGuizmoConsts.FLT_EPSILON) ? -1f : 1f;
            float mulAxisX = (allowFlip && lenDirPlaneX < lenDirMinusPlaneX && MathF.Abs(lenDirPlaneX - lenDirMinusPlaneX) > ImGuizmoConsts.FLT_EPSILON) ? -1f : 1f;
            float mulAxisY = (allowFlip && lenDirPlaneY < lenDirMinusPlaneY && MathF.Abs(lenDirPlaneY - lenDirMinusPlaneY) > ImGuizmoConsts.FLT_EPSILON) ? -1f : 1f;
            dirAxis *= mulAxis;
            dirPlaneX *= mulAxisX;
            dirPlaneY *= mulAxisY;

            // for axis
            float axisLengthInClipSpace = GetSegmentLengthClipSpace(Vector3.Zero, dirAxis * _context.ScreenFactor, localCoordinates);

            float paraSurf = GetParallelogram(Vector3.Zero, dirPlaneX * _context.ScreenFactor, dirPlaneY * _context.ScreenFactor);
            belowPlaneLimit = paraSurf > 0.0025f;
            belowAxisLimit = (axisLengthInClipSpace > 0.02f);

            // and store values
            _context.AxisFactor[axisIndex] = mulAxis;
            _context.AxisFactor[(axisIndex + 1) % 3] = mulAxisX;
            _context.AxisFactor[(axisIndex + 2) % 3] = mulAxisY;
            _context.BelowAxisLimit[axisIndex] = belowAxisLimit;
            _context.BelowPlaneLimit[axisIndex] = belowPlaneLimit;
        }
    }

    private float GetSegmentLengthClipSpace(Vector3 start, Vector3 end, bool localCoordinates = false)
    {
        Vector4 startOfSegment = new Vector4(start, 0);
        var mvp = localCoordinates ? _context.MVPLocal : _context.MVP;

        startOfSegment = startOfSegment.TransformPoint(mvp);
        if (MathF.Abs(startOfSegment.W) > ImGuizmoConsts.FLT_EPSILON) // check for axis aligned with camera direction
        {
            startOfSegment *= 1.0f / startOfSegment.W;
        }

        Vector4 endOfSegment = new Vector4(end, 0);
        endOfSegment = endOfSegment.TransformPoint(mvp);
        if (MathF.Abs(endOfSegment.W) > ImGuizmoConsts.FLT_EPSILON) // check for axis aligned with camera direction
        {
            endOfSegment *= 1.0f / endOfSegment.W;
        }

        var clipSpaceAxis = endOfSegment - startOfSegment;
        clipSpaceAxis.Y /= _context.DisplayRatio;
        float segmentLengthInClipSpace = MathF.Sqrt(clipSpaceAxis.X * clipSpaceAxis.X + clipSpaceAxis.Y * clipSpaceAxis.Y);
        return segmentLengthInClipSpace;
    }

    private float GetParallelogram(Vector3 ptO, Vector3 ptA, Vector3 ptB)
    {
        Vector4[] pts = new[] { new Vector4(ptO, 0), new Vector4(ptA, 0), new Vector4(ptB, 0) };
        for (uint i = 0; i < 3; i++)
        {
            pts[i] = pts[i].TransformPoint(_context.MVP);
            if (MathF.Abs(pts[i].W) > ImGuizmoConsts.FLT_EPSILON) // check for axis aligned with camera direction
            {
                pts[i] *= 1f / pts[i].W;
            }
        }
        var segA = pts[1] - pts[0];
        var segB = pts[2] - pts[0];
        segA.Y /= _context.DisplayRatio;
        segB.Y /= _context.DisplayRatio;
        var segAOrtho = new Vector3(-segA.Y, segA.X, 0);
        segAOrtho.Normalize();
        float dt = Vector3.Dot(segAOrtho, segB.Xyz);
        float surface = MathF.Sqrt(segA.X * segA.X + segA.Y * segA.Y) * MathF.Abs(dt);
        return surface;
    }

    private void DrawScaleUniversalGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        var drawList = _context.DrawList;

        if (!Intersects(op, ImGuizmoOperation.ScaleUniversal))
        {
            return;
        }

        // colors
        uint[] colors = ComputeColors(type, ImGuizmoOperation.ScaleUniversal);

        // draw
        var scaleDisplay = new Vector3(1);

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID))
        {
            scaleDisplay = _context.Scale;
        }

        for (int i = 0; i < 3; i++)
        {
            if (!Intersects(op, (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleXUniversal << i)))
            {
                continue;
            }
            bool usingAxis = (_context.IsUsing && type == ImGuizmoMoveType.ScaleX + i);
            if (!_context.IsUsing || usingAxis)
            {
                ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY, out bool belowAxisLimit, out bool belowPlaneLimit, true);

                // draw axis
                if (belowAxisLimit)
                {
                    bool hasTranslateOnAxis = Contains(op, (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i));
                    float markerScale = hasTranslateOnAxis ? 1.4f : 1.0f;
                    var worldDirSSpace = ImGuizmoUtils.WorldToPos((dirAxis * markerScale * scaleDisplay[i]) * _context.ScreenFactor, _context.MVPLocal, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

                    drawList.AddCircleFilled(worldDirSSpace, 12f, colors[i + 1]);
                }
            }
        }

        // draw screen cirle
        var squareCenter = new System.Numerics.Vector2(_context.ScreenSquareCenter.X, _context.ScreenSquareCenter.Y);
        drawList.AddCircle(squareCenter, 20f, colors[0], 32, _context.Style.CenterCircleSize);

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsScaleType(type))
        {
            var destinationPosOnScreen = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

            int componentInfoIndex = (type - ImGuizmoMoveType.ScaleX) * 3;
            string formattedString = string.Format(ImGuizmoConsts.ScaleInfoMask[type - ImGuizmoMoveType.ScaleX], scaleDisplay[ImGuizmoConsts.TranslationInfoIndex[componentInfoIndex]]);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15), GetColorU32(_context.Style.Colors.TextShadow), formattedString);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14), GetColorU32(_context.Style.Colors.Text), formattedString);
        }
    }

    private void DrawScaleGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        var drawList = _context.DrawList;

        if (!Intersects(op, ImGuizmoOperation.Scale))
        {
            return;
        }

        // colors
        uint[] colors = ComputeColors(type, ImGuizmoOperation.Scale);

        // draw
        var scaleDisplay = new Vector3(1);

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID))
        {
            scaleDisplay = _context.Scale;
        }

        for (int i = 0; i < 3; i++)
        {
            if (!Intersects(op, (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleX << i)))
            {
                continue;
            }
            bool usingAxis = (_context.IsUsing && type == ImGuizmoMoveType.ScaleX + i);
            if (!_context.IsUsing || usingAxis)
            {
                ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY, out bool belowAxisLimit, out bool belowPlaneLimit, true);

                // draw axis
                if (belowAxisLimit)
                {
                    bool hasTranslateOnAxis = Contains(op, (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i));
                    float markerScale = hasTranslateOnAxis ? 1.4f : 1.0f;
                    var baseSSpace = ImGuizmoUtils.WorldToPos(dirAxis * 0.1f * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
                    var worldDirSSpaceNoScale = ImGuizmoUtils.WorldToPos(dirAxis * markerScale * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
                    var worldDirSSpace = ImGuizmoUtils.WorldToPos((dirAxis * markerScale * scaleDisplay[i]) * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

                    if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID))
                    {
                        uint scaleLineColor = GetColorU32(_context.Style.Colors.ScaleLine);
                        drawList.AddLine(baseSSpace, worldDirSSpaceNoScale, scaleLineColor, _context.Style.ScaleLineThickness);
                        drawList.AddCircleFilled(worldDirSSpaceNoScale, _context.Style.ScaleLineCircleSize, scaleLineColor);
                    }

                    if (!hasTranslateOnAxis || _context.IsUsing)
                    {
                        drawList.AddLine(baseSSpace, worldDirSSpace, colors[i + 1], _context.Style.ScaleLineThickness);
                    }
                    drawList.AddCircleFilled(worldDirSSpace, _context.Style.ScaleLineCircleSize, colors[i + 1]);

                    if (_context.AxisFactor[i] < 0f)
                    {
                        DrawHatchedAxis(dirAxis * scaleDisplay[i]);
                    }
                }
            }
        }

        // draw screen cirle
        var squareCenter = new System.Numerics.Vector2(_context.ScreenSquareCenter.X, _context.ScreenSquareCenter.Y);
        drawList.AddCircleFilled(squareCenter, _context.Style.CenterCircleSize, colors[0], 32);

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsScaleType(type))
        {
            var destinationPosOnScreen = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

            int componentInfoIndex = (type - ImGuizmoMoveType.ScaleX) * 3;
            string formattedString = string.Format(ImGuizmoConsts.ScaleInfoMask[type - ImGuizmoMoveType.ScaleX], scaleDisplay[ImGuizmoConsts.TranslationInfoIndex[componentInfoIndex]]);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15), GetColorU32(_context.Style.Colors.TextShadow), formattedString);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14), GetColorU32(_context.Style.Colors.Text), formattedString);
        }
    }

    private void DrawTranslationGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        var drawList = _context.DrawList;
        //if (!drawList)
        //{
        //   return;
        //}

        if (!Intersects(op, ImGuizmoOperation.Translate))
        {
            return;
        }

        //// colors
        //ImU32 colors[7];
        uint[] colors = ComputeColors(type, ImGuizmoOperation.Translate);

        var origin = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

        // draw
        for (int i = 0; i < 3; ++i)
        {
            ComputeTripodAxisAndVisibility(i, out Vector3 dirAxis, out Vector3 dirPlaneX, out Vector3 dirPlaneY,
                out bool belowAxisLimit, out bool belowPlaneLimit);

            if (!_context.IsUsing || (_context.IsUsing && type == ImGuizmoMoveType.MoveX + i))
            {
                // draw axis
                if (belowAxisLimit && Intersects(op, (ImGuizmoOperation)(((uint)ImGuizmoOperation.TranslateX) << i)))
                {
                    var baseSSpace = ImGuizmoUtils.WorldToPos(dirAxis * 0.1f * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
                    var worldDirSSpace = ImGuizmoUtils.WorldToPos(dirAxis * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));

                    drawList.AddLine(baseSSpace, worldDirSSpace, colors[i + 1], _context.Style.TranslationLineThickness);

                    // Arrow head begin
                    var dir = origin - worldDirSSpace;

                    float d = MathF.Sqrt(dir.LengthSquared());
                    dir /= d; // Normalize
                    dir *= _context.Style.TranslationLineArrowSize;

                    var ortogonalDir = new System.Numerics.Vector2(dir.Y, -dir.X); // Perpendicular vector
                    var a = worldDirSSpace + dir;

                    drawList.AddTriangleFilled(worldDirSSpace - dir, a + ortogonalDir, a - ortogonalDir, colors[i + 1]);
                    // Arrow head end

                    if (_context.AxisFactor[i] < 0f)
                    {
                        DrawHatchedAxis(dirAxis);
                    }
                }
            }

            // draw plane
            if (!_context.IsUsing || (_context.IsUsing && type == ImGuizmoMoveType.MoveYZ + i))
            {
                if (belowPlaneLimit && Contains(op, ImGuizmoConsts.TranslatePlaneOperations[i]))
                {
                    System.Numerics.Vector2[] screenQuadPts = new System.Numerics.Vector2[4];
                    for (int j = 0; j < 4; ++j)
                    {
                        var cornerWorldPos = (dirPlaneX * ImGuizmoConsts.QuadUVs[j * 2] + dirPlaneY * ImGuizmoConsts.QuadUVs[j * 2 + 1]) * _context.ScreenFactor;
                        screenQuadPts[j] = ImGuizmoUtils.WorldToPos(cornerWorldPos, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
                    }
                    drawList.AddPolyline(ref screenQuadPts[0], 4, ComputeDirectionColor(i), ImDrawFlags.Closed, 1.0f);
                    drawList.AddConvexPolyFilled(ref screenQuadPts[0], 4, colors[i + 4]);
                }
            }
        }

        var screenSquareCenter = new System.Numerics.Vector2(_context.ScreenSquareCenter.X, _context.ScreenSquareCenter.Y);
        drawList.AddCircleFilled(screenSquareCenter, _context.Style.CenterCircleSize, colors[0], 32);

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsTranslateType(type))
        {
            uint translationLineColor = GetColorU32(_context.Style.Colors.TranslationLine);

            var sourcePosOnScreen = ImGuizmoUtils.WorldToPos(_context.MatrixOrigin, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            var destinationPosOnScreen = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            var dif = new Vector3(destinationPosOnScreen.X - sourcePosOnScreen.X, destinationPosOnScreen.Y - sourcePosOnScreen.Y, 0);
            dif.Normalize();
            dif *= 5f;
            drawList.AddCircle(sourcePosOnScreen, 6f, translationLineColor);
            drawList.AddCircle(destinationPosOnScreen, 6f, translationLineColor);
            drawList.AddLine(new System.Numerics.Vector2(sourcePosOnScreen.X + dif.X, sourcePosOnScreen.Y + dif.Y),
                new System.Numerics.Vector2(destinationPosOnScreen.X - dif.X, destinationPosOnScreen.Y - dif.Y), translationLineColor, 2f);

            var deltaInfo = _context.Model.Row3.Xyz - _context.MatrixOrigin;
            int componentInfoIndex = (type - ImGuizmoMoveType.MoveX) * 3;
            string formattedString = string.Format(ImGuizmoConsts.TranslationInfoMask[type - ImGuizmoMoveType.MoveX],
                deltaInfo[ImGuizmoConsts.TranslationInfoIndex[componentInfoIndex]],
                deltaInfo[ImGuizmoConsts.TranslationInfoIndex[componentInfoIndex + 1]],
                deltaInfo[ImGuizmoConsts.TranslationInfoIndex[componentInfoIndex + 2]]);

            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15), GetColorU32(_context.Style.Colors.TextShadow), formattedString);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14), GetColorU32(_context.Style.Colors.Text), formattedString);
        }
    }

    private void DrawHatchedAxis(Vector3 axis)
    {
        if (_context.Style.HatchedAxisLineThickness <= 0.0f)
        {
            return;
        }

        for (int j = 1; j < 10; j++)
        {
            var baseSSpace2 = ImGuizmoUtils.WorldToPos(axis * 0.05f * (float)(j * 2) * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            var worldDirSSpace2 = ImGuizmoUtils.WorldToPos(axis * 0.05f * (float)(j * 2 + 1) * _context.ScreenFactor, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            _context.DrawList.AddLine(baseSSpace2, worldDirSSpace2, GetColorU32(_context.Style.Colors.HatchedAxisLines), _context.Style.HatchedAxisLineThickness);
        }
    }

    private void DrawRotationGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        if (!Intersects(op, ImGuizmoOperation.Rotate))
        {
            return;
        }
        var drawList = _context.DrawList;

        // colors
        uint[] colors = ComputeColors(type, ImGuizmoOperation.Rotate);

        Vector3 cameraToModelNormalized;
        if (_context.IsOrthographic)
        {
            var viewInverse = _context.ViewMat.Inverted();
            cameraToModelNormalized = -viewInverse.Row2.Xyz;
        }
        else
        {
            cameraToModelNormalized = (_context.Model.Row3.Xyz - _context.CameraEye).Normalized();
        }

        cameraToModelNormalized = Vector3.TransformVector(cameraToModelNormalized, _context.ModelInverse);

        _context.RadiusSquareCenter = ImGuizmoConsts.SCREEN_ROTATE_SIZE * _context.Height;

        bool hasRSC = Intersects(op, ImGuizmoOperation.RotateScreen);
        for (int axis = 0; axis < 3; axis++)
        {
            if (!Intersects(op, (ImGuizmoOperation)(((int)ImGuizmoOperation.RotateZ) >> axis)))
            {
                continue;
            }
            bool usingAxis = (_context.IsUsing && type == ImGuizmoMoveType.RotateZ - axis);
            int circleMul = (hasRSC && !usingAxis) ? 1 : 2;

            var circlePos = new System.Numerics.Vector2[circleMul * ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT + 1];

            float angleStart = MathF.Atan2(cameraToModelNormalized[(4 - axis) % 3], cameraToModelNormalized[(3 - axis) % 3]) + MathF.PI * 0.5f;

            for (int i = 0; i < circlePos.Length; i++)
            {
                float ng = angleStart + circleMul * MathF.PI * (i / (float)ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT);
                var axisPos = new Vector3(MathF.Cos(ng), MathF.Sin(ng), 0f);
                var pos = new Vector3(axisPos[axis], axisPos[(axis + 1) % 3], axisPos[(axis + 2) % 3]) * _context.ScreenFactor * ImGuizmoConsts.ROTATION_DISPLAY_FACTOR;
                circlePos[i] = ImGuizmoUtils.WorldToPos(pos, _context.MVP, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            }
            if (!_context.IsUsing || usingAxis)
            {
                drawList.AddPolyline(ref circlePos[0], circleMul * ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT + 1, colors[3 - axis], ImDrawFlags.None, _context.Style.RotationLineThickness);
            }

            var worldToPos = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height)) - circlePos[0];
            float radiusAxis = MathF.Sqrt(worldToPos.LengthSquared());
            if (radiusAxis > _context.RadiusSquareCenter)
            {
                _context.RadiusSquareCenter = radiusAxis;
            }
        }
        if (hasRSC && (!_context.IsUsing || type == ImGuizmoMoveType.RotateScreen))
        {
            drawList.AddCircle(ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height)),
                _context.RadiusSquareCenter, colors[0], 64, _context.Style.RotationOuterLineThickness);
        }

        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsRotateType(type))
        {
            var circlePos = new System.Numerics.Vector2[ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT + 1];

            circlePos[0] = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            for (uint i = 1; i < ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT; i++)
            {
                float ng = _context.RotationAngle * ((i - 1) / (float)(ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT - 1));
                var rotateVectorMatrix = Matrix4.CreateFromAxisAngle(_context.TranslationPlane.Xyz, ng);
                var pos = _context.RotationVectorSource.TransformPoint(rotateVectorMatrix);
                pos *= _context.ScreenFactor * ImGuizmoConsts.ROTATION_DISPLAY_FACTOR;
                circlePos[i] = ImGuizmoUtils.WorldToPos(pos + _context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
            }
            drawList.AddConvexPolyFilled(ref circlePos[0], ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT, GetColorU32(_context.Style.Colors.RotationUsingFill));
            drawList.AddPolyline(ref circlePos[0], ImGuizmoConsts.HALF_CIRCLE_SEGMENT_COUNT, GetColorU32(_context.Style.Colors.RotationUsingBorder), ImDrawFlags.None, _context.Style.RotationLineThickness);

            var destinationPosOnScreen = circlePos[1];
            string formattedString = string.Format(ImGuizmoConsts.RotationInfoMask[type - ImGuizmoMoveType.RotateX], (_context.RotationAngle / MathF.PI) * 180f, _context.RotationAngle);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15), GetColorU32(_context.Style.Colors.TextShadow), formattedString);
            drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14), GetColorU32(_context.Style.Colors.Text), formattedString);
        }
    }

    private bool HandleAndDrawLocalBounds(Box3d localBounds, ref Matrix4 matrix, Vector3? snapValues, ImGuizmoOperation operation)
    {
        float[] bounds =
        [
            (float)localBounds.Min.X, (float)localBounds.Min.Y, (float)localBounds.Min.Z,
            (float)localBounds.Max.X, (float)localBounds.Max.Y, (float)localBounds.Max.Z
        ];

        bool modified = false;

        var io = ImGui.GetIO();
        var drawList = _context.DrawList;

        // compute best projection axis
        Vector3[] axesWorldDirections = new Vector3[3];
        Vector3 bestAxisWorldDirection = Vector3.Zero;
        int[] axes = new int[3];
        int numAxes = 1;
        axes[0] = _context.BoundsBestAxis;
        int bestAxis = axes[0];
        if (!_context.IsUsingBounds)
        {
            numAxes = 0;
            float bestDot = 0f;
            for (int i = 0; i < 3; i++)
            {
                var dirPlaneNormalWorld = Vector3.TransformVector(ImGuizmoConsts.DirectionUnary[i], _context.ModelSource).Normalized();

                float dt = MathF.Abs(Vector3.Dot((_context.CameraEye - _context.ModelSource.Row3.Xyz).Normalized(), dirPlaneNormalWorld));
                if (dt >= bestDot)
                {
                    bestDot = dt;
                    bestAxis = i;
                    bestAxisWorldDirection = dirPlaneNormalWorld;
                }

                if (dt >= 0.1f)
                {
                    axes[numAxes] = i;
                    axesWorldDirections[numAxes] = dirPlaneNormalWorld;
                    ++numAxes;
                }
            }
        }

        if (numAxes == 0)
        {
            axes[0] = bestAxis;
            axesWorldDirections[0] = bestAxisWorldDirection;
            numAxes = 1;
        }
        else if (bestAxis != axes[0])
        {
            int bestIndex = 0;
            for (int i = 0; i < numAxes; i++)
            {
                if (axes[i] == bestAxis)
                {
                    bestIndex = i;
                    break;
                }
            }
            int tempAxis = axes[0];
            axes[0] = axes[bestIndex];
            axes[bestIndex] = tempAxis;
            var tempDirection = axesWorldDirections[0];
            axesWorldDirections[0] = axesWorldDirections[bestIndex];
            axesWorldDirections[bestIndex] = tempDirection;
        }

        for (int axisIndex = 0; axisIndex < numAxes; ++axisIndex)
        {
            bestAxis = axes[axisIndex];
            bestAxisWorldDirection = axesWorldDirections[axisIndex];

            // corners
            Vector4[] aabb = new Vector4[4];

            int secondAxis = (bestAxis + 1) % 3;
            int thirdAxis = (bestAxis + 2) % 3;

            for (int i = 0; i < 4; i++)
            {
                aabb[i][3] = aabb[i][bestAxis] = 0f;

                int index = secondAxis + 3 * (i >> 1);
                aabb[i][secondAxis] = bounds[index];

                index = thirdAxis + 3 * ((i >> 1) ^ (i & 1));
                aabb[i][thirdAxis] = bounds[index];
            }

            // draw bounds
            uint boundsColor = GetColorU32(_context.Enabled ? _context.Style.Colors.LocalBounds : _context.Style.Colors.LocalBoundsDisabled);

            Matrix4 boundsMVP = _context.ModelSource * _context.ViewProjection;

            for (int i = 0; i < 4; i++)
            {
                var worldBound1 = ImGuizmoUtils.WorldToPos(aabb[i].Xyz, boundsMVP, (_context.X, _context.Y), (_context.Width, _context.Height));
                var worldBound2 = ImGuizmoUtils.WorldToPos(aabb[(i + 1) % 4].Xyz, boundsMVP, (_context.X, _context.Y), (_context.Width, _context.Height));

                if (!IsInContextRect(worldBound1) || !IsInContextRect(worldBound2))
                    continue;

                float boundDistance = MathF.Sqrt((worldBound1 - worldBound2).LengthSquared());
                int stepCount = (int)(boundDistance / 10f);
                stepCount = Math.Min(stepCount, 1000);
                for (int j = 0; j < stepCount; j++)
                {
                    float stepLength = 1f / stepCount;
                    float t1 = j * stepLength;
                    float t2 = j * stepLength + stepLength * 0.5f;
                    var worldBoundSS1 = System.Numerics.Vector2.Lerp(worldBound1, worldBound2, t1);
                    var worldBoundSS2 = System.Numerics.Vector2.Lerp(worldBound1, worldBound2, t2);
                    drawList.AddLine(worldBoundSS1, worldBoundSS2, boundsColor, _context.Style.LocalBoundsLineThickness);
                }
                var midPoint = (aabb[i] + aabb[(i + 1) % 4]) * 0.5f;
                var midBound = ImGuizmoUtils.WorldToPos(midPoint.Xyz, boundsMVP, (_context.X, _context.Y), (_context.Width, _context.Height));
                float anchorBigRadius = _context.Style.LocalBoundsAnchorBigRadius;
                float anchorSmallRadius = _context.Style.LocalBoundsAnchorSmallRadius;
                bool overBigAnchor = (worldBound1 - io.MousePos).LengthSquared() <= (anchorBigRadius * anchorBigRadius);
                bool overSmallAnchor = (midBound - io.MousePos).LengthSquared() <= (anchorBigRadius * anchorBigRadius);

                ImGuizmoMoveType type = ImGuizmoMoveType.None;

                if (Intersects(operation, ImGuizmoOperation.Translate))
                {
                    type = GetMoveType(operation, out var gizmoHitProportion);
                }
                if (Intersects(operation, ImGuizmoOperation.Rotate) && type == ImGuizmoMoveType.None)
                {
                    type = GetRotateType(operation);
                }
                if (Intersects(operation, ImGuizmoOperation.Scale) && type == ImGuizmoMoveType.None)
                {
                    type = GetScaleType(operation);
                }

                if (type != ImGuizmoMoveType.None)
                {
                    overBigAnchor = false;
                    overSmallAnchor = false;
                }

                uint selectionColor = GetColorU32(_context.Style.Colors.Selection);
                uint borderColor = GetColorU32(_context.Style.Colors.LocalBoundsCircleBorder);

                uint bigAnchorColor = overBigAnchor ? selectionColor : boundsColor;
                uint smallAnchorColor = overSmallAnchor ? selectionColor : boundsColor;

                drawList.AddCircleFilled(worldBound1, anchorBigRadius, borderColor);
                drawList.AddCircleFilled(worldBound1, anchorBigRadius - _context.Style.LocalBoundsAnchorBorderLineThickness, bigAnchorColor);

                drawList.AddCircleFilled(midBound, anchorSmallRadius, borderColor);
                drawList.AddCircleFilled(midBound, anchorSmallRadius - _context.Style.LocalBoundsAnchorBorderLineThickness, smallAnchorColor);

                int oppositeIndex = (i + 2) % 4;

                // big anchor on corners
                if (!_context.IsUsingBounds && _context.Enabled && overBigAnchor && _canActivate)
                {
                    _forceActivate = false;

                    _context.BoundsPivot = aabb[(i + 2) % 4].Xyz.TransformPoint(_context.ModelSource);
                    _context.BoundsAnchor = aabb[i].Xyz.TransformPoint(_context.ModelSource);
                    _context.BoundsPlane = ImGuizmoUtils.BuildPlane(_context.BoundsAnchor, bestAxisWorldDirection);
                    _context.BoundsBestAxis = bestAxis;
                    _context.BoundsAxis[0] = secondAxis;
                    _context.BoundsAxis[1] = thirdAxis;

                    _context.BoundsLocalPivot = Vector3.Zero;
                    _context.BoundsLocalPivot[secondAxis] = aabb[oppositeIndex][secondAxis];
                    _context.BoundsLocalPivot[thirdAxis] = aabb[oppositeIndex][thirdAxis];

                    _context.IsUsingBounds = true;
                    _context.EditingID = _context.ActualID;
                    _context.BoundsMatrix = _context.ModelSource;
                }

                // small anchor on middle of segment
                if (!_context.IsUsingBounds && _context.Enabled && overSmallAnchor && _canActivate)
                {
                    _forceActivate = false;

                    var midPointOpposite = (aabb[(i + 2) % 4] + aabb[(i + 3) % 4]) * 0.5f;
                    _context.BoundsPivot = midPointOpposite.Xyz.TransformPoint(_context.ModelSource);
                    _context.BoundsAnchor = midPoint.Xyz.TransformPoint(_context.ModelSource);
                    _context.BoundsPlane = ImGuizmoUtils.BuildPlane(_context.BoundsAnchor, bestAxisWorldDirection);
                    _context.BoundsBestAxis = bestAxis;
                    int[] indices = new[] { secondAxis, thirdAxis };
                    _context.BoundsAxis[0] = indices[i % 2];
                    _context.BoundsAxis[1] = -1;

                    _context.BoundsLocalPivot = Vector3.Zero;
                    _context.BoundsLocalPivot[_context.BoundsAxis[0]] = aabb[oppositeIndex][indices[i % 2]];

                    _context.IsUsingBounds = true;
                    _context.EditingID = _context.ActualID;
                    _context.BoundsMatrix = _context.ModelSource;
                }
            }

            if (_context.IsUsingBounds && (_context.ActualID == -1 || _context.ActualID == _context.EditingID))
            {
                var scale = Matrix4.Identity;

                // compute projected mouse position on plan
                float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.BoundsPlane);
                var newPos = _context.RayOrigin + _context.RayVector * len;

                // compute a reference and delta vectors base on mouse move
                var deltaVector = newPos - _context.BoundsPivot;
                deltaVector = new Vector3(MathF.Abs(deltaVector.X), MathF.Abs(deltaVector.Y), MathF.Abs(deltaVector.Z));
                var referenceVector = (_context.BoundsAnchor - _context.BoundsPivot);
                referenceVector = new Vector3(MathF.Abs(referenceVector.X), MathF.Abs(referenceVector.Y), MathF.Abs(referenceVector.Z));

                // for 1 or 2 axes, compute a ratio that's used for scale and snap it based on resulting length
                for (int i = 0; i < 2; i++)
                {
                    int axisIndex1 = _context.BoundsAxis[i];
                    if (axisIndex1 == -1)
                    {
                        continue;
                    }

                    float ratioAxis = 1f;
                    var axisDir = new Vector3(
                        MathF.Abs(_context.BoundsMatrix[axisIndex1, 0]),
                        MathF.Abs(_context.BoundsMatrix[axisIndex1, 1]),
                        MathF.Abs(_context.BoundsMatrix[axisIndex1, 2]));

                    float dtAxis = Vector3.Dot(axisDir, referenceVector);
                    float boundSize = bounds[axisIndex1 + 3] - bounds[axisIndex1];
                    if (dtAxis > ImGuizmoConsts.FLT_EPSILON)
                    {
                        ratioAxis = Vector3.Dot(axisDir, deltaVector) / dtAxis;
                    }

                    if (snapValues.HasValue)
                    {
                        float length = boundSize * ratioAxis;
                        length = ImGuizmoUtils.ComputeSnap(length, snapValues.Value[axisIndex1], ImGuizmoConsts.SNAP_TENSION);
                        if (boundSize > ImGuizmoConsts.FLT_EPSILON)
                        {
                            ratioAxis = length / boundSize;
                        }
                    }

                    scale[axisIndex1, 0] *= ratioAxis;
                    scale[axisIndex1, 1] *= ratioAxis;
                    scale[axisIndex1, 2] *= ratioAxis;
                    scale[axisIndex1, 3] *= ratioAxis;
                }

                // transform matrix
                var preScale = Matrix4.CreateTranslation(-_context.BoundsLocalPivot);
                var postScale = Matrix4.CreateTranslation(_context.BoundsLocalPivot);

                var res = preScale * scale * postScale * _context.BoundsMatrix;
                matrix = res;
                modified = true;

                // info text
                var destinationPosOnScreen = ImGuizmoUtils.WorldToPos(_context.Model.Row3.Xyz, _context.ViewProjection, new Vector2(_context.X, _context.Y), new Vector2(_context.Width, _context.Height));
                string formattedString = string.Format(ImGuizmoConsts.TranslationInfoMask[((int)ImGuizmoMoveType.MoveScreen) - 1]
                   , (bounds[3] - bounds[0]) * _context.BoundsMatrix.Row0.Length * scale.Row0.Length
                   , (bounds[4] - bounds[1]) * _context.BoundsMatrix.Row1.Length * scale.Row1.Length
                   , (bounds[5] - bounds[2]) * _context.BoundsMatrix.Row2.Length * scale.Row2.Length
                );
                drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15), GetColorU32(_context.Style.Colors.TextShadow), formattedString);
                drawList.AddText(new System.Numerics.Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14), GetColorU32(_context.Style.Colors.Text), formattedString);
            }

            bool confirm = false;

            switch (ConfirmAction)
            {
                case ImGuizmoConfirmAction.MouseUp:
                    if (!io.MouseDown[0])
                        confirm = true;
                    break;
                case ImGuizmoConfirmAction.MouseClickOrEnter:
                    if (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        confirm = true;
                    break;
            }

            if (confirm)
            {
                _context.IsUsingBounds = false;
                _context.EditingID = -1;
            }

            if (_context.IsUsingBounds)
            {
                break;
            }
        }

        return modified;
    }

    private bool HandleRotation(ref Matrix4 matrix, ref Matrix4 deltaMatrix, ImGuizmoOperation op, ref ImGuizmoMoveType type, Vector3? snap)
    {
        if (!Intersects(op, ImGuizmoOperation.Rotate) || type != ImGuizmoMoveType.None /*|| !_context.MouseOver*/)
        {
            return false;
        }
        var io = ImGui.GetIO();
        bool applyRotationLocaly = _context.Mode == ImGuizmoMode.Local;
        bool modified = false;

        if (_forceActivate || !_context.IsUsing)
        {
            type = _forceActivate && IsRotateType(_context.CurrentOperation) ? _context.CurrentOperation : GetRotateType(op);

            if (type != ImGuizmoMoveType.None)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }

            if (type == ImGuizmoMoveType.RotateScreen)
            {
                applyRotationLocaly = true;
            }

            if (_canActivate && type != ImGuizmoMoveType.None)
            {
                _forceActivate = false;

                _context.IsUsing = true;
                _context.EditingID = _context.ActualID;
                _context.CurrentOperation = type;

                var rotatePlanNormal = new[] { _context.Model.Row0.Xyz, _context.Model.Row1.Xyz, _context.Model.Row2.Xyz, -_context.CameraDir };
                // pickup plan
                if (applyRotationLocaly)
                {
                    _context.TranslationPlane = ImGuizmoUtils.BuildPlane(_context.Model.Row3.Xyz, rotatePlanNormal[type - ImGuizmoMoveType.RotateX]);
                }
                else
                {
                    _context.TranslationPlane = ImGuizmoUtils.BuildPlane(_context.ModelSource.Row3.Xyz, ImGuizmoConsts.DirectionUnary[type - ImGuizmoMoveType.RotateX]);
                }

                float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.TranslationPlane);
                var localPos = _context.RayOrigin + _context.RayVector * len - _context.Model.Row3.Xyz;
                _context.RotationVectorSource = localPos.Normalized();
                _context.RotationAngleOrigin = ComputeAngleOnPlane();
            }
        }

        // rotation
        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsRotateType(_context.CurrentOperation))
        {
            ImGui.SetNextFrameWantCaptureMouse(true);

            _context.RotationAngle = !OverrideValue.HasValue ? ComputeAngleOnPlane() : OverrideValue.Value * ImGuizmoConsts.DEG_TO_RAD;
            if (snap.HasValue)
            {
                float snapInRadian = snap.Value[0] * ImGuizmoConsts.DEG_TO_RAD;

                _context.RotationAngle = ImGuizmoUtils.ComputeSnap(_context.RotationAngle, snapInRadian, ImGuizmoConsts.SNAP_TENSION);
            }

            var rotationAxisLocalSpace = Vector3.TransformVector(_context.TranslationPlane.Xyz, _context.ModelInverse);
            rotationAxisLocalSpace.Normalize();

            var deltaRotation = Matrix4.CreateFromAxisAngle(rotationAxisLocalSpace, _context.RotationAngle - _context.RotationAngleOrigin);

            if (_context.RotationAngle != _context.RotationAngleOrigin)
            {
                modified = true;
            }
            _context.RotationAngleOrigin = _context.RotationAngle;


            var scaleOrigin = Matrix4.CreateScale(_context.ModelScaleOrigin);

            if (applyRotationLocaly)
            {
                matrix = scaleOrigin * deltaRotation * _context.ModelLocal;
            }
            else
            {
                Matrix4 res = _context.ModelSource;
                //res.v.position.Set(0.f);
                //res.Row3 = Vector4.Zero;
                res = res.ClearTranslation();

                matrix = res * deltaRotation;
                matrix.Row3 = _context.ModelSource.Row3;
            }

            deltaMatrix = _context.ModelInverse * deltaRotation * _context.Model;

            bool confirm = false;

            switch (ConfirmAction)
            {
                case ImGuizmoConfirmAction.MouseUp:
                    if (!io.MouseDown[0])
                        confirm = true;
                    break;
                case ImGuizmoConfirmAction.MouseClickOrEnter:
                    if (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        confirm = true;
                    break;
            }

            if (confirm)
            {
                _context.IsUsing = false;
                _context.EditingID = -1;
            }

            type = _context.CurrentOperation;
        }
        return modified;
    }

    private bool HandleScale(ref Matrix4 matrix, ref Matrix4 deltaMatrix, ImGuizmoOperation op, ref ImGuizmoMoveType type, Vector3? snap)
    {
        if ((!Intersects(op, ImGuizmoOperation.Scale) && !Intersects(op, ImGuizmoOperation.ScaleUniversal)) || type != ImGuizmoMoveType.None /*|| !_context.MouseOver*/)
        {
            return false;
        }
        var io = ImGui.GetIO();
        bool modified = false;

        if (_forceActivate || !_context.IsUsing)
        {
            // find new possible way to scale
            type = _forceActivate && IsScaleType(_context.CurrentOperation) ? _context.CurrentOperation : GetScaleType(op);

            if (type != ImGuizmoMoveType.None)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }

            if (_canActivate && type != ImGuizmoMoveType.None)
            {
                _forceActivate = false;

                _context.IsUsing = true;
                _context.EditingID = _context.ActualID;
                _context.CurrentOperation = type;
                var movePlanNormal = new[]{ _context.Model.Row1.Xyz, _context.Model.Row2.Xyz, _context.Model.Row0.Xyz,
                    _context.Model.Row2.Xyz, _context.Model.Row1.Xyz, _context.Model.Row0.Xyz, -_context.CameraDir };
                // pickup plan

                _context.TranslationPlane = ImGuizmoUtils.BuildPlane(_context.Model.Row3.Xyz, movePlanNormal[type - ImGuizmoMoveType.ScaleX]);
                float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.TranslationPlane);
                _context.TranslationPlaneOrigin = _context.RayOrigin + _context.RayVector * len;
                _context.MatrixOrigin = _context.Model.Row3.Xyz;
                _context.Scale = new Vector3(1);
                _context.RelativeOrigin = (_context.TranslationPlaneOrigin - _context.Model.Row3.Xyz) * (1f / _context.ScreenFactor);
                _context.ScaleValueOrigin = new Vector3(_context.ModelSource.Row0.Length, _context.ModelSource.Row1.Length, _context.ModelSource.Row2.Length);
                _context.SaveMousePosX = io.MousePos.X;
            }
        }
        // scale
        if (_context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsScaleType(_context.CurrentOperation))
        {
            ImGui.SetNextFrameWantCaptureMouse(true);

            float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.TranslationPlane);
            var newPos = _context.RayOrigin + _context.RayVector * len;
            var newOrigin = newPos - _context.RelativeOrigin * _context.ScreenFactor;
            var delta = newOrigin - _context.ModelLocal.Row3.Xyz;

            // 1 axis constraint
            if (_context.CurrentOperation >= ImGuizmoMoveType.ScaleX && _context.CurrentOperation <= ImGuizmoMoveType.ScaleZ)
            {
                int axisIndex = _context.CurrentOperation - ImGuizmoMoveType.ScaleX;

                if (OverrideValue.HasValue)
                {
                    _context.Scale[axisIndex] = OverrideValue.Value;
                }
                else
                {
                    var axisValue = new Vector3(_context.ModelLocal[axisIndex, 0], _context.ModelLocal[axisIndex, 1], _context.ModelLocal[axisIndex, 2]);
                    float lengthOnAxis = Vector3.Dot(axisValue, delta);
                    delta = axisValue * lengthOnAxis;

                    var baseVector = _context.TranslationPlaneOrigin - _context.ModelLocal.Row3.Xyz;
                    float ratio = Vector3.Dot(axisValue, baseVector + delta) / Vector3.Dot(axisValue, baseVector);

                    _context.Scale[axisIndex] = MathF.Max(ratio, 0.001f);
                }
            }
            else
            {
                if (OverrideValue.HasValue)
                {
                    _context.Scale = new Vector3(OverrideValue.Value);
                }
                else
                {
                    float scaleDelta = (io.MousePos.X - _context.SaveMousePosX) * 0.01f;
                    _context.Scale = new Vector3(MathF.Max(1f + scaleDelta, 0.001f));
                }
            }

            // snap
            if (snap.HasValue)
            {
                var scaleSnap = new Vector3(snap.Value.X);
                _context.Scale = ImGuizmoUtils.ComputeSnap(_context.Scale, scaleSnap, ImGuizmoConsts.SNAP_TENSION);
            }

            // no 0 allowed
            for (int i = 0; i < 3; i++)
            {
                if (_context.Scale[i] > 0 && _context.Scale[i] < 0.001f)
                    _context.Scale[i] = MathF.Max(_context.Scale[i], 0.001f);
                if (_context.Scale[i] < 0 && _context.Scale[i] > -0.001f)
                    _context.Scale[i] = MathF.Min(_context.Scale[i], -0.001f);
            }

            if (_context.ScaleLast != _context.Scale)
            {
                modified = true;
            }
            _context.ScaleLast = _context.Scale;

            // compute matrix & delta
            var deltaMatrixScale = Matrix4.CreateScale(_context.Scale * _context.ScaleValueOrigin);

            Matrix4 res = deltaMatrixScale * _context.ModelLocal;
            matrix = res;

            var deltaScale = _context.Scale * _context.ScaleValueOrigin;

            var originalScaleDivider =
                new Vector3(1 / _context.ModelScaleOrigin.X,
                            1 / _context.ModelScaleOrigin.Y,
                            1 / _context.ModelScaleOrigin.Z);

            deltaScale = deltaScale * originalScaleDivider;

            deltaMatrix = Matrix4.CreateScale(deltaScale);

            bool confirm = false;

            switch (ConfirmAction)
            {
                case ImGuizmoConfirmAction.MouseUp:
                    if (!io.MouseDown[0])
                        confirm = true;
                    break;
                case ImGuizmoConfirmAction.MouseClickOrEnter:
                    if (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        confirm = true;
                    break;
            }

            if (confirm)
            {
                _context.IsUsing = false;
                _context.Scale = new Vector3(1);
            }

            type = _context.CurrentOperation;
        }
        return modified;
    }

    private bool HandleTranslation(ref Matrix4 matrix, ref Matrix4 deltaMatrix, ImGuizmoOperation op, ref ImGuizmoMoveType type, Vector3? snap)
    {
        if (!Intersects(op, ImGuizmoOperation.Translate) || type != ImGuizmoMoveType.None)
        {
            return false;
        }
        var io = ImGui.GetIO();
        bool applyRotationLocaly = _context.Mode == ImGuizmoMode.Local || type == ImGuizmoMoveType.MoveScreen;
        bool modified = false;

        if (_forceActivate)
            type = _context.CurrentOperation;

        // move
        if (!_forceActivate && _context.IsUsing && (_context.ActualID == -1 || _context.ActualID == _context.EditingID) && IsTranslateType(_context.CurrentOperation))
        {
            ImGui.SetNextFrameWantCaptureMouse(true);

            float signedLength = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.TranslationPlane);
            float len = MathF.Abs(signedLength); // near plan
            var newPos = _context.RayOrigin + _context.RayVector * len;

            // compute delta
            var newOrigin = newPos - _context.RelativeOrigin * _context.ScreenFactor;
            var delta = newOrigin - _context.Model.Row3.Xyz;

            // 1 axis constraint
            if (_context.CurrentOperation >= ImGuizmoMoveType.MoveX && _context.CurrentOperation <= ImGuizmoMoveType.MoveZ)
            {
                int axisIndex = _context.CurrentOperation - ImGuizmoMoveType.MoveX;
                var axisValue = new Vector3(_context.Model[axisIndex, 0], _context.Model[axisIndex, 1], _context.Model[axisIndex, 2]);

                if (OverrideValue.HasValue)
                {
                    var desiredPos = _context.MatrixOrigin + (axisValue * new Vector3(OverrideValue.Value));
                    delta = desiredPos - _context.Model.Row3.Xyz;
                }
                else
                {
                    //This would cause weird NaN calculations so I simplified it
                    float lengthOnAxis = Vector3.Dot(axisValue, delta);
                    delta = axisValue * lengthOnAxis;

                    //Prevent shooting up really fast
                    //todo: find a better way to do this
                    if (delta.LengthFast > 100000)
                        delta = new Vector3(0);
                }
            }

            // snap
            if (snap.HasValue)
            {
                var cumulativeDelta = _context.Model.Row3.Xyz + delta - _context.MatrixOrigin;
                if (applyRotationLocaly)
                {
                    var modelSourceNormalized = _context.ModelSource;
                    modelSourceNormalized.OrthoNormalize();
                    var modelSourceNormalizedInverse = modelSourceNormalized.Inverted();

                    cumulativeDelta = Vector3.TransformVector(cumulativeDelta, modelSourceNormalizedInverse);
                    cumulativeDelta = ImGuizmoUtils.ComputeSnap(cumulativeDelta, snap.Value, ImGuizmoConsts.SNAP_TENSION);
                    cumulativeDelta = Vector3.TransformVector(cumulativeDelta, modelSourceNormalized);
                }
                else
                {
                    cumulativeDelta = ImGuizmoUtils.ComputeSnap(cumulativeDelta, snap.Value, ImGuizmoConsts.SNAP_TENSION);
                }
                delta = _context.MatrixOrigin + cumulativeDelta - _context.Model.Row3.Xyz;

            }

            if (delta != _context.TranslationLastDelta)
            {
                modified = true;
            }
            _context.TranslationLastDelta = delta;

            // compute matrix & delta
            var deltaMatrixTranslation = Matrix4.CreateTranslation(delta);

            deltaMatrix = deltaMatrixTranslation;

            var res = _context.ModelSource * deltaMatrixTranslation;
            matrix = res;

            switch (ConfirmAction)
            {
                case ImGuizmoConfirmAction.MouseUp:
                    if (!io.MouseDown[0])
                        _context.IsUsing = false;
                    break;
                case ImGuizmoConfirmAction.MouseClickOrEnter:
                    if (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        _context.IsUsing = false;
                    break;
            }

            type = _context.CurrentOperation;
        }
        else
        {
            // find new possible way to move
            type = type == ImGuizmoMoveType.None ? GetMoveType(op, out var gizmoHitProportion) : type;

            if (type != ImGuizmoMoveType.None)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }
            if (_canActivate && type != ImGuizmoMoveType.None)
            {
                _forceActivate = false;

                _context.IsUsing = true;
                _context.EditingID = _context.ActualID;
                _context.CurrentOperation = type;
                var movePlanNormal = new[] {
                    _context.Model.Row0.Xyz, _context.Model.Row1.Xyz, _context.Model.Row2.Xyz,
                   _context.Model.Row0.Xyz, _context.Model.Row1.Xyz, _context.Model.Row2.Xyz,
                   -_context.CameraDir };

                var cameraToModelNormalized = (_context.Model.Row3.Xyz - _context.CameraEye).Normalized();
                for (uint i = 0; i < 3; i++)
                {
                    var orthoVector = Vector3.Cross(movePlanNormal[i], cameraToModelNormalized);
                    movePlanNormal[i] = Vector3.Cross(movePlanNormal[i], orthoVector);
                    movePlanNormal[i] = movePlanNormal[i].Normalized();
                }
                // pickup plan
                _context.TranslationPlane = ImGuizmoUtils.BuildPlane(_context.Model.Row3.Xyz, movePlanNormal[type - ImGuizmoMoveType.MoveX]);
                float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.TranslationPlane);
                _context.TranslationPlaneOrigin = _context.RayOrigin + _context.RayVector * len;
                _context.MatrixOrigin = _context.Model.Row3.Xyz;

                _context.RelativeOrigin = (_context.TranslationPlaneOrigin - _context.Model.Row3.Xyz) * (1f / _context.ScreenFactor);
            }
        }
        return modified;
    }

    private static bool IsTranslateType(ImGuizmoMoveType type)
    {
        return type >= ImGuizmoMoveType.MoveX && type <= ImGuizmoMoveType.MoveScreen;
    }

    private static bool IsRotateType(ImGuizmoMoveType type)
    {
        return type >= ImGuizmoMoveType.RotateX && type <= ImGuizmoMoveType.RotateScreen;
    }

    private static bool IsScaleType(ImGuizmoMoveType type)
    {
        return type >= ImGuizmoMoveType.ScaleX && type <= ImGuizmoMoveType.ScaleXYZ;
    }

    private static bool Intersects(ImGuizmoOperation lhs, ImGuizmoOperation rhs)
    {
        return (lhs & rhs) != 0;
    }

    // True if lhs contains rhs
    private static bool Contains(ImGuizmoOperation lhs, ImGuizmoOperation rhs)
    {
        return (lhs & rhs) == rhs;
    }

    private float ComputeAngleOnPlane()
    {
        float len = ImGuizmoUtils.IntersectRayPlane(_context.RayOrigin, _context.RayVector, _context.TranslationPlane);
        var localPos = (_context.RayOrigin + _context.RayVector * len - _context.Model.Row3.Xyz).Normalized();

        var perpendicularVector = Vector3.Cross(_context.RotationVectorSource, _context.TranslationPlane.Xyz);
        perpendicularVector.Normalize();
        float acosAngle = Math.Clamp(Vector3.Dot(localPos, _context.RotationVectorSource), -1f, 1f);
        float angle = MathF.Acos(acosAngle);
        angle *= (Vector3.Dot(localPos, perpendicularVector) < 0f) ? 1f : -1f;
        return angle;
    }

    private bool IsInContextRect(System.Numerics.Vector2 p)
        => IsWithin(p.X, _context.X, _context.XMax) && IsWithin(p.Y, _context.Y, _context.YMax);

    private static bool IsWithin(float value, float min, float max)
        => (value >= min) && (value <= max);
}