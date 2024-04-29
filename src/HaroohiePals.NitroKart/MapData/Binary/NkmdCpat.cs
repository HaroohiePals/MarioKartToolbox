using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdCpat : NkmdSection<NkmdCpat.CpatEntry>
    {
        public const uint CPATSignature = 0x54415043;

        public NkmdCpat() : base(CPATSignature, true) { }

        public NkmdCpat(EndianBinaryReaderEx er)
            : base(er, CPATSignature, true) { }

        public void UpdateSectionOrder()
        {
            if (Entries.Count == 0)
                return;
            foreach (var entry in Entries)
                entry.SectionOrder = -1;
            int depth = 0;
            var stack = new Stack<(CpatEntry, int)>();
            var visited = new HashSet<CpatEntry>();
            stack.Push((Entries[0], 0));
            while (stack.Count > 0)
            {
                var (entry, entryDepth) = stack.Pop();
                if (visited.Contains(entry))
                    continue;
                entry.SectionOrder = (short)entryDepth;
                foreach (byte next in entry.Next)
                {
                    if (next == 0 || next == 0xFF || next >= Entries.Count)
                        continue;
                    stack.Push((Entries[next], entryDepth + 1));
                }
            }
        }

        public class CpatEntry : NkmdSectionEntry
        {
            public CpatEntry()
            {
                Next = new byte[] { 255, 255, 255 };
                Previous = new byte[] { 255, 255, 255 };
            }

            public CpatEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            public short StartIndex;
            public short Length;

            [ArraySize(3)]
            public byte[] Next; //3

            [ArraySize(3)]
            public byte[] Previous; //3
            public short SectionOrder;
        }
    }
}