using HaroohiePals.Gui.View;
using HaroohiePals.IO.Archive;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class CourseEditorViewFactory : ICourseEditorViewFactory
{
    private readonly IApplicationSettingsService _applicationSettings;

    public CourseEditorViewFactory(IApplicationSettingsService applicationSettings)
    {
        _applicationSettings = applicationSettings;
    }

    public IView CreateArchiveTreeView(Archive archive)
        => new ArchiveTreeView(new ArchiveTreeViewPaneViewModel(archive));

    public IView CreateMapDataExplorerView(ICourseEditorContext context)
        => new MapDataExplorerView(new MapDataExplorerViewModel(context));

    public PropertyGridPaneView CreatePropertyGridPaneView(ICourseEditorContext context)
        => new PropertyGridPaneView(context);

    public IView CreateTopDownViewportView(ICourseEditorContext context)
        => new TopDownViewportView(context, _applicationSettings);

    public PerspectiveViewportView CreatePerspectiveViewportView(ICourseEditorContext context)
        => new PerspectiveViewportView(context, _applicationSettings);

    public IView CreateCameraPreviewView(ICourseEditorContext context)
        => new CameraPreviewView(context, _applicationSettings);

    public IView CreateValidationPaneView(ICourseEditorContext context)
        => new ValidationPaneView(new ValidationPaneViewModel(context));

    public IView CreateTimelinePaneView(ICourseEditorContext context)
        => new TimelinePaneView(new TimelinePaneViewModel(context));
}