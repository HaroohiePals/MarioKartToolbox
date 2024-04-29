using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Linq;
using Vector2 = System.Numerics.Vector2;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class PerspectiveViewportView : CourseViewportView
{
    private readonly IApplicationSettingsService _applicationSettings;
    private readonly MapDataRenderGroupFactory _renderGroupFactory;

    private CourseModelRenderGroup _perspectiveNsbmdGroup;

    public PerspectiveViewportView(ICourseEditorContext context, IApplicationSettingsService applicationSettings,
        string title = "Perspective") : base(title, context)
    {
        _applicationSettings = applicationSettings;
        _renderGroupFactory = new MapDataRenderGroupFactory(_applicationSettings);

        _applicationSettings.ApplicationSettingsChanged += OnApplicationSettingsChanged;

        if (Context.Course.MapData == null)
            return;

        var scene = new NitroKartRenderGroupScenePerspective
        {
            StageInfo = Context.Course.MapData.StageInfo
        };

        if (Context.Course.MapData.MapObjects != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.MapObjects, false));
            scene.RenderGroups.Add(new MkdsMObjRenderGroup(Context));
        }

        if (Context.Course.MapData.EnemyPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.EnemyPaths, false));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.EnemyPaths, false));
        }

        if (Context.Course.MapData.MgEnemyPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.MgEnemyPaths, false));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.MgEnemyPaths, false));
        }

        if (Context.Course.MapData.ItemPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.ItemPaths, false));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.ItemPaths, false));
        }

        if (Context.Course.MapData.Paths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.Paths, false));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.Paths, false));
        }

        if (Context.Course.MapData.StartPoints != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.StartPoints, false));
            scene.RenderGroups.Add(
               _renderGroupFactory.CreateKartPointRenderGroup(Context.Course.MapData, Context.Course.MapData.StartPoints, false));
        }

        if (Context.Course.MapData.RespawnPoints != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.RespawnPoints, false));

        if (Context.Course.MapData.KartPoint2D != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.KartPoint2D, false));

        if (Context.Course.MapData.CannonPoints != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.CannonPoints, false));

        if (Context.Course.MapData.KartPointMission != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.KartPointMission, false));

        if (Context.Course.MapData.Areas != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.Areas, false));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateRadiusRenderGroup(Context.Course.MapData.Areas));
        }

        if (Context.Course.MapData.Cameras != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.Cameras, false));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateCameraTargetsRenderGroup(Context.Course.MapData.Cameras, false));
        }

        scene.RenderGroups.Add(new PlaneGridRenderGroup());

        _perspectiveNsbmdGroup = new CourseModelRenderGroup();
        _perspectiveNsbmdGroup.Load(Context.Course);
        scene.RenderGroups.Add(_perspectiveNsbmdGroup);
        _scene = scene;

        AddOrUpdateKclPrismRenderGroup();


        _viewportPanel = new InteractivePerspectiveViewportPanel(scene, applicationSettings);
        _viewportPanel.Context.SceneObjectHolder = Context.SceneObjectHolder;
        _viewportPanel.Context.ActionStack = Context.ActionStack;

        if (Context.Course.MapData?.StartPoints?.Count > 0)
        {
            var pos = Context.Course.MapData?.StartPoints?[0];
            if (pos != null)
            {
                float distance = -400f;
                float yOffset = 100f;
                var displacement = new Vector3(
                    MathF.Sin(MathHelper.DegreesToRadians((float)pos?.Rotation.Y)) * distance, yOffset,
                    MathF.Cos(MathHelper.DegreesToRadians((float)pos?.Rotation.Y)) * distance);
                var eye = (Vector3)pos?.Position;
                eye += displacement;

                var viewMatrix = Matrix4.LookAt(eye, (Vector3)pos?.Position, Vector3.UnitY);


                _viewportPanel.Context.ViewMatrix = viewMatrix;
            }
        }

        Context.FrameSelection += FrameSelection;
        _viewportPanel.DoubleClick += Context.PerformExpandSelected;

        //Support viewport collision
        _viewportPanel.ViewportCollision = new CourseViewportCollision(Context.Course);
    }

    private void OnApplicationSettingsChanged(object sender, EventArgs e)
    {
        foreach (var renderGroup in _scene.RenderGroups)
        {
            _renderGroupFactory.UpdateRenderGroupColor(renderGroup);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public void ToggleEditMode()
    {
        var perspectiveMObjGroup =
            (MkdsMObjRenderGroup)_scene.RenderGroups.FirstOrDefault(x => x is MkdsMObjRenderGroup);
        perspectiveMObjGroup.EditMode = !perspectiveMObjGroup.EditMode;
    }

    public void FrameSelection()
    {
        _scene?.FrameSelection(_viewportPanel.Context);
    }

    public override bool Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin(_title))
        {
            ImGui.SetWindowSize(new Vector2(600, 400), ImGuiCond.Once);

            _viewportPanel.Draw();

            ImGui.End();
        }

        ImGui.PopStyleVar();

        return true;
    }
}