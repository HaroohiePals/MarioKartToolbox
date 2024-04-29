using HaroohiePals.IO;
using System;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d.Intermediate
{
    public class Ncl
    {
        public const uint NclSignature = 0x4C43434E;

        public Ncl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Ncl(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NclSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NclSignature,
                        er.BaseStream.Position - 0x10);

                for (int i = 0; i < Header.DataBlocks; i++)
                {
                    uint type = er.Read<uint>();
                    er.BaseStream.Position -= 4;
                    switch (type)
                    {
                        case Palt.PaltBlockType:
                            PaletteData = new Palt(er);
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

        public Palt PaletteData;

        public class Palt
        {
            public const uint PaltBlockType = 0x544C4150;

            public Palt(EndianBinaryReaderEx er)
            {
                BlockType          = er.ReadSignature(PaltBlockType);
                BlockSize          = er.Read<uint>();
                PaletteColorNumber = er.Read<uint>();
                PaletteNumber      = er.Read<uint>();
                PaletteData        = er.Read<ushort>((int)(PaletteColorNumber * PaletteNumber));
            }

            public uint BlockType;
            public uint BlockSize;
            public uint PaletteColorNumber;
            public uint PaletteNumber;

            public ushort[] PaletteData;
        }

        public Cmnt CommentData;
        public Extr ExtendedData;
    }
}