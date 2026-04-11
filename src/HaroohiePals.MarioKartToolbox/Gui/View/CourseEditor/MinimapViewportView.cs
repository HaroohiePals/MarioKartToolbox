using System.Numerics;
using HaroohiePals.Gui;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.Minimap;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

sealed class MinimapViewportView : CourseViewportView
{
    private const string PANE_TITLE = "Minimap View";

    private readonly MinimapViewportViewModel _viewModel;
    private readonly MapDataRenderGroupFactory _renderGroupFactory;
    private readonly CourseModelRenderGroup _courseModelRenderGroup;
    private readonly LocalMapRenderGroup _localMapRenderGroup;
    private readonly GlobalMapRenderGroup _globalMapRenderGroup;

    public MinimapViewportView(MinimapViewportViewModel viewModel, IApplicationSettingsService applicationSettings)
        : base(PANE_TITLE, viewModel.Context)
    {
        _viewModel = viewModel;
        _renderGroupFactory = new MapDataRenderGroupFactory(applicationSettings);

        var scene = new NitroKartRenderGroupSceneTopDown();
        _courseModelRenderGroup = new CourseModelRenderGroup();
        _courseModelRenderGroup.Load(Context.Course);
        scene.RenderGroups.Add(_courseModelRenderGroup);
        _localMapRenderGroup = new LocalMapRenderGroup(_viewModel.Context.Course);
        _globalMapRenderGroup = new GlobalMapRenderGroup(_viewModel.Context.Course);
        scene.RenderGroups.Add(_localMapRenderGroup);
        scene.RenderGroups.Add(_globalMapRenderGroup);

        _scene = scene;
        _viewportPanel = new MinimapViewportPanel(scene, applicationSettings,
        [
            new($"{FontAwesome6.Globe}", "Show Global Map",
                _viewModel.ToggleGlobalMap, () => !_viewModel.ShowGlobalMap),
            new($"{FontAwesome6.MapLocationDot}", "Show Local Map",
                _viewModel.ToggleGlobalMap, () => _viewModel.ShowGlobalMap),
            new($"{FontAwesome6.CircleHalfStroke}", "Toggle Translucency",
                _viewModel.ToggleTranslucent, IsSelected: () => _viewModel.RenderTranslucent),
            new($"{FontAwesome6.SquareCaretLeft}", "Extend Left",
                _viewModel.ExtendLeft, _viewModel.IsExtendButtonVisible),
            new($"{FontAwesome6.SquareCaretRight}", "Extend Right",
                _viewModel.ExtendRight, _viewModel.IsExtendButtonVisible),
            new($"{FontAwesome6.SquareCaretDown}", "Extend Down",
                _viewModel.ExtendDown, _viewModel.IsExtendButtonVisible),
            new($"{FontAwesome6.SquareCaretUp}", "Extend Up",
                _viewModel.ExtendUp, _viewModel.IsExtendButtonVisible),
        ]);

        _viewportPanel.Context.SceneObjectHolder = Context.SceneObjectHolder;
        _viewportPanel.Context.ActionStack = Context.ActionStack;
    }

    public override bool Draw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        if (ImGui.Begin(_title))
        {
            ImGui.SetWindowSize(new Vector2(600, 400), ImGuiCond.Once);

            _courseModelRenderGroup.EnableCourseModelV = false;
            _viewportPanel.Draw();
        }

        ImGui.End();

        ImGui.PopStyleVar();

        _globalMapRenderGroup.Enabled = _viewModel.ShowGlobalMap;
        _localMapRenderGroup.Enabled = !_viewModel.ShowGlobalMap;
        _globalMapRenderGroup.RenderTranslucent = _viewModel.RenderTranslucent;
        _localMapRenderGroup.RenderTranslucent = _viewModel.RenderTranslucent;

        return true;
    }
}