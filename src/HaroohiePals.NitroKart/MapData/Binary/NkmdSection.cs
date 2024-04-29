using HaroohiePals.IO;
using System;
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public abstract class NkmdSectionEntry
    {
        public virtual void Write(EndianBinaryWriterEx er)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class NkmdSection<T> where T : NkmdSectionEntry
    {
        public readonly List<T> Entries = new List<T>();
        public T this[int index]
        {
            get => Entries[index];
            set => Entries[index] = value;
        }

        protected uint Signature { get; }
        protected bool DummyIfEmpty { get; }

        public NkmdSection(uint blockSignature, bool dummyIfEmpty)
        {
            Signature = blockSignature;
            DummyIfEmpty = dummyIfEmpty;
        }

        public NkmdSection() { }

        public NkmdSection(EndianBinaryReaderEx er, uint blockSignature, bool dummyIfEmpty)
        {
            Signature = blockSignature;
            DummyIfEmpty = dummyIfEmpty;
            uint signature = er.Read<uint>();
            if (signature != Signature)
                throw new SignatureNotCorrectException(signature, Signature, er.BaseStream.Position - 4);
            uint nrEntries = er.Read<uint>();
            for (int i = 0; i < nrEntries; i++)
                Entries.Add((T)Activator.CreateInstance(typeof(T), er));
        }

        public virtual void Write(EndianBinaryWriterEx er)
        {
            er.Write(Signature);
            er.Write((uint)Entries.Count);
            foreach (var entry in Entries)
                entry.Write(er);

            if (DummyIfEmpty && Entries.Count == 0)
                er.Write(0);
        }
    }
}
