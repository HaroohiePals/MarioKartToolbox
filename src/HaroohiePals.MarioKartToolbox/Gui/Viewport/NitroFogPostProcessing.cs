using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class NitroFogPostProcessing : IDisposable
{
    private static readonly Vector2[] ScreenQuadVertices =
    {
        (0, 0), (1, 0), (1, 1), (0, 1)
    };

    private static readonly uint[] ScreenQuadIndices =
    {
        0, 1, 2,
        0, 2, 3
    };

    private bool _initialized = false;
    private GLShader _fogShader;

    private GLVertexArray _vertexArray;
    private GLBuffer<Vector2> _vertexBuffer;
    private GLBuffer<uint> _elementBuffer;

    public bool FogEnabled { get; set; }
    public Rgb555 FogColor { get; set; }
    public int FogShift { get; set; }
    public int FogOffset { get; set; }
    public int[] FogTable { get; } = new int[32];
    public Matrix4 FogProjectionMatrix { get; set; }

    public void Render(ViewportContext context, GLTexture depthBuffer, GLTexture fogBuffer)
    {
        if (!FogEnabled)
            return;

        if (!_initialized)
            Initialize();

        _fogShader.Use();
        _fogShader.SetMatrix4("projMtxScreenQuad", Matrix4.CreateOrthographicOffCenter(
            0.0f, context.ViewportSize.X, context.ViewportSize.Y, 0.0f, -1.0f, 1.0f));
        _fogShader.SetMatrix4("invProjMtx", context.ProjectionMatrix.Inverted());
        _fogShader.SetMatrix4("invViewMtx", context.ViewMatrix.Inverted());
        _fogShader.SetMatrix4("mkdsProjMtx", FogProjectionMatrix);
        _fogShader.SetVector2("viewportSize", (context.ViewportSize.X, context.ViewportSize.Y));
        _fogShader.SetInt("depthBufferTex", 0);
        _fogShader.SetInt("fogBufferTex", 1);
        _fogShader.SetVector4("fogColor", (FogColor.R / 31f, FogColor.G / 31f, FogColor.B / 31f, 1f));
        _fogShader.SetInt("fogShift", FogShift);
        _fogShader.SetInt("fogOffset", FogOffset);
        _fogShader.SetIntArray("fogTable", FogTable);

        GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DMultisample, depthBuffer.Handle);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2DMultisample, fogBuffer.Handle);
        _vertexArray.Bind();
        GL.EnableVertexAttribArray(0);
        GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
    }

    public void Dispose()
    {
        _fogShader?.Dispose();
        _vertexArray?.Dispose();
        _vertexBuffer?.Dispose();
        _elementBuffer?.Dispose();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _fogShader = new GLShader(Shaders.FogPassVertex, Shaders.FogPassFragment);

        _vertexArray = new GLVertexArray();
        _vertexArray.Bind();

        // 2. copy our vertices array in a buffer for OpenGL to use
        _vertexBuffer = new GLBuffer<Vector2>(ScreenQuadVertices, BufferUsageHint.StaticDraw);
        _vertexBuffer.Bind(BufferTarget.ArrayBuffer);

        _elementBuffer = new GLBuffer<uint>(ScreenQuadIndices, BufferUsageHint.StaticDraw);
        _elementBuffer.Bind(BufferTarget.ElementArrayBuffer);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);

        GL.BindVertexArray(0);

        _initialized = true;
    }
}