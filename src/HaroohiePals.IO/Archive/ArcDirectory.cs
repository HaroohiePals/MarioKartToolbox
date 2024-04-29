using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.IO.Archive
{
    public class ArcDirectory : ArcEntry
    {
        public ArcDirectory()
            : base("/", null) { }

        public ArcDirectory(string name, ArcDirectory parent)
            : base(name, parent) { }

        private readonly Dictionary<string, ArcDirectory> _directories = new();
        private readonly Dictionary<string, ArcFile>      _files       = new();

        public IReadOnlyCollection<ArcDirectory> Directories => _directories.Values;
        public IReadOnlyCollection<ArcFile>      Files       => _files.Values;

        public bool ContainsDirectory(string name) => _directories.ContainsKey(name);
        public bool ContainsFile(string name) => _files.ContainsKey(name);

        public ArcDirectory GetDirectory(string name) => _directories[name];
        public ArcFile GetFile(string name) => _files[name];

        public ArcFile CreateFile(string name, byte[] data)
        {
            var file = new ArcFile(name, this, data);
            _files.Add(name, file);
            return file;
        }

        public ArcDirectory CreateDirectory(string name)
        {
            var dir = new ArcDirectory(name, this);
            _directories.Add(name, dir);
            return dir;
        }

        public void DeleteFile(string name)
            => _files.Remove(name);

        public void DeleteFile(ArcFile file)
        {
            if (file.Parent == this)
                _files.Remove(file.Name);
        }

        public void DeleteDirectory(string name)
            => _directories.Remove(name);

        public void DeleteDirectory(ArcDirectory directory)
        {
            if (directory.Parent == this)
                _directories.Remove(directory.Name);
        }

        public bool ExistsName(string name)
        {
            return _directories.Keys.Any(k => k.Equals(name, StringComparison.OrdinalIgnoreCase)) ||
                   _files.Keys.Any(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}