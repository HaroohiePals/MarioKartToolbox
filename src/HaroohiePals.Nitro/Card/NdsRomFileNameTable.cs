using HaroohiePals.IO;
using HaroohiePals.Nitro.Fs;
using System.Collections.Generic;

namespace HaroohiePals.Nitro.Card;

public class NdsRomFileNameTable
{
    public NdsRomFileNameTable()
    {
        DirectoryTable = new[] { new DirectoryTableEntry { ParentId = 1 } };
        NameTable = new[] { new[] { NameTableEntry.EndOfDirectory() } };
    }

    public NdsRomFileNameTable(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        {
            var root = new DirectoryTableEntry(er);
            DirectoryTable = new DirectoryTableEntry[root.ParentId];
            DirectoryTable[0] = root;
            for (int i = 1; i < root.ParentId; i++)
                DirectoryTable[i] = new DirectoryTableEntry(er);

            NameTable = new NameTableEntry[root.ParentId][];
            for (int i = 0; i < root.ParentId; i++)
            {
                er.JumpRelative(DirectoryTable[i].EntryStart);
                var entries = new List<NameTableEntry>();

                NameTableEntry entry;
                do
                {
                    entry = new NameTableEntry(er);
                    entries.Add(entry);
                } while (entry.Type != NameTableEntryType.EndOfDirectory);

                NameTable[i] = entries.ToArray();
            }
        }
        er.EndChunk();
    }

    public void Write(EndianBinaryWriterEx er)
    {
        DirectoryTable[0].ParentId = (ushort)DirectoryTable.Length;
        er.BeginChunk();
        {
            long dirTabAddr = er.BaseStream.Position;
            er.BaseStream.Position += DirectoryTable.Length * 8;
            for (int i = 0; i < DirectoryTable.Length; i++)
            {
                DirectoryTable[i].EntryStart = (uint)er.GetCurposRelative();
                foreach (var entry in NameTable[i])
                    entry.Write(er);
            }

            long curPos = er.BaseStream.Position;
            er.BaseStream.Position = dirTabAddr;
            foreach (var entry in DirectoryTable)
                entry.Write(er);
            er.BaseStream.Position = curPos;
        }
        er.EndChunk();
    }

    public DirectoryTableEntry[] DirectoryTable;
    public NameTableEntry[][] NameTable;
}
