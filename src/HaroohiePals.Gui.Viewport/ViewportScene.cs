using HaroohiePals.Gui.Viewport.Framebuffers;

namespace HaroohiePals.Gui.Viewport;

public abstract class ViewportScene : IDisposable
{
    public IGLFramebufferProvider FramebufferProvider { get; }

    protected ViewportScene(IGLFramebufferProvider framebufferProvider)
    {
        FramebufferProvider = framebufferProvider;
    }

    public virtual void Update(ViewportContext context, float deltaTime) { }
    public abstract void Render(ViewportContext context);
    public virtual void RenderPostProcessing(ViewportContext context) { }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            FramebufferProvider.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ViewportScene() => Dispose(false);
}