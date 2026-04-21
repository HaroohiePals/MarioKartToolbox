namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class GenerateGlobalMapViewModel
{
    private readonly ICourseEditorContext _courseEditorContext;

    public GenerateGlobalMapViewModel(ICourseEditorContext courseEditorContext)
    {
        _courseEditorContext = courseEditorContext;
    }
}
