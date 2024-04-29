#nullable enable
using HaroohiePals.Gui.View;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public class ViewportPanel : IView
{
    private readonly ViewportScene _scene;

    public ViewportContext Context { get; } = new();

    public System.Numerics.Vector2 Size { get; set; }

    public event Action? DoubleClick;

    public ViewportPanel(ViewportScene scene)
    {
        _scene = scene;
    }

    private void DrawGL()
    {
        _scene.FramebufferProvider.BeginRendering(Context.ViewportSize.X, Context.ViewportSize.Y);
        {
            GL.Viewport(0, 0, Context.ViewportSize.X, Context.ViewportSize.Y);

            Context.TranslucentPass = false;
            _scene.Render(Context);

            Context.TranslucentPass = true;
            _scene.Render(Context);

            _scene.RenderPostProcessing(Context);
        }
        var texture = _scene.FramebufferProvider.EndRendering();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        var   region     = ImGui.GetContentRegionAvail();
        var   contentPos = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
        float vertRatio  = region.X / Context.ViewportSize.X;
        float horizRatio = region.Y / Context.ViewportSize.Y;

        ImGui.GetWindowDrawList().AddImage((IntPtr)texture.Handle,
            contentPos, contentPos + region,
            new System.Numerics.Vector2(0, horizRatio),
            new System.Numerics.Vector2(vertRatio, 0));
    }

    public bool Draw()
    {
        bool focus = ImGui.IsWindowFocused();

        if (ImGui.BeginChild($"Viewport_{GetHashCode()}", Size))
        {
            if (focus)
                ImGui.SetWindowFocus();
            var region    = ImGui.GetContentRegionAvail();
            var regionInt = new Vector2i((int)region.X, (int)region.Y);
            Context.ViewportSize = regionInt;

            if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
            {
                var cursorPos = ImGui.GetCursorPos();
                ImGui.InvisibleButton($"FakeButton_{GetHashCode()}", region, ImGuiButtonFlags.MouseButtonRight);
                ImGui.SetCursorPos(cursorPos);
            }

            _scene.Update(Context, ImGui.GetIO().DeltaTime);
            UpdateControls(ImGui.GetIO().DeltaTime);

            DrawGL();

            if (ImGui.IsWindowHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                DoubleClick?.Invoke();

            RenderControls();

            ImGui.EndChild();
        }

        return true;
    }

    public virtual void UpdateControls(float deltaTime) { }
    public virtual void RenderControls() { }
}