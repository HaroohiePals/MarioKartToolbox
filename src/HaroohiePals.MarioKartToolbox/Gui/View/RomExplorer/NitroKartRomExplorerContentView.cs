using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Menu;
using HaroohiePals.IO.Archive;
using HaroohiePals.Nitro.Card;
using HaroohiePals.Nitro.Fs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;

class NitroKartRomExplorerContentView : WindowContentView
{
    public readonly string FileName;
    private readonly NdsRom _rom;
    public readonly NitroFsArchive NitroFsArchive;

    private ArchiveTreeView _tree;

    internal Action<string> OnNkmOpen;
    internal Action<string> OnCarcOpen;

    public Action CloseCallback;

    public override IReadOnlyCollection<MenuItem> MenuItems =>
    [
        new("File")
        {
            Items = new()
            {
                new() { Separator = true },
                new("Close ROM", CloseCallback)
            }
        }
    ];

    public NitroKartRomExplorerContentView(string fileName)
    {
        FileName = fileName;

        if (fileName.ToLower().EndsWith(".nds"))
        {
            _rom = new NdsRom(File.ReadAllBytes(fileName));
            NitroFsArchive = _rom.ToArchive();

            _tree = new("RomTree", IconConsts.FileExtIcons);

            _tree.Archive = NitroFsArchive;
            _tree.Activate += (view, path, item2) =>
            {
                if (path.EndsWith("carc") && !path.EndsWith("Tex.carc") && path.StartsWith("/data/Course/"))
                {
                    Console.WriteLine($"Load course editor: {path}");

                    OnCarcOpen?.Invoke(path);
                }
            };
        }
        else
        {
            var fileInfo = new FileInfo(fileName);
            var romFsBasePath = Path.Combine(fileInfo.DirectoryName, "data");

            _tree = new("RomTree", IconConsts.FileExtIcons);
            _tree.Archive = new DiskArchive(romFsBasePath);
            _tree.Activate += (view, path, item2) =>
            {
                if (path.EndsWith("nkm"))
                {
                    var nkmDiskPath = Path.Combine(romFsBasePath, path.Remove(0, 1));

                    Console.WriteLine($"Load course editor: {nkmDiskPath}");

                    OnNkmOpen?.Invoke(nkmDiskPath);
                }
            };
        }
    }

    public override bool Draw()
    {
        if (ImGui.Begin("Rom Explorer"))
        {
            ImGui.SetWindowSize(new Vector2(600, 800), ImGuiCond.Once);

            if (ImGui.BeginTabBar("##Tabs_RomExplorer"))
            {
                if (ImGui.BeginTabItem("File System"))
                {
                    _tree?.Draw();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Courses"))
                {
                    if (ImGui.CollapsingHeader("Mushroom Cup")) { }

                    if (ImGui.CollapsingHeader("Flower Cup")) { }

                    if (ImGui.CollapsingHeader("Star Cup")) { }

                    if (ImGui.CollapsingHeader("Special Cup")) { }

                    if (ImGui.CollapsingHeader("Shell Cup")) { }

                    if (ImGui.CollapsingHeader("Banana Cup")) { }

                    if (ImGui.CollapsingHeader("Leaf Cup")) { }

                    if (ImGui.CollapsingHeader("Lightning Cup")) { }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Missions"))
                {
                    ImGui.EndTabItem();
                }


                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        return true;
    }
}