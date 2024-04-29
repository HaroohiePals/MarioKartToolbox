using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui.Viewport;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Text;

namespace HaroohiePals.KclViewer.Gui;

internal class KclPrismRenderGroup : RenderGroup, IDisposable
{
    public enum PrismColoringMode
    {
        Solid,
        OctreeHeatMap
    }

    private KclViewerContext _context;
    private GLShader         _shader;
    private bool             _initialized;

    private GLBuffer<PrismVtx> _vertexBuffer;
    private GLVertexArray      _vertexArray;

    private struct PrismVtx
    {
        [GLVertexAttrib(0)]
        public Vector3 Position;

        [GLVertexAttrib(1)]
        public Color4 Color;

        [GLVertexAttribI(2)]
        public uint PickingId;

        [GLVertexAttribI(3)]
        public uint CornerIdx;

        public static readonly int Size = Unsafe.SizeOf<PrismVtx>();
    }

    private PrismVtx[] _vertices    = new PrismVtx[16];
    private int        _vertexCount = 0;

    public Color4 FaceColor = new(47, 82, 96, 255);    //39, 50, 69, 255);
    public Color4 EdgeColor = new(141, 221, 174, 255); //169, 243, 252, 255);

    public Color4 HoverColor    = new(134, 123, 90, 255);
    public Color4 SelectedColor = new(250, 202, 84, 255);

    public PrismColoringMode ColoringMode = PrismColoringMode.Solid;

    public Color4 HeatMapLowColor  = new(0, 255, 0, 255);
    public Color4 HeatMapHighColor = new(255, 0, 0, 255);

    public KclPrismRenderGroup(KclViewerContext context)
    {
        _context = context;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _shader = new GLShader(File.ReadAllText("Files/kclPrism.vert", Encoding.UTF8),
            File.ReadAllText("Files/kclPrism.frag", Encoding.UTF8));

        _vertexArray = new GLVertexArray();
        _vertexArray.Bind();

        _vertexBuffer = new GLBuffer<PrismVtx>();
        _vertexBuffer.Bind(BufferTarget.ArrayBuffer);
        GLUtil.SetupVertexAttribPointers<PrismVtx>();

        GL.BindVertexArray(0);

        _initialized = true;
    }

    private void ResetVertexBuffer()
    {
        _vertexCount = 0;
    }

    private void AddVertex(in PrismVtx vertex)
    {
        if (_vertexCount >= _vertices.Length)
            Array.Resize(ref _vertices, _vertices.Length * 2);

        _vertices[_vertexCount++] = vertex;
    }

    private Color4 GetHeatMapColor(Vector3d point)
    {
        var node = _context.FindNodeForPoint(point);
        if (node == null)
            return FaceColor;

        int   prisms = node.Prisms.Length;
        float ratio  = prisms / (float)(_context.LeafHistogram.Length - 1);

        float r = MathHelper.Lerp(HeatMapLowColor.R, HeatMapHighColor.R, ratio);
        float g = MathHelper.Lerp(HeatMapLowColor.G, HeatMapHighColor.G, ratio);
        float b = MathHelper.Lerp(HeatMapLowColor.B, HeatMapHighColor.B, ratio);

        return new Color4(r, g, b, 1);
    }

    private void UpdateBuffer(ViewportContext context)
    {
        ResetVertexBuffer();

        for (int i = 0; i < _context.Triangles.Length; i++)
        {
            var triangle = _context.Triangles[i];

            Color4 color0, color1, color2;

            switch (ColoringMode)
            {
                case PrismColoringMode.Solid:
                    color0 = FaceColor;
                    color1 = FaceColor;
                    color2 = FaceColor;
                    break;
                case PrismColoringMode.OctreeHeatMap:
                    color0 = GetHeatMapColor(triangle.PointA);
                    color1 = GetHeatMapColor(triangle.PointB);
                    color2 = GetHeatMapColor(triangle.PointC);
                    break;
                default:
                    throw new Exception();
            }

            if (context.IsHovered(_context.Collision.PrismData[i]))
            {
                color0 = HoverColor;
                color1 = HoverColor;
                color2 = HoverColor;
            }
            else if (context.IsSelected(_context.Collision.PrismData[i]))
            {
                color0 = SelectedColor;
                color1 = SelectedColor;
                color2 = SelectedColor;
            }


            uint pickingId = ViewportContext.GetPickingId(PickingGroupId, i);
            AddVertex(new PrismVtx
            {
                Position  = ((float)triangle.PointA.X, (float)triangle.PointA.Y, (float)triangle.PointA.Z),
                Color     = color0,
                PickingId = pickingId,
                CornerIdx = 0
            });
            AddVertex(new PrismVtx
            {
                Position  = ((float)triangle.PointB.X, (float)triangle.PointB.Y, (float)triangle.PointB.Z),
                Color     = color1,
                PickingId = pickingId,
                CornerIdx = 1
            });
            AddVertex(new PrismVtx
            {
                Position  = ((float)triangle.PointC.X, (float)triangle.PointC.Y, (float)triangle.PointC.Z),
                Color     = color2,
                PickingId = pickingId,
                CornerIdx = 2
            });
        }

        _vertexArray.Bind();
        _vertexBuffer.BufferData(_vertices, BufferUsageHint.DynamicDraw);
    }

    public override void Render(ViewportContext context)
    {
        if (context.TranslucentPass)
            return;

        if (!_initialized)
            Initialize();

        UpdateBuffer(context);

        _shader.Use();
        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", context.ViewMatrix);
        _shader.SetMatrix4("projection", context.ProjectionMatrix);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(true);
        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);

        _vertexArray.Bind();

        _shader.SetFloat("wireframeThickness", 0.5f);
        _shader.SetVector4("wireframeColor", (Vector4)EdgeColor);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    public override object GetObject(int index) => _context.Collision.PrismData[index];

    public void Dispose()
    {
        _shader?.Dispose();
        _vertexBuffer?.Dispose();
        _vertexArray?.Dispose();
    }
}