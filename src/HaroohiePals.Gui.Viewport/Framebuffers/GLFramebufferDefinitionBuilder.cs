using OpenTK.Graphics.OpenGL4;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class GLFramebufferDefinitionBuilder
{
    private const string ATTACHMENT_ALREADY_EXISTS_EXCEPTION_MESSAGE = "Attachment already exists.";

    private readonly Dictionary<FramebufferAttachment, GLTextureAttachmentDefinition> _textureAttachments = new();

    private readonly Dictionary<FramebufferAttachment, GLMultiSampleTextureAttachmentDefinition>
        _multiSampleTextureAttachments = new();

    private readonly Dictionary<FramebufferAttachment, GLRenderbufferAttachmentDefinition>
        _renderbufferAttachments = new();

    public GLFramebufferDefinitionBuilder AddTextureAttachment(GLTextureAttachmentDefinition textureAttachment)
    {
        if (AttachmentExists(textureAttachment.Attachment))
            throw new ArgumentException(ATTACHMENT_ALREADY_EXISTS_EXCEPTION_MESSAGE, nameof(textureAttachment));

        _textureAttachments.Add(textureAttachment.Attachment, textureAttachment);

        return this;
    }

    public GLFramebufferDefinitionBuilder AddTextureAttachment(
        GLMultiSampleTextureAttachmentDefinition textureAttachment)
    {
        if (AttachmentExists(textureAttachment.Attachment))
            throw new ArgumentException(ATTACHMENT_ALREADY_EXISTS_EXCEPTION_MESSAGE, nameof(textureAttachment));

        _multiSampleTextureAttachments.Add(textureAttachment.Attachment, textureAttachment);

        return this;
    }

    public GLFramebufferDefinitionBuilder AddRenderbufferAttachment(
        GLRenderbufferAttachmentDefinition renderbufferAttachment)
    {
        if (AttachmentExists(renderbufferAttachment.Attachment))
            throw new ArgumentException(ATTACHMENT_ALREADY_EXISTS_EXCEPTION_MESSAGE, nameof(renderbufferAttachment));

        _renderbufferAttachments.Add(renderbufferAttachment.Attachment, renderbufferAttachment);

        return this;
    }

    public GLFramebufferDefinition Build()
    {
        return new GLFramebufferDefinition(_textureAttachments.Values, _multiSampleTextureAttachments.Values,
            _renderbufferAttachments.Values);
    }

    private bool AttachmentExists(FramebufferAttachment attachment)
    {
        return _textureAttachments.ContainsKey(attachment) ||
               _multiSampleTextureAttachments.ContainsKey(attachment) ||
               _renderbufferAttachments.ContainsKey(attachment);
    }
}