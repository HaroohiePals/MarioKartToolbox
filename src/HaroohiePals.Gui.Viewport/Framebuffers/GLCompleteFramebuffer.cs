#nullable enable
using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class GLCompleteFramebuffer : IDisposable
{
    private readonly Dictionary<FramebufferAttachment, GLTexture>      _textureAttachments      = new();
    private readonly Dictionary<FramebufferAttachment, GLRenderbuffer> _renderbufferAttachments = new();

    private bool _disposed;

    public GLFramebuffer Framebuffer { get; }

    public GLCompleteFramebuffer(GLFramebufferDefinition definition, int width, int height)
    {
        int curFrameBuffer = GL.GetInteger(GetPName.FramebufferBinding);
        {
            Framebuffer = new GLFramebuffer();
            Framebuffer.Bind(FramebufferTarget.Framebuffer);

            foreach (var textureAttachment in definition.TextureAttachments)
                _textureAttachments.Add(textureAttachment.Attachment, textureAttachment.Create(width, height));

            foreach (var textureAttachment in definition.MultiSampleTextureAttachments)
                _textureAttachments.Add(textureAttachment.Attachment, textureAttachment.Create(width, height));

            foreach (var renderbufferAttachment in definition.RenderbufferAttachments)
                _renderbufferAttachments.Add(renderbufferAttachment.Attachment,
                    renderbufferAttachment.Create(width, height));

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Creating framebuffer failed");
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, curFrameBuffer);
    }

    public GLTexture GetTextureAttachment(FramebufferAttachment attachment)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

        return _textureAttachments[attachment];
    }

    public GLRenderbuffer GetRenderbufferAttachment(FramebufferAttachment attachment)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

        return _renderbufferAttachments[attachment];
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var textureAttachment in _textureAttachments.Values)
            textureAttachment.Dispose();

        foreach (var renderbufferAttachments in _renderbufferAttachments.Values)
            renderbufferAttachments.Dispose();

        _disposed = true;
    }
}