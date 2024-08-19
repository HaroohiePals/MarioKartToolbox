using HaroohiePals.IO;
using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.Card;
using HaroohiePals.Nitro.Fs;
using HaroohiePals.Nitro.NitroSystem.Fnd;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.Rom;

public class MkdsRomFactory
{
    private static readonly NdsRomNitroFooter NitroStaticFooter = new NdsRomNitroFooter
    {
        NitroCode = 0xdec00621,
        Unknown = 0x0016f2f0,
        ModuleParamsOffset = 0x00000b4c
    };

    public async Task<NdsRom> CreateAsync(MkdsRomProject project, string workingDirPath)
    {
        var header = await ReadHeaderAsync(Path.Combine(workingDirPath, project.RomInfo.HeaderPath));
        var arm9OverlayTable = await ReadOverlayTableAsync(Path.Combine(workingDirPath, project.RomInfo.Arm9OvtPath), header.MainOvtSize);
        var arm7OverlayTable = await ReadOverlayTableAsync(Path.Combine(workingDirPath, project.RomInfo.Arm7OvtPath), header.SubOvtSize);

        byte[] banner = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.BannerPath));
        byte[] rsaSignature = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.RsaSignaturePath));
        byte[] arm9Binary = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.Arm9Path));
        byte[] arm7Binary = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.Arm7Path));

        var arm9Overlays = await ReadOverlayFilesAsync(arm9OverlayTable, project.RomInfo.Arm9OverlaysPaths, workingDirPath);
        var arm7Overlays = await ReadOverlayFilesAsync(arm7OverlayTable, project.RomInfo.Arm7OverlaysPaths, workingDirPath);

        var fatEntries = arm9Overlays.FatEntries.Concat(arm7Overlays.FatEntries).ToArray();
        var fileData = arm9Overlays.FileData.Concat(arm7Overlays.FileData).ToArray();

        var rom = new NdsRom
        {
            Header = header,
            StaticFooter = NitroStaticFooter,
            Banner = banner,
            RsaSignature = rsaSignature,

            Arm9Binary = arm9Binary,
            Arm9OverlayTable = arm9OverlayTable,
            Arm7Binary = arm7Binary,
            Arm7OverlayTable = arm7OverlayTable,

            Fnt = new NdsRomFileNameTable(),
            Fat = fatEntries,
            FileData = fileData
        };

        //string romFilePath = @"testfiles/rom.nds";
        //var testRom = new NdsRom(File.ReadAllBytes(romFilePath));
        //rom.FromArchive(testRom.ToArchive());

        rom.FromArchive(await CreateArchiveAsync(Path.Combine(workingDirPath, project.RomInfo.FsRootPath)));

        return rom;
    }

    private async Task<NdsRomHeader> ReadHeaderAsync(string path)
    {
        using (var m = new MemoryStream(await File.ReadAllBytesAsync(path)))
        {
            var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);
            return new NdsRomHeader(er);
        }
    }

    private async Task<NdsRomOverlayTable> ReadOverlayTableAsync(string path, uint overlayTableSize)
    {
        using (var m = new MemoryStream(await File.ReadAllBytesAsync(path)))
        {
            var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);
            return new NdsRomOverlayTable(er, overlayTableSize);
        }
    }

    private async Task<ReadOverlayFilesResult> ReadOverlayFilesAsync(NdsRomOverlayTable overlayTable, string[] overlayPaths, string workingDirPath)
    {
        var fatEntries = new List<FatEntry>();
        var fileData = new List<byte[]>();

        int i = 0;

        foreach (var vv in overlayTable.Entries)
        {
            string overlayPath = overlayPaths[i++];
            string targetFilePath = Path.Combine(workingDirPath, overlayPath);

            fatEntries.Add(new FatEntry(0, 0));
            fileData.Add(await File.ReadAllBytesAsync(targetFilePath));
        }

        return new ReadOverlayFilesResult(fatEntries, fileData);
    }

    private async Task<Archive> CreateArchiveAsync(string rootPath)
    {
        var rootDir = new ArcDirectory("/", null);
        await AddDirectoriesAsync(rootDir, rootPath);
        return new MemoryArchive(rootDir);
    }

    private bool IsArchiveDirectory(string path)
        => path.EndsWith("_arc");

    private async Task AddDirectoriesAsync(ArcDirectory dir, string sourcePath)
    {
        var filesToAdd = await GetFilesToAddAsync(sourcePath);
        foreach (var file in filesToAdd)
        {
            dir.CreateFile(file.Name, file.Data);
        }

        var dirPaths = Directory.EnumerateDirectories(sourcePath, "*", SearchOption.TopDirectoryOnly)
            .Where(x => !IsArchiveDirectory(x));
        foreach (var dirPath in dirPaths)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            var newDir = dir.CreateDirectory(dirInfo.Name);
            await AddDirectoriesAsync(newDir, dirPath);
        }
    }

    private async Task<IEnumerable<FileToAddInfo>> GetFilesToAddAsync(string sourcePath)
    {
        var files = new List<FileToAddInfo>();
        var filePaths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.TopDirectoryOnly);

        foreach (var filePath in filePaths)
        {
            var fileInfo = new FileInfo(filePath);
            files.Add(new FileToAddInfo(fileInfo.Name, await File.ReadAllBytesAsync(filePath)));
        }

        var arcDirPaths = Directory.EnumerateDirectories(sourcePath, "*", SearchOption.TopDirectoryOnly).Where(IsArchiveDirectory);
        
        //files.AddRange(await Task.WhenAll(arcDirPaths.Select(PackDirectoryAsync)));
        foreach (var arcDirPath in arcDirPaths)
        {
            files.Add(await PackDirectoryAsync(arcDirPath));
        }

        return files.OrderBy(x => x.Name);
    }

    private async Task<FileToAddInfo> PackDirectoryAsync(string sourcePath)
    {
        var dirInfo = new DirectoryInfo(sourcePath);
        
        string fileName = dirInfo.Name.Replace("_arc", ".carc");

        var archive = await CreateArchiveAsync(sourcePath);
        var narc = new Narc(archive);

        return new FileToAddInfo(fileName, Lz77.Compress(narc.Write()));
    }
}
