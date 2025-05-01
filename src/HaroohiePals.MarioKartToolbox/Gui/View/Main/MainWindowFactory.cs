using System;
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
        => new NitroKartRomExplorerContentView(fileName, _modalService);

    public ModalView CreatePreferencesModal()
        => new PreferencesModalView(_applicationSettings);

    public ModalView CreateAboutModal()
        => new AboutModalView();

    public ModalView CreateRomProjectModal(Action<string> onCreateProject = null)
        => new RomProjectModalView(onCreateProject);

    public NitroKartCourseProjectModalView CreateNitroKartCourseProjectModal()
        => new NitroKartCourseProjectModalView();

    public ModalView CreateObsoleteNkprojWarningModalView()
        => new MessageModalView("Warning",
            "The \"nkproj\" file format is outdated and no longer supported.\n" +
            "To continue working on this project, please first convert it back to a Nintendo DS ROM using\n" +
            "Mario Kart Toolbox 1.x. Then, create a new project in the updated format via:\n" +
            "File > New > Nitro ROM Project\n\n" +
            "You can then rebuild the ROM by first opening the newly created project:\n" +
            "File > Open > Open ROM > Your Project.json\n" +
            "And then:\n" +
            "File > Save As... > Nintendo DS ROM");

    public ModalView CreateNdsRomWarningModalView()
        => new MessageModalView("Warning",
            "Directly editing a ROM file is strongly discouraged as some operations might not be supported.\n\n" +
            "Instead, extract the ROM into a Nitro ROM Project using:\n" +
            "File > New > Nitro ROM Project\n\n" +
            "You can then rebuild the ROM by first opening the newly created project:\n" +
            "File > Open > Open ROM > Your Project.json\n" +
            "And then:\n" +
            "File > Save As... > Nintendo DS ROM");
}