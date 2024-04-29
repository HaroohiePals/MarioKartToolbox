using HaroohiePals.IO;
using System;
using System.IO;
using System.Text;
using ImageFormat = HaroohiePals.Nitro.Gx.ImageFormat;

namespace HaroohiePals.Nitro.G3
{
    public class NitroTga
    {
        public NitroTga()
        {
            Header    = new TgaHeader();
            NitroId   = new NitroTgaId();
            NitroData = new NitroTgaData();
        }

        public NitroTga(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public NitroTga(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Header = new TgaHeader(er);
                if (Header.IdFieldLength != 0x14)
                    throw new Exception("Unexpected id field length!");
                NitroId = new NitroTgaId(er);
                ImageData =
                    er.Read<byte>(Header.ImageWidth * Header.ImageHeight * Header.ImagePixelSize / 8);
                er.BaseStream.Position = NitroId.NitroDataOffset;
                NitroData              = new NitroTgaData(er);
            }
        }

        public byte[] Write()
        {
            using (var m = new MemoryStream())
            {
                var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
                Header.Write(ew);
                long nitroIdOffset = ew.BaseStream.Position;
                NitroId.Write(ew);
                ew.Write(ImageData, 0, ImageData.Length);
                NitroId.NitroDataOffset = (uint)ew.BaseStream.Position;
                NitroData.Write(ew);
                ew.BaseStream.Position = nitroIdOffset;
                NitroId.Write(ew);
                ew.Close();
                return m.ToArray();
            }
        }

        public TgaHeader Header;

        public class TgaHeader
        {
            public TgaHeader()
            {
                IdFieldLength     = 0x14;
                ColorMapType      = 0;
                ImageType         = 2;
                ColorMapOrigin    = 0;
                ColorMapLength    = 0;
                ColorMapEntrySize = 0;
                ImageXOrigin      = 0;
                ImageYOrigin      = 0;
                ImagePixelSize    = 32;
                ImageDescriptor   = 0;
            }

            public TgaHeader(EndianBinaryReaderEx er) => er.ReadObject(this);
            public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            public byte   IdFieldLength;
            public byte   ColorMapType;
            public byte   ImageType;
            public ushort ColorMapOrigin;
            public ushort ColorMapLength;
            public byte   ColorMapEntrySize;
            public ushort ImageXOrigin;
            public ushort ImageYOrigin;
            public ushort ImageWidth;
            public ushort ImageHeight;
            public byte   ImagePixelSize;
            public byte   ImageDescriptor;
        }

        public NitroTgaId NitroId;

        public class NitroTgaId
        {
            public NitroTgaId()
            {
                Version = "NNS_Tga Ver 1.0\0";
            }

            public NitroTgaId(EndianBinaryReaderEx er)
            {
                Version         = er.ReadString(Encoding.ASCII, 16);
                NitroDataOffset = er.Read<uint>();
            }

            public void Write(EndianBinaryWriterEx er)
            {
                if (Version.Length > 16)
                    er.Write(Version.Remove(16), Encoding.ASCII, false);
                else
                    er.Write(Version.PadRight(16, '\0'), Encoding.ASCII, false);
                er.Write(NitroDataOffset);
            }

            public string Version;

            public uint NitroDataOffset;
        }

        public byte[] ImageData;

        public NitroTgaData NitroData;

        public class NitroTgaData
        {
            public NitroTgaData() { }

            public NitroTgaData(EndianBinaryReaderEx er)
            {
                while (er.BaseStream.Position + 0xC <= er.BaseStream.Length)
                {
                    string sig = er.ReadString(Encoding.ASCII, 8);
                    uint   len = er.Read<uint>();
                    switch (sig)
                    {
                        case "nns_frmt":
                            Format = er.ReadString(Encoding.ASCII, (int)len - 12);
                            break;
                        case "nns_txel":
                            TexelData = er.Read<byte>((int)len - 12);
                            break;
                        case "nns_pidx":
                            PlttIdxData = er.Read<byte>((int)len - 12);
                            break;
                        case "nns_pnam":
                            PlttName = er.ReadString(Encoding.ASCII, (int)len - 12);
                            break;
                        case "nns_pcol":
                            Palette = er.Read<byte>((int)len - 12);
                            break;
                        case "nns_c0xp":
                            Color0Transparent = true;
                            break;
                        case "nns_gnam":
                            GeneratorName = er.ReadString(Encoding.ASCII, (int)len - 12);
                            break;
                        case "nns_gver":
                            GeneratorVersion = er.ReadString(Encoding.ASCII, (int)len - 12);
                            break;
                        case "nns_imst":
                            OptpixData = er.Read<byte>((int)len - 12);
                            break;
                        case "nns_pshp":
                            PhotoshopData = er.Read<byte>((int)len - 12);
                            break;
                        case "nns_endb":
                            return;
                    }
                }
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.BeginChunk();
                {
                    er.Write("nns_frmt", Encoding.ASCII, false);
                    er.WriteChunkSize();
                    er.Write(Format, Encoding.ASCII, false);
                }
                er.EndChunk();
                er.BeginChunk();
                {
                    er.Write("nns_txel", Encoding.ASCII, false);
                    er.WriteChunkSize();
                    er.Write(TexelData, 0, TexelData.Length);
                }
                er.EndChunk();
                if (PlttIdxData != null)
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_pidx", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(PlttIdxData, 0, PlttIdxData.Length);
                    }
                    er.EndChunk();
                }

                if (!string.IsNullOrWhiteSpace(PlttName))
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_pnam", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(PlttName, Encoding.ASCII, false);
                    }
                    er.EndChunk();
                }

                if (Format != "direct" && Palette != null)
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_pcol", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(Palette, 0, Palette.Length);
                    }
                    er.EndChunk();
                }

                if (Color0Transparent)
                {
                    er.Write("nns_c0xp", Encoding.ASCII, false);
                    er.Write(12u);
                }

                if (!string.IsNullOrWhiteSpace(GeneratorName))
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_gnam", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(GeneratorName, Encoding.ASCII, false);
                    }
                    er.EndChunk();
                }

                if (!string.IsNullOrWhiteSpace(GeneratorVersion))
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_gver", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(GeneratorVersion, Encoding.ASCII, false);
                    }
                    er.EndChunk();
                }

                if (PhotoshopData != null)
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_pshp", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(PhotoshopData, 0, PhotoshopData.Length);
                    }
                    er.EndChunk();
                }

                if (OptpixData != null)
                {
                    er.BeginChunk();
                    {
                        er.Write("nns_imst", Encoding.ASCII, false);
                        er.WriteChunkSize();
                        er.Write(OptpixData, 0, OptpixData.Length);
                    }
                    er.EndChunk();
                }

                er.Write("nns_endb", Encoding.ASCII, false);
                er.Write(12u);
            }

            public string Format;
            public byte[] TexelData;
            public byte[] PlttIdxData;
            public string PlttName;
            public byte[] Palette;
            public bool   Color0Transparent = false;
            public string GeneratorName;
            public string GeneratorVersion;
            public byte[] OptpixData;
            public byte[] PhotoshopData;

            public ImageFormat GetNitroFormat() => Format switch
            {
                "a3i5"       => ImageFormat.A3I5,
                "palette4"   => ImageFormat.Pltt4,
                "palette16"  => ImageFormat.Pltt16,
                "palette256" => ImageFormat.Pltt256,
                "tex4x4"     => ImageFormat.Comp4x4,
                "a5i3"       => ImageFormat.A5I3,
                "direct"     => ImageFormat.Direct,
                _            => throw new Exception("Unknown image format!")
            };

            public void SetNitroFormat(ImageFormat format) => Format = format switch
            {
                ImageFormat.A3I5    => "a3i5",
                ImageFormat.Pltt4   => "palette4",
                ImageFormat.Pltt16  => "palette16",
                ImageFormat.Pltt256 => "palette256",
                ImageFormat.Comp4x4 => "tex4x4",
                ImageFormat.A5I3    => "a5i3",
                ImageFormat.Direct  => "direct",
                _                   => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }

        //public unsafe Bitmap ToBitmap()
        //{
        //    var d = new NitroGraphicDecoder();
        //    d.SetImageData(NitroData.TexelData, NitroData.PlttIdxData, NitroData.GetNitroFormat(), CharFormat.Bmp);
        //    d.SetPalette(NitroData.Palette,
        //        (NitroData.GetNitroFormat() == ImageFormat.Pltt4 || NitroData.GetNitroFormat() == ImageFormat.Pltt16 ||
        //         NitroData.GetNitroFormat() == ImageFormat.Pltt256) && NitroData.Color0Transparent);
        //    //width and height are actually nitrofied, so we need to fix it and crop later
        //    int w  = Header.ImageWidth >> 3;
        //    int w2 = 0;
        //    while (w > 1)
        //    {
        //        w >>= 1;
        //        w2++;
        //    }

        //    if (8 << w2 < Header.ImageWidth)
        //        w2++;

        //    int h  = Header.ImageHeight >> 3;
        //    int h2 = 0;
        //    while (h > 1)
        //    {
        //        h >>= 1;
        //        h2++;
        //    }

        //    if (8 << h2 < Header.ImageHeight)
        //        h2++;

        //    int newW = 8 << w2;
        //    int newH = 8 << h2;

        //    var img = d.Decode(newW, newH);
        //    if (newW == Header.ImageWidth && newH == Header.ImageHeight)
        //        return img;
        //    //crop
        //    var result = new Bitmap(Header.ImageWidth, Header.ImageHeight);
        //    var bd = img.LockBits(
        //        new Rectangle(0, 0, Header.ImageWidth, Header.ImageHeight),
        //        ImageLockMode.ReadOnly,
        //        PixelFormat.Format32bppArgb);
        //    var bd2 = result.LockBits(
        //        new Rectangle(0, 0, result.Width, result.Height),
        //        ImageLockMode.WriteOnly,
        //        PixelFormat.Format32bppArgb);
        //    uint* pOrig = (uint*)bd.Scan0;
        //    uint* pDst  = (uint*)bd2.Scan0;
        //    for (int y = 0; y < Header.ImageHeight; y++)
        //    {
        //        Buffer.MemoryCopy(pOrig, pDst, 4 * Header.ImageWidth, 4 * Header.ImageWidth);
        //        pOrig += bd.Stride / 4;
        //        pDst  += bd2.Stride / 4;
        //    }

        //    result.UnlockBits(bd2);
        //    img.UnlockBits(bd);
        //    img.Dispose();
        //    return result;
        //}
    }
}