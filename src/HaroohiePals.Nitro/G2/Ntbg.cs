using HaroohiePals.Graphics;
using HaroohiePals.IO;
using HaroohiePals.Nitro.Gx;
using System;
using System.IO;

namespace HaroohiePals.Nitro.G2
{
    //5bg format
    public class Ntbg
    {
        public const uint NtbgSignature = 0x4742544E;

        public Ntbg(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Ntbg(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new BinaryFileHeader(er);
                if (Header.Signature != NtbgSignature)
                    throw new SignatureNotCorrectException(Header.Signature, NtbgSignature, 0);

                for (int i = 0; i < Header.DataBlocks; i++)
                {
                    uint sig = er.Read<uint>();
                    er.BaseStream.Position -= 4;
                    switch (sig)
                    {
                        case Palt.PaltSignature:
                            Palette = new Palt(er);
                            break;
                        case Bgdt.BgdtSignature:
                            BgData = new Bgdt(er);
                            break;
                        case Objd.ObjdSignature:
                            //OBJData = new OBJD(er);
                            throw new NotImplementedException("OBJD not supported yet!");
                            break;
                        case Dfpl.DfplSignature:
                            DFPalette = new Dfpl(er);
                            break;
                    }
                }
            }
        }

        public BinaryFileHeader Header;

        public Palt Palette;

        public class Palt
        {
            public const uint PaltSignature = 0x544C4150;

            public Palt(EndianBinaryReaderEx er)
            {
                Signature = er.ReadSignature(PaltSignature);
                BlockSize = er.Read<uint>();
                NrColors  = er.Read<uint>();
                Colors    = er.Read<ushort>((int)NrColors);
            }

            public uint     Signature;
            public uint     BlockSize;
            public uint     NrColors;
            public ushort[] Colors;
        }

        public Bgdt BgData;

        public class Bgdt
        {
            public const uint BgdtSignature = 0x54444742;

            public Bgdt(EndianBinaryReaderEx er)
            {
                Signature        = er.ReadSignature(BgdtSignature);
                BlockSize        = er.Read<uint>();
                DataFormat       = er.Read<byte>();
                NitroScreenSize  = er.Read<byte>();
                Unknown          = er.Read<ushort>();
                ScreenDataLength = er.Read<uint>();
                ScreenTileWidth  = er.Read<ushort>();
                ScreenTileHeight = er.Read<ushort>();
                CharTileWidth    = er.Read<ushort>();
                CharTileHeight   = er.Read<ushort>();
                CharDataLength   = er.Read<uint>();
                ScreenData       = er.Read<byte>((int)ScreenDataLength);
                CharData         = er.Read<byte>((int)CharDataLength);
            }

            public uint Signature;
            public uint BlockSize;

            //0x00=text 16x16, 0x01=text 256x1, 0x03=affine 256x1, 0x10=8bpp bmp, 0x12=direct bmp
            public byte   DataFormat;
            public byte   NitroScreenSize; //value depends on mode, 0xFF = freesize 
            public ushort Unknown;
            public uint   ScreenDataLength;
            public ushort ScreenTileWidth;
            public ushort ScreenTileHeight;
            public ushort CharTileWidth;
            public ushort CharTileHeight;
            public uint   CharDataLength;

            public byte[] ScreenData;
            public byte[] CharData;
        }

        public Objd ObjData;

        public class Objd
        {
            public const uint ObjdSignature = 0x444A424F;

            public uint Signature;
            public uint BlockSize;

            //?
        }

        public Dfpl DFPalette;

        public class Dfpl
        {
            public const uint DfplSignature = 0x4C504644;

            public Dfpl(EndianBinaryReaderEx er)
            {
                Signature          = er.ReadSignature(DfplSignature);
                BlockSize          = er.Read<uint>();
                NrTiles            = er.Read<uint>();
                PaletteRowForTiles = er.Read<byte>((int)NrTiles);
            }

            public uint Signature;
            public uint BlockSize;

            public uint   NrTiles;
            public byte[] PaletteRowForTiles;
        }

        public ImageFormat GetNitroFormat()
        {
            if (BgData != null)
            {
                switch (BgData.DataFormat)
                {
                    case 0:
                        return ImageFormat.Pltt16;
                    case 1:
                    case 3:
                    case 0x10:
                        return ImageFormat.Pltt256;
                    case 0x12:
                        return ImageFormat.Direct;
                }
            }

            if (ObjData != null) { }

            return 0;
        }

        public int GetWidth()
        {
            if (BgData != null)
                return BgData.ScreenTileWidth * 8;

            if (ObjData != null) { }

            return 0;
        }

        public int GetHeight()
        {
            if (BgData != null)
                return BgData.ScreenTileHeight * 8;

            if (ObjData != null) { }

            return 0;
        }

        public int GetTextureSize()
        {
            if (BgData != null)
                return (int)(BgData.ScreenDataLength + BgData.CharDataLength);

            if (ObjData != null) { }

            return 0;
        }

        public int GetPaletteSize()
        {
            if (Palette != null)
                return (int)Palette.NrColors * 2;
            return 0;
        }

        public Rgba8Bitmap ToBitmap()
        {
            if (BgData != null)
            {
                switch (BgData.DataFormat)
                {
                    case 0:
                    {
                        byte[] mapData = BgData.ScreenData;
                        if (DFPalette != null)
                        {
                            mapData = new byte[BgData.ScreenData.Length];
                            Array.Copy(BgData.ScreenData, mapData, BgData.ScreenData.Length);
                            for (int i = 0; i < mapData.Length / 2; i++)
                            {
                                ushort val = (ushort)(mapData[i * 2] | (mapData[i * 2 + 1] << 8));
                                val                |= (ushort)(DFPalette.PaletteRowForTiles[val & 0x3FF] << 12);
                                mapData[i * 2]     =  (byte)(val & 0xFF);
                                mapData[i * 2 + 1] =  (byte)((val >> 8) & 0xFF);
                            }
                        }

                        return GxUtil.DecodeChar(BgData.CharData, Palette.Colors, mapData, ImageFormat.Pltt16,
                            MapFormat.Text, BgData.ScreenTileWidth * 8, BgData.ScreenTileHeight * 8, true);
                    }
                    case 1:
                        return GxUtil.DecodeChar(BgData.CharData, Palette.Colors, BgData.ScreenData,
                            ImageFormat.Pltt256,
                            MapFormat.Text, BgData.ScreenTileWidth * 8, BgData.ScreenTileHeight * 8, true);
                    case 3:
                    {
                        var newMapData = new byte[BgData.ScreenData.Length / 2];
                        for (int i = 0; i < newMapData.Length; i++)
                            newMapData[i] = BgData.ScreenData[i * 2];
                        return GxUtil.DecodeChar(BgData.CharData, Palette.Colors, newMapData, ImageFormat.Pltt256,
                            MapFormat.Affine, BgData.ScreenTileWidth * 8, BgData.ScreenTileHeight * 8, true);
                    }
                    case 0x10:
                        return GxUtil.DecodeBmp(BgData.CharData, ImageFormat.Pltt256, BgData.ScreenTileWidth * 8,
                            BgData.ScreenTileHeight * 8, Palette.Colors, true);
                    case 0x12:
                        return GxUtil.DecodeBmp(BgData.CharData, ImageFormat.Direct, BgData.ScreenTileWidth * 8,
                            BgData.ScreenTileHeight * 8);
                }
            }

            if (ObjData != null) { }

            return null;
        }
    }
}