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
using System.Threading.Tasks;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.NitroKart.Rom;
using NativeFileDialogs.Net;
using Newtonsoft.Json;

namespace HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;

class NitroKartRomExplorerContentView : WindowContentView
{
    private readonly string _fileName;
    private readonly MkdsRomProject _project = null;
    private readonly LoadingModalView _loadingModal = new("Building ROM... Please wait.");
    private readonly MkdsRomFactory _romFactory = new();

    private ArchiveTreeView _tree;

    internal Action<string> OnNkmOpen;
    internal Action<string> OnCarcOpen;

    public Action CloseCallback;
    public readonly NitroFsArchive NitroFsArchive;

    public override IReadOnlyCollection<MenuItem> MenuItems
    {
        get
        {
            var fileMenu = new MenuItem("File")
            {
                Items =
                [
                    new() { Separator = true },
                    new("Close ROM", CloseCallback)
                ]
            };

            if (_project is not null)
            {
                fileMenu.Items.Insert(0, new("Save As...")
                {
                    Items =
                    [
                        new("Nintendo DS ROM", BuildRom)
                    ]
                });
            }

            return
            [
                fileMenu
            ];
        }
    }

    public NitroKartRomExplorerContentView(string fileName)
    {
        _fileName = fileName;

        var fileInfo = new FileInfo(fileName);
        string ext = fileInfo.Extension.ToLower();

        switch (ext)
        {
            case ".nds":
            case ".srl":
                var rom = new NdsRom(File.ReadAllBytes(fileName));
                NitroFsArchive = rom.ToArchive();

                _tree = new("RomTree", IconConsts.FileExtIcons);

                _tree.Archive = NitroFsArchive;
                _tree.Activate += (view, path, item2) =>
                {
                    if (!path.EndsWith("carc") || path.EndsWith("Tex.carc") ||
                        !path.StartsWith("/data/Course/")) return;

                    Console.WriteLine($"Load course editor: {path}");

                    OnCarcOpen?.Invoke(path);
                };
                break;
            case ".json":
                _project = JsonConvert.DeserializeObject<MkdsRomProject>(File.ReadAllText(fileName));
                InitializeDiskRom(Path.Combine(fileInfo.DirectoryName!, _project.RomInfo.FsRootPath));
                break;
            case ".nkproj":
                InitializeDiskRom(Path.Combine(fileInfo.DirectoryName!, "data"));
                break;
        }
    }

    public override bool Draw()
    {
        if (ImGui.Begin($"Rom Explorer ({_project?.Name ?? _fileName})"))
        {
            ImGui.SetWindowSize(new Vector2(600, 800), ImGuiCond.Once);

            if (ImGui.BeginTabBar("##Tabs_RomExplorer"))
            {
                if (ImGui.BeginTabItem("File System"))
                {
                    _tree?.Draw();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        ImGui.End();

        _loadingModal.Draw();

        return true;
    }

    private void InitializeDiskRom(string romFsBasePath)
    {
        _tree = new("RomTree", IconConsts.FileExtIcons);
        _tree.Archive = new DiskArchive(romFsBasePath);
        _tree.Activate += (view, path, item2) =>
        {
            if (!path.EndsWith("nkm")) return;

            string nkmDiskPath = Path.Combine(romFsBasePath, path.Remove(0, 1));

            Console.WriteLine($"Load course editor: {nkmDiskPath}");

            OnNkmOpen?.Invoke(nkmDiskPath);
        };
    }

    private void BuildRom()
    {
        Task.Factory.StartNew(BuildRomAsync);
    }

    private async Task BuildRomAsync()
    {
        if (_project is null)
            return;
        
        _loadingModal.Open();
        
        var result = Nfd.SaveDialog(out string outPath, new Dictionary<string, string>
        {
            { "Nintendo DS ROM", "nds" }
        }, $"{_project.Name}.nds");

        if (result != NfdStatus.Ok || outPath is null)
        {
            _loadingModal.Close();
            return;
        }

        var fileInfo = new FileInfo(_fileName);
        var rom = await _romFactory.CreateAsync(_project, fileInfo.DirectoryName);
        await File.WriteAllBytesAsync(outPath, rom.Write());

        _loadingModal.Close();
    }
}