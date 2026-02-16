using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.Card;
using HaroohiePals.Nitro.Fs;
using HaroohiePals.Nitro.NitroSystem.Fnd;
using HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.Rom;

public class MkdsRomProjectFactory
{
    public async Task<MkdsRomProject> CreateAsync(NdsRom rom, string projectName, string outputPath, bool unpackArc = true)
    {
        string[] arm9OverlaysPaths = rom.Arm9OverlayTable.Entries
            .Select(x => $"overlay9/overlay9_{x.Id}.bin").ToArray();
        string[] arm7OverlaysPaths = rom.Arm7OverlayTable.Entries
            .Select(x => $"overlay7/overlay7_{x.Id}.bin").ToArray();

        var romInfo = new NdsRomInfo
        {
            FsRootPath = "root/",
            BannerPath = "banner.bin",
            HeaderPath = "header.bin",
            RsaSignaturePath = rom.RsaSignature is not null ? "rsasig.bin" : null,
            Arm9Path = "arm9.bin",
            Arm9OvtPath = "arm9ovt.bin",
            Arm9OverlaysPaths = arm9OverlaysPaths,
            Arm7Path = "arm7.bin",
            Arm7OvtPath = "arm7ovt.bin",
            Arm7OverlaysPaths = arm7OverlaysPaths
        };

        uint version = 1;

        var project = new MkdsRomProject
        {
            Name = projectName,
            RomInfo = romInfo,
            Version = version
        };

        Directory.CreateDirectory(outputPath);

        await ExtractArchiveAsync(rom.ToArchive(), Path.Combine(outputPath, romInfo.FsRootPath), unpackArc).ConfigureAwait(false);

        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.BannerPath), rom.Banner).ConfigureAwait(false);
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.HeaderPath), rom.Header.Write()).ConfigureAwait(false);
        if (romInfo.RsaSignaturePath is not null && rom.RsaSignature is not null)
            await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.RsaSignaturePath), rom.RsaSignature).ConfigureAwait(false);
        await ExtractArm9BinaryAsync(rom, Path.Combine(outputPath, romInfo.Arm9Path)).ConfigureAwait(false);
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.Arm9OvtPath), rom.Arm9OverlayTable.Write()).ConfigureAwait(false);
        await ExtractOverlaysAsync(rom, rom.Arm9OverlayTable, outputPath, romInfo.Arm9OverlaysPaths).ConfigureAwait(false);

        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.Arm7Path), rom.Arm7Binary).ConfigureAwait(false);
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.Arm7OvtPath), rom.Arm7OverlayTable.Write()).ConfigureAwait(false);
        await ExtractOverlaysAsync(rom, rom.Arm7OverlayTable, outputPath, romInfo.Arm7OverlaysPaths).ConfigureAwait(false);

        await File.WriteAllTextAsync(Path.Combine(outputPath, $"{projectName}.json"), JsonConvert.SerializeObject(project, Formatting.Indented));

        return project;
    }

    private async Task ExtractArm9BinaryAsync(NdsRom rom, string outputPath)
    {
        await File.WriteAllBytesAsync(outputPath, rom.Arm9Binary);
    }

    private async Task ExtractOverlaysAsync(NdsRom rom, NdsRomOverlayTable table, string outputPath, string[] overlayPaths)
    {
        int i = 0;

        foreach (var entry in table.Entries)
        {
            string overlayPath = overlayPaths[i++]; 
            string targetFilePath = Path.Combine(outputPath, overlayPath);

            new FileInfo(targetFilePath).Directory?.Create();
            await File.WriteAllBytesAsync(targetFilePath, rom.FileData[entry.FileId]).ConfigureAwait(false);
        }
    }

    private async Task ExtractArchiveAsync(NitroFsArchive archive, string outputPath, bool unpackArc, string sourcePath = Archive.RootPath)
    {
        var tasks = new List<Task>();
        
        foreach (string fileName in archive.EnumerateFiles(sourcePath, false))
        {
            tasks.Add(ExtractFileAsync(archive, outputPath, sourcePath, fileName, unpackArc));
        }

        foreach (string dir in archive.EnumerateDirectories(sourcePath, false))
        {
            tasks.Add(ExtractArchiveAsync(archive, outputPath, unpackArc, Archive.JoinPath(sourcePath, dir)));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task ExtractFileAsync(NitroFsArchive archive, string outputPath, string parentDir, string fileName, bool unpackArc)
    {
        string sourceFilePath = Archive.JoinPath(parentDir, fileName);
        string targetDir = Path.Combine(outputPath, parentDir.Remove(0, 1));
        string targetFilePath = Path.Combine(targetDir, fileName);

        byte[] data = archive.GetFileData(sourceFilePath);

        if (unpackArc && fileName.EndsWith(".carc"))
        {
            var compressionAlgo = new Lz77CompressionAlgorithm();

            targetFilePath = targetFilePath.Replace(".carc", "_arc");
            var narc = new Narc(compressionAlgo.Decompress(data)).ToArchive();
            await ExtractArchiveAsync(narc, targetFilePath, false).ConfigureAwait(false);
        }
        else
        {
            Directory.CreateDirectory(targetDir);
            await File.WriteAllBytesAsync(targetFilePath, data).ConfigureAwait(false);
        }
    }
}
