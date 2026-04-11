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

    private readonly MapDataRenderGroupFactory _renderGroupFactory;
    private readonly CourseModelRenderGroup _courseModelRenderGroup;

    public MinimapViewportView(MinimapViewportViewModel viewModel, IApplicationSettingsService applicationSettings)
        : base(PANE_TITLE, viewModel.Context)
    {
        _renderGroupFactory = new MapDataRenderGroupFactory(applicationSettings);

        var scene = new NitroKartRenderGroupSceneTopDown();
        _courseModelRenderGroup = new CourseModelRenderGroup();
        _courseModelRenderGroup.Load(Context.Course);
        scene.RenderGroups.Add(_courseModelRenderGroup);
        scene.RenderGroups.Add(new LocalMapRenderGroup(viewModel.Context.Course));
        //scene.RenderGroups.Add(new GlobalMapRenderGroup(context.Course));

        _scene = scene;
        _viewportPanel = new MinimapViewportPanel(scene, applicationSettings,
        [
            new($"{FontAwesome6.SquareCaretLeft}", "Extend Left", viewModel.ExtendLeft),
            new($"{FontAwesome6.SquareCaretRight}", "Extend Right", viewModel.ExtendRight),
            new($"{FontAwesome6.SquareCaretDown}", "Extend Down", viewModel.ExtendDown),
            new($"{FontAwesome6.SquareCaretUp}", "Extend Up", viewModel.ExtendUp)
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

        return true;
    }
}