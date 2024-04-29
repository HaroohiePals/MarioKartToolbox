using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public abstract class InstancedPointRenderer : IDisposable
{
    public InstancedPoint[] Points = new InstancedPoint[0];

    public float Offset2dY = 0f;

    private bool _render2d = false;
    private bool _renderTranslucentPass = false;

    private InstancedPointGlData[] _instanceData = new InstancedPointGlData[16];
    private int _instanceCount = 0;

    private readonly GLShader _shader;
    private GLBuffer<InstancedPointGlData> _instanceBuffer;
    private GLTexture _texture;

    private int _vtxCount;
    private readonly GLVertexArray _vertexArray;
    private GLBuffer<VertexData> _vertexBuffer;

    public InstancedPointRenderer(GLShader shader, bool render2d, bool renderTranslucentPass, VertexData[] vertices, byte[] texData = null)
    {
        _shader = shader;
        _render2d = render2d;
        _renderTranslucentPass = renderTranslucentPass;
        if (texData != null)
            _texture = SetupTexture(texData);

        // 1. bind Vertex Array Object
        _vertexArray = new GLVertexArray();
        _vertexArray.Bind();

        _vtxCount = vertices.Length;

        // 2. copy our vertices array in a buffer for OpenGL to use
        _vertexBuffer = new GLBuffer<VertexData>(vertices, BufferUsageHint.StaticDraw);
        _vertexBuffer.Bind(BufferTarget.ArrayBuffer);

        // 3. Setup vertex attribute pointers
        GLVertexData.SetupVertexAttribPointers();

        _instanceBuffer = new GLBuffer<InstancedPointGlData>();
        _instanceBuffer.Bind(BufferTarget.ArrayBuffer);
        GLUtil.SetupVertexAttribPointers<InstancedPointGlData>(VertexData.MtxIdIdx + 1, 1);
    }

    protected virtual GLTexture SetupTexture(byte[] texData)
    {
        using (var texImage = Image.Load<Rgba32>(texData))
        {
            var data = new byte[texImage.Width * texImage.Height * 8];
            texImage.CopyPixelDataTo(data);
            var texture = new GLTexture(PixelInternalFormat.Rgba8, texImage.Width, texImage.Height, PixelFormat.Bgra,
                PixelType.UnsignedByte, data);
            texture.Use();
            texture.SetWrapMode(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -2.0f);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texture;
        }
    }

    private void SetupInstanceBuffer(Matrix4 view, Matrix4 projection)
    {
        // Set instances
        int idx = 0;
        for (int i = 0; i < Points.Length; i++)
        {
            var point = Points[i];
            if (idx >= _instanceData.Length)
                Array.Resize(ref _instanceData, _instanceData.Length * 2);

            _instanceData[idx].Hover = point.IsHovered ? 1u : 0;
            _instanceData[idx].Highlight = point.IsSelected ? 1u : 0;

            if (_render2d)
            {
                var invMtx = (view * projection).Inverted();
                var near = Vector3.Unproject((0, 0, 0), 0, 0, 1, 1, 0, 1, invMtx).Y;
                float targetY = near - 2f + Offset2dY;

                var pos = point.Transform.ExtractTranslation();
                point.Transform.Row3 = new(pos.X, targetY, pos.Z, 1f);
            }

            _instanceData[idx].Transform = point.Transform;

            _instanceData[idx].UseTexture = (uint)(_texture != null ? point.UseTexture ? 1 : 0 : 0);
            _instanceData[idx].TexCoordAngle = point.TexCoordAngle;

            _instanceData[idx].Color = (point.Color.R, point.Color.G, point.Color.B);
            _instanceData[idx].PickingId = point.PickingId;

            idx++;
        }

        _instanceCount = idx;
        _instanceBuffer.BufferData(_instanceData.AsSpan(0, _instanceCount), BufferUsageHint.DynamicDraw);
    }

    public virtual void Render(Matrix4 view, Matrix4 projection, bool translucentPass)
    {
        if (Points == null || Points.Length == 0)
            return;

        if (_renderTranslucentPass && !translucentPass || !_renderTranslucentPass && translucentPass)
            return;

        SetupInstanceBuffer(view, projection);

        _shader.Use();

        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(true);
        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);
        GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(2, BlendEquationMode.FuncAdd);

        if (_texture != null)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            _texture.Use();
        }

        _vertexArray.Bind();
        GLVertexData.EnableAllAttribs();

        RenderInstanced(_instanceCount);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    protected virtual void RenderInstanced(int instanceCount) => GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, _vtxCount, instanceCount);

    public virtual void Dispose()
    {
        _shader?.Dispose();
        _instanceBuffer?.Dispose();
        _texture?.Dispose();
        _vertexArray?.Dispose();
        _vertexBuffer?.Dispose();
    }
}
