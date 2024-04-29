using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G2d
{
    public class Nscr
    {
        public const uint NscrSignature = 0x4E534352;

        public Nscr(byte[] map, int width, int height, Scrn.ScreenColorMode colorMode, Scrn.ScreenType type)
        {
            Header = new BinaryFileHeader(NscrSignature, 1);
            Screen = new Scrn(map, width, height, colorMode, type);
        }

        public Nscr(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Nscr(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NscrSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NscrSignature, 0);
                Screen = new Scrn(er);
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk(8);
            Header.DataBlocks = 1;
            Header.Write(er);
            Screen.Write(er);
            er.EndChunk();
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public BinaryFileHeader Header;
        public Scrn             Screen;

        public class Scrn
        {
            public const uint ScrnSignature = 0x5343524E;

            public enum ScreenColorMode
            {
                _16x16,
                _256x1,
                _256x16
            }

            public enum ScreenType
            {
                Text,
                Affine,
                AffineExt
            }

            public Scrn(byte[] map, int width, int height, ScreenColorMode colorMode, ScreenType type)
            {
                Signature    = ScrnSignature;
                Width        = (ushort)width;
                Height       = (ushort)height;
                ColorMode    = colorMode;
                ScreenFormat = type;
                ScreenData   = map;
            }

            public Scrn(EndianBinaryReaderEx er)
            {
                er.BeginChunk();
                Signature      = er.ReadSignature(ScrnSignature);
                SectionSize    = er.Read<uint>();
                Width          = er.Read<ushort>();
                Height         = er.Read<ushort>();
                ColorMode      = (ScreenColorMode)er.Read<ushort>();
                ScreenFormat   = (ScreenType)er.Read<ushort>();
                ScreenDataSize = er.Read<uint>();
                ScreenData     = er.Read<byte>((int)ScreenDataSize);
                er.EndChunk(SectionSize);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk(4);
                er.Write(ScrnSignature);
                er.Write((uint)0);
                er.Write(Width);
                er.Write(Height);
                er.Write((ushort)ColorMode);
                er.Write((ushort)ScreenFormat);
                er.Write((uint)ScreenData.Length);
                er.Write(ScreenData, 0, ScreenData.Length);
                er.WritePadding(4);
                er.EndChunk();
            }

            public uint            Signature;
            public uint            SectionSize;
            public ushort          Width;
            public ushort          Height;
            public ScreenColorMode ColorMode;
            public ScreenType      ScreenFormat;
            public uint            ScreenDataSize;

            public byte[] ScreenData;
        }
    }
}