#nullable enable
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

// Inspired by:
// https://asliceofrendering.com/scene%20helper/2020/01/05/InfiniteGrid/

public class GridRenderer : IDisposable
{
    private readonly GLShader _shader;
    private readonly GLVertexArray _vertexArray;
    private readonly int _vertexCount;

    public float Scale { get; set; } = 1000f;
    public float Near { get; set; } = 0.25f;
    public float Far { get; set; } = 1600f;
    public uint PickingId { get; set; }

    public GridRenderer()
    {
        _shader = new GLShader(Resources.Shaders.GridVertex, Resources.Shaders.GridFragment);

        VertexData[] vertices =
        [
            new() { Position = new(1, 1, 0) }, new() { Position = new(-1, -1, 0) }, new() { Position = new(-1, 1, 0) },
            new() { Position = new(-1, -1, 0) }, new() { Position = new(1, 1, 0) }, new() { Position = new(1, -1, 0) }
        ];

        // 1. bind Vertex Array Object
        _vertexArray = new GLVertexArray();
        _vertexArray.Bind();

        _vertexCount = vertices.Length;

        // 2. copy our vertices array in a buffer for OpenGL to use
        var vertexBuffer = new GLBuffer<VertexData>(vertices, BufferUsageHint.StaticDraw);
        vertexBuffer.Bind(BufferTarget.ArrayBuffer);

        // 3. Setup vertex attribute pointers
        GLVertexData.SetupVertexAttribPointers();
    }

    public void Render(Matrix4 view, Matrix4 projection, bool translucentPass)
    {
        if (!translucentPass)
            return;

        _shader.Use();

        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);
        _shader.SetFloat("gridScale", Scale);
        _shader.SetFloat("gridFar", Far);
        _shader.SetFloat("gridNear", Near);
        _shader.SetUint("uPickingId", PickingId);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(true);
        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);
        GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(2, BlendEquationMode.FuncAdd);

        _vertexArray.Bind();

        GLVertexData.EnableAllAttribs();
        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        _shader.Dispose();
    }
}
