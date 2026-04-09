#nullable enable
using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport.Actions;
using HaroohiePals.Mathematics;
using ImGuiNET;
using OpenTK.Mathematics;
using System.Globalization;

namespace HaroohiePals.Gui.Viewport;

public sealed class Gizmo
{
    public GizmoKeyBindings KeyBindings = new();

    public GizmoTool Tool = GizmoTool.Translate;
    public ImGuizmoMode Mode = ImGuizmoMode.World;
    public GizmoRotateScaleMode RotateScaleMode = GizmoRotateScaleMode.MedianPoint;

    public DrawTool? DrawTool;
    public IViewportCollision? ViewportCollision;

    public IReadOnlyList<GizmoTool> EnabledTools { get; private set; }
    public bool Started { get; private set; } = false;
    public bool IsOrthographic { get; set; } = false;
    public bool IsUsing => Started || _imGuizmo.IsUsing;
    public bool IsOver => _imGuizmo.IsOver();
    public bool IsUsingDrawTool => DrawTool is { IsUsing: true };

    private readonly ImGuizmo _imGuizmo = new();
    private readonly RenderGroupScene _renderGroupScene;
    private readonly Dictionary<(object obj, int subIndex), Transform> _oldTransforms = new();
    private readonly Dictionary<(object obj, int subIndex), Transform> _currentTransforms = new();
    private readonly Dictionary<(object obj, int subIndex), Box3d> _currentLocalBounds = new();

    private string _inputOverrideValue = "";
    private Vector3d _oldAveragePos = Vector3.Zero;
    private Vector3d _currentRotation = Vector3.Zero;
    private Box3d? _currentBounds = null;

    public Gizmo(RenderGroupScene renderGroupScene, IReadOnlyList<GizmoTool>? enabledTools = null)
    {
        _renderGroupScene = renderGroupScene;
        EnabledTools = enabledTools ?? [GizmoTool.Draw, GizmoTool.Translate, GizmoTool.Rotate, GizmoTool.Scale];
    }

    public void Draw(ViewportContext context)
    {
        if (!EnabledTools.Contains(Tool))
            return;

        try
        {
            TryFinishGizmoTransform(context);

            switch (Tool)
            {
                case GizmoTool.Translate:
                case GizmoTool.Rotate:
                case GizmoTool.Scale:
                    DrawGizmo(context);
                    break;
                case GizmoTool.Draw:
                    if (ImGui.IsWindowHovered() && !_imGuizmo.IsOver())
                        DrawTool?.PerformDraw(context);
                    break;
            }
        }
        catch
        {
            CancelGizmoTransform();
        }
    }

    public void CancelGizmoTransform()
    {
        if (!Started)
            return;

        RestoreOldTransforms();

        Started = false;
        _imGuizmo.ConfirmAction = ImGuizmoConfirmAction.MouseUp;
        _imGuizmo.StopUsing();

        _oldTransforms.Clear();
        _currentTransforms.Clear();
    }

    public void Update()
    {
        if (KeyBindings.CancelOperation.IsPressed())
            CancelGizmoTransform();

        if (KeyBindings.SnapToCollision.IsPressed())
            ApplyCollision();

        HandleOverrideValue();
        HandleImGuizmoShortcuts();
    }

    private ImGuizmoOperation _guizmoOperation =>
        Tool switch
        {
            GizmoTool.Translate => ImGuizmoOperation.Translate,
            GizmoTool.Rotate => ImGuizmoOperation.Rotate,
            GizmoTool.Scale => ImGuizmoOperation.Scale,
            _ => throw new Exception()
        };

    private void DrawInputOverrideValue()
    {
        if (_imGuizmo.IsUsing && !string.IsNullOrEmpty(_inputOverrideValue))
        {
            var drawList = ImGui.GetWindowDrawList();

            var mousePos = ImGui.GetMousePos();

            float uiScale = ImGuiEx.GetUiScale();

            string text = $"Input Value: {_inputOverrideValue}";

            var textPos = new System.Numerics.Vector2(mousePos.X + (14 * uiScale), mousePos.Y + (14 * uiScale));

            drawList.AddText(new System.Numerics.Vector2(textPos.X + 1, textPos.Y + 1), 0xFF000000, text);
            drawList.AddText(textPos, 0xFFFFFFFF, text);
        }
    }

    private void UpdateCurrentTransformsAndBounds(ViewportContext context, bool updateBounds)
    {
        _currentTransforms.Clear();
        _currentLocalBounds.Clear();
        _currentBounds = null;

        foreach (object obj in context.SceneObjectHolder.GetSelection())
        {
            for (int i = -1; i <= 2; i++)
            {
                if (!context.SceneObjectHolder.IsSubIndexSelected(obj, i))
                    continue;

                if (_renderGroupScene.RenderGroups.TryGetObjectTransform(obj, i, out var transform))
                    _currentTransforms.Add((obj, i), transform);

                if (updateBounds && _renderGroupScene.RenderGroups.TryGetLocalObjectBounds(obj, i, out var bounds))
                    _currentLocalBounds[(obj, i)] = bounds;
            }
        }

        if (!updateBounds || _currentLocalBounds.Count == 0)
            return;

        if (_currentLocalBounds.Count == 1)
        {
            _currentBounds = _currentLocalBounds.Values.First();
            return;
        }

        var worldMin = new Vector3d(double.MaxValue);
        var worldMax = new Vector3d(double.MinValue);

        foreach (var kv in _currentLocalBounds)
        {
            if (!_currentTransforms.TryGetValue(kv.Key, out var t))
                continue;

            var m = BuildTransformMatrix(t);
            var b = kv.Value;

            for (int c = 0; c < 8; c++)
            {
                var corner = new Vector3(
                    (float)((c & 1) == 0 ? b.Min.X : b.Max.X),
                    (float)((c & 2) == 0 ? b.Min.Y : b.Max.Y),
                    (float)((c & 4) == 0 ? b.Min.Z : b.Max.Z));

                var w = Vector3.TransformPosition(corner, m);

                if (w.X < worldMin.X) worldMin.X = w.X;
                if (w.Y < worldMin.Y) worldMin.Y = w.Y;
                if (w.Z < worldMin.Z) worldMin.Z = w.Z;
                if (w.X > worldMax.X) worldMax.X = w.X;
                if (w.Y > worldMax.Y) worldMax.Y = w.Y;
                if (w.Z > worldMax.Z) worldMax.Z = w.Z;
            }
        }

        _currentBounds = new Box3d(worldMin, worldMax);
    }

    private Vector3d GetAveragePosition()
    {
        var avgPos = Vector3d.Zero;

        var currentPositions = _currentTransforms.Select(x => x.Value.Translation).ToList();

        foreach (var pos in currentPositions)
            avgPos += pos;

        return avgPos / currentPositions.Count;
    }

    private void DrawGizmo(ViewportContext context)
    {
        if (context.SceneObjectHolder.SelectionSize == 0)
            return;

        DrawInputOverrideValue();

        var guizmoOperation = _guizmoOperation;

        var contentPos = ImGui.GetCursorScreenPos();

        _imGuizmo.SetOrthographic(IsOrthographic);
        _imGuizmo.BeginFrame();
        _imGuizmo.SetDrawlist();
        _imGuizmo.SetRect(contentPos.X, contentPos.Y, context.ViewportSize.X, context.ViewportSize.Y);

        bool useLocalBounds = context.SceneObjectHolder.SelectionSize >= 1 && Tool == GizmoTool.Scale;

        UpdateCurrentTransformsAndBounds(context, useLocalBounds);

        var averagePos = GetAveragePosition();

        if (_currentTransforms.Count > 1 && _currentBounds is not null)
        {
            var b = _currentBounds.Value;
            _currentBounds = new Box3d(b.Min - averagePos, b.Max - averagePos);
        }

        if (!Started && context.SceneObjectHolder.SelectionSize > 1)
            _currentRotation = Vector3d.Zero;

        if (context.SceneObjectHolder.SelectionSize == 1)
            _currentRotation = _currentTransforms.First().Value.Rotation;

        var mtx = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_currentRotation.X)) *
                  Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_currentRotation.Y)) *
                  Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(_currentRotation.Z)) *
                  Matrix4.CreateTranslation((Vector3)averagePos);

        var actualMode = context.SceneObjectHolder.SelectionSize > 1 ? ImGuizmoMode.World : Mode;

        if (!_imGuizmo.Manipulate(context.ViewMatrix, context.ProjectionMatrix, guizmoOperation, actualMode,
            ref mtx, out var deltaMtx, null, _currentBounds))
            return;

        if (!Started)
        {
            Started = true;
            if (context.ActionStack != null)
            {
                _oldTransforms.Clear();
                foreach (var keyValue in _currentTransforms)
                    _oldTransforms.Add(keyValue.Key, keyValue.Value);
            }
            _oldAveragePos = averagePos;
        }

        var resultPosDiff = deltaMtx.ExtractTranslation();
        var resultPosition = mtx.ExtractTranslation();
        var resultScale = mtx.ExtractScale();

        foreach (var transformKeyValue in _currentTransforms)
        {
            // Avoid issues when changing the selection on the same frame
            if (!AreCurrentTransformsValid() || !_oldTransforms.TryGetValue(transformKeyValue.Key, out var oldTransform))
            {
                CancelGizmoTransform();
                return;
            }

            var transform = transformKeyValue.Value;

            switch (Tool)
            {
                case GizmoTool.Translate:
                    transform.Translation += resultPosDiff;
                    break;
                case GizmoTool.Rotate:
                    var resultRotMtx = mtx.ClearTranslation().ClearScale();
                    ImGuizmoUtils.DecomposeMatrixToComponents(mtx, out _, out _currentRotation, out _);

                    if (RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                        or GizmoRotateScaleMode.MedianPointTranslateOnly)
                    {
                        var diff = oldTransform.Translation - _oldAveragePos;
                        var vec4 = new Vector4((float)diff.X, (float)diff.Y, (float)diff.Z, 0);
                        var rotated = vec4 * resultRotMtx;

                        transform.Translation = rotated.Xyz + _oldAveragePos;
                    }

                    if (RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                        or GizmoRotateScaleMode.IndividualOrigins)
                    {
                        // Multiple selection
                        if (_currentTransforms.Count > 1)
                        {
                            var oldMatrix =
                                Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(oldTransform.Rotation.X)) *
                                Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(oldTransform.Rotation.Y)) *
                                Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(oldTransform.Rotation.Z)) *
                                Matrix4.CreateTranslation((Vector3)oldTransform.Translation);

                            var matrix = _imGuizmo.ComputeRotation(oldMatrix, ImGuizmoMode.World);

                            ImGuizmoUtils.DecomposeMatrixToComponents(matrix, out _, out var newRot, out _);
                            transform.Rotation = newRot;
                        }
                        else
                        {
                            transform.Rotation = _currentRotation;
                        }
                    }

                    break;
                case GizmoTool.Scale:
                    if (_currentBounds is not null && _currentTransforms.Count > 1)
                    {
                        var s = (Vector3d)resultScale;
                        var newCenter = (Vector3d)resultPosition;
                        var oldCenter = _oldAveragePos;

                        const float tolerance = 0.000000001f;

                        Vector3d anchor;
                        anchor.X = Math.Abs(s.X - 1.0) < tolerance
                            ? oldCenter.X
                            : (newCenter.X - s.X * oldCenter.X) / (1.0 - s.X);
                        anchor.Y = Math.Abs(s.Y - 1.0) < tolerance
                            ? oldCenter.Y
                            : (newCenter.Y - s.Y * oldCenter.Y) / (1.0 - s.Y);
                        anchor.Z = Math.Abs(s.Z - 1.0) < tolerance
                            ? oldCenter.Z
                            : (newCenter.Z - s.Z * oldCenter.Z) / (1.0 - s.Z);

                        transform.Translation = ((oldTransform.Translation - anchor) * s) + anchor;
                        transform.Scale = oldTransform.Scale * WorldToLocalScaleRatio(oldTransform.Rotation,
                            new Vector3((float)s.X, (float)s.Y, (float)s.Z));
                        break;
                    }

                    if (_currentTransforms.Count != 1 &&
                        RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                            or GizmoRotateScaleMode.MedianPointTranslateOnly)
                    {
                        transform.Translation = ((oldTransform.Translation - _oldAveragePos) * resultScale) + _oldAveragePos;
                    }

                    if (_currentTransforms.Count == 1 ||
                        RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                            or GizmoRotateScaleMode.IndividualOrigins)
                    {
                        var localScale = _currentTransforms.Count == 1
                            ? (Vector3d)resultScale
                            : WorldToLocalScaleRatio(oldTransform.Rotation, resultScale);
                        transform.Scale = oldTransform.Scale * localScale;
                    }

                    if (_currentBounds is not null)
                    {
                        transform.Translation = resultPosition;
                    }

                    break;
            }

            if (IsOrthographic && Tool == GizmoTool.Translate)
                transform.Translation = transform.Translation with { Y = oldTransform.Translation.Y };

            //Apply collision
            if (KeyBindings.SnapToCollision.IsDown() && FindCollisionIntersection(oldTransform, transform, out var newPos))
                transform.Translation = newPos;

            _renderGroupScene.RenderGroups.TrySetObjectTransform(transformKeyValue.Key.obj, transformKeyValue.Key.subIndex, transform);
        }
    }

    private bool FindCollisionIntersection(Transform oldTransform, Transform transform, out Vector3d newPos)
    {
        newPos = Vector3d.Zero;

        return ViewportCollision is not null &&
            ViewportCollision.FindIntersection(oldTransform.Translation, transform.Translation, out newPos);
    }

    private bool ApplyCollision()
    {
        if (!Started || !_imGuizmo.IsUsing)
            return false;

        foreach (var transformKeyValue in _currentTransforms)
        {
            var transform = transformKeyValue.Value;
            var oldTransform = _oldTransforms[transformKeyValue.Key];

            if (FindCollisionIntersection(oldTransform, transform, out var newPos))
            {
                transform.Translation = newPos;
                _renderGroupScene.RenderGroups.TrySetObjectTransform(transformKeyValue.Key.obj, transformKeyValue.Key.subIndex, transform);
            }
        }

        return true;
    }

    private void TryFinishGizmoTransform(ViewportContext context)
    {
        if (!Started || _imGuizmo.IsUsing)
            return;

        // Avoid issues when changing the selection on the same frame
        if (!AreCurrentTransformsValid())
        {
            CancelGizmoTransform();
            return;
        }

        Started = false;
        _imGuizmo.ConfirmAction = ImGuizmoConfirmAction.MouseUp;

        if (context.ActionStack is null || _currentTransforms.Count == 0)
            return;

        var objs = _currentTransforms.Keys;
        var actions = new List<IAction>(objs.Count);
        foreach (var transformKeyValue in _currentTransforms)
        {
            if (_renderGroupScene.RenderGroups.TryGetObjectTransformGroup(transformKeyValue.Key.obj,
                transformKeyValue.Key.subIndex, out var transform, out var group))
            {
                // Avoid issues when changing the selection on the same frame
                if (!_oldTransforms.TryGetValue(transformKeyValue.Key, out var oldTransform))
                {
                    CancelGizmoTransform();
                    return;
                }

                actions.Add(new SetObjectTransformAction(group, transformKeyValue.Key.obj,
                    transformKeyValue.Key.subIndex, oldTransform, transform));
            }
        }
        _currentTransforms.Clear();

        context.ActionStack.Add(new BatchAction(actions));
    }

    private void RestoreOldTransforms()
    {
        foreach (var oldTransform in _oldTransforms)
            _renderGroupScene.RenderGroups.TrySetObjectTransform(oldTransform.Key.obj, oldTransform.Key.subIndex, oldTransform.Value);
    }

    private void HandleOverrideValue()
    {
        if (!_imGuizmo.IsUsing)
        {
            _inputOverrideValue = "";
            _imGuizmo.OverrideValue = null;
            return;
        }

        var io = ImGui.GetIO();
        var charQueue = io.InputQueueCharacters;

        if (_inputOverrideValue.Length <= 20)
        {
            for (int i = 0; i < charQueue.Size; i++)
            {
                char character = (char)charQueue[i];

                if ((character >= '0' && character <= '9') || character == ',' || character == '.' || character == '-')
                {
                    if (character == ',')
                        character = '.';

                    _inputOverrideValue += character;
                }
            }
        }

        if (ImGui.IsKeyPressed(ImGuiKey.Backspace) && _inputOverrideValue.Length > 0)
            _inputOverrideValue = _inputOverrideValue.Remove(_inputOverrideValue.Length - 1, 1);

        if (string.IsNullOrEmpty(_inputOverrideValue))
        {
            _imGuizmo.OverrideValue = null;
            return;
        }

        if (float.TryParse(_inputOverrideValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            int min = Tool == GizmoTool.Scale ? -100 : -100000;
            int max = Tool == GizmoTool.Scale ? 100 : 100000;

            bool outOfRange = value < min || value > max;

            _imGuizmo.OverrideValue = Math.Clamp(value, min, max);

            if (outOfRange)
                _inputOverrideValue = _imGuizmo.OverrideValue?.ToString(CultureInfo.InvariantCulture) ?? "";
        }
    }

    private void HandleImGuizmoShortcuts()
    {
        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            CancelGizmoTransform();
            return;
        }

        if (!ImGui.IsWindowFocused() || ImGui.IsMouseDown(ImGuiMouseButton.Right))
            return;

        bool startNewOperation =
            HandleTranslationShortcuts() ||
            HandleRotationShortcuts() ||
            HandleScaleShortcuts();

        if (startNewOperation)
        {
            if (Started && _imGuizmo.IsUsing)
                RestoreOldTransforms();
            _imGuizmo.ConfirmAction = ImGuizmoConfirmAction.MouseClickOrEnter;
        }
    }

    private bool HandleTranslationShortcuts()
    {
        if (!EnabledTools.Contains(GizmoTool.Translate))
            return false;

        bool result = false;

        if (KeyBindings.ToolsTranslate.IsPressed())
        {
            Tool = GizmoTool.Translate;
            _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveScreen);
            result = true;
        }

        if (_imGuizmo.IsUsing && Tool == GizmoTool.Translate)
        {
            if (KeyBindings.ToolsAxisConstraintXZ.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveZX);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintXY.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveXY);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintYZ.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveYZ);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintX.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveX);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintY.IsPressed() && !IsOrthographic)
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveY);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintZ.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.MoveZ);
                result = true;
            }
        }

        return result;
    }

    private bool HandleRotationShortcuts()
    {
        if (!EnabledTools.Contains(GizmoTool.Rotate))
            return false;

        bool result = false;

        if (KeyBindings.ToolsRotate.IsPressed())
        {
            Tool = GizmoTool.Rotate;
            _imGuizmo.StartMoveType(ImGuizmoMoveType.RotateScreen);
            result = true;
        }

        if (_imGuizmo.IsUsing && Tool == GizmoTool.Rotate)
        {
            if (KeyBindings.ToolsAxisConstraintX.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.RotateX);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintY.IsPressed() && !IsOrthographic)
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.RotateY);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintZ.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.RotateZ);
                result = true;
            }
        }

        return result;
    }

    private bool HandleScaleShortcuts()
    {
        if (!EnabledTools.Contains(GizmoTool.Scale))
            return false;

        bool result = false;

        if (KeyBindings.ToolsScale.IsPressed())
        {
            Tool = GizmoTool.Scale;
            _imGuizmo.StartMoveType(ImGuizmoMoveType.ScaleXYZ);
            result = true;
        }

        if (_imGuizmo.IsUsing && Tool == GizmoTool.Scale)
        {
            if (KeyBindings.ToolsAxisConstraintX.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.ScaleX);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintY.IsPressed() && !IsOrthographic)
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.ScaleY);
                result = true;
            }
            else if (KeyBindings.ToolsAxisConstraintZ.IsPressed())
            {
                _imGuizmo.StartMoveType(ImGuizmoMoveType.ScaleZ);
                result = true;
            }
        }

        return result;
    }

    private Matrix4 BuildTransformMatrix(Transform t) =>
        Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(t.Rotation.X)) *
        Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(t.Rotation.Y)) *
        Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(t.Rotation.Z)) *
        Matrix4.CreateTranslation((Vector3)t.Translation);

    private Vector3d WorldToLocalScaleRatio(Vector3d rotationDeg, Vector3 worldScale)
    {
        var rot =
            Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(rotationDeg.X)) *
            Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(rotationDeg.Y)) *
            Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(rotationDeg.Z));

        return new Vector3d(
            new Vector3(rot.Row0.X * worldScale.X, rot.Row0.Y * worldScale.Y, rot.Row0.Z * worldScale.Z).Length,
            new Vector3(rot.Row1.X * worldScale.X, rot.Row1.Y * worldScale.Y, rot.Row1.Z * worldScale.Z).Length,
            new Vector3(rot.Row2.X * worldScale.X, rot.Row2.Y * worldScale.Y, rot.Row2.Z * worldScale.Z).Length);
    }

    private bool AreCurrentTransformsValid()
        => _oldTransforms.Count == _currentTransforms.Count;
}