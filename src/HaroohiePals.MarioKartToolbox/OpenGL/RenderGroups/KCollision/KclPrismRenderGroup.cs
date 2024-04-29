using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.MarioKartToolbox.Resources;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.KCollision;

internal class KclPrismRenderGroup : RenderGroup, IDisposable
{
    public enum PrismColoringMode
    {
        Material,
        Light
    }

    private readonly KclViewerContext _context;
    private readonly MkdsStageInfo _stageInfo;
    private GLShader _shader;
    private bool _initialized;

    private GLBuffer<PrismVtx> _vertexBuffer;
    private GLVertexArray _vertexArray;

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

    private PrismVtx[] _vertices = new PrismVtx[16];
    private int _vertexCount = 0;

    public Color4 FaceColor = new(47, 82, 96, 255);    //39, 50, 69, 255);
    public Color4 EdgeColor = new(141, 221, 174, 255); //169, 243, 252, 255);

    public bool Wireframe = false;
    public bool Seethrough = false;

    public PrismColoringMode ColoringMode = PrismColoringMode.Material;

    public KclPrismRenderGroup(KclViewerContext context, MkdsStageInfo stageInfo)
    {
        _context = context;
        _stageInfo = stageInfo;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _shader = new GLShader(Shaders.KclPrismVertex, Shaders.KclPrismFragment);

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

    //todo
    private Color4 GetColorByKclAttribute(MkdsKclPrismAttribute kclAttribute)
    {
        var color = FaceColor;

        if (ColoringMode == PrismColoringMode.Light)
        {
            return _stageInfo.KclLightColors[(int)kclAttribute.LightId].Color;
        }

        int colType = kclAttribute >> 8 & 0x1F;

        switch (kclAttribute.Type)
        {
            case MkdsCollisionType.Road:
            case MkdsCollisionType.SlipperyRoad:
            case MkdsCollisionType.SlipperyRoad2:
            case MkdsCollisionType.RoadNoDrivers:
                break;
            case MkdsCollisionType.WeakOffRoad:
            case MkdsCollisionType.OffRoad:
            case MkdsCollisionType.SoundTrigger:
            case MkdsCollisionType.HeavyOffRoad:
            case MkdsCollisionType.BoostPad:
            case MkdsCollisionType.Wall:
            case MkdsCollisionType.InvisibleWall:
            case MkdsCollisionType.OutOfBounds:
            case MkdsCollisionType.FallBoundary:
            case MkdsCollisionType.JumpPad:
            case MkdsCollisionType.WallNoDrivers:
            case MkdsCollisionType.CannonActivator:
            case MkdsCollisionType.EdgeWall:
            case MkdsCollisionType.FallsWater:
            case MkdsCollisionType.BoostPadMinSpeed:
            case MkdsCollisionType.Loop:
            case MkdsCollisionType.SpecialRoad:
            case MkdsCollisionType.Wall3:
            case MkdsCollisionType.ForceRecalculateRoute:
                break;
        }


        bool isRoad()
        {
            return colType is 0 or 1 || colType == 6 || colType == 7 || colType == 12 || colType == 13 || colType == 17 || colType == 18 || colType == 20;
        }

        bool isOffroad()
        {
            return colType == 3;
        }

        bool isBoost()
        {
            return colType == 7 || colType == 19;
        }

        bool isWall()
        {
            return colType == 8 /*|| colType == 9*/ || colType == 14 ||
                   colType == 16 || colType == 21;
        }

        bool isFall()
        {
            return colType == 11;
        }

        if (isOffroad())
            color = Color4.DarkGreen;
        if (isRoad())
            color = Color4.Gray;
        if (isBoost())
            color = Color4.Purple;
        if (isWall())
            color = Color4.Yellow;
        if (isFall())
        {
            color = Color4.Blue;
            color.A = 0.5f;
        }

        return color;
    }

    private void UpdateBuffer(ViewportContext context)
    {
        ResetVertexBuffer();

        for (int i = 0; i < _context.Triangles.Length; i++)
        {
            var triangle = _context.Triangles[i];
            var mkdsPrism = _context.PrismData[i];
            var color = GetColorByKclAttribute(mkdsPrism.Attribute);

            if (Seethrough)
                color.A *= 0.5f;

            if (context.IsHovered(mkdsPrism))
                color = new(134, 123, 90, 255); //240, 198, 137, 255);
            else if (context.IsSelected(mkdsPrism))
                color = new(250, 202, 84, 255);
            else if (Wireframe)
            {
                color = EdgeColor;
                color.A = 0;
            }

            uint pickingId = Wireframe ? ViewportContext.InvalidPickingId
                : ViewportContext.GetPickingId(PickingGroupId, i);

            AddVertex(new PrismVtx
            {
                Position = ((float)triangle.PointA.X, (float)triangle.PointA.Y, (float)triangle.PointA.Z),
                Color = color,
                PickingId = pickingId,
                CornerIdx = 0
            });
            AddVertex(new PrismVtx
            {
                Position = ((float)triangle.PointB.X, (float)triangle.PointB.Y, (float)triangle.PointB.Z),
                Color = color,
                PickingId = pickingId,
                CornerIdx = 1
            });
            AddVertex(new PrismVtx
            {
                Position = ((float)triangle.PointC.X, (float)triangle.PointC.Y, (float)triangle.PointC.Z),
                Color = color,
                PickingId = pickingId,
                CornerIdx = 2
            });
        }

        _vertexArray.Bind();
        _vertexBuffer.BufferData(_vertices, BufferUsageHint.DynamicDraw);
    }

    public override void Render(ViewportContext context)
    {
        if (!Seethrough && context.TranslucentPass || Seethrough && !context.TranslucentPass)
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
        GL.Enable(EnableCap.PolygonOffsetLine);
        GL.Enable(EnableCap.PolygonOffsetFill);
        GL.PolygonOffset(-2, 0);
        // if (Wireframe)
        // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        _vertexArray.Bind();

        // if (Wireframe)
        // _shader.SetFloat("wireframeThickness", 0f);
        // else
        // {
        _shader.SetFloat("wireframeThickness", 0.5f);
        _shader.SetVector4("wireframeColor", (Vector4)EdgeColor);
        // }

        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
        GL.PolygonOffset(0, 0);
        // if (Wireframe)
        // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

    public override object GetObject(int index) => _context.PrismData[index];

    public void Dispose()
    {
        _shader?.Dispose();
        _vertexBuffer?.Dispose();
        _vertexArray?.Dispose();
    }
}