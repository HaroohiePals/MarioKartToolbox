using HaroohiePals.IO;
using HaroohiePals.Nitro.Gx;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d
{
    public class Nclr
    {
        public const uint NclrSignature = 0x4E434C52;

        public Nclr(ushort[] palette)
        {
            Header  = new BinaryFileHeader(NclrSignature, 1);
            Palette = new Pltt(palette);
        }

        public Nclr(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Nclr(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NclrSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NclrSignature, 0);
                Palette = new Pltt(er);
                if (Header.DataBlocks > 1)
                    PaletteCompression = new Pcmp(er);
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk();
            Header.DataBlocks = (ushort)(PaletteCompression != null ? 2 : 1);
            Header.Write(er);
            Palette.Write(er);
            PaletteCompression?.Write(er);
            er.EndChunk();
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public BinaryFileHeader Header;
        public Pltt             Palette;

        public class Pltt
        {
            public const uint PlttSignature = 0x504C5454;

            public Pltt(ushort[] palette)
            {
                Signature     = PlttSignature;
                PaletteFormat = ImageFormat.Pltt16;
                IsExtended    = false;
                PaletteSize   = (uint)(palette.Length * 2);
                PaletteOffset = 0x10;
                Palette       = palette;
            }

            public Pltt(EndianBinaryReaderEx er)
            {
                er.BeginChunk();
                Signature   = er.ReadSignature(PlttSignature);
                SectionSize = er.Read<uint>();
                er.BeginChunk();
                PaletteFormat = (ImageFormat)er.Read<uint>();
                IsExtended    = er.Read<uint>() == 1;
                PaletteSize   = er.Read<uint>();
                PaletteOffset = er.Read<uint>();
                er.JumpRelative(PaletteOffset);
                //Palette = er.Read<ushort>((int)PaletteSize / 2);
                //hack: the palette size is sometimes 0 for whatever reason
                Palette = er.Read<ushort>((int)SectionSize - 0x18);
                er.EndChunk();
                er.EndChunk(SectionSize);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk();
                er.Write(PlttSignature);
                er.WriteChunkSize();
                er.Write((uint)PaletteFormat);
                er.Write((uint)(IsExtended ? 1 : 0));
                er.Write((uint)(Palette.Length * 2));
                er.Write((uint)0x10);
                er.Write(Palette, 0, Palette.Length);
                er.WritePadding(4);
                er.EndChunk();
            }

            public uint        Signature;
            public uint        SectionSize;
            public ImageFormat PaletteFormat;
            public bool        IsExtended;
            public uint        PaletteSize;
            public uint        PaletteOffset;

            public ushort[] Palette;
        }

        public Pcmp PaletteCompression;

        public class Pcmp
        {
            public const uint PcmpSignature = 0x50434D50;

            public Pcmp(EndianBinaryReaderEx er)
            {
                er.BeginChunk();
                Signature   = er.ReadSignature(PcmpSignature);
                SectionSize = er.Read<uint>();
                er.BeginChunk();
                NrPalettes = er.Read<ushort>();
                er.Read<ushort>();
                PaletteIndexTableOffset = er.Read<uint>();
                er.JumpRelative(PaletteIndexTableOffset);
                PaletteIndexTable = er.Read<ushort>(NrPalettes);
                er.EndChunk();
                er.EndChunk(SectionSize);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk();
                er.Write(PcmpSignature);
                er.WriteChunkSize();
                er.Write((ushort)PaletteIndexTable.Length);
                er.Write((ushort)0);
                er.Write((uint)8);
                er.Write(PaletteIndexTable, 0, PaletteIndexTable.Length);
                er.WritePadding(4);
                er.EndChunk();
            }

            public uint   Signature;
            public uint   SectionSize;
            public ushort NrPalettes;
            public uint   PaletteIndexTableOffset;

            public ushort[] PaletteIndexTable;
        }
    }
}