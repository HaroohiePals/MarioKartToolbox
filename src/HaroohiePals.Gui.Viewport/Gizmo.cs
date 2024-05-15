using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport.Actions;
using ImGuiNET;
using OpenTK.Mathematics;
using System.Globalization;

namespace HaroohiePals.Gui.Viewport;

public class Gizmo
{
    public enum GizmoTool
    {
        Pointer,
        Translate,
        Rotate,
        Scale,
        Draw
    }

    public enum GizmoRotateScaleMode
    {
        IndividualOrigins,
        MedianPoint,
        MedianPointTranslateOnly
    }

    private ImGuizmo _imGuizmo = new();
    private string _inputOverrideValue = "";

    private readonly Dictionary<(object obj, int subIndex), Transform> _oldTransforms = new();
    private readonly Dictionary<(object obj, int subIndex), Transform> _currentTransforms = new();
    private Vector3d _oldAvgPos = Vector3.Zero;
    private Vector3d _currentRotation = Vector3.Zero;

    private readonly RenderGroupScene _renderGroupScene;

    public GizmoKeyBindings KeyBindings = new();

    public GizmoTool Tool = GizmoTool.Translate;
    public GizmoRotateScaleMode RotateScaleMode = GizmoRotateScaleMode.MedianPoint;

    public DrawTool DrawTool;
    public IViewportCollision ViewportCollision;

    public bool Started { get; private set; } = false;

    public bool IsOrthographic = false;
    public bool IsUsing => Started || _imGuizmo.IsUsing;
    public bool IsOver => _imGuizmo.IsOver();

    public bool IsUsingDrawTool => DrawTool is { IsUsing: true };

    public Gizmo(RenderGroupScene renderGroupScene)
    {
        _renderGroupScene = renderGroupScene;
    }

    public void Draw(ViewportContext context)
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

    private void DrawGizmo(ViewportContext context)
    {
        if (context.SceneObjectHolder.SelectionSize == 0)
            return;

        DrawInputOverrideValue();

        var guizmoOperation = _guizmoOperation;

        _imGuizmo.SetOrthographic(IsOrthographic);
        _imGuizmo.BeginFrame();

        _imGuizmo.SetDrawlist();

        var contentPos = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
        _imGuizmo.SetRect(contentPos.X, contentPos.Y, context.ViewportSize.X, context.ViewportSize.Y);

        var avgPos = Vector3d.Zero;

        if (!Started && context.SceneObjectHolder.SelectionSize > 1)
            _currentRotation = Vector3d.Zero;

        _currentTransforms.Clear();
        foreach (object obj in context.SceneObjectHolder.GetSelection())
        {
            for (int i = -1; i <= 2; i++)
            {
                if (!context.SceneObjectHolder.IsSubIndexSelected(obj, i))
                    continue;

                if (!_renderGroupScene.RenderGroups.GetObjectTransform(obj, i, out var transform))
                    continue;

                _currentTransforms.Add((obj, i), transform);
                avgPos += transform.Translation;
            }
        }

        if (_currentTransforms.Count == 1)
            _currentRotation = _currentTransforms.First().Value.Rotation;

        avgPos /= _currentTransforms.Count;

        var mtx = Matrix4.CreateTranslation((Vector3)avgPos);

        if (Tool == GizmoTool.Rotate)
        {
            mtx = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_currentRotation.X)) *
                  Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_currentRotation.Y)) *
                  Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(_currentRotation.Z)) * mtx;
        }

        if (!_imGuizmo.Manipulate(context.ViewMatrix, context.ProjectionMatrix, guizmoOperation, ImGuizmoMode.Local,
            ref mtx, out var deltaMtx/*, null, new float[] { -16f, -16f, -16f, 16f, 16f, 16f }*/))
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
            _oldAvgPos = avgPos;
        }

        var posDiff = Vector3d.Zero;
        var scale = Vector3d.One;
        var rotMtx = Matrix4.Identity;

        if (Tool == GizmoTool.Translate)
            posDiff = deltaMtx.ExtractTranslation();
        if (Tool == GizmoTool.Rotate)
        {
            rotMtx = mtx.ClearTranslation().ClearScale();

            ImGuizmoUtils.DecomposeMatrixToComponents(mtx, out _, out _currentRotation, out _);
        }

        if (Tool == GizmoTool.Scale)
            scale = mtx.ExtractScale();

        foreach (var transformKeyValue in _currentTransforms)
        {
            var transform = transformKeyValue.Value;
            var oldTransform = _oldTransforms[transformKeyValue.Key];

            switch (Tool)
            {
                case GizmoTool.Translate:
                    transform.Translation += posDiff;
                    break;
                case GizmoTool.Rotate:
                    if (RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                        or GizmoRotateScaleMode.MedianPointTranslateOnly)
                    {
                        var diff = oldTransform.Translation - _oldAvgPos;
                        var vec4 = new Vector4((float)diff.X, (float)diff.Y, (float)diff.Z, 0);
                        var rotated = vec4 * rotMtx;

                        transform.Translation = rotated.Xyz + _oldAvgPos;
                    }

                    if (RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                        or GizmoRotateScaleMode.IndividualOrigins)
                    {
                        //Multiple selection
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
                    if (_currentTransforms.Count != 1 &&
                        RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                            or GizmoRotateScaleMode.MedianPointTranslateOnly)
                    {
                        transform.Translation = ((oldTransform.Translation - _oldAvgPos) * scale) + _oldAvgPos;
                    }

                    if (_currentTransforms.Count == 1 ||
                        RotateScaleMode is GizmoRotateScaleMode.MedianPoint
                            or GizmoRotateScaleMode.IndividualOrigins)
                    {
                        transform.Scale = oldTransform.Scale * scale;
                    }

                    break;
            }

            if (IsOrthographic && Tool == GizmoTool.Translate)
                transform.Translation = transform.Translation with { Y = oldTransform.Translation.Y };

            //Apply collision
            if (KeyBindings.SnapToCollision.IsDown() && FindCollisionIntersection(oldTransform, transform, out var newPos))
                transform.Translation = newPos;
            
            _renderGroupScene.RenderGroups.SetObjectTransform(transformKeyValue.Key.obj, transformKeyValue.Key.subIndex, transform);
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
                _renderGroupScene.RenderGroups.SetObjectTransform(transformKeyValue.Key.obj, transformKeyValue.Key.subIndex, transform);
            }
        }

        return true;
    }

    private void TryFinishGizmoTransform(ViewportContext context)
    {
        if (!Started || _imGuizmo.IsUsing)
            return;

        Started = false;
        _imGuizmo.ConfirmAction = ImGuizmoConfirmAction.MouseUp;

        if (context.ActionStack is null || _currentTransforms.Count == 0)
            return;

        var objs = _currentTransforms.Keys;
        var actions = new List<IAction>(objs.Count);
        foreach (var transformKeyValue in _currentTransforms)
        {
            var oldTransform = _oldTransforms[transformKeyValue.Key];
            var group = _renderGroupScene.RenderGroups.GetObjectTransformGroup(transformKeyValue.Key.obj,
                transformKeyValue.Key.subIndex, out var transform);
            if (group != null)
            {
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
            _renderGroupScene.RenderGroups.SetObjectTransform(oldTransform.Key.obj, oldTransform.Key.subIndex, oldTransform.Value);
    }

    public void CancelGizmoTransform()
    {
        if (!Started)
            return;

        RestoreOldTransforms();

        Started = false;
        _imGuizmo.ConfirmAction = ImGuizmoConfirmAction.MouseUp;
        _imGuizmo.StopUsing();
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
                _inputOverrideValue = _imGuizmo.OverrideValue?.ToString(CultureInfo.InvariantCulture);
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
}