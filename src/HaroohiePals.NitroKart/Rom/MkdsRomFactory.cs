using HaroohiePals.IO;
using HaroohiePals.IO.Archive;
using HaroohiePals.IO.Compression;
using HaroohiePals.Nitro.Card;
using HaroohiePals.Nitro.Fs;
using HaroohiePals.Nitro.NitroSystem.Fnd;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.Rom;

public sealed class MkdsRomFactory
{
    private static readonly NdsRomNitroFooter NitroStaticFooter = new NdsRomNitroFooter
    {
        NitroCode = 0xdec00621,
        Unknown = 0x0016f2f0,
        ModuleParamsOffset = 0x00000b4c
    };

    public async Task<NdsRom> CreateAsync(MkdsRomProject project, string workingDirPath)
    {
        var header = await ReadHeaderAsync(Path.Combine(workingDirPath, project.RomInfo.HeaderPath))
            .ConfigureAwait(false);
        var arm9OverlayTable =
            await ReadOverlayTableAsync(Path.Combine(workingDirPath, project.RomInfo.Arm9OvtPath), header.MainOvtSize)
                .ConfigureAwait(false);
        var arm7OverlayTable =
            await ReadOverlayTableAsync(Path.Combine(workingDirPath, project.RomInfo.Arm7OvtPath), header.SubOvtSize)
                .ConfigureAwait(false);

        byte[] banner = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.BannerPath))
            .ConfigureAwait(false);
        byte[] rsaSignature = project.RomInfo.RsaSignaturePath is null
            ? null
            : await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.RsaSignaturePath))
                .ConfigureAwait(false);
        byte[] arm9Binary = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.Arm9Path))
            .ConfigureAwait(false);
        byte[] arm7Binary = await File.ReadAllBytesAsync(Path.Combine(workingDirPath, project.RomInfo.Arm7Path))
            .ConfigureAwait(false);

        var arm9Overlays =
            await ReadOverlayFilesAsync(arm9OverlayTable, project.RomInfo.Arm9OverlaysPaths, workingDirPath,
                project.Version).ConfigureAwait(false);
        var arm7Overlays =
            await ReadOverlayFilesAsync(arm7OverlayTable, project.RomInfo.Arm7OverlaysPaths, workingDirPath,
                project.Version).ConfigureAwait(false);

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

        string fsRoot = Path.Combine(workingDirPath, project.RomInfo.FsRootPath);
        var ignoreMatcher = new RomFileIgnoreMatcher(project.IgnoreFilePatterns);
        rom.FromArchive(await CreateArchiveAsync(fsRoot, ignoreMatcher).ConfigureAwait(false));

        return rom;
    }

    private async Task<NdsRomHeader> ReadHeaderAsync(string path)
    {
        using var m = new MemoryStream(await File.ReadAllBytesAsync(path));
        var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);
        return new NdsRomHeader(er);
    }

    private async Task<NdsRomOverlayTable> ReadOverlayTableAsync(string path, uint overlayTableSize)
    {
        using var m = new MemoryStream(await File.ReadAllBytesAsync(path));
        var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);
        return new NdsRomOverlayTable(er, overlayTableSize);
    }

    private async Task<ReadOverlayFilesResult> ReadOverlayFilesAsync(NdsRomOverlayTable overlayTable,
        string[] overlayPaths, string workingDirPath, uint version)
    {
        var fatEntries = new List<FatEntry>();
        var fileData = new List<byte[]>();

        int currFileId = 0;
        if (version > 0)
        {
            while (currFileId < overlayTable.Length)
            {
                int i = 0;
                foreach (var entry in overlayTable.Entries)
                {
                    string overlayPath = overlayPaths[i++];
                    string targetFilePath = Path.Combine(workingDirPath, overlayPath);

                    if (entry.FileId == currFileId)
                    {
                        fatEntries.Add(new FatEntry(0, 0));
                        fileData.Add(await File.ReadAllBytesAsync(targetFilePath));
                        currFileId++;
                    }
                }
            }
        }
        else
        {
            int i = 0;
            foreach (var entry in overlayTable.Entries)
            {
                string overlayPath = overlayPaths[i++];
                string targetFilePath = Path.Combine(workingDirPath, overlayPath);

                fatEntries.Add(new FatEntry(0, 0));
                fileData.Add(await File.ReadAllBytesAsync(targetFilePath));
            }
        }

        return new ReadOverlayFilesResult(fatEntries, fileData);
    }

    private async Task<Archive> CreateArchiveAsync(string rootPath, RomFileIgnoreMatcher ignoreMatcher)
    {
        var rootDir = new ArcDirectory(Archive.RootPath, null);
        await AddDirectoriesAsync(rootDir, rootPath, rootPath, ignoreMatcher);
        return new MemoryArchive(rootDir);
    }

    private bool IsArchiveDirectory(string path)
        => path.EndsWith("_arc");

    private async Task AddDirectoriesAsync(ArcDirectory dir, string sourcePath, string fsRoot,
        RomFileIgnoreMatcher ignoreMatcher)
    {
        var filesToAdd = await GetFilesToAddAsync(sourcePath, fsRoot, ignoreMatcher);
        foreach (var file in filesToAdd)
        {
            dir.CreateFile(file.Name, file.Data);
        }

        var dirPaths = Directory.EnumerateDirectories(sourcePath, "*", SearchOption.TopDirectoryOnly)
            .Where(x => !IsArchiveDirectory(x))
            .Where(x => !ignoreMatcher.IsIgnored(fsRoot, x));
        
        foreach (var dirPath in dirPaths)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            var newDir = dir.CreateDirectory(dirInfo.Name);
            await AddDirectoriesAsync(newDir, dirPath, fsRoot, ignoreMatcher);
        }
    }

    private async Task<IEnumerable<FileToAddInfo>> GetFilesToAddAsync(string sourcePath, string fsRoot,
        RomFileIgnoreMatcher ignoreMatcher)
    {
        var files = new List<FileToAddInfo>();
        var filePaths = Directory.EnumerateFiles(sourcePath, "*", SearchOption.TopDirectoryOnly);

        foreach (string filePath in filePaths)
        {
            if (ignoreMatcher.IsIgnored(fsRoot, filePath))
                continue;
            var fileInfo = new FileInfo(filePath);
            files.Add(new FileToAddInfo(fileInfo.Name, await File.ReadAllBytesAsync(filePath)));
        }

        var arcDirPaths = Directory.EnumerateDirectories(sourcePath, "*", SearchOption.TopDirectoryOnly)
            .Where(IsArchiveDirectory)
            .Where(x => !ignoreMatcher.IsIgnored(fsRoot, x));

        files.AddRange(await Task.WhenAll(arcDirPaths.Select(arcDirPath =>
            PackDirectoryAsync(arcDirPath, fsRoot, ignoreMatcher))).ConfigureAwait(false));

        return files.OrderBy(x => x.Name);
    }

    private async Task<FileToAddInfo> PackDirectoryAsync(string sourcePath, string fsRoot,
        RomFileIgnoreMatcher ignoreMatcher)
    {
        var dirInfo = new DirectoryInfo(sourcePath);

        string fileName = dirInfo.Name.Replace("_arc", ".carc");

        byte[] carc = null;
        await Task.Run(() =>
        {
            var compressionAlgo = new Lz77CompressionAlgorithm();

            var diskArchive = new DiskArchive(sourcePath);
            var archive = new MemoryArchive(diskArchive);
            RemoveIgnoredEntries(archive, Archive.RootPath, sourcePath, fsRoot, ignoreMatcher);
            var narc = new Narc(archive);
            carc = compressionAlgo.Compress(narc.Write());
        }).ConfigureAwait(false);

        return new FileToAddInfo(fileName, carc);
    }

    private static void RemoveIgnoredEntries(MemoryArchive archive, string archivePath, string diskPath,
        string fsRoot, RomFileIgnoreMatcher ignoreMatcher)
    {
        foreach (string dirName in archive.EnumerateDirectories(archivePath, false).ToList())
        {
            string subArchivePath = Archive.JoinPath(archivePath, dirName);
            string subDiskPath = Path.Combine(diskPath, dirName);
            if (ignoreMatcher.IsIgnored(fsRoot, subDiskPath))
            {
                archive.DeleteDirectory(subArchivePath);
            }
            else
            {
                RemoveIgnoredEntries(archive, subArchivePath, subDiskPath, fsRoot, ignoreMatcher);
            }
        }

        foreach (string fileName in archive.EnumerateFiles(archivePath, false).ToList())
        {
            string fileDiskPath = Path.Combine(diskPath, fileName);
            if (ignoreMatcher.IsIgnored(fsRoot, fileDiskPath))
            {
                archive.DeleteFile(Archive.JoinPath(archivePath, fileName));
            }
        }
    }
}