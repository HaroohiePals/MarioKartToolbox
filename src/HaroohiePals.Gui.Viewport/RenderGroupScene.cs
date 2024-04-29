using HaroohiePals.Gui.Viewport.Framebuffers;

namespace HaroohiePals.Gui.Viewport;

public abstract class RenderGroupScene : ViewportScene
{
    public RenderGroupCollection RenderGroups { get; } = new();

    protected RenderGroupScene(IGLFramebufferProvider framebufferProvider)
        : base(framebufferProvider) { }

    public override void Update(ViewportContext context, float deltaTime)
    {
        RenderGroups.Update(deltaTime);
    }

    public override void Render(ViewportContext context)
    {
        if (context.ForceCustomProjectionMatrix)
            context.ProjectionMatrix = context.CustomProjectionMatrix;

        for (int i = 0; i < RenderGroups.Count; i++)
        {
            var group = RenderGroups[i];
            if (!group.Enabled)
                continue;

            group.PickingGroupId = i;
            group.Render(context);
        }
    }

    public abstract void FrameSelection(ViewportContext context);
}