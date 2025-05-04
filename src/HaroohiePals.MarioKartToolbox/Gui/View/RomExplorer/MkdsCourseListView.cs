#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using HaroohiePals.Gui.View;
using HaroohiePals.IO.Archive;
using ImGuiNET;

namespace HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;

class MkdsCourseListView(IReadOnlyArchive archive, Action<string> onOpenNkm) : IView
{
    private List<MkdsCourseListViewItem>? _items;
    private bool _showMissions;

    public bool Draw()
    {
        InitializeCourseList();

        ImGui.Checkbox("Show Mission Run", ref _showMissions);

        if (!ImGui.BeginTable($"{nameof(MkdsCourseListView)}##{GetHashCode()}",
                3, ImGuiTableFlags.Resizable | ImGuiTableFlags.NoSavedSettings 
                                             | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY))
            return false;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Course", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("File Name");
        ImGui.TableSetupColumn("Type");
        ImGui.TableHeadersRow();

        var shownItems = _showMissions ? _items ?? [] : _items?.Where(x => x.Type == "Main") ?? [];

        foreach (var item in shownItems)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Selectable($"{item.Name}##{item.Path}", false, ImGuiSelectableFlags.SpanAllColumns);
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                onOpenNkm?.Invoke(item.Path);
            ImGui.TableNextColumn();
            ImGui.Text(item.FileName);
            ImGui.TableNextColumn();
            ImGui.Text(item.Type);
        }

        ImGui.EndTable();
        return true;
    }

    private void InitializeCourseList()
    {
        if (_items is not null)
            return;

        _items = [];
        TraverseFileSystem();

        _items = _items.OrderBy(x => x.Name).ToList();
    }

    private void TraverseFileSystem(string sourcePath = Archive.RootPath)
    {
        foreach (string fileName in archive.EnumerateFiles(sourcePath, false))
        {
            if (!fileName.ToLower().EndsWith("nkm")) 
                continue;
            
            string fullFilePath = Archive.JoinPath(sourcePath, fileName);
            _items?.Add(new MkdsCourseListViewItem(
                GetCourseNameFromPath(sourcePath),
                fullFilePath,
                fileName,
                GetTypeFromPath(sourcePath)));
        }

        foreach (string dir in archive.EnumerateDirectories(sourcePath, false))
            TraverseFileSystem(Archive.JoinPath(sourcePath, dir));
    }

    private string GetCourseNameFromPath(string path)
    {
        path = path.Replace("\\", "/");
        string[] parts = path.Split('/');
        string name = path.Contains("MissionRun") ? parts[^2] : parts[^1];
        return name.Replace("_arc", "");
    }

    private string GetTypeFromPath(string path)
        => path.Contains("MissionRun") ? "Mission Run" : "Main";
}