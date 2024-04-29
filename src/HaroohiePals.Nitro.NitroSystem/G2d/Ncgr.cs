using HaroohiePals.IO;
using HaroohiePals.Nitro.Gx;
using System;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d
{
    public class Ncgr
    {
        public const uint NcgrSignature = 0x4E434752;

        public Ncgr(byte[] charData, int width, int height, ImageFormat format)
        {
            Header         = new BinaryFileHeader(NcgrSignature, 1);
            Header.Version = 0x101;
            Character      = new Char(charData, width, height, format);
        }

        public Ncgr(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Ncgr(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NcgrSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NcgrSignature, 0);
                Character = new Char(er);
                if (Header.DataBlocks > 1)
                {
                    //TODO: Character Position Data Block
                    throw new NotImplementedException("Character position data block not supported yet");
                }
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk(8);
            Header.DataBlocks = 1;
            Header.Write(er);
            Character.Write(er);
            //TODO: Character Position Data Block
            er.EndChunk();
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public BinaryFileHeader Header;
        public Char             Character;

        public class Char
        {
            public const uint CharSignature = 0x43484152;

            public Char(byte[] charData, int width, int height, ImageFormat format)
            {
                Signature     = CharSignature;
                Width         = (ushort)(width / 8);
                Height        = (ushort)(height / 8);
                PixelFormat   = format;
                CharacterData = charData;
            }

            public Char(EndianBinaryReaderEx er)
            {
                er.BeginChunk();
                Signature   = er.ReadSignature(CharSignature);
                SectionSize = er.Read<uint>();
                er.BeginChunk();
                Height              = er.Read<ushort>();
                Width               = er.Read<ushort>();
                PixelFormat         = (ImageFormat)er.Read<uint>();
                MappingType         = er.Read<uint>();
                CharacterFormat     = er.Read<uint>();
                CharacterDataSize   = er.Read<uint>();
                CharacterDataOffset = er.Read<uint>();
                er.JumpRelative(CharacterDataOffset);
                CharacterData = er.Read<byte>((int)CharacterDataSize);
                er.EndChunk();
                er.EndChunk(SectionSize);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk();
                er.Write(CharSignature);
                er.WriteChunkSize();
                er.Write(Height);
                er.Write(Width);
                er.Write((uint)PixelFormat);
                er.Write(MappingType);
                er.Write(CharacterFormat);
                er.Write((uint)CharacterData.Length);
                er.Write((uint)0x18);
                er.Write(CharacterData, 0, CharacterData.Length);
                er.WritePadding(4);
                er.EndChunk();
            }

            public uint        Signature;
            public uint        SectionSize;
            public ushort      Height;
            public ushort      Width;
            public ImageFormat PixelFormat;
            public uint        MappingType;
            public uint        CharacterFormat;
            public uint        CharacterDataSize;
            public uint        CharacterDataOffset;

            public byte[] CharacterData;
        }

        //TODO: Character Position Data Block
    }
}