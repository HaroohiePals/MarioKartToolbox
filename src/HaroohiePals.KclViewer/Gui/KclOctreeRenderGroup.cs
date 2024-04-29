using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui.Viewport;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Text;

namespace HaroohiePals.KclViewer.Gui;

internal class KclOctreeRenderGroup : RenderGroup, IDisposable
{
    private KclViewerContext _context;
    private GLShader         _shader;
    private bool             _initialized;

    private GLBuffer<Vector3>            _vertexBuffer;
    private GLBuffer<OctreeCubeInstance> _instanceBuffer;
    private GLVertexArray                _vertexArray;

    private readonly List<OctreeNodeEx> _cubes = new();

    public KclOctreeRenderGroup(KclViewerContext context)
    {
        _context = context;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _shader = new GLShader(File.ReadAllText("Files/kclOctreeCube.vert", Encoding.UTF8),
            File.ReadAllText("Files/kclOctreeCube.frag", Encoding.UTF8));

        _vertexArray = new GLVertexArray();
        _vertexArray.Bind();

        _vertexBuffer = new GLBuffer<Vector3>(CubeLines, BufferUsageHint.StaticDraw);
        _vertexBuffer.Bind(BufferTarget.ArrayBuffer);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Unsafe.SizeOf<Vector3>(), 0);
        GL.VertexAttribDivisor(0, 0);
        GL.EnableVertexAttribArray(0);

        _instanceBuffer = new GLBuffer<OctreeCubeInstance>();
        _instanceBuffer.Bind(BufferTarget.ArrayBuffer);
        GLUtil.SetupVertexAttribPointers<OctreeCubeInstance>(1, 1);

        GL.BindVertexArray(0);

        InitBuffer();

        _initialized = true;
    }

    private static readonly Vector3[] CubeLines =
    {
        //front
        (0, 1, 1), (1, 1, 1),
        (1, 1, 1), (1, 0, 1),
        (1, 0, 1), (0, 0, 1),
        (0, 0, 1), (0, 1, 1),
        //right
        (1, 1, 1), (1, 1, 0),
        (1, 1, 0), (1, 0, 0),
        (1, 0, 0), (1, 0, 1),
        //back
        (1, 1, 0), (0, 1, 0),
        (0, 0, 0), (1, 0, 0),
        (0, 0, 0), (0, 1, 0),
        //left
        (0, 1, 0), (0, 1, 1),
        (0, 0, 1), (0, 0, 0)
    };

    private struct OctreeCubeInstance
    {
        [GLVertexAttrib(0)]
        public Vector3 MinPos;

        [GLVertexAttrib(1)]
        public float Size;

        [GLVertexAttribI(2)]
        public uint PickingId;

        public static readonly int StructSize = Unsafe.SizeOf<OctreeCubeInstance>();
    }

    private OctreeCubeInstance[] _instances     = new OctreeCubeInstance[16];
    private int                  _instanceCount = 0;

    private void ResetInstanceBuffer()
    {
        _instanceCount = 0;
    }

    private void AddInstance(in OctreeCubeInstance vertex)
    {
        if (_instanceCount >= _instances.Length)
            Array.Resize(ref _instances, _instances.Length * 2);

        _instances[_instanceCount++] = vertex;
    }

    private void RenderCube(OctreeNodeEx cube)
    {
        _cubes.Add(cube);

        AddInstance(new OctreeCubeInstance()
        {
            MinPos    = ((float)cube.MinPos.X, (float)cube.MinPos.Y, (float)cube.MinPos.Z),
            Size      = cube.Size,
            PickingId = (uint)(_cubes.Count - 1)
        });

        if (cube.IsLeaf)
            return;

        for (int z = 0; z < 2; z++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    RenderCube(cube.Children[z, y, x]);
                }
            }
        }
    }

    private void InitBuffer()
    {
        ResetInstanceBuffer();

        for (int z = 0; z < _context.Collision.OctreeZNodes; z++)
            for (int y = 0; y < _context.Collision.OctreeYNodes; y++)
                for (int x = 0; x < _context.Collision.OctreeXNodes; x++)
                    RenderCube(_context.OctreeRootNodes[z, y, x]);

        _vertexArray.Bind();
        _instanceBuffer.BufferData(_instances, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    public override void Render(ViewportContext context)
    {
        if (!context.TranslucentPass)
            return;

        if (!_initialized)
            Initialize();

        _shader.Use();
        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", context.ViewMatrix);
        _shader.SetMatrix4("projection", context.ProjectionMatrix);
        _shader.SetUint("pickingGroupId", (uint)PickingGroupId);
        _shader.SetUint("hoverId",
            context.HoverObject != null && context.HoverObject.Object is OctreeNodeEx
                ? (uint)context.PickingResult.Index : ~0u);
        _shader.SetVector4("normalColor", new Vector4(1.0f, 1.0f, 1.0f, 0.5f));
        _shader.SetVector4("hoverColor", new Vector4(134 / 255f, 123 / 255f, 90 / 255f, 1));

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(true);
        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);

        _vertexArray.Bind();

        GL.DrawArraysInstanced(PrimitiveType.Lines, 0, CubeLines.Length, _instanceCount);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public override object GetObject(int index) => _cubes[index];

    public void Dispose()
    {
        _shader?.Dispose();
        _vertexBuffer?.Dispose();
        _instanceBuffer?.Dispose();
        _vertexArray?.Dispose();
    }
}