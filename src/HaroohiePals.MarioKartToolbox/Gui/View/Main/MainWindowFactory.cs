using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.Course;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

class MainWindowFactory : IMainWindowFactory
{
    private readonly IModalService _modalService;
    private readonly IApplicationSettingsService _applicationSettings;
    private readonly ICourseEditorViewFactory _courseEditorSubWindowFactory;
    private readonly IMapDataClipboard _mapDataClipboard;
    private readonly IMkdsMapObjDatabase _mkdsMapObjDatabase;
    private readonly IMkdsCourseValidatorFactory _mkdsCourseValidatorFactory;

    public MainWindowFactory(IModalService modalService, IApplicationSettingsService applicationSettings,
        ICourseEditorViewFactory courseEditorSubWindowFactory, IMapDataClipboard mapDataClipboard,
        IMkdsMapObjDatabase mkdsMapObjDatabase, IMkdsCourseValidatorFactory mkdsCourseValidatorFactory)
    {
        _modalService = modalService;
        _applicationSettings = applicationSettings;
        _courseEditorSubWindowFactory = courseEditorSubWindowFactory;
        _mapDataClipboard = mapDataClipboard;
        _mkdsMapObjDatabase = mkdsMapObjDatabase;
        _mkdsCourseValidatorFactory = mkdsCourseValidatorFactory;
    }

    public CourseEditorContentView CreateCourseEditorView(IMkdsCourse course)
        => new CourseEditorContentView(new CourseEditorViewModel(course, _modalService, _mkdsCourseValidatorFactory,
            _mkdsMapObjDatabase, _mapDataClipboard),
            _applicationSettings, _modalService, _courseEditorSubWindowFactory);

    public NitroKartRomExplorerContentView CreateNitroKartRomExplorerContentView(string fileName)
        => new NitroKartRomExplorerContentView(fileName);

    public ModalView CreatePreferencesModal()
        => new PreferencesModalView(_applicationSettings);

    public ModalView CreateAboutModal()
        => new AboutModalView();

    public NitroKartCourseProjectModalView CreateNitroKartCourseProjectModal()
        => new NitroKartCourseProjectModalView();
}