using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Nitro.G3.OpenGL;
using HaroohiePals.Nitro.G3;
using OpenTK.Mathematics;
using System;
using OpenTK.Graphics.OpenGL4;
using HaroohiePals.Nitro.NitroSystem.G3d.OpenGL.Resources;
using HaroohiePals.Graphics;

namespace HaroohiePals.Nitro.NitroSystem.G3d.OpenGL;

public class GLG3dModelRenderer : IDisposable
{
    private readonly GLG3dModelShader _shader;

    private GLGeStateUniformBuffer _uniformBuffer;
    private RenderContext _renderContext;
    private GeometryEngineState _geState;

    public G3dRenderObject RenderObj { get; set; }
    public Vector3d BaseScale { get; set; } = new(16);
    public bool EnableWireframe { get; set; } = false;

    public Matrix4d MultMatrix { get; set; } = Matrix4d.Identity;
    public Vector3d Scale { get; set; } = new(1);

    public readonly Vector3d[] LightVectors = new Vector3d[4]
    {
        (0, -1, 0),
        (-1, 0, 0),
        (0, -1, 0),
        (0, 1, 0)
    };

    public readonly Rgb555[] LightColors = new Rgb555[4]
    {
        new(31, 31, 31),
        new(31, 0, 0),
        new(0, 31, 0),
        new(0, 0, 31)
    };

    public GLG3dModelRenderer()
    {
        _shader = new GLG3dModelShader();
        _uniformBuffer = _shader.GetUniformBuffer();
        _geState = new GeometryEngineState();
        _renderContext = new GLRenderContext(_geState, _uniformBuffer);
    }

    private void PrepareRender(Matrix4 view, Matrix4 projection, uint pickingId)
    {
        _shader.Use();

        _shader.SetModelMatrix(Matrix4.Identity);
        _shader.SetViewMatrix(Matrix4.Identity);
        _shader.SetProjectionMatrix(projection);
        _shader.SetPickingId(pickingId);

        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
                _renderContext.GlobalState.CameraMatrix[i, j] = view[i, j];

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(true);
        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);
        GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(2, BlendEquationMode.FuncAdd);

        _renderContext.GlobalState.LightVectors[0] = LightVectors[0];
        _renderContext.GlobalState.LightVectors[1] = LightVectors[1];
        _renderContext.GlobalState.LightVectors[2] = LightVectors[2];
        _renderContext.GlobalState.LightVectors[3] = LightVectors[3];

        _renderContext.GlobalState.LightColors[0] = LightColors[0];
        _renderContext.GlobalState.LightColors[1] = LightColors[1];
        _renderContext.GlobalState.LightColors[2] = LightColors[2];
        _renderContext.GlobalState.LightColors[3] = LightColors[3];
    }

    public void Render(Matrix4 view, Matrix4 projection, uint pickingId, bool translucentPass)
    {
        if (RenderObj is null)
            return;

        PrepareRender(view, projection, pickingId);

        RenderModel(translucentPass);

        GL.BindVertexArray(0);
        GL.UseProgram(0);
    }

    private void RenderModel(bool translucentPass)
    {
        _geState.TranslucentPass = translucentPass;

        _renderContext.GlobalState.BaseTrans = Vector3d.Zero;
        _renderContext.GlobalState.BaseRot = Matrix3d.Identity;
        _renderContext.GlobalState.BaseScale = BaseScale;

        _renderContext.GlobalState.FlushP(_geState);

        if (EnableWireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        _renderContext.GeState.MultMatrix(MultMatrix);
        _renderContext.GeState.Scale(Scale);

        _renderContext.Sbc.Draw(RenderObj);
        if (EnableWireframe)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }


    public void Dispose()
    {
        _shader?.Dispose();
        _uniformBuffer?.Dispose();
    }
}
