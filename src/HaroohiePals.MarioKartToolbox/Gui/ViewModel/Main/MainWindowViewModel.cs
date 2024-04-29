using HaroohiePals.Gui.Themes;
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Application.Discord;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.View.Main;
using HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;
using HaroohiePals.NitroKart.Course;
using ImGuiNET;
using NativeFileDialogs;
using NativeFileDialogs.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.Main;

class MainWindowViewModel
{
    private readonly IModalService _modalService;
    private readonly IMainWindowFactory _windowFactory;

    private readonly IApplicationDiscordRichPresenceService _discordRichPresenceService;
    private readonly IApplicationSettingsService _applicationSettingsService;

    private CourseEditorContentView _courseEditorView;
    private NitroKartRomExplorerContentView _romExplorer;

    public MainWindowViewModel(IModalService modalService, IMainWindowFactory windowFactory,
        IApplicationDiscordRichPresenceService discordRichPresenceService,
        IApplicationSettingsService applicationSettingsService)
    {
        _modalService = modalService;
        _windowFactory = windowFactory;
        _discordRichPresenceService = discordRichPresenceService;
        _applicationSettingsService = applicationSettingsService;
    }

    /// <summary>
    /// Workaround
    /// </summary>
    public Action<WindowContentView> SetMainWindowContent;

    public void ShowPreferences()
    {
        _modalService.ShowModal(_windowFactory.CreatePreferencesModal());
    }

    public void ShowAbout()
    {
        _modalService.ShowModal(_windowFactory.CreateAboutModal());
    }

    public void NewNitroKartCourse()
    {
        var window = _windowFactory.CreateNitroKartCourseProjectModal();
        window.OnProjectCreated = LoadCourseProject;
        _modalService.ShowModal(window);
    }

    public void OpenFile(string fileName)
    {
        var fileInfo = new FileInfo(fileName);
        string ext = fileInfo.Extension.ToLower();

        if (ext == ".nkm")
        {
            LoadBinaryCourseEditor(fileName);
        }
        else if (ext == ".inkm")
        {
            LoadIntermediateCourseEditor(fileName);
        }
        else if (ext == ".nds" || ext == ".xml" || ext == ".nkproj")
        {
            //try
            //{
            //    LoadCourseProject(result.Path);
            //    return;
            //}
            //catch { }

            try
            {
                CloseAllWindows();

                _romExplorer = _windowFactory.CreateNitroKartRomExplorerContentView(fileName);
                _romExplorer.CloseCallback = () => CloseRomExplorer(false);

                _romExplorer.OnNkmOpen += LoadBinaryCourseEditor;
                _romExplorer.OnCarcOpen += ext == ".nds" ? LoadRomCarcCourseEditor : LoadCarcCourseEditor;

                // todo: Open rom explorer through the state machine
                //_modalService.OpenWindow(_romExplorer);
                SetMainWindowContent.Invoke(_romExplorer);

                _discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.RomExplorer);
            }
            catch { }
        }
        else if (ext == ".carc")
        {
            LoadCarcCourseEditor(fileName);
        }
    }

    public void OpenFile()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
        {
            { "All compatible files", "nkm,inkm,carc,nds,nkproj" },
            { "Nitro Kart Map Data", "nkm" },
            { "Intermediate Nitro Kart Map Data", "inkm" },
            { "Compressed Nitro Archive", "carc" },
            { "Nintendo DS ROM File", "nds" },
            { "Nitro Kart Project", "nkproj" }
        });

        if (result == NfdStatus.Ok)
            OpenFile(outPath);
    }

    private void LoadBinaryCourseEditor(string path)
    {
        CloseRomExplorer();

        string basePath = Path.GetDirectoryName(path);
        string courseMapPath = $"/{Path.GetFileName(path)}";

        if (basePath.EndsWith("\\MissionRun", StringComparison.InvariantCultureIgnoreCase) ||
            basePath.EndsWith("/MissionRun", StringComparison.InvariantCultureIgnoreCase))
        {
            basePath = basePath.Replace("\\MissionRun", "", StringComparison.InvariantCultureIgnoreCase);
            basePath = basePath.Replace("/MissionRun", "", StringComparison.InvariantCultureIgnoreCase);
            courseMapPath = $"/MissionRun{courseMapPath}";
        }

        string baseTexPath = basePath.Replace("_arc", "Tex_arc");

        if (!Directory.Exists(baseTexPath))
            baseTexPath = null;

        _courseEditorView = _windowFactory.CreateCourseEditorView(new MkdsFolderCourse(basePath, baseTexPath, courseMapPath));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_courseEditorView);
        SetMainWindowContent.Invoke(_courseEditorView);

        _discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadIntermediateCourseEditor(string path)
    {
        string basePath = Path.GetDirectoryName(path);
        string courseMapPath = $"/{Path.GetFileName(path)}";

        if (basePath.EndsWith("\\MissionRun", StringComparison.InvariantCultureIgnoreCase) ||
            basePath.EndsWith("/MissionRun", StringComparison.InvariantCultureIgnoreCase))
        {
            basePath = basePath.Replace("\\MissionRun", "", StringComparison.InvariantCultureIgnoreCase);
            basePath = basePath.Replace("/MissionRun", "", StringComparison.InvariantCultureIgnoreCase);
            courseMapPath = $"/MissionRun{courseMapPath}";
        }

        _courseEditorView = _windowFactory.CreateCourseEditorView(new MkdsIntermediateCourse(basePath, courseMapPath));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_courseEditorView);
        SetMainWindowContent.Invoke(_courseEditorView);

        _discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadRomCarcCourseEditor(string path)
    {
        CloseRomExplorer();

        string basePath = path;
        string baseTexPath = path.Replace(".carc", "Tex.carc");
        if (!_romExplorer.NitroFsArchive.ExistsFile(baseTexPath))
            baseTexPath = null;

        _courseEditorView = _windowFactory.CreateCourseEditorView(new MkdsRomCarcCourse(_romExplorer.NitroFsArchive, basePath, baseTexPath, "/course_map.nkm"));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_mapDataEditor);
        SetMainWindowContent.Invoke(_courseEditorView);

        _discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadCarcCourseEditor(string path)
    {
        CloseRomExplorer();

        string basePath = path;
        string baseTexPath = Path.Join(Path.GetDirectoryName(path),
            Path.GetFileNameWithoutExtension(basePath) + "Tex.carc");
        if (!File.Exists(baseTexPath))
            baseTexPath = null;

        _courseEditorView = _windowFactory.CreateCourseEditorView(new MkdsCarcCourse(basePath, baseTexPath, "/course_map.nkm"));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_mapDataEditor);
        SetMainWindowContent.Invoke(_courseEditorView);

        _discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadCourseProject(string projectPath)
    {
        //var course = new ProjectCourse(projectPath);

        //Container.FirstByType<MapDataEditor>().Close();

        //var editor = new MapDataEditor(this);
        //editor.LoadCourse(course);

        //Add(editor);
    }

    private void CloseCourseEditor()
    {
        if (_courseEditorView is null)
            return;

        // reopen rom explorer
        SetMainWindowContent.Invoke(_romExplorer ?? null);

        _courseEditorView.Dispose();
        _courseEditorView = null;
    }

    private void CloseRomExplorer(bool keepLoaded = true)
    {
        if (_romExplorer is null)
            return;

        //// Why?
        //foreach (var modal in _modalService.GetAllModals())
        //    modal.Close();

        SetMainWindowContent.Invoke(null);
        if (!keepLoaded)
            _romExplorer = null;
    }

    private void CloseAllWindows()
    {
        CloseRomExplorer(false);
        CloseCourseEditor();

        _discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.Idle);
    }

    public void RestoreDefaultLayout()
    {
        File.WriteAllText("imgui.ini", Resources.Config.ImGuiIni);
        ImGui.LoadIniSettingsFromDisk("imgui.ini");
    }

    public float GetUiScaleSetting() => _applicationSettingsService.Settings.Appearance.UiScale;

    public void LoadTheme() => ImGuiThemeManager.Apply(_applicationSettingsService.Settings.Appearance.Theme);
}