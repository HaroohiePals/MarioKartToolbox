#nullable enable
using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics.CodeAnalysis;

namespace HaroohiePals.Gui.Viewport.Framebuffers;

public sealed class TestGLFramebufferProvider : IPickableFramebufferProvider
{
    public const int PickingBufferId = 1;
    public const int FogBufferId     = 2;

    private static readonly GLTextureAttachmentDefinition ImageBufDefinition =
        new(PixelInternalFormat.Rgb, PixelFormat.Rgb, PixelType.UnsignedByte,
            TextureFilterMode.Linear, FramebufferAttachment.ColorAttachment0);

    private static readonly GLTextureAttachmentDefinition PickingBufDefinition =
        new(PixelInternalFormat.Rgba8ui, PixelFormat.RgbaInteger, PixelType.UnsignedByte,
            TextureFilterMode.Nearest, FramebufferAttachment.ColorAttachment1);

    private static readonly GLRenderbufferAttachmentDefinition DepthBufDefinition =
        new(RenderbufferStorage.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment);

    private static readonly GLMultiSampleTextureAttachmentDefinition ImageBufMultiDefinition =
        new(4, PixelInternalFormat.Rgb, FramebufferAttachment.ColorAttachment0);

    private static readonly GLMultiSampleTextureAttachmentDefinition PickingBufMultiDefinition =
        new(4, PixelInternalFormat.Rgba8ui, FramebufferAttachment.ColorAttachment1);

    private static readonly GLMultiSampleTextureAttachmentDefinition FogBufMultiDefinition =
        new(4, PixelInternalFormat.R8ui, FramebufferAttachment.ColorAttachment2);

    private static readonly GLMultiSampleTextureAttachmentDefinition DepthBufMultiDefinition =
        new(4, PixelInternalFormat.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment);

    private static readonly Vector2[] ScreenQuadVertices =
    {
        (0, 0),
        (1, 0),
        (1, 1),
        (0, 1)
    };

    private static readonly uint[] ScreenQuadIndices =
    {
        0, 1, 2,
        0, 2, 3
    };

    private readonly GLFramebufferDefinition _framebufferDefinition;
    private readonly GLFramebufferDefinition _multiFramebufferDefinition;
    private readonly bool                    _withPickingBuffer;
    private readonly bool                    _withFogBuffer;

    private GLCompleteFramebuffer? _framebuffer;
    private GLCompleteFramebuffer? _multiFramebuffer;

    private bool     _isInBegin = false;
    private Vector2i _lastResolution;
    private Vector2i _lastResolutionMulti;

    private GLShader?          _idBlitShader;
    private GLVertexArray?     _idBlitVertexArray;
    private GLBuffer<Vector2>? _idBlitVertexBuffer;
    private GLBuffer<uint>?    _idBlitElementBuffer;

    public Color4 BgColor { get; set; }

    public GLTexture ImageBufferMultiTex => _multiFramebuffer.GetTextureAttachment(ImageBufMultiDefinition.Attachment);
    public GLTexture DepthBufferMultiTex => _multiFramebuffer.GetTextureAttachment(DepthBufMultiDefinition.Attachment);
    public GLTexture FogBufferMultiTex => _multiFramebuffer.GetTextureAttachment(FogBufMultiDefinition.Attachment);

    public TestGLFramebufferProvider(Color4 bgColor, bool withPickingBuffer, bool withFogBuffer)
    {
        BgColor                     = bgColor;
        _withPickingBuffer          = withPickingBuffer;
        _withFogBuffer              = withFogBuffer;
        _framebufferDefinition      = CreateFramebufferDefinition(withPickingBuffer);
        _multiFramebufferDefinition = CreateMultiFramebufferDefinition(withPickingBuffer, withFogBuffer);
    }

    private GLFramebufferDefinition CreateFramebufferDefinition(bool withPickingBuffer)
    {
        var builder = new GLFramebufferDefinitionBuilder()
            .AddTextureAttachment(ImageBufDefinition)
            .AddRenderbufferAttachment(DepthBufDefinition);
        if (withPickingBuffer)
            builder.AddTextureAttachment(PickingBufDefinition);
        return builder.Build();
    }

    private GLFramebufferDefinition CreateMultiFramebufferDefinition(bool withPickingBuffer, bool withFogBuffer)
    {
        var builder = new GLFramebufferDefinitionBuilder()
            .AddTextureAttachment(ImageBufMultiDefinition)
            .AddTextureAttachment(DepthBufMultiDefinition);
        if (withPickingBuffer)
            builder.AddTextureAttachment(PickingBufMultiDefinition);
        if (withFogBuffer)
            builder.AddTextureAttachment(FogBufMultiDefinition);
        return builder.Build();
    }

    [MemberNotNull(nameof(_idBlitShader))]
    private void InitIdBlit()
    {
        if (_idBlitShader is not null)
            return;

        _idBlitShader = new GLShader(Resources.Shaders.IdBlitVertex, Resources.Shaders.IdBlitFragment);

        _idBlitVertexArray = new GLVertexArray();
        _idBlitVertexArray.Bind();

        _idBlitVertexBuffer =
            new GLBuffer<Vector2>(ScreenQuadVertices, BufferUsageHint.StaticDraw);
        _idBlitVertexBuffer.Bind(BufferTarget.ArrayBuffer);

        _idBlitElementBuffer = new GLBuffer<uint>(ScreenQuadIndices, BufferUsageHint.StaticDraw);
        _idBlitElementBuffer.Bind(BufferTarget.ElementArrayBuffer);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);

        GL.BindVertexArray(0);
    }

    [MemberNotNull(nameof(_framebuffer))]
    private void CreateFbo(Vector2i size)
    {
        if (_framebuffer is not null && _lastResolution == size)
            return;

        _lastResolution = size;

        _framebuffer?.Dispose();
        _framebuffer = new GLCompleteFramebuffer(_framebufferDefinition, size.X, size.Y);
    }

    [MemberNotNull(nameof(_multiFramebuffer))]
    private void CreateFboMulti(Vector2i size)
    {
        if (_multiFramebuffer is not null && _lastResolutionMulti == size)
            return;

        _lastResolutionMulti = size;

        _multiFramebuffer?.Dispose();
        _multiFramebuffer = new GLCompleteFramebuffer(_multiFramebufferDefinition, size.X, size.Y);
    }

    public void BeginRendering(int width, int height)
    {
        if (_isInBegin)
            throw new Exception();

        _isInBegin = true;

        CreateFbo((width, height));
        CreateFboMulti((width, height));

        _multiFramebuffer.Framebuffer.Bind(FramebufferTarget.Framebuffer);

        var drawBuffers = new[]
        {
            (DrawBuffersEnum)ImageBufMultiDefinition.Attachment,
            (DrawBuffersEnum)PickingBufMultiDefinition.Attachment,
            (DrawBuffersEnum)FogBufMultiDefinition.Attachment
        };
        GL.DrawBuffers(drawBuffers.Length, drawBuffers);

        ClearBuffers();
    }

    public GLTexture EndRendering()
    {
        if (!_isInBegin)
            throw new Exception();

        _isInBegin = false;

        if (_framebuffer is null || _multiFramebuffer is null)
            throw new Exception();

        _framebuffer.Framebuffer.Bind(FramebufferTarget.Framebuffer);

        _multiFramebuffer.Framebuffer.Bind(FramebufferTarget.ReadFramebuffer);
        ImageBufMultiDefinition.SetAsReadBuffer();
        _framebuffer.Framebuffer.Bind(FramebufferTarget.DrawFramebuffer);
        ImageBufDefinition.SetAsDrawBuffer();
        GL.BlitFramebuffer(0, 0, _lastResolution.X, _lastResolution.Y, 0, 0, _lastResolution.X, _lastResolution.Y,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

        if (_withPickingBuffer)
        {
            if (_idBlitShader is null)
                InitIdBlit();

            _framebuffer.Framebuffer.Bind(FramebufferTarget.Framebuffer);
            PickingBufDefinition.SetAsDrawBuffer();
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.ActiveTexture(TextureUnit.Texture0);
            _multiFramebuffer.GetTextureAttachment(PickingBufDefinition.Attachment).Use();
            _idBlitVertexArray!.Bind();
            GL.EnableVertexAttribArray(0);

            _idBlitShader.Use();
            _idBlitShader.SetMatrix4("projMtxScreenQuad", Matrix4.CreateOrthographicOffCenter(
                0.0f, _lastResolution.X, _lastResolution.Y, 0.0f, -1.0f, 1.0f));
            _idBlitShader.SetVector2("viewportSize", _lastResolution);

            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return _framebuffer.GetTextureAttachment(ImageBufDefinition.Attachment);
    }

    public void Dispose()
    {
        _framebuffer?.Dispose();
        _multiFramebuffer?.Dispose();
        _idBlitShader?.Dispose();
        _idBlitVertexArray?.Dispose();
        _idBlitVertexBuffer?.Dispose();
        _idBlitElementBuffer?.Dispose();
    }

    public uint GetPickingId(int x, int y) => GetPickingIds(x, y, 1, 1)[0];

    public void RipFramebuffer(ReadBufferMode readBufferMode, string outputPath, bool noAlpha = false)
    {
        byte[] readPixels = ReadPixels(readBufferMode, 0, _lastResolution.Y, _lastResolution.X, _lastResolution.Y);

        if (noAlpha)
        {
            for (int i = 3; i < readPixels.Length; i += 4)
                readPixels[i] = 0xFF;
        }

        var image = Image.LoadPixelData<Rgba32>(readPixels, _lastResolution.X, _lastResolution.Y);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        image.SaveAsPng(outputPath);
    }

    public uint[] GetPickingIds(int x, int y, int width, int height)
    {
        byte[] pickingIdBytes = ReadPixels(ReadBufferMode.ColorAttachment1, x, y, width, height);

        var pickingIds = new HashSet<uint>();

        for (int i = 0; i < pickingIdBytes.Length; i += 4)
        {
            uint pickingId = (uint)(pickingIdBytes[i] | pickingIdBytes[i + 1] << 8 | pickingIdBytes[i + 2] << 16 |
                                    pickingIdBytes[i + 3] << 24);

            pickingIds.Add(pickingId);
        }

        return pickingIds.ToArray();
    }

    private byte[] ReadPixels(ReadBufferMode readBufferMode, int x, int y, int width, int height)
    {
        if (_framebuffer is null)
            throw new Exception();

        _framebuffer.Framebuffer.Bind(FramebufferTarget.ReadFramebuffer);
        GL.ReadBuffer(readBufferMode);

        var format = readBufferMode == ReadBufferMode.ColorAttachment1 ? PixelFormat.RgbaInteger : PixelFormat.Rgba;

        //Clamp values to avoid weird operations
        width  = Math.Max(0, width);
        height = Math.Max(0, height);

        byte[] readBytes = new byte[4 * width * height];
        GL.ReadPixels(x, _lastResolution.Y - y - 1, width, height, format, PixelType.UnsignedByte,
            readBytes);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return readBytes;
    }

    private void ClearBuffers()
    {
        GL.DrawBuffers(3, new[]
        {
            (DrawBuffersEnum)ImageBufMultiDefinition.Attachment,
            (DrawBuffersEnum)PickingBufMultiDefinition.Attachment,
            (DrawBuffersEnum)FogBufMultiDefinition.Attachment
        });

        var invalidPickingId = new uint[4]
        {
            ViewportContext.InvalidPickingId & 0xFF,
            ViewportContext.InvalidPickingId >> 8 & 0xFF,
            ViewportContext.InvalidPickingId >> 16 & 0xFF,
            ViewportContext.InvalidPickingId >> 24 & 0xFF
        };
        GL.ClearBuffer(ClearBuffer.Color, PickingBufferId, invalidPickingId);
        var fogOn = new uint[4] { 1, 0, 0, 0 };
        GL.ClearBuffer(ClearBuffer.Color, FogBufferId, fogOn);

        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        GL.ClearColor(BgColor);
        GL.DepthMask(true);
        GL.StencilMask(0xFF);
        GL.ClearStencil(0x7F);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                 ClearBufferMask.StencilBufferBit);

        GL.DrawBuffers(2, new[]
        {
            (DrawBuffersEnum)ImageBufMultiDefinition.Attachment,
            (DrawBuffersEnum)PickingBufMultiDefinition.Attachment,
        });

        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);
    }
}