using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class GLTextureAttachmentDefinition
{
    public PixelInternalFormat   TexturePixelInternalFormat { get; }
    public PixelFormat           TexturePixelFormat         { get; }
    public PixelType             TexturePixelType           { get; }
    public TextureFilterMode     TextureFilterMode          { get; }
    public FramebufferAttachment Attachment                 { get; }

    public GLTextureAttachmentDefinition(PixelInternalFormat texturePixelInternalFormat, PixelFormat texturePixelFormat,
        PixelType texturePixelType, TextureFilterMode textureFilterMode, FramebufferAttachment attachment)
    {
        TexturePixelInternalFormat = texturePixelInternalFormat;
        TexturePixelFormat         = texturePixelFormat;
        TexturePixelType           = texturePixelType;
        TextureFilterMode          = textureFilterMode;
        Attachment                 = attachment;
    }

    /// <summary>
    /// Creates a texture of the given <paramref name="width"/> and
    /// <paramref name="height"/> and attaches it to the current framebuffer.
    /// </summary>
    /// <param name="width">The framebuffer width.</param>
    /// <param name="height">The framebuffer height.</param>
    /// <returns>The texture.</returns>
    public GLTexture Create(int width, int height)
    {
        var texture = new GLTexture(TexturePixelInternalFormat, width, height, TexturePixelFormat, TexturePixelType,
            IntPtr.Zero);
        texture.SetFilterMode(TextureFilterMode, TextureFilterMode);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
            Attachment, TextureTarget.Texture2D, texture.Handle, 0);
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