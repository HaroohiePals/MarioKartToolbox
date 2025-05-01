using System;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.RomExplorer;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.Course;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

class MainWindowFactory(
    IModalService modalService,
    IApplicationSettingsService applicationSettings,
    ICourseEditorViewFactory courseEditorSubWindowFactory,
    IMapDataClipboard mapDataClipboard,
    IMkdsMapObjDatabase mkdsMapObjDatabase,
    IMkdsCourseValidatorFactory mkdsCourseValidatorFactory)
    : IMainWindowFactory
{
    public CourseEditorContentView CreateCourseEditorView(IMkdsCourse course)
        => new CourseEditorContentView(new CourseEditorViewModel(course, modalService, mkdsCourseValidatorFactory,
            mkdsMapObjDatabase, mapDataClipboard),
            applicationSettings, modalService, courseEditorSubWindowFactory);

    public RomExplorerContentView CreateRomExplorerContentView(string fileName)
        => new RomExplorerContentView(new RomExplorerViewModel(fileName, modalService));

    public ModalView CreatePreferencesModal()
        => new PreferencesModalView(applicationSettings);

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
            "File > Open > Open Project > Your Project.json\n" +
            "And then:\n" +
            "File > Export > Nintendo DS ROM");

    public ModalView CreateNdsRomWarningModalView()
        => new MessageModalView("Warning",
            "Directly editing a ROM file is strongly discouraged as some operations might not be supported.\n\n" +
            "Instead, extract the ROM into a Nitro ROM Project using:\n" +
            "File > New > Nitro ROM Project\n\n" +
            "You can then rebuild the ROM by first opening the newly created project:\n" +
            "File > Open > Open Project > Your Project.json\n" +
            "And then:\n" +
            "File > Export > Nintendo DS ROM");
}