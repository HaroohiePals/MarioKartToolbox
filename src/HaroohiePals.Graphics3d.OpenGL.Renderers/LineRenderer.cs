using HaroohiePals.Graphics3d.OpenGL.Renderers.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public class LineRenderer : IDisposable
{
    private const int LINE_SHADER_UBO_SIZE = 1024;

    private readonly GLShader _shader;
    private readonly GLBuffer<Vector4> _ubo;
    private readonly GLVertexArray _vertexArray;
    private readonly int _bufferIdx;

    public float Thickness = 1;

    public float Offset2dY = -0.5f;

    public Vector3[] Points;
    public bool Loop = false;
    public Color4 Color = Color4.White;
    public uint PickingId;

    public bool Render2d = false;

    public LineRenderer()
    {
        _shader = new GLShader(Shaders.LineVertex, Shaders.LineFragment);
        _bufferIdx = GL.GetUniformBlockIndex(_shader.Handle, "TVertex");
        _ubo = new GLBuffer<Vector4>(LINE_SHADER_UBO_SIZE, BufferUsageHint.DynamicDraw);
        _vertexArray = new GLVertexArray();
    }

    private IReadOnlyList<Vector4> ProcessPoints()
    {
        var points = Points.ToList();

        if (Loop)
            points.Add(points[0]);

        var shaderPoints = new List<Vector4>();

        shaderPoints.Add(new(points[0], 1));

        for (int i = 1; i < points.Count; i++)
        {
            var lastPoint = shaderPoints[^1].Xyz;
            if (lastPoint == points[i])
                continue;

            float len = (points[i] - lastPoint).Length;
            if ((points[i] - lastPoint).Length > 100)
            {
                var dir = (points[i] - lastPoint).Normalized();
                int parts = (int)Math.Ceiling(len / 100);
                float partsLength = len / parts;
                for (int j = 1; j < parts; j++)
                {
                    shaderPoints.Add(new(lastPoint + dir * partsLength * j, 1));
                }
            }

            shaderPoints.Add(new(points[i], 1));
        }

        return shaderPoints;
    }

    private IReadOnlyList<IReadOnlyList<Vector4>> GetGroupedProcessedPoints()
    {
        var groups = new List<List<Vector4>>();
        var shaderPoints = ProcessPoints();

        //Group by 1024
        int groupSize = LINE_SHADER_UBO_SIZE - 2;
        for (int i = 0; i < shaderPoints.Count; i += groupSize)
        {
            var group = shaderPoints.Skip(i).Take(groupSize).ToList();

            group.Add(group[^1]);

            if (groups.Count > 0)
                group.Insert(0, groups[^1][^1]);
            else
                groupSize--;

            group.Insert(0, group[0]);

            groups.Add(group);
        }

        return groups;
    }

    public void Render(Matrix4 view, Matrix4 projection, bool translucentPass, Vector2 viewportSize)
    {
        if (Points == null || Points.Length < 2)
            return;

        _shader.Use();
        _shader.SetFloat("u_thickness", Thickness);
        _shader.SetMatrix4("u_mvp", view * projection);
        _shader.SetVector2("u_resolution", (viewportSize.X, viewportSize.Y));
        _shader.SetVector4("uColor", (Vector4)Color);
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

        if (Render2d)
        {
            var invMtx = (view * projection).Inverted();
            var near = Vector3.Unproject((0, 0, 0), 0, 0, 1, 1, 0, 1, invMtx).Y;
            //Put points on Near Y
            for (int i = 0; i < Points.Length; i++)
                Points[i] = new Vector3(Points[i].X, near - 2f + Offset2dY, Points[i].Z);
        }

        foreach (var group in GetGroupedProcessedPoints())
        {
            _ubo.BufferSubData(0, group.ToArray());

            _vertexArray.Bind();
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, _bufferIdx, _ubo.Handle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6 * (group.Count() - 3));
        }

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public void Dispose()
    {
        _shader?.Dispose();
        _ubo?.Dispose();
        _vertexArray?.Dispose();
    }
}