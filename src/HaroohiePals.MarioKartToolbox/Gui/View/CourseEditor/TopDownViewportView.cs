using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem;
using ImGuiNET;
using System;
using Vector2 = System.Numerics.Vector2;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class TopDownViewportView : CourseViewportView
{
    private readonly IApplicationSettingsService _applicationSettings;
    private readonly MapDataRenderGroupFactory _renderGroupFactory;

    public TopDownViewportView(ICourseEditorContext context, IApplicationSettingsService applicationSettings,
        string title = "Top-down") : base(title, context)
    {
        _applicationSettings = applicationSettings;
        _renderGroupFactory = new MapDataRenderGroupFactory(_applicationSettings);

        _applicationSettings.ApplicationSettingsChanged += OnApplicationSettingsChanged;

        if (Context.Course.MapData == null)
            return;

        var scene = new NitroKartRenderGroupSceneTopDown();

        var topDownNsbmdGroup = new CourseModelRenderGroup();
        topDownNsbmdGroup.Load(Context.Course);
        scene.RenderGroups.Add(topDownNsbmdGroup);

        if (Context.Course.MapData.MapObjects != null)
        {
            scene.RenderGroups.Add(new MkdsMObjRenderGroup(Context));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.MapObjects, true));
        }

        if (Context.Course.MapData.EnemyPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.EnemyPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.EnemyPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateRadiusRenderGroup(Context.Course.MapData.EnemyPaths));
        }

        if (Context.Course.MapData.MgEnemyPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.MgEnemyPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.MgEnemyPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateRadiusRenderGroup(Context.Course.MapData.MgEnemyPaths));
        }

        if (Context.Course.MapData.ItemPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.ItemPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.ItemPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateRadiusRenderGroup(Context.Course.MapData.ItemPaths));
        }

        if (Context.Course.MapData.Paths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.Paths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.Paths, true));
        }

        if (Context.Course.MapData.StartPoints != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.StartPoints, true));

        if (Context.Course.MapData.RespawnPoints != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.RespawnPoints, true));

        if (Context.Course.MapData.KartPoint2D != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.KartPoint2D, true));

        if (Context.Course.MapData.CannonPoints != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.CannonPoints, true));

        if (Context.Course.MapData.KartPointMission != null)
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.KartPointMission, true));

        if (Context.Course.MapData.Areas != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.Areas, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateRadiusRenderGroup(Context.Course.MapData.Areas, true));
        }

        if (Context.Course.MapData.Cameras != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.Cameras, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateCameraTargetsRenderGroup(Context.Course.MapData.Cameras, true));
        }

        if (Context.Course.MapData.CheckPointPaths != null)
        {
            scene.RenderGroups.Add(
                _renderGroupFactory.CreateLineRenderGroup(Context.Course.MapData.CheckPointPaths, true));
            scene.RenderGroups.Add(
                _renderGroupFactory.CreatePointRenderGroup(Context.Course.MapData.CheckPointPaths, true));
        }

        _scene = scene;
        AddOrUpdateKclPrismRenderGroup();

        _viewportPanel = new InteractiveTopDownViewportPanel(scene, applicationSettings);
        _viewportPanel.Context.SceneObjectHolder = Context.SceneObjectHolder;
        _viewportPanel.Context.ActionStack = Context.ActionStack;

        _viewportPanel.DoubleClick += Context.PerformExpandSelected;
    }

    private void OnApplicationSettingsChanged(object sender, EventArgs e)
    {
        foreach (var renderGroup in _scene.RenderGroups)
        {
            _renderGroupFactory.UpdateRenderGroupColor(renderGroup);
        }
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

    public override void Dispose()
    {
        base.Dispose();
    }
}