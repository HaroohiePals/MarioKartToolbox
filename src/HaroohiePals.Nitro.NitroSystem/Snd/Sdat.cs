using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Sdat
    {
        public Sdat(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Sdat(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new SdatHeader(er);
                if (Header.SymbOffset != 0 && Header.SymbLength != 0)
                {
                    er.BaseStream.Position = Header.SymbOffset;
                    SymbolBlock            = new Symb(er);
                }

                er.BaseStream.Position = Header.InfoOffset;
                InfoBlock              = new Info(er);
                er.BaseStream.Position = Header.FatOffset;
                FileAllocationTable    = new Fat(er);
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk(8);
            {
                Header.HeaderSize = 0x40;
                Header.SymbOffset = 0;
                Header.SymbLength = 0;
                Header.InfoOffset = 0;
                Header.InfoLength = 0;
                Header.FatOffset  = 0;
                Header.FatLength  = 0;
                Header.FileOffset = 0;
                Header.FileLength = 0;
                Header.NrBlocks   = (ushort)(SymbolBlock != null ? 4 : 3);
                Header.Write(er);
                long curpos, curpos2;
                if (SymbolBlock != null)
                {
                    er.WriteCurposRelative(0x10);
                    curpos  = er.BaseStream.Position;
                    curpos2 = SymbolBlock.Write(er);
                    long curpos3 = er.BaseStream.Position;
                    er.BaseStream.Position = 0x14;
                    er.Write((uint)(curpos2 - curpos));
                    er.BaseStream.Position = curpos3;
                }

                er.WriteCurposRelative(0x18);
                curpos = er.BaseStream.Position;
                InfoBlock.Write(er);
                curpos2                = er.BaseStream.Position;
                er.BaseStream.Position = 0x1C;
                er.Write((uint)(curpos2 - curpos));
                er.BaseStream.Position = curpos2;

                er.WriteCurposRelative(0x20);
                curpos = er.BaseStream.Position;
                FileAllocationTable.Write(er);
                curpos2                = er.BaseStream.Position;
                er.BaseStream.Position = 0x24;
                er.Write((uint)(curpos2 - curpos));
                er.BaseStream.Position = curpos2;

                er.WriteCurposRelative(0x28);
                curpos = er.BaseStream.Position;
                er.Write("FILE", Encoding.ASCII, false);
                er.Write((uint)(er.BaseStream.Length - curpos));
                er.Write((uint)FileAllocationTable.Entries.Count);
                er.BaseStream.Position = 0x2C;
                er.Write((uint)(er.BaseStream.Length - curpos));
                er.BaseStream.Position = er.BaseStream.Length;
            }
            er.EndChunk();
            er.FlushReferences();
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public SdatHeader Header;

        public class SdatHeader
        {
            public const uint SdatSignature = 0x54414453;

            public SdatHeader(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
                er.Read<byte>(16);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.WriteObject(this);
                er.Write(new byte[16], 0, 16);
            }

            [Constant(SdatSignature)]
            public uint Signature;

            [Constant((ushort)0xFEFF)]
            public ushort Endianness;

            public ushort Version;
            public uint   FileSize;
            public ushort HeaderSize;
            public ushort NrBlocks;
            public uint   SymbOffset;
            public uint   SymbLength;
            public uint   InfoOffset;
            public uint   InfoLength;
            public uint   FatOffset;
            public uint   FatLength;
            public uint   FileOffset;
            public uint   FileLength;
        }

        public Symb SymbolBlock;

        public class Symb
        {
            public const uint SymbSignature = 0x424D5953;

            public Symb(EndianBinaryReaderEx er)
            {
                er.BeginChunk();
                er.ReadObject(this);
                er.Read<byte>(24);
                er.EndChunk(SectionSize);
            }

            public long Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk(4);
                er.Write(SymbSignature);
                er.Write((uint)0);
                er.Write(new uint[8], 0, 8);
                er.Write(new byte[24], 0, 24);
                er.WriteCurposRelative(8 + 0 * 4);
                long seq = SequenceSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 1 * 4);
                var (seqArc, seqArcSub) = SequenceArchiveSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 2 * 4);
                long bank = BankSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 3 * 4);
                long wavArc = WaveArchiveSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 4 * 4);
                long player = PlayerSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 5 * 4);
                long group = GroupSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 6 * 4);
                long strmPlayer = StreamPlayerSymbols.WritePointers(er);
                er.WriteCurposRelative(8 + 7 * 4);
                long strm = StreamSymbols.WritePointers(er);

                SequenceSymbols.WriteStrings(er, seq);
                SequenceArchiveSymbols.WriteStrings(er, seqArc, seqArcSub);
                BankSymbols.WriteStrings(er, bank);
                WaveArchiveSymbols.WriteStrings(er, wavArc);
                PlayerSymbols.WriteStrings(er, player);
                GroupSymbols.WriteStrings(er, group);
                StreamPlayerSymbols.WriteStrings(er, strmPlayer);
                StreamSymbols.WriteStrings(er, strm);
                long endNoPad = er.BaseStream.Position;
                er.WritePadding(4);
                er.EndChunk();
                return endNoPad;
            }

            [Constant(SymbSignature)]
            public uint Signature;

            [ChunkSize]
            public uint SectionSize;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord SequenceSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public ArchiveSymbolRecord SequenceArchiveSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord BankSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord WaveArchiveSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord PlayerSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord GroupSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord StreamPlayerSymbols;

            [Reference(ReferenceType.ChunkRelative)]
            public SymbolRecord StreamSymbols;

            public class SymbolRecord
            {
                public SymbolRecord(EndianBinaryReaderEx er)
                {
                    NrEntries    = er.Read<uint>();
                    EntryOffsets = er.Read<uint>((int)NrEntries);
                    long curpos = er.BaseStream.Position;
                    Entries = new List<string>();
                    for (int i = 0; i < NrEntries; i++)
                    {
                        if (EntryOffsets[i] != 0)
                        {
                            er.JumpRelative(EntryOffsets[i]);
                            Entries.Add(er.ReadStringNT(Encoding.ASCII));
                        }
                        else Entries.Add(null);
                    }

                    er.BaseStream.Position = curpos;
                }

                public long WritePointers(EndianBinaryWriterEx er)
                {
                    er.Write((uint)Entries.Count);
                    long offspos = er.GetCurposRelative();
                    er.Write(new uint[Entries.Count], 0, Entries.Count);
                    return offspos;
                }

                public void WriteStrings(EndianBinaryWriterEx er, long offspos)
                {
                    for (int i = 0; i < Entries.Count; i++)
                    {
                        if (Entries[i] == null) 
                            continue;

                        er.WriteCurposRelative((int)(offspos + i * 4));
                        er.Write(Entries[i], Encoding.ASCII, true);
                    }
                }

                public uint   NrEntries;
                public uint[] EntryOffsets;

                public List<string> Entries;
            }

            public class ArchiveSymbolRecord
            {
                public ArchiveSymbolRecord(EndianBinaryReaderEx er)
                {
                    er.ReadObject(this);
                }

                public (long, long[]) WritePointers(EndianBinaryWriterEx er)
                {
                    er.Write((uint)Entries.Length);
                    long pos    = er.GetCurposRelative();
                    var  posSub = new long[Entries.Length];
                    for (int i = 0; i < Entries.Length; i++)
                        Entries[i].Write(er);
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i].ArchiveSubRecord == null)
                            continue;

                        er.WriteCurposRelative((int)(pos + i * 8 + 4));
                        posSub[i] = Entries[i].ArchiveSubRecord.WritePointers(er);
                    }

                    return (pos, posSub);
                }

                public void WriteStrings(EndianBinaryWriterEx er, long pos, long[] posSub)
                {
                    for (int i = 0; i < Entries.Length; i++)
                    {
                        if (Entries[i].ArchiveName != null)
                        {
                            er.WriteCurposRelative((int)(pos + i * 8));
                            er.Write(Entries[i].ArchiveName, Encoding.ASCII, true);
                        }

                        if (Entries[i].ArchiveSubRecord != null)
                            Entries[i].ArchiveSubRecord.WriteStrings(er, posSub[i]);
                    }
                }

                public uint NrEntries;

                [ArraySize(nameof(NrEntries))]
                public ArchiveSymbolRecordEntry[] Entries;

                public class ArchiveSymbolRecordEntry
                {
                    public ArchiveSymbolRecordEntry(EndianBinaryReaderEx er)
                    {
                        ArchiveNameOffset      = er.Read<uint>();
                        ArchiveSubRecordOffset = er.Read<uint>();
                        if (ArchiveNameOffset != 0)
                        {
                            long curpos = er.BaseStream.Position;
                            er.JumpRelative(ArchiveNameOffset);
                            ArchiveName            = er.ReadStringNT(Encoding.ASCII);
                            er.BaseStream.Position = curpos;
                        }

                        if (ArchiveSubRecordOffset != 0)
                        {
                            long curpos = er.BaseStream.Position;
                            er.JumpRelative(ArchiveSubRecordOffset);
                            ArchiveSubRecord       = new SymbolRecord(er);
                            er.BaseStream.Position = curpos;
                        }
                    }

                    public void Write(EndianBinaryWriterEx er)
                    {
                        er.Write((uint)0);
                        er.Write((uint)0);
                    }

                    public uint ArchiveNameOffset;
                    public uint ArchiveSubRecordOffset;

                    public string       ArchiveName;
                    public SymbolRecord ArchiveSubRecord;
                }
            }
        }

        public Info InfoBlock;

        public class Info
        {
            public const uint InfoSignature = 0x4F464E49;

            public Info(EndianBinaryReaderEx er)
            {
                er.BeginChunk();
                er.ReadObject(this);
                er.Read<byte>(24);
                er.EndChunk(SectionSize);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk(4);
                er.WriteObject(this);
                er.Write(new byte[24], 0, 24);
                SequenceInfos.Write(er);
                SequenceArchiveInfos.Write(er);
                BankInfos.Write(er);
                WaveArchiveInfos.Write(er);
                PlayerInfos.Write(er);
                GroupInfos.Write(er);
                StreamPlayerInfos.Write(er);
                StreamInfos.Write(er);
                er.EndChunk();
            }

            [Constant(InfoSignature)]
            public uint Signature;

            [ChunkSize]
            public uint SectionSize;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<SequenceInfo> SequenceInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<SequenceArchiveInfo> SequenceArchiveInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<BankInfo> BankInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<WaveArchiveInfo> WaveArchiveInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<PlayerInfo> PlayerInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<GroupInfo> GroupInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<StreamPlayerInfo> StreamPlayerInfos;

            [Reference(ReferenceType.ChunkRelative)]
            public InfoRecord<StreamInfo> StreamInfos;

            public class InfoRecord<T> where T : SdatInfo, new()
            {
                public InfoRecord(EndianBinaryReaderEx er)
                {
                    NrEntries    = er.Read<uint>();
                    EntryOffsets = er.Read<uint>((int)NrEntries);
                    long curpos = er.BaseStream.Position;
                    for (int i = 0; i < NrEntries; i++)
                    {
                        if (EntryOffsets[i] == 0)
                        {
                            Entries.Add(null);
                            continue;
                        }

                        er.JumpRelative(EntryOffsets[i]);
                        Entries.Add(new T());
                        Entries[i].Read(er);
                    }

                    er.BaseStream.Position = curpos;
                }

                public void Write(EndianBinaryWriterEx er)
                {
                    er.StartOfRefObject(this);
                    er.Write((uint)Entries.Count);
                    long offspos = er.GetCurposRelative();
                    er.Write(new uint[Entries.Count], 0, Entries.Count);
                    for (int i = 0; i < Entries.Count; i++)
                    {
                        if (Entries[i] != null)
                        {
                            er.WriteCurposRelative((int)(offspos + i * 4));
                            Entries[i].Write(er);
                        }
                    }
                }

                public uint   NrEntries;
                public uint[] EntryOffsets;

                public readonly List<T> Entries = new();

                public T this[int i]
                {
                    get => Entries[i];
                    set => Entries[i] = value;
                }
            }

            public abstract class SdatInfo
            {
                public abstract void Read(EndianBinaryReaderEx er);
                public abstract void Write(EndianBinaryWriterEx er);
            }

            public class SequenceInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er) => er.ReadObject(this);
                public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

                public uint   FileId;
                public ushort Bank;
                public byte   Volume;
                public byte   ChannelPriority;
                public byte   PlayerPriority;
                public byte   PlayerNr;
                public ushort Reserved;
            }

            public class SequenceArchiveInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er)
                {
                    FileId = er.Read<uint>();
                }

                public override void Write(EndianBinaryWriterEx er)
                {
                    er.Write(FileId);
                }

                public uint FileId;
            }

            public class BankInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er) => er.ReadObject(this);
                public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

                public uint FileId;

                [ArraySize(4)]
                public ushort[] WaveArchives;
            }

            public class WaveArchiveInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er)
                {
                    uint tmp = er.Read<uint>();
                    FileId = tmp & 0xFFFFFF;
                    Flags  = (byte)(tmp >> 24);
                }

                public override void Write(EndianBinaryWriterEx er)
                {
                    er.Write((uint)((uint)Flags << 24 | (FileId & 0xFFFFFF)));
                }

                public uint FileId;
                public byte Flags;
            }

            [FieldAlignment(FieldAlignment.FieldSize)]
            public class PlayerInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er) => er.ReadObject(this);
                public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

                public byte   MaxNrSequences;
                public ushort ChannelAllocationMask;
                public uint   HeapSize;
            }

            public class GroupInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er)
                {
                    NrSubItems = er.Read<uint>();
                    SubItems   = new List<GroupItem>();
                    for (int i = 0; i < NrSubItems; i++)
                        SubItems.Add(new GroupItem(er));
                }

                public override void Write(EndianBinaryWriterEx er)
                {
                    er.Write((uint)SubItems.Count);
                    foreach (var i in SubItems)
                        i.Write(er);
                }

                public uint            NrSubItems;
                public List<GroupItem> SubItems { get; set; }

                public class GroupItem
                {
                    public GroupItem(EndianBinaryReaderEx er) => er.ReadObject(this);
                    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

                    public byte   Type;
                    public byte   LoadFlag;
                    public ushort Padding;
                    public uint   Index;
                }
            }

            public class StreamPlayerInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er)
                {
                    er.ReadObject(this);
                    er.Read<byte>(7);
                }

                public override void Write(EndianBinaryWriterEx er)
                {
                    er.WriteObject(this);
                    er.Write(new byte[7], 0, 7);
                }

                public byte NrChannels;

                [ArraySize(16)]
                public byte[] ChannelNumbers;
            }

            public class StreamInfo : SdatInfo
            {
                public override void Read(EndianBinaryReaderEx er)
                {
                    er.ReadObject(this);
                }

                public override void Write(EndianBinaryWriterEx er)
                {
                    er.WriteObject(this);
                    er.Write(0);
                }

                public uint FileId;
                public byte Volume;
                public byte PlayerPriority;
                public byte PlayerNr;
                public byte Flags;
            }
        }

        public Fat FileAllocationTable;

        public class Fat
        {
            public const uint FatSignature = 0x20544146;

            public Fat(EndianBinaryReaderEx er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.Read<uint>();
                if (Signature != FatSignature)
                    throw new SignatureNotCorrectException(Signature, FatSignature, er.BaseStream.Position - 4);
                SectionSize = er.Read<uint>();
                NrEntries   = er.Read<uint>();
                Entries     = new List<FatEntry>();
                for (int i = 0; i < NrEntries; i++)
                    Entries.Add(new FatEntry(er));

                er.BaseStream.Position = baseoffset + SectionSize;
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk(4);
                er.Write(FatSignature);
                er.Write((uint)0);
                er.Write((uint)Entries.Count);
                long filepos = er.BaseStream.Position + Entries.Count * 16 + 12;
                while ((filepos % 32) != 0)
                    filepos++;
                for (int i = 0; i < Entries.Count; i++)
                    Entries[i].Write(er, ref filepos);

                er.EndChunk();
            }

            public uint           Signature;
            public uint           SectionSize;
            public uint           NrEntries;
            public List<FatEntry> Entries;

            public class FatEntry
            {
                public FatEntry() { }

                public FatEntry(EndianBinaryReaderEx er)
                {
                    Offset = er.Read<uint>();
                    Length = er.Read<uint>();
                    er.Read<byte>(8);

                    long curpos = er.BaseStream.Position;
                    er.BaseStream.Position = Offset;
                    Data                   = er.Read<byte>((int)Length);
                    er.BaseStream.Position = curpos;
                }

                public void Write(EndianBinaryWriterEx er, ref long fileOffset)
                {
                    er.Write((uint)fileOffset);
                    er.Write((uint)Data.Length);
                    er.Write(new byte[8], 0, 8);
                    long curpos = er.BaseStream.Position;
                    er.BaseStream.Position = fileOffset;
                    er.Write(Data, 0, Data.Length);
                    int pad = 0;
                    while ((er.BaseStream.Position % 32) != 0)
                    {
                        er.Write((byte)0);
                        pad++;
                    }

                    er.BaseStream.Position =  curpos;
                    fileOffset             += Data.Length + pad;
                }

                public uint Offset;
                public uint Length;

                public byte[] Data;
            }
        }

        /*public FILE File;
        public class FILE
        {
            public FILE(EndianBinaryReaderEx er)
            {
                long baseoffset = er.BaseStream.Position;
                Signature = er.ReadString(Encoding.ASCII, 4);
                if (Signature != "FILE") throw new SignatureNotCorrectException(Signature, "FILE", er.BaseStream.Position - 4);
                SectionSize = er.Read<uint>();
                NrFiles = er.Read<uint>();
                while ((er.BaseStream.Position % 32) != 0) er.Read<byte>();
                FileData = er.Read<byte>((int)(baseoffset + SectionSize - er.BaseStream.Position));
                er.BaseStream.Position = baseoffset + SectionSize;
            }
            public String Signature;
            public UInt32 SectionSize;
            public UInt32 NrFiles;
            public Byte[] FileData;
        }*/

        public byte[] GetFileData(uint fileId)
        {
            if (fileId >= FileAllocationTable.Entries.Count)
                return null;
            if (FileAllocationTable.Entries[(int)fileId] == null)
                return null;
            return FileAllocationTable.Entries[(int)fileId].Data;
        }

        public int GetUsageForBank(int id)
        {
            int cnt = 0;
            foreach (var s in InfoBlock.SequenceInfos.Entries)
            {
                if (s == null)
                    continue;
                if (s.Bank == id)
                    cnt++;
            }

            foreach (var s in InfoBlock.SequenceArchiveInfos.Entries)
            {
                if (s == null)
                    continue;
                var ss = new Ssar(GetFileData(s.FileId));
                foreach (var sss in ss.Sequences)
                {
                    if (sss.Bank == id)
                        cnt++;
                }
            }

            return cnt;
        }

        public int GetUsageForWaveArchive(int Id)
        {
            int cnt = 0;
            foreach (var s in InfoBlock.BankInfos.Entries)
            {
                if (s == null)
                    continue;
                if (s.WaveArchives[0] == Id || s.WaveArchives[1] == Id ||
                    s.WaveArchives[2] == Id || s.WaveArchives[3] == Id)
                    cnt++;
            }

            return cnt;
        }
    }
}