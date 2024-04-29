using System;

namespace HaroohiePals.IO.Archive
{
    public abstract class ArcEntry
    {
        protected ArcEntry(string name, ArcDirectory parent)
        {
            Name   = name;
            Parent = parent;
            if (Parent != null && Parent.ExistsName(Name))
                throw new Exception("Name already exists");
        }

        public string       Name   { get; private set; }
        public ArcDirectory Parent { get; private set; }

        public string FullPath => Parent == null ? "/" : (Parent?.FullPath.TrimEnd('/') + "/" + Name);
    }
}