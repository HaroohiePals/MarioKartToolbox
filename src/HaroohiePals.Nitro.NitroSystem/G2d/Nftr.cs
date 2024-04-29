using HaroohiePals.IO;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d
{
    public class Nftr
    {
        public const uint NftrSignature = 0x4E465452;

        public Nftr(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Nftr(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NftrSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NftrSignature, 0);
                FontInformation        = new Finf(er);
                er.BaseStream.Position = FontInformation.CharacterGlyphsOffset - 8;
                CharacterGlyphs        = new Cglp(er);
                if (FontInformation.CharacterWidthsOffset != 0)
                {
                    CharacterWidths = new Dictionary<ushort, CharWidths>();
                    uint ptr = FontInformation.CharacterWidthsOffset;
                    while (ptr != 0)
                    {
                        er.BaseStream.Position = ptr - 8;
                        var cwdh = new Cwdh(er);
                        cwdh.AppendWidths(CharacterWidths);
                        ptr = cwdh.NextOffset;
                    }
                }

                if (FontInformation.CharacterCodeMapOffset != 0)
                {
                    CharacterCodeMap = new Dictionary<ushort, ushort>();
                    uint ptr = FontInformation.CharacterCodeMapOffset;
                    while (ptr != 0)
                    {
                        er.BaseStream.Position = ptr - 8;
                        var cmap = new Cmap(er);
                        cmap.AppendCharacterCodes(CharacterCodeMap);
                        ptr = cmap.NextOffset;
                    }
                }
            }
        }

        public BinaryFileHeader Header;
        public Finf             FontInformation;

        public class Finf
        {
            public const uint FinfSignature = 0x46494E46;

            public Finf(EndianBinaryReaderEx er)
            {
                Signature              = er.ReadSignature(FinfSignature);
                BlockSize              = er.Read<uint>();
                FontType               = er.Read<byte>();
                LineFeed               = er.Read<byte>();
                AlternateCharIndex     = er.Read<ushort>();
                DefaultWidth           = new CharWidths(er);
                CharacterEncoding      = er.Read<byte>();
                CharacterGlyphsOffset  = er.Read<uint>();
                CharacterWidthsOffset  = er.Read<uint>();
                CharacterCodeMapOffset = er.Read<uint>();
            }

            public uint       Signature;
            public uint       BlockSize;
            public byte       FontType;
            public byte       LineFeed;
            public ushort     AlternateCharIndex;
            public CharWidths DefaultWidth;
            public byte       CharacterEncoding;
            public uint       CharacterGlyphsOffset;
            public uint       CharacterWidthsOffset;
            public uint       CharacterCodeMapOffset;
        }

        public Cglp CharacterGlyphs;

        public class Cglp
        {
            public const uint CglpSignature = 0x43474C50;

            public Cglp(EndianBinaryReaderEx er)
            {
                Signature    = er.ReadSignature(CglpSignature);
                BlockSize    = er.Read<uint>();
                CellWidth    = er.Read<byte>();
                CellHeight   = er.Read<byte>();
                CellSize     = er.Read<ushort>();
                BaselinePos  = er.Read<sbyte>();
                MaxCharWidth = er.Read<byte>();
                Bpp          = er.Read<byte>();
                Reserved     = er.Read<byte>();
                int nrGlyphs = (int)((BlockSize - 0x10) / CellSize);
                Glyphs = new byte[nrGlyphs][];
                for (int i = 0; i < nrGlyphs; i++)
                    Glyphs[i] = er.Read<byte>(CellSize);
            }

            public uint     Signature;
            public uint     BlockSize;
            public byte     CellWidth;
            public byte     CellHeight;
            public ushort   CellSize;
            public sbyte    BaselinePos;
            public byte     MaxCharWidth;
            public byte     Bpp;
            public byte     Reserved;
            public byte[][] Glyphs;
        }

        public Dictionary<ushort, CharWidths> CharacterWidths;

        public class Cwdh
        {
            public const uint CwdhSignature = 0x43574448;

            public Cwdh(EndianBinaryReaderEx er)
            {
                Signature  = er.ReadSignature(CwdhSignature);
                BlockSize  = er.Read<uint>();
                IndexBegin = er.Read<ushort>();
                IndexEnd   = er.Read<ushort>();
                NextOffset = er.Read<uint>();
                int nrWidths = IndexEnd - IndexBegin + 1;
                Widths = new CharWidths[nrWidths];
                for (int i = 0; i < nrWidths; i++)
                    Widths[i] = new CharWidths(er);
            }

            public uint         Signature;
            public uint         BlockSize;
            public ushort       IndexBegin;
            public ushort       IndexEnd;
            public uint         NextOffset;
            public CharWidths[] Widths;

            public void AppendWidths(Dictionary<ushort, CharWidths> dict)
            {
                for (int i = 0; i < IndexEnd - IndexBegin + 1; i++)
                    dict[(ushort)(IndexBegin + i)] = Widths[i];
            }
        }

        public Dictionary<ushort, ushort> CharacterCodeMap;

        public class Cmap
        {
            public const uint CmapSignature = 0x434D4150;

            public enum CharacterMappingMode : ushort
            {
                Direct = 0,
                Table  = 1,
                Scan   = 2
            }

            public Cmap(EndianBinaryReaderEx er)
            {
                Signature          = er.ReadSignature(CmapSignature);
                BlockSize          = er.Read<uint>();
                CharacterCodeBegin = er.Read<ushort>();
                CharacterCodeEnd   = er.Read<ushort>();
                MappingMethod      = (CharacterMappingMode)er.Read<ushort>();
                Reserved           = er.Read<ushort>();
                NextOffset         = er.Read<uint>();
                if (MappingMethod == CharacterMappingMode.Direct)
                    GlyphIndexOffset = er.Read<ushort>();
                else if (MappingMethod == CharacterMappingMode.Table)
                    GlyphIndices = er.Read<ushort>(CharacterCodeEnd - CharacterCodeBegin + 1);
                else if (MappingMethod == CharacterMappingMode.Scan)
                {
                    NrScanEntries = er.Read<ushort>();
                    ScanEntries   = new ScanEntry[NrScanEntries];
                    for (int i = 0; i < NrScanEntries; i++)
                        ScanEntries[i] = new ScanEntry(er);
                }
            }

            public uint                 Signature;
            public uint                 BlockSize;
            public ushort               CharacterCodeBegin;
            public ushort               CharacterCodeEnd;
            public CharacterMappingMode MappingMethod;
            public ushort               Reserved;
            public uint                 NextOffset;

            //direct
            public ushort GlyphIndexOffset;

            //table
            public ushort[] GlyphIndices;

            //scan
            public ushort      NrScanEntries;
            public ScanEntry[] ScanEntries;

            public struct ScanEntry
            {
                public ScanEntry(EndianBinaryReader er)
                {
                    CharacterCode = er.Read<ushort>();
                    GlyphIndex    = er.Read<ushort>();
                }

                public ushort CharacterCode;
                public ushort GlyphIndex;
            }

            public void AppendCharacterCodes(Dictionary<ushort, ushort> dict)
            {
                if (MappingMethod == CharacterMappingMode.Direct)
                {
                    for (int i = 0; i < CharacterCodeEnd - CharacterCodeBegin + 1; i++)
                        dict[(ushort)(CharacterCodeBegin + i)] = (ushort)(GlyphIndexOffset + i);
                }
                else if (MappingMethod == CharacterMappingMode.Table)
                {
                    for (int i = 0; i < CharacterCodeEnd - CharacterCodeBegin + 1; i++)
                        dict[(ushort)(CharacterCodeBegin + i)] = GlyphIndices[i];
                }
                else if (MappingMethod == CharacterMappingMode.Scan)
                {
                    foreach (var scanEntry in ScanEntries)
                    {
                        dict[scanEntry.CharacterCode] = scanEntry.GlyphIndex;
                    }
                }
            }
        }

        public struct CharWidths
        {
            public CharWidths(EndianBinaryReader er)
            {
                Left       = er.Read<sbyte>();
                GlyphWidth = er.Read<byte>();
                CharWidth  = er.Read<sbyte>();
            }

            public sbyte Left;
            public byte  GlyphWidth;
            public sbyte CharWidth;

            public int GetCharWidth() => CharWidth;
        }

        public ushort GetGlyphIdxFromCharacterCode(ushort characterCode)
        {
            if (CharacterCodeMap != null)
            {
                if (CharacterCodeMap.ContainsKey(characterCode))
                    return CharacterCodeMap[characterCode];
                return FontInformation.AlternateCharIndex;
            }

            if (characterCode < CharacterGlyphs.Glyphs.Length)
                return characterCode;
            return FontInformation.AlternateCharIndex;
        }

        public CharWidths GetCharWidthsFromGlyphIdx(ushort glyphIdx)
        {
            if (CharacterWidths != null)
            {
                if (CharacterWidths.ContainsKey(glyphIdx))
                    return CharacterWidths[glyphIdx];
                return FontInformation.DefaultWidth;
            }

            return FontInformation.DefaultWidth;
        }

        public int GetCharWidth(ushort characterCode)
            => GetCharWidthsFromGlyphIdx(GetGlyphIdxFromCharacterCode(characterCode)).GetCharWidth();
    }
}