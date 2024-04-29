using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaroohiePals.IO.Archive
{
    public class MemoryArchive : Archive
    {
        public ArcDirectory Root { get; }

        public MemoryArchive(ArcDirectory root)
        {
            Root = root;
        }

        private void FillDirectory(Archive sourceArchive, ArcDirectory directory, string path, bool copyData)
        {
            foreach (string dir in sourceArchive.EnumerateDirectories(path, false))
            {
                var arcDir = directory.CreateDirectory(dir);
                FillDirectory(sourceArchive, arcDir, JoinPath(path, dir), copyData);
            }

            foreach (string file in sourceArchive.EnumerateFiles(path, false))
            {
                var data = sourceArchive.GetFileData(JoinPath(path, file));
                if (copyData)
                {
                    var copy = new byte[data.Length];
                    Array.Copy(data, copy, data.Length);
                    directory.CreateFile(file, copy);
                }
                else
                    directory.CreateFile(file, data);
            }
        }

        public MemoryArchive(Archive sourceArchive, bool copyData = false)
        {
            Root = new ArcDirectory();
            FillDirectory(sourceArchive, Root, RootPath, copyData);
        }

        public override bool IsReadOnly => false;

        private ArcDirectory GetDirectoryByPath(string path)
        {
            path = NormalizePath(path).Trim(PathSeparator);
            var parts = path.Split(PathSeparator);

            if (parts.Length == 0 || parts[0].Length == 0)
                return Root;

            var cur = Root;
            foreach (string part in parts)
            {
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    cur = cur.Parent;
                    if (cur == null)
                        throw new Exception("Invalid path specified");
                    continue;
                }

                if (!cur.ContainsDirectory(part))
                    throw new Exception("Invalid path specified");

                cur = cur.GetDirectory(part);
            }

            return cur;
        }

        private ArcFile GetFileByPath(string path)
        {
            path = path.Trim(PathSeparator);
            int val = path.LastIndexOf(PathSeparator);
            return val >= 0 ? GetDirectoryByPath(path.Substring(0, val)).GetFile(path.Substring(val + 1))
                : Root.GetFile(path);
        }

        public override IEnumerable<string> EnumerateFiles(string path, bool fullPath)
            => fullPath ? GetDirectoryByPath(path).Files.Select(f => f.FullPath)
                : GetDirectoryByPath(path).Files.Select(f => f.Name);

        public override IEnumerable<string> EnumerateDirectories(string path, bool fullPath)
            => fullPath ? GetDirectoryByPath(path).Directories.Select(f => f.FullPath)
                : GetDirectoryByPath(path).Directories.Select(f => f.Name);

        public override void DeleteFile(string path)
        {
            var file = GetFileByPath(path);
            file.Parent.DeleteFile(file);
        }

        public override void DeleteDirectory(string path)
        {
            var dir = GetDirectoryByPath(path);
            if (dir == Root)
                throw new Exception("Can't delete root directory");
            dir.Parent.DeleteDirectory(dir);
        }

        public override bool ExistsFile(string path)
        {
            try
            {
                GetFileByPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool ExistsDirectory(string path)
        {
            try
            {
                GetDirectoryByPath(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void CreateDirectory(string path)
        {
            path = path.Trim(PathSeparator);
            var parts = path.Split(PathSeparator);

            var cur = Root;
            foreach (string part in parts)
            {
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    cur = cur.Parent;
                    if (cur == null)
                        throw new Exception("Invalid path specified");
                    continue;
                }

                if (!cur.ContainsDirectory(part))
                    cur.CreateDirectory(part);

                cur = cur.GetDirectory(part);
            }
        }

        public override byte[] GetFileData(string path)
            => GetFileByPath(path).Data;

        public override Stream OpenFileReadStream(string path)
            => new MemoryStream(GetFileData(path), false);

        public override void SetFileData(string path, byte[] data)
        {
            int    val  = path.Trim(PathSeparator).LastIndexOf(PathSeparator);
            var    dir  = val >= 0 ? GetDirectoryByPath(path.Substring(0, val)) : Root;
            string name = val >= 0 ? path.Substring(val + 1) : path;
            if (dir.ContainsFile(name))
                dir.GetFile(name).Data = data;
            else
                dir.CreateFile(name, data);
        }
    }
}