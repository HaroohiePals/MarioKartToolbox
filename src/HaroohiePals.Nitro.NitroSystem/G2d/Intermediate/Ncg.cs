using HaroohiePals.IO;
using System;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d.Intermediate
{
    public class Ncg
    {
        public const uint NcgSignature = 0x4743434E;

        public Ncg(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Ncg(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NcgSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NcgSignature,
                        er.BaseStream.Position - 0x10);

                for (int i = 0; i < Header.DataBlocks; i++)
                {
                    uint type = er.Read<uint>();
                    er.BaseStream.Position -= 4;
                    switch (type)
                    {
                        case Char.CharBlockType:
                            CharacterData = new Char(er);
                            break;
                        case Attr.AttrBlockType:
                            AttributeData = new Attr(er);
                            break;
                        case Link.LinkBlockType:
                            LinkFileNameData = new Link(er);
                            break;
                        case Cmnt.CmntBlockType:
                            CommentData = new Cmnt(er);
                            break;
                        case Extr.ExtrBlockType:
                            ExtendedData = new Extr(er);
                            break;
                        default:
                            throw new Exception("Unknown block found!");
                    }
                }
            }
        }

        public BinaryFileHeader Header;

        public Char CharacterData;

        public class Char
        {
            public const uint CharBlockType = 0x52414843;

            public enum Mode : uint
            {
                Bpp4    = 0,
                Bpp8    = 1,
                Bpp8Ext = 2
            }

            public Char(EndianBinaryReaderEx er)
            {
                BlockType  = er.ReadSignature(CharBlockType);
                BlockSize  = er.Read<uint>();
                CharWidth  = er.Read<uint>();
                CharHeight = er.Read<uint>();
                BitMode    = (Mode)er.Read<uint>();
                CharData   = er.Read<byte>((int)(CharWidth * CharHeight * (BitMode == Mode.Bpp4 ? 32 : 64)));
            }

            public uint   BlockType;
            public uint   BlockSize;
            public uint   CharWidth;
            public uint   CharHeight;
            public Mode   BitMode;
            public byte[] CharData;
        }

        public Attr AttributeData;

        public class Attr
        {
            public const uint AttrBlockType = 0x52545441;

            public Attr(EndianBinaryReaderEx er)
            {
                long start = er.BaseStream.Position;
                BlockType              = er.ReadSignature(AttrBlockType);
                BlockSize              = er.Read<uint>();
                CharWidth              = er.Read<uint>();
                CharHeight             = er.Read<uint>();
                AttrData               = er.Read<byte>((int)(CharWidth * CharHeight));
                er.BaseStream.Position = start + BlockSize;
            }

            public uint   BlockType;
            public uint   BlockSize;
            public uint   CharWidth;
            public uint   CharHeight;
            public byte[] AttrData;
        }

        public Link LinkFileNameData;
        public Cmnt CommentData;
        public Extr ExtendedData;
    }
}