#nullable enable
namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class GLFramebufferDefinition
{
    public IReadOnlyList<GLTextureAttachmentDefinition>            TextureAttachments            { get; }
    public IReadOnlyList<GLMultiSampleTextureAttachmentDefinition> MultiSampleTextureAttachments { get; }
    public IReadOnlyList<GLRenderbufferAttachmentDefinition>       RenderbufferAttachments       { get; }

    public GLFramebufferDefinition(IEnumerable<GLTextureAttachmentDefinition> textureAttachments,
        IEnumerable<GLMultiSampleTextureAttachmentDefinition> multiSampleTextureAttachments,
        IEnumerable<GLRenderbufferAttachmentDefinition> renderbufferAttachments)
    {
        TextureAttachments            = textureAttachments.ToArray();
        MultiSampleTextureAttachments = multiSampleTextureAttachments.ToArray();
        RenderbufferAttachments       = renderbufferAttachments.ToArray();
    }
}