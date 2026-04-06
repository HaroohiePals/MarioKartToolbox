#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.IO.Archive;
using HaroohiePals.Nitro.Card;
using HaroohiePals.NitroKart.Rom;
using NativeFileDialogs.Net;
using Newtonsoft.Json;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.RomExplorer;

class RomExplorerViewModel
{
    private readonly IModalService _modalService;
    private readonly string _fileName;
    private readonly string? _romFsBasePath;
    private readonly RomExplorerRomType _romType;
    private readonly MkdsRomProject? _project;
    private readonly LoadingModalView _loadingModal = new("Building ROM... Please wait.");
    private readonly MessageModalView _errorModal = new("Error", "An unexpected error has occured.");
    private readonly MessageModalView _romBuildSuccessModal = new("Success", "Rom built successfully.");
    private readonly MkdsRomFactory _romFactory = new();

    public Action<string>? OnNkmOpen { get; set; }
    public Action<string, Archive>? OnCarcOpen { get; set; }
    public Archive? RomArchive { get; private set; }

    public RomExplorerViewModel(string fileName, IModalService modalService)
    {
        _fileName = fileName;
        _modalService = modalService;

        var fileInfo = new FileInfo(fileName);
        string ext = fileInfo.Extension.ToLower();

        switch (ext)
        {
            case ".nds" or ".srl":
                var rom = new NdsRom(File.ReadAllBytes(fileName));
                RomArchive = rom.ToArchive();
                _romType = RomExplorerRomType.NdsRom;
                break;
            case ".json":
                _project = JsonConvert.DeserializeObject<MkdsRomProject>(File.ReadAllText(fileName));
                if (_project is not null)
                    _romFsBasePath = Path.Combine(fileInfo.DirectoryName!, _project.RomInfo.FsRootPath);
                RomArchive = new DiskArchive(_romFsBasePath);
                _romType = RomExplorerRomType.DiskRom;
                break;
            case ".nkproj":
                _romFsBasePath = Path.Combine(fileInfo.DirectoryName!, "data");
                RomArchive = new DiskArchive(_romFsBasePath);
                _romType = RomExplorerRomType.DiskRom;
                break;
        }
    }

    public string GetTitle()
        => _project?.Name ?? _fileName;

    public bool IsProjectLoaded()
        => _project is not null;

    public void ActivateItem(string path)
    {
        switch (_romType)
        {
            case RomExplorerRomType.NdsRom:
                ActivateNdsRomItem(path);
                break;
            case RomExplorerRomType.DiskRom:
                ActivateDiskRomItem(path);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void BuildRom() => BuildRomAsync().GetAwaiter().GetResult();

    private async Task BuildRomAsync()
    {
        if (_project is null)
            return;

        _modalService.ShowModal(_loadingModal);

        var result = Nfd.SaveDialog(out string? outPath, new Dictionary<string, string>
            {
                { "Nintendo DS ROM", "nds" }
            }, $"{_project.Name}.nds");

        if (result != NfdStatus.Ok || outPath is null)
        {
            _modalService.HideModal(_loadingModal);
            return;
        }

        try
        {
            var fileInfo = new FileInfo(_fileName);
            var rom = await _romFactory.CreateAsync(_project, fileInfo.DirectoryName);
            await File.WriteAllBytesAsync(outPath, rom.Write());
        }
        catch
        {
            _modalService.HideModal(_loadingModal);
            _modalService.ShowModal(_errorModal);
            return;
        }

        _modalService.HideModal(_loadingModal);
        _modalService.ShowModal(_romBuildSuccessModal);
    }

    private void ActivateNdsRomItem(string path)
    {
        if (!path.EndsWith("carc") || path.EndsWith("Tex.carc") ||
            !path.StartsWith("/data/Course/")) return;
        OnCarcOpen?.Invoke(path, RomArchive!);
    }

    private void ActivateDiskRomItem(string path)
    {
        if (!path.EndsWith("nkm"))
            return;
        string nkmDiskPath = Path.Combine(_romFsBasePath!, path.Remove(0, 1));
        OnNkmOpen?.Invoke(nkmDiskPath);
    }
}