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
    public async Task<MkdsRomProject> CreateAsync(NdsRom rom, string projectName, string outputPath)
    {
        string[] arm9OverlaysPaths = rom.Arm9OverlayTable
            .Select(x => $"overlay9/overlay9_{x.Id}.bin").ToArray();
        string[] arm7OverlaysPaths = rom.Arm7OverlayTable
            .Select(x => $"overlay7/overlay7_{x.Id}.bin").ToArray();

        var romInfo = new NdsRomInfo
        {
            FsRootPath = "root/",
            BannerPath = "banner.bin",
            HeaderPath = "header.bin",
            RsaSignaturePath = "rsasig.bin",
            Arm9Path = "arm9.bin",
            Arm9OvtPath = "arm9ovt.bin",
            Arm9OverlaysPaths = arm9OverlaysPaths,
            Arm7Path = "arm7.bin",
            Arm7OvtPath = "arm7ovt.bin",
            Arm7OverlaysPaths = arm7OverlaysPaths
        };

        var project = new MkdsRomProject
        {
            Name = projectName,
            RomInfo = romInfo
        };

        Directory.CreateDirectory(outputPath);

        await ExtractArchiveAsync(rom.ToArchive(), Path.Combine(outputPath, romInfo.FsRootPath));

        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.BannerPath), rom.Banner.Write());
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.HeaderPath), rom.Header.Write());
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.RsaSignaturePath), rom.RsaSignature);

        await ExtractArm9BinaryAsync(rom, Path.Combine(outputPath, romInfo.Arm9Path));
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.Arm9OvtPath), rom.WriteArm9OverlayTable());
        await ExtractOverlaysAsync(rom, rom.Arm9OverlayTable, outputPath, romInfo.Arm9OverlaysPaths);

        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.Arm7Path), rom.Arm7Binary);
        await File.WriteAllBytesAsync(Path.Combine(outputPath, romInfo.Arm7OvtPath), rom.WriteArm7OverlayTable());
        await ExtractOverlaysAsync(rom, rom.Arm7OverlayTable, outputPath, romInfo.Arm7OverlaysPaths);

        File.WriteAllText(Path.Combine(outputPath, $"{projectName}.json"), JsonConvert.SerializeObject(project, Formatting.Indented));

        return project;
    }

    private async Task ExtractArm9BinaryAsync(NdsRom rom, string outputPath)
    {
        //todo: decompression
        await File.WriteAllBytesAsync(outputPath, rom.Arm9Binary);
    }

    private async Task ExtractOverlaysAsync(NdsRom rom, NdsRomOverlayTable[] table, string outputPath, string[] overlayPaths)
    {
        int i = 0;

        foreach (var entry in table)
        {
            string overlayPath = overlayPaths[i++]; 
            string targetFilePath = Path.Combine(outputPath, overlayPath);

            new FileInfo(targetFilePath).Directory.Create();
            await File.WriteAllBytesAsync(targetFilePath, rom.FileData[entry.Id]);
        }
    }

    private async Task ExtractArchiveAsync(NitroFsArchive archive, string outputPath, string sourcePath = Archive.RootPath)
    {
        var tasks = new List<Task>();
        
        foreach (string fileName in archive.EnumerateFiles(sourcePath, false))
        {
            tasks.Add(ExtractFileAsync(archive, outputPath, sourcePath, fileName));
        }

        foreach (string dir in archive.EnumerateDirectories(sourcePath, false))
        {
            tasks.Add(ExtractArchiveAsync(archive, outputPath, Archive.JoinPath(sourcePath, dir)));
        }

        await Task.WhenAll(tasks);
    }

    private async Task ExtractFileAsync(NitroFsArchive archive, string outputPath, string parentDir, string fileName)
    {
        string sourceFilePath = Archive.JoinPath(parentDir, fileName);
        string targetDir = Path.Combine(outputPath, parentDir.Remove(0, 1));
        string targetFilePath = Path.Combine(targetDir, fileName);

        var data = archive.GetFileData(sourceFilePath);

        if (fileName.EndsWith(".carc"))
        {
            targetFilePath = targetFilePath.Replace(".carc", "_arc");
            var narc = new Narc(Lz77.Decompress(data)).ToArchive();
            await ExtractArchiveAsync(narc, targetFilePath);
        }
        else
        {
            Directory.CreateDirectory(targetDir);
            await File.WriteAllBytesAsync(targetFilePath, data);
        }
    }
}
