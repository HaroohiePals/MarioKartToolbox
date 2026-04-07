using System.Numerics;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class MinimapViewportView : CourseViewportView
{
    private const string PANE_TITLE = "Minimap View";

    private readonly MapDataRenderGroupFactory _renderGroupFactory;
    private readonly ICourseEditorContext _context;
    private readonly CourseModelRenderGroup _courseModelRenderGroup;

    public MinimapViewportView(ICourseEditorContext context, IApplicationSettingsService applicationSettings) 
        : base(PANE_TITLE, context)
    {
        _renderGroupFactory = new MapDataRenderGroupFactory(applicationSettings);
        _context = context;
        
        var scene = new NitroKartRenderGroupSceneTopDown();
        _courseModelRenderGroup = new CourseModelRenderGroup();
        _courseModelRenderGroup.Load(Context.Course);
        _courseModelRenderGroup.EnableCourseModelV = false;
        scene.RenderGroups.Add(_courseModelRenderGroup);
        
        _scene = scene;
        _viewportPanel = new MinimapViewportPanel(scene, applicationSettings);
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