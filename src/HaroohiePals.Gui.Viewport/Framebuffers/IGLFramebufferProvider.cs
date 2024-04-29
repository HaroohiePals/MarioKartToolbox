using HaroohiePals.Graphics3d.OpenGL;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public interface IGLFramebufferProvider : IDisposable
{
    void BeginRendering(int width, int height);
    GLTexture EndRendering();
}