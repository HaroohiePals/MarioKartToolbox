using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.Fs;
using System.Collections.Generic;

namespace HaroohiePals.Nitro.NitroSystem.Fnd;

public sealed class NarcFileNameTable
{
    public const uint FntbSignature = 0x464E5442;

    public NarcFileNameTable()
    {
        Signature = FntbSignature;
    }

    public NarcFileNameTable(EndianBinaryReaderEx reader)
    {
        reader.BeginChunk();
        reader.ReadObject(this);
        if (SectionSize > 16)
        {
            var root = new DirectoryTableEntry(reader);
            DirectoryTable    = new DirectoryTableEntry[root.ParentId];
            DirectoryTable[0] = root;
            for (int i = 1; i < root.ParentId; i++)
                DirectoryTable[i] = new DirectoryTableEntry(reader);

            NameTable = new NameTableEntry[root.ParentId][];
            for (int i = 0; i < root.ParentId; i++)
            {
                reader.JumpRelative(DirectoryTable[i].EntryStart + 8);
                var entries = new List<NameTableEntry>();

                NameTableEntry entry;
                do
                {
                    entry = new NameTableEntry(reader);
                    entries.Add(entry);
                } while (entry.Type != NameTableEntryType.EndOfDirectory);

                NameTable[i] = entries.ToArray();
            }
        }

        reader.EndChunk(SectionSize);
    }

    public void Write(EndianBinaryWriterEx writer)
    {
        writer.BeginChunk();
        writer.WriteObject(this);
        if (DirectoryTable != null && DirectoryTable.Length != 0 && NameTable != null)
        {
            DirectoryTable[0].ParentId = (ushort)DirectoryTable.Length;
            writer.BeginChunk();
            {
                long dirTabAddr = writer.BaseStream.Position;
                writer.BaseStream.Position += DirectoryTable.Length * 8;
                for (int i = 0; i < DirectoryTable.Length; i++)
                {
                    DirectoryTable[i].EntryStart = (uint)writer.GetCurposRelative();
                    foreach (var entry in NameTable[i])
                        entry.Write(writer);
                }

                long curPos = writer.BaseStream.Position;
                writer.BaseStream.Position = dirTabAddr;
                foreach (var entry in DirectoryTable)
                    entry.Write(writer);
                writer.BaseStream.Position = curPos;
            }
            writer.EndChunk();
        }

        writer.WritePadding(4, 0xFF);
        writer.EndChunk();
    }

    [Constant(FntbSignature)]
    public uint Signature;

    [ChunkSize]
    public uint SectionSize;

    [Ignore]
    public DirectoryTableEntry[] DirectoryTable;

    [Ignore]
    public NameTableEntry[][] NameTable;
}