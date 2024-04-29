using System;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.IO.Archive
{
    public interface IReadOnlyArchive
    {
        IEnumerable<string> EnumerateFiles(string path, bool fullPath);
        IEnumerable<string> EnumerateDirectories(string path, bool fullPath);

        bool ExistsFile(string path);
        bool ExistsDirectory(string path);

        ReadOnlySpan<byte> GetFileDataSpan(string path);
        Stream OpenFileReadStream(string path);
    }
}
