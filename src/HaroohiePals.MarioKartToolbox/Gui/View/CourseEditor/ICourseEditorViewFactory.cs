using HaroohiePals.Gui.View;
using HaroohiePals.IO.Archive;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

interface ICourseEditorViewFactory
{
    IView CreateArchiveTreeView(Archive archive);
    PropertyGridPaneView CreatePropertyGridPaneView(ICourseEditorContext context);
    IView CreateMapDataExplorerView(ICourseEditorContext context);
    IView CreateTopDownViewportView(ICourseEditorContext context);
    PerspectiveViewportView CreatePerspectiveViewportView(ICourseEditorContext context);
    IView CreateCameraPreviewView(ICourseEditorContext context);
    IView CreateValidationPaneView(ICourseEditorContext context);
    IView CreateTimelinePaneView(ICourseEditorContext context);
}