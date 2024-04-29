using HaroohiePals.Gui.Viewport.Framebuffers;
using HaroohiePals.Gui.Viewport.Projection;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public class RenderGroupSceneTopDown : RenderGroupScene
{
    public OrthographicProjection OrthographicProjection { get; } =
        new(8192, Vector2.Zero, 8192, -16384);

    public RenderGroupSceneTopDown(IGLFramebufferProvider framebufferProvider)
        : base(framebufferProvider) { }

    public override void Render(ViewportContext context)
    {
        context.ProjectionMatrix = OrthographicProjection.GetProjectionMatrix(context);
        context.ViewMatrix       = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90));

        base.Render(context);
    }

    public override void FrameSelection(ViewportContext context)
    {
        //todo
    }
}