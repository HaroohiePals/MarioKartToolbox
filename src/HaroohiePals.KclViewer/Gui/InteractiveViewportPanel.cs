using HaroohiePals.Gui.Viewport;
using HaroohiePals.Gui.Viewport.Framebuffers;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class InteractiveViewportPanel : ViewportPanel
{
    private readonly   RenderGroupScenePerspective _perspectiveScene;

    public PerspectiveCameraControls CameraControls { get; } = new();

    public InteractiveViewportPanel(RenderGroupScenePerspective scene)
        : base(scene)
    {
        _perspectiveScene = scene;
    }

    public override void UpdateControls(float deltaTime)
    {
        CameraControls.Update(Context, deltaTime);
    }

    public override void RenderControls()
    {
        if (ImGui.IsWindowHovered() &&
            _perspectiveScene.FramebufferProvider is IPickableFramebufferProvider pickableFramebufferProvider)
        {
            var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos();
            Context.PickingResult =
                new PickingResult(pickableFramebufferProvider.GetPickingId((int)mousePos.X, (int)mousePos.Y));
        }
        else
            Context.PickingResult = PickingResult.Invalid;
        
        HandlePickingResult();
        CameraControls.Render(Context);
    }

    private void HandlePickingResult()
    {
        if (!Context.PickingResult.IsInvalid)
        {
            try
            {
                Context.HoverObject =
                    new(
                        _perspectiveScene.RenderGroups[Context.PickingResult.GroupId]
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
                ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.GetIO().KeyCtrl && !ImGui.GetIO().KeyShift)
                Context.SceneObjectHolder.ClearSelection();
        }
    }
}