using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.G3.OpenGL;
using HaroohiePals.Nitro.NitroSystem.G3d;
using HaroohiePals.Nitro.NitroSystem.G3d.OpenGL;
using HaroohiePals.NitroKart;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

internal class MkdsMObjRenderGroup : RenderGroup, IDisposable
{
    private readonly ICourseEditorContext _courseEditorContext;

    private GLG3dModelShader _shader;
    private MkdsContext _mkdsContext;
    private GLG3dModelManager _modelManager;
    private GLGeStateUniformBuffer _uniformBuffer;
    private RenderContext _renderContext;
    private GeometryEngineState _geState;

    private HashSet<MkdsMapObject> _excludeObji = new();
    private bool _shouldCheckErrors = true;
    private int _prevActionStackCount = 0;

    public bool EditMode { get; set; } = true;

    public MkdsMObjRenderGroup(ICourseEditorContext courseEditorContext)
    {
        _courseEditorContext = courseEditorContext;
        _shader = new GLG3dModelShader();
        _uniformBuffer = _shader.GetUniformBuffer();
        _modelManager = new GLG3dModelManager();
        _geState = new GeometryEngineState();
        _renderContext = new GLRenderContext(_geState, _uniformBuffer);
        _mkdsContext = new MkdsContext(_modelManager, _renderContext);
        _mkdsContext.Course = _courseEditorContext.Course;
        CreateMobjState();
    }

    private void UpdateExcludeObji()
    {
        if (_courseEditorContext.ActionStack.UndoActionsCount != _prevActionStackCount)
            _shouldCheckErrors = true;

        _prevActionStackCount = _courseEditorContext.ActionStack.UndoActionsCount;

        if (!_shouldCheckErrors)
            return;

        // Validate the course to check if there are any mobj errors and exclude rendering the ones that generate an error.
        var errors = _courseEditorContext.CourseValidator.Validate(_courseEditorContext.Course);
        _excludeObji = new HashSet<MkdsMapObject>(errors.Select(x => x.Source).OfType<MkdsMapObject>());
        _shouldCheckErrors = false;
    }

    private void CreateMobjState()
    {
        UpdateExcludeObji();
        _mkdsContext.MObjState = new MObjState(_mkdsContext, _excludeObji);
    }

    private double _fraction = 0;

    public override void Update(float deltaTime)
    {
        double frames;
        if (EditMode)
        {
            CreateMobjState();
            frames = 1;
        }
        else
            frames = _fraction + deltaTime * 60.0;

        while (frames > 0)
        {
            _mkdsContext.MObjState.Update();
            frames -= 1.0;
        }

        _fraction = frames;
    }

    public override void Render(ViewportContext context)
    {
        _shader.Use();

        _shader.SetModelMatrix(Matrix4.Identity);
        _shader.SetViewMatrix(Matrix4.Identity);
        _shader.SetProjectionMatrix(context.ProjectionMatrix);
        _shader.SetPickingId(ViewportContext.InvalidPickingId);

        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                _renderContext.GlobalState.CameraMatrix[i, j] = context.ViewMatrix[i, j];

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(true);
        GL.BlendFunc(1, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(1, BlendEquationMode.FuncAdd);
        GL.BlendFunc(2, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.BlendEquation(2, BlendEquationMode.FuncAdd);

        _geState.TranslucentPass = context.TranslucentPass;

        _renderContext.GlobalState.LightVectors[0] = (0, -1, 0);
        _renderContext.GlobalState.LightVectors[1] =
            (-context.ViewMatrix.Column2.Xyz - context.ViewMatrix.Column1.Xyz).Normalized();
        _renderContext.GlobalState.LightVectors[2] = (0, -1, 0);
        _renderContext.GlobalState.LightVectors[3] = (0, 1, 0);

        _renderContext.GlobalState.LightColors[0] = new(31, 31, 31);
        _renderContext.GlobalState.LightColors[1] = new(31, 31, 31);
        _renderContext.GlobalState.LightColors[2] = new(31, 0, 0);
        _renderContext.GlobalState.LightColors[3] = new(31, 31, 0); //this should be white in koopa_course

        _renderContext.GlobalState.BaseTrans = Vector3d.Zero;
        _renderContext.GlobalState.BaseRot = Matrix3d.Identity;
        _renderContext.GlobalState.BaseScale = new(16);

        _renderContext.GlobalState.FlushP(_geState);
        _renderContext.GeState.StoreMatrix(30);

        if (!context.TranslucentPass)
            _mkdsContext.MObjState.BackupPolygonIds();

        var camMtx = new Matrix4x3d();
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 3; j++)
                camMtx[i, j] = context.ViewMatrix[i, j];
        _mkdsContext.MObjState.Render(camMtx);

        if (!context.TranslucentPass)
            _mkdsContext.MObjState.RestorePolygonIds();
    }

    public void Dispose()
    {
        _shader?.Dispose();
        _modelManager?.Dispose();
        _uniformBuffer?.Dispose();
    }
}