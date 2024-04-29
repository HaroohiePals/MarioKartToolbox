using HaroohiePals.Gui.Viewport.Framebuffers;
using HaroohiePals.Gui.Viewport.Projection;

namespace HaroohiePals.Gui.Viewport;

public class RenderGroupScenePerspective : RenderGroupScene
{
    public PerspectiveProjection     Projection { get; } = new(60, 0.25f * 16f, 1600f * 16f);

    public RenderGroupScenePerspective(IGLFramebufferProvider framebufferProvider)
        : base(framebufferProvider) { }

    public override void Render(ViewportContext context)
    {
        context.ProjectionMatrix = Projection.GetProjectionMatrix(context);

        base.Render(context);
    }

    public override void FrameSelection(ViewportContext context)
    {
        // none
    }
}