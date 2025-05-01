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
using NativeFileDialogs.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.Main;

class MainWindowViewModel(
    IModalService modalService,
    IMainWindowFactory windowFactory,
    IApplicationDiscordRichPresenceService discordRichPresenceService,
    IApplicationSettingsService applicationSettingsService)
{
    private CourseEditorContentView _courseEditorView;
    private NitroKartRomExplorerContentView _romExplorer;

    /// <summary>
    /// Workaround
    /// </summary>
    public Action<WindowContentView> SetMainWindowContent;

    public void ShowPreferences()
        => modalService.ShowModal(windowFactory.CreatePreferencesModal());

    public void ShowAbout()
        => modalService.ShowModal(windowFactory.CreateAboutModal());

    public void ShowRomProjectModal()
        => modalService.ShowModal(windowFactory.CreateRomProjectModal(OpenFile));

    public void OpenFile(string fileName)
    {
        var fileInfo = new FileInfo(fileName);
        string ext = fileInfo.Extension.ToLower();

        switch (ext)
        {
            case ".nkm":
                LoadBinaryCourseEditor(fileName);
                break;
            case ".inkm":
                LoadIntermediateCourseEditor(fileName);
                break;
            case ".carc":
                LoadCarcCourseEditor(fileName);
                break;
            case ".nds" or ".srl":
            case ".nkproj" or ".xml":
            case ".json":
                try
                {
                    CloseAllWindows();

                    switch (ext)
                    {
                        case ".nkproj" or ".xml":
                            modalService.ShowModal(windowFactory.CreateObsoleteNkprojWarningModalView());
                            break;
                        case ".nds" or ".srl":
                            modalService.ShowModal(windowFactory.CreateNdsRomWarningModalView());
                            break;
                    }

                    _romExplorer = windowFactory.CreateNitroKartRomExplorerContentView(fileName);
                    _romExplorer.CloseCallback = () => CloseRomExplorer(false);

                    _romExplorer.OnNkmOpen += LoadBinaryCourseEditor;
                    _romExplorer.OnCarcOpen +=
                        (ext is ".nds" or ".srl") ? LoadRomCarcCourseEditor : LoadCarcCourseEditor;

                    SetMainWindowContent.Invoke(_romExplorer);

                    discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.RomExplorer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Opening ROM: {ex.Message}");
                }
                break;
        }
    }

    public void OpenCourseFile()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
        {
            { "All compatible files", "nkm,carc" },
            { "Nitro Kart Map Data", "nkm" },
            { "Compressed Nitro Archive", "carc" }
        });

        if (result == NfdStatus.Ok)
            OpenFile(outPath);
    }
    
    public void OpenRomProjectFile()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
        {
            { "Nitro ROM Project", "json" },
            { "Nitro Kart Project (Legacy)", "nkproj" }
        });

        if (result == NfdStatus.Ok)
            OpenFile(outPath);
    }
    
    public void OpenRomFile()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
        {
            { "Nintendo DS ROM File", "nds,srl" }
        });

        if (result == NfdStatus.Ok)
            OpenFile(outPath);
    }
    
    private void LoadBinaryCourseEditor(string path)
    {
        CloseRomExplorer();

        string basePath = Path.GetDirectoryName(path)!;
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

        _courseEditorView = windowFactory.CreateCourseEditorView(new MkdsFolderCourse(basePath, baseTexPath, courseMapPath));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_courseEditorView);
        SetMainWindowContent.Invoke(_courseEditorView);

        discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadIntermediateCourseEditor(string path)
    {
        string basePath = Path.GetDirectoryName(path)!;
        string courseMapPath = $"/{Path.GetFileName(path)}";

        if (basePath.EndsWith("\\MissionRun", StringComparison.InvariantCultureIgnoreCase) ||
            basePath.EndsWith("/MissionRun", StringComparison.InvariantCultureIgnoreCase))
        {
            basePath = basePath.Replace("\\MissionRun", "", StringComparison.InvariantCultureIgnoreCase);
            basePath = basePath.Replace("/MissionRun", "", StringComparison.InvariantCultureIgnoreCase);
            courseMapPath = $"/MissionRun{courseMapPath}";
        }

        _courseEditorView = windowFactory.CreateCourseEditorView(new MkdsIntermediateCourse(basePath, courseMapPath));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_courseEditorView);
        SetMainWindowContent.Invoke(_courseEditorView);

        discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadRomCarcCourseEditor(string path)
    {
        CloseRomExplorer();

        string basePath = path;
        string baseTexPath = path.Replace(".carc", "Tex.carc");
        if (!_romExplorer.NitroFsArchive.ExistsFile(baseTexPath))
            baseTexPath = null;

        _courseEditorView = windowFactory.CreateCourseEditorView(new MkdsRomCarcCourse(_romExplorer.NitroFsArchive, basePath, baseTexPath, "/course_map.nkm"));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        // todo: Open course editor through the state machine
        //_modalService.OpenWindow(_mapDataEditor);
        SetMainWindowContent.Invoke(_courseEditorView);

        discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
    }

    private void LoadCarcCourseEditor(string path)
    {
        CloseRomExplorer();

        string basePath = path;
        string baseTexPath = Path.Join(Path.GetDirectoryName(path),
            Path.GetFileNameWithoutExtension(basePath) + "Tex.carc");
        if (!File.Exists(baseTexPath))
            baseTexPath = null;

        _courseEditorView = windowFactory.CreateCourseEditorView(new MkdsCarcCourse(basePath, baseTexPath, "/course_map.nkm"));
        _courseEditorView.CloseCallback += CloseCourseEditor;

        SetMainWindowContent.Invoke(_courseEditorView);

        discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.CourseEditor);
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

        SetMainWindowContent.Invoke(null);
        if (!keepLoaded)
            _romExplorer = null;
    }

    private void CloseAllWindows()
    {
        CloseRomExplorer(false);
        CloseCourseEditor();

        discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.Idle);
    }

    public void RestoreDefaultLayout()
    {
        File.WriteAllText("imgui.ini", Resources.Config.ImGuiIni);
        ImGui.LoadIniSettingsFromDisk("imgui.ini");
    }

    public float GetUiScaleSetting() => applicationSettingsService.Settings.Appearance.UiScale;

    public void LoadTheme() => ImGuiThemeManager.Apply(applicationSettingsService.Settings.Appearance.Theme);
}