using HaroohiePals.Gui.Viewport;
using HaroohiePals.Gui.Viewport.Framebuffers;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using ImGuiNET;
using System;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

abstract class InteractiveViewportPanel : ViewportPanel
{
    protected readonly IApplicationSettingsService _applicationSettings;
    protected readonly Gizmo _gizmo;
    protected readonly RenderGroupVisibilityManager _visibilityManager;
    private readonly SelectionRectangle _selectionRect = new();
    private readonly RenderGroupScene _renderGroupScene;
    private readonly ViewportSideToolbar _sideToolbar = new();
    private readonly ViewportCameraToolInfo _helpInfo;

    public DrawTool DrawTool
    {
        get => _gizmo.DrawTool;
        set => _gizmo.DrawTool = value;
    }

    public IViewportCollision ViewportCollision
    {
        get => _gizmo.ViewportCollision;
        set => _gizmo.ViewportCollision = value;
    }

    public bool IsGizmoStarted => _gizmo is { Started: true };

    protected InteractiveViewportPanel(string visibilityPreferencesKey, RenderGroupScene scene,
        IApplicationSettingsService applicationSettingsService)
        : base(scene)
    {
        _renderGroupScene = scene;
        _gizmo = new Gizmo(scene);
        _applicationSettings = applicationSettingsService;
        _visibilityManager = new RenderGroupVisibilityManager(
            scene, visibilityPreferencesKey, applicationSettingsService);
        _helpInfo = new ViewportCameraToolInfo(applicationSettingsService);
    }

    public override void UpdateControls(float deltaTime)
    {
        _gizmo.KeyBindings = _applicationSettings.Settings.KeyBindings.Gizmo.ToGizmoKeyBindings();
        _gizmo.Update();
        _visibilityManager.Update();
    }

    public override void RenderControls()
    {
        if (ImGui.IsWindowHovered() &&
            _renderGroupScene.FramebufferProvider is IPickableFramebufferProvider pickableFramebufferProvider)
        {
            var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos();
            Context.PickingResult =
                new PickingResult(pickableFramebufferProvider.GetPickingId((int)mousePos.X, (int)mousePos.Y));
        }
        else
            Context.PickingResult = PickingResult.Invalid;

        HandleSelectionRectangle();
        RenderGizmos();
        if (Context.SceneObjectHolder.SelectionSize == 0 || (!_gizmo.IsUsing && !_gizmo.IsOver && !_gizmo.IsUsingDrawTool))
            HandlePickingResult();
        _sideToolbar.Draw(Context, _gizmo);
        _helpInfo.Draw(Context, _gizmo, this is InteractiveTopDownViewportPanel);
        RenderTopToolbar();
    }

    protected abstract void RenderTopToolbar();

    private void RenderGizmos()
    {
        //if (!ImGui.IsWindowFocused())
        //    return;

        _gizmo.Draw(Context);
    }

    private void HandleSelectionRectangle()
    {
        if (_renderGroupScene.FramebufferProvider is not IPickableFramebufferProvider pickableFramebufferProvider)
            return;

        if (_gizmo.Tool == GizmoTool.Draw || _gizmo.IsUsing)
            return;

        if (_selectionRect.Draw())
        {
            var ids = pickableFramebufferProvider.GetPickingIds(_selectionRect.TopLeft.X, _selectionRect.BottomRight.Y,
                _selectionRect.Size.X, _selectionRect.Size.Y);
            var pickingResults = ids.Select(x => new PickingResult(x)).Where(x => !x.IsInvalid).ToArray();
            HandlePickingResults(pickingResults);
        }
    }

    private void HandlePickingResult()
    {
        if (!Context.PickingResult.IsInvalid)
        {
            try
            {
                Context.HoverObject =
                    new(
                        _renderGroupScene.RenderGroups[Context.PickingResult.GroupId]
                            .GetObject(Context.PickingResult.Index),
                        Context.PickingResult.SubIndex);

                if (Context.HoverObject.Object == null)
                    throw new Exception();
            }
            catch
            {
                Console.WriteLine(
                    $"ERROR: Group {Context.PickingResult.GroupId}, Idx {Context.PickingResult.Index}, SubIdx {Context.PickingResult.SubIndex}");
                return;
            }

            if (ImGui.IsWindowFocused() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (ImGui.GetIO().KeyCtrl || ImGui.GetIO().KeyShift)
                {
                    if (Context.SceneObjectHolder.IsSubIndexSelected(
                            Context.HoverObject.Object, Context.HoverObject.SubIndex))
                    {
                        Context.SceneObjectHolder.RemoveSubIndexFromSelection(
                            Context.HoverObject.Object, Context.HoverObject.SubIndex);
                    }
                    else
                        Context.SceneObjectHolder.AddToSelection(Context.HoverObject.Object,
                            Context.HoverObject.SubIndex);
                }
                else
                    Context.SceneObjectHolder.SetSelection(Context.HoverObject.Object,
                        Context.HoverObject.SubIndex);
            }
        }
        else
        {
            Context.HoverObject = null;
            if (ImGui.IsWindowFocused() && ImGui.IsWindowHovered() &&
                ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.GetIO().KeyCtrl && !ImGui.GetIO().KeyShift &&
                _gizmo.Tool != GizmoTool.Draw)
                Context.SceneObjectHolder.ClearSelection();
        }
    }

    private void HandlePickingResults(PickingResult[] results)
    {
        var addToSelection = ImGui.GetIO().KeyCtrl || ImGui.GetIO().KeyShift;

        if (!addToSelection)
            Context.SceneObjectHolder.ClearSelection();

        foreach (var result in results)
        {
            try
            {
                var obj = new SelectionHandle(_renderGroupScene.RenderGroups[result.GroupId].GetObject(result.Index),
                    result.SubIndex);
                Context.SceneObjectHolder.AddToSelection(obj.Object, obj.SubIndex);
            }
            catch
            {
                Console.WriteLine(
                    $"ERROR: Group {result.GroupId}, Idx {result.Index}, SubIdx {result.SubIndex}");
            }
        }
    }

    public void CancelGizmoTransform() => _gizmo.CancelGizmoTransform();
}