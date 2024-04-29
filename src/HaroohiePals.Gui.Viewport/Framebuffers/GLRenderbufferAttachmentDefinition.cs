using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class GLRenderbufferAttachmentDefinition
{
    public RenderbufferStorage   InternalFormat { get; }
    public FramebufferAttachment Attachment     { get; }

    public GLRenderbufferAttachmentDefinition(RenderbufferStorage internalFormat, FramebufferAttachment attachment)
    {
        InternalFormat = internalFormat;
        Attachment     = attachment;
    }

    /// <summary>
    /// Creates a renderbuffer of the given <paramref name="width"/> and
    /// <paramref name="height"/> and attaches it to the current framebuffer.
    /// </summary>
    /// <param name="width">The framebuffer width.</param>
    /// <param name="height">The framebuffer height.</param>
    /// <returns>The renderbuffer.</returns>
    public GLRenderbuffer Create(int width, int height)
    {
        var rbo = new GLRenderbuffer();
        rbo.Storage(InternalFormat, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Attachment,
            RenderbufferTarget.Renderbuffer, rbo.Handle);
        return rbo;
    }
}