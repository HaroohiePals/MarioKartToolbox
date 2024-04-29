using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaroohiePals.IO.Archive
{
    public abstract class Archive : IReadOnlyArchive
    {
        public const char   PathSeparator = '/';
        public const string RootPath      = "/";

        public virtual bool IsReadOnly => true;

        public abstract IEnumerable<string> EnumerateFiles(string path, bool fullPath);
        public abstract IEnumerable<string> EnumerateDirectories(string path, bool fullPath);

        public virtual void DeleteFile(string path) => throw new NotSupportedException();
        public virtual void DeleteDirectory(string path) => throw new NotSupportedException();

        public abstract bool ExistsFile(string path);
        public abstract bool ExistsDirectory(string path);

        public virtual void CreateDirectory(string path) => throw new NotSupportedException();

        public abstract byte[] GetFileData(string path);

        public ReadOnlySpan<byte> GetFileDataSpan(string path)
            => GetFileData(path);

        public abstract Stream OpenFileReadStream(string path);
        public virtual void SetFileData(string path, byte[] data) => throw new NotSupportedException();

        public static string JoinPath(params string[] parts)
            => JoinPath((IEnumerable<string>)parts);

        public static string JoinPath(IEnumerable<string> parts)
            => PathSeparator + string.Join(PathSeparator, parts.Select(p => p.Trim(PathSeparator)).Where(p => p != ""));

        public static string NormalizePath(string path)
        {
            path = path.Trim(PathSeparator);
            var parts = path.Split(PathSeparator);

            var newPath = new Stack<string>();
            foreach (string part in parts)
            {
                if (part == ".")
                    continue;

                if (part == "..")
                {
                    if (newPath.Count == 0)
                        throw new Exception("Invalid path specified");
                    newPath.Pop();
                    continue;
                }

                newPath.Push(part);
            }

            return JoinPath(newPath.Reverse());
        }

        public static bool PathEqual(string path1, string path2)
            => NormalizePath(path1) == NormalizePath(path2);
    }
}