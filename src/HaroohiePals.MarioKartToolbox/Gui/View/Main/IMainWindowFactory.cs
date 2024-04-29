using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;
using HaroohiePals.NitroKart.Course;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

interface IMainWindowFactory
{
    CourseEditorContentView CreateCourseEditorView(IMkdsCourse course);
    NitroKartRomExplorerContentView CreateNitroKartRomExplorerContentView(string fileName);
    ModalView CreatePreferencesModal();
    ModalView CreateAboutModal();
    NitroKartCourseProjectModalView CreateNitroKartCourseProjectModal();
}