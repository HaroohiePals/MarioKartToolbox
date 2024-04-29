using HaroohiePals.IO.Archive;
using System;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.Nitro.Fs;

public class NitroFsArchive : Archive
{
    public DirectoryTableEntry[] DirTable     { get; }
    public NameTableEntry[][]    NameTable    { get; }
    public byte[][]              FileData     { get; }
    public ushort                FileIdOffset { get; }

    public NitroFsArchive(DirectoryTableEntry[] dirTable, NameTableEntry[][] nameTable,
        byte[][] fileData, ushort fileIdOffset = 0)
    {
        DirTable     = dirTable;
        NameTable    = nameTable;
        FileData     = fileData;
        FileIdOffset = fileIdOffset;
    }

    public NitroFsArchive(Archive archive, ushort fileIdOffset = 0)
    {
        if(archive is NitroFsArchive nitroFsArc)
        {
            DirTable     = nitroFsArc.DirTable;
            NameTable    = nitroFsArc.NameTable;
            FileData     = nitroFsArc.FileData;
            FileIdOffset = nitroFsArc.FileIdOffset;
            return;
        }

        var    dirTabEntries  = new List<DirectoryTableEntry>();
        var    nameTabEntries = new List<NameTableEntry[]>();
        var    fileDatas      = new List<byte[]>();
        ushort dirId          = 0xF000;
        ushort fileId         = fileIdOffset;
        var    stack          = new Queue<(ushort id, string path, ushort parentId)>();
        stack.Enqueue((dirId, RootPath, 0));
        dirId++;
        while (stack.Count > 0)
        {
            var (id, path, parentId) = stack.Dequeue();
            var dir = new DirectoryTableEntry
            {
                EntryFileId = fileId,
                ParentId    = parentId
            };
            dirTabEntries.Add(dir);
            var entries = new List<NameTableEntry>();
            foreach (string directory in archive.EnumerateDirectories(path, false))
            {
                entries.Add(NameTableEntry.Directory(directory, dirId));
                stack.Enqueue((dirId, JoinPath(path, directory), id));
                dirId++;
                if (dirId < 0xF000)
                    throw new Exception("Directory id out of range");
            }

            foreach (string file in archive.EnumerateFiles(path, false))
            {
                if (fileId >= 0xF000)
                    throw new Exception("File id out of range");
                entries.Add(NameTableEntry.File(file));
                fileDatas.Add(archive.GetFileData(JoinPath(path, file)));
                fileId++;
            }

            entries.Add(NameTableEntry.EndOfDirectory());

            nameTabEntries.Add(entries.ToArray());
        }

        dirTabEntries[0].ParentId = (ushort)dirTabEntries.Count;

        DirTable     = dirTabEntries.ToArray();
        NameTable    = nameTabEntries.ToArray();
        FileData     = fileDatas.ToArray();
        FileIdOffset = fileIdOffset;
    }

    private int FindDirectory(string path)
    {
        path = NormalizePath(path).Trim(PathSeparator);
        var parts = path.Split(PathSeparator);

        if (parts.Length == 0 || parts[0].Length == 0)
            return 0;

        int partIdx = 0;
        int dir     = 0;
        while (partIdx < parts.Length)
        {
            foreach (var entry in NameTable[dir])
            {
                if (entry.Type == NameTableEntryType.EndOfDirectory)
                    throw new Exception("Invalid path specified");

                if (entry.Type == NameTableEntryType.Directory)
                {
                    if (entry.Name != parts[partIdx])
                        continue;

                    dir = entry.DirectoryId & 0xFFF;

                    if (++partIdx == parts.Length)
                        return dir;

                    break;
                }
            }
        }

        throw new Exception("Invalid path specified");
    }

    public override IEnumerable<string> EnumerateFiles(string path, bool fullPath)
    {
        string normPath = NormalizePath(path);
        int    dir      = FindDirectory(normPath);

        for (int i = 0; i < NameTable[dir].Length; i++)
        {
            var entry = NameTable[dir][i];

            if (entry.Type == NameTableEntryType.EndOfDirectory)
                break;

            if (entry.Type == NameTableEntryType.File)
            {
                if (fullPath)
                    yield return JoinPath(normPath, entry.Name);
                else
                    yield return entry.Name;
            }
        }
    }

    public override IEnumerable<string> EnumerateDirectories(string path, bool fullPath)
    {
        string normPath = NormalizePath(path);
        int    dir      = FindDirectory(normPath);

        for (int i = 0; i < NameTable[dir].Length; i++)
        {
            var entry = NameTable[dir][i];

            if (entry.Type == NameTableEntryType.EndOfDirectory)
                break;

            if (entry.Type == NameTableEntryType.Directory)
            {
                if (fullPath)
                    yield return JoinPath(normPath, entry.Name);
                else
                    yield return entry.Name;
            }
        }
    }

    public override bool ExistsFile(string path)
    {
        if (path.EndsWith(PathSeparator))
            return false;

        string normPath = NormalizePath(path).Trim(PathSeparator);

        int    val     = normPath.LastIndexOf(PathSeparator);
        string dirPath = val < 0 ? RootPath : normPath.Substring(0, val);
        int    dir     = FindDirectory(dirPath);

        string fileName = normPath.Substring(val + 1);

        for (int i = 0; i < NameTable[dir].Length; i++)
        {
            var entry = NameTable[dir][i];

            if (entry.Type == NameTableEntryType.EndOfDirectory)
                break;

            if (entry.Type == NameTableEntryType.File && entry.Name == fileName)
                return true;
        }

        return false;
    }

    public override bool ExistsDirectory(string path)
    {
        try
        {
            FindDirectory(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override byte[] GetFileData(string path)
    {
        if (path.EndsWith(PathSeparator))
            throw new Exception("Invalid path specified");

        string normPath = NormalizePath(path).Trim(PathSeparator);

        int    val     = normPath.LastIndexOf(PathSeparator);
        string dirPath = val < 0 ? RootPath : normPath.Substring(0, val);
        int    dir     = FindDirectory(dirPath);

        string fileName = normPath.Substring(val + 1);

        int fileId = DirTable[dir].EntryFileId;
        for (int i = 0; i < NameTable[dir].Length; i++)
        {
            var entry = NameTable[dir][i];

            if (entry.Type == NameTableEntryType.EndOfDirectory)
                break;

            if (entry.Type == NameTableEntryType.File)
            {
                if (entry.Name == fileName)
                    return FileData[fileId - FileIdOffset];
                fileId++;
            }
        }

        throw new Exception("Invalid path specified");
    }

    public override Stream OpenFileReadStream(string path)
        => new MemoryStream(GetFileData(path), false);
}