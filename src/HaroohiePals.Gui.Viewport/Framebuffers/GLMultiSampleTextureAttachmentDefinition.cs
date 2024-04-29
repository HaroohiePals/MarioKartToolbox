using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class GLMultiSampleTextureAttachmentDefinition
{
    public int                   Samples                    { get; }
    public PixelInternalFormat   TexturePixelInternalFormat { get; }
    public FramebufferAttachment Attachment                 { get; }

    public GLMultiSampleTextureAttachmentDefinition(int samples, PixelInternalFormat texturePixelInternalFormat, FramebufferAttachment attachment)
    {
        Samples                    = samples;
        TexturePixelInternalFormat = texturePixelInternalFormat;
        Attachment                 = attachment;
    }

    /// <summary>
    /// Creates a multisample texture of the given <paramref name="width"/> and
    /// <paramref name="height"/> and attaches it to the current framebuffer.
    /// </summary>
    /// <param name="width">The framebuffer width.</param>
    /// <param name="height">The framebuffer height.</param>
    /// <returns>The texture.</returns>
    public GLTexture Create(int width, int height)
    {
        var texture = new GLTexture(Samples, TexturePixelInternalFormat, width, height, true);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            Attachment, TextureTarget.Texture2DMultisample, texture.Handle, 0);
        return texture;
    }

    /// <summary>
    /// Sets this attachment as read target using <see cref="GL.ReadBuffer"/>.
    /// This assumes this attachment belongs to the current framebuffer.
    /// </summary>
    public void SetAsReadBuffer()
    {
        GL.ReadBuffer((ReadBufferMode)Attachment);
    }

    /// <summary>
    /// Sets this attachment as draw target using <see cref="GL.DrawBuffer"/>.
    /// This assumes this attachment belongs to the current framebuffer.
    /// </summary>
    public void SetAsDrawBuffer()
    {
        GL.DrawBuffer((DrawBufferMode)Attachment);
    }
}