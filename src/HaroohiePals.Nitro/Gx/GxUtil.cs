using HaroohiePals.Graphics;
using HaroohiePals.IO;
using System;
using System.Drawing;

namespace HaroohiePals.Nitro.Gx
{
    public static class GxUtil
    {
        private static readonly int[] DataSize = { 0, 8, 2, 4, 8, 2, 8, 16 };

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<byte> palette,
            ReadOnlySpan<byte> map, ImageFormat imageFormat, MapFormat mapFormat, int width, int height,
            bool firstTransparent = false, int mapTileOffset = 0)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return DecodeChar(data,
                GfxUtil.ConvertColorFormatFromU16(IOUtil.ReadU16Le(palette, palette.Length / 2),
                    ColorFormat.XBGR1555, ColorFormat.ARGB8888),
                map, imageFormat, mapFormat, width,
                height, firstTransparent, mapTileOffset);
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<ushort> palette,
            ReadOnlySpan<byte> map, ImageFormat imageFormat, MapFormat mapFormat, int width, int height,
            bool firstTransparent = false, int mapTileOffset = 0)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return DecodeChar(data,
                GfxUtil.ConvertColorFormatFromU16(palette, ColorFormat.XBGR1555, ColorFormat.ARGB8888),
                map, imageFormat, mapFormat, width,
                height, firstTransparent, mapTileOffset);
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<Color> palette,
            ReadOnlySpan<byte> map, ImageFormat imageFormat, MapFormat mapFormat, int width, int height,
            bool firstTransparent = false, int mapTileOffset = 0)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return DecodeChar(data, GfxUtil.ColorToU32(palette), map, imageFormat, mapFormat, width, height,
                firstTransparent, mapTileOffset);
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<uint> palette,
            ReadOnlySpan<byte> map, ImageFormat imageFormat, MapFormat mapFormat, int width, int height,
            bool firstTransparent = false, int mapTileOffset = 0)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            if (map == null)
                throw new ArgumentNullException(nameof(map));
            if (imageFormat != ImageFormat.Pltt16 && imageFormat != ImageFormat.Pltt256)
                throw new ArgumentException(nameof(imageFormat));
            if ((width & 7) != 0)
                throw new ArgumentException(nameof(width));
            if ((height & 7) != 0)
                throw new ArgumentException(nameof(height));

            int    tileSize    = imageFormat == ImageFormat.Pltt16 ? 32 : 64;
            int    paletteSize = imageFormat == ImageFormat.Pltt16 ? 16 : 256;
            uint[] bmpdata     = new uint[width * height];
            int    tileIdx     = 0;
            for (int y = 0; y < height / 8; y++)
            {
                for (int x = 0; x < width / 8; x++)
                {
                    if (mapFormat == MapFormat.Text)
                    {
                        ushort tileData = IOUtil.ReadU16Le(map[(tileIdx * 2)..]);
                        int    charName = (int)(tileData & 0x3FF) - mapTileOffset;
                        if (charName < 0)
                            continue;
                        bool hf     = ((tileData >> 10) & 0x1) == 1;
                        bool vf     = ((tileData >> 11) & 0x1) == 1;
                        int  palidx = tileData >> 12;
                        uint[] pixels = DecodeRaw(data.Slice(charName * tileSize, tileSize),
                            palette.Slice(paletteSize * palidx), imageFormat, firstTransparent);
                        if (!hf && !vf)
                            Array.Copy(pixels, 0, bmpdata, tileIdx * 64, 64);
                        else if (hf && !vf)
                        {
                            int offset = tileIdx * 64;
                            for (int y2 = 0; y2 < 8; y2++)
                                for (int x2 = 7; x2 >= 0; x2--)
                                    bmpdata[offset++] = pixels[y2 * 8 + x2];
                        }
                        else if (!hf && vf)
                        {
                            int offset = tileIdx * 64;
                            for (int y2 = 7; y2 >= 0; y2--)
                                for (int x2 = 0; x2 < 8; x2++)
                                    bmpdata[offset++] = pixels[y2 * 8 + x2];
                        }
                        else if (hf && vf)
                        {
                            int offset = tileIdx * 64;
                            for (int y2 = 7; y2 >= 0; y2--)
                                for (int x2 = 7; x2 >= 0; x2--)
                                    bmpdata[offset++] = pixels[y2 * 8 + x2];
                        }
                    }
                    else
                    {
                        int charName = map[tileIdx];
                        uint[] pixels = DecodeRaw(data.Slice(charName * tileSize, tileSize), palette, imageFormat,
                            firstTransparent);
                        Array.Copy(pixels, 0, bmpdata, tileIdx * 64, 64);
                    }

                    tileIdx++;
                }
            }

            return new Rgba8Bitmap(width, height, GfxUtil.Detile(bmpdata, 8, width, height));
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<byte> palette,
            ImageFormat imageFormat, int width, int height, bool firstTransparent = false)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return DecodeChar(data,
                GfxUtil.ConvertColorFormatFromU16(IOUtil.ReadU16Le(palette, palette.Length / 2),
                    ColorFormat.XBGR1555, ColorFormat.ARGB8888),
                imageFormat, width, height, firstTransparent);
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<ushort> palette,
            ImageFormat imageFormat, int width, int height, bool firstTransparent = false)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return DecodeChar(data,
                GfxUtil.ConvertColorFormatFromU16(palette, ColorFormat.XBGR1555, ColorFormat.ARGB8888),
                imageFormat, width, height, firstTransparent);
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<Color> palette,
            ImageFormat imageFormat, int width, int height, bool firstTransparent = false)
        {
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            return DecodeChar(data, GfxUtil.ColorToU32(palette), imageFormat, width, height, firstTransparent);
        }

        public static Rgba8Bitmap DecodeChar(ReadOnlySpan<byte> data, ReadOnlySpan<uint> palette,
            ImageFormat imageFormat, int width, int height, bool firstTransparent = false)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (palette == null)
                throw new ArgumentNullException(nameof(palette));
            if (imageFormat != ImageFormat.Pltt16 && imageFormat != ImageFormat.Pltt256)
                throw new ArgumentException(nameof(imageFormat));
            if ((width & 7) != 0)
                throw new ArgumentException(nameof(width));
            if ((height & 7) != 0)
                throw new ArgumentException(nameof(height));

            uint[] bmpdata = DecodeRaw(data, palette, imageFormat, firstTransparent);

            return new Rgba8Bitmap(width, height, GfxUtil.Detile(bmpdata, 8, width, height));
        }

        public static Rgba8Bitmap DecodeBmp(ReadOnlySpan<byte> data, ImageFormat imageFormat, int width, int height,
            ReadOnlySpan<byte> palette, bool firstTransparent = false, ReadOnlySpan<byte> tex4x4Data = default)
        {
            uint[] pltt = null;
            if (palette != null)
            {
                pltt = GfxUtil.ConvertColorFormatFromU16(IOUtil.ReadU16Le(palette, palette.Length / 2),
                    ColorFormat.XBGR1555, ColorFormat.ARGB8888);
            }

            return DecodeBmp(data, imageFormat, width, height, pltt, firstTransparent, tex4x4Data);
        }

        public static Rgba8Bitmap DecodeBmp(ReadOnlySpan<byte> data, ImageFormat imageFormat, int width, int height,
            ReadOnlySpan<ushort> palette, bool firstTransparent = false, ReadOnlySpan<byte> tex4x4Data = default)
        {
            uint[] pltt = null;
            if (palette != null)
                pltt = GfxUtil.ConvertColorFormatFromU16(palette, ColorFormat.XBGR1555, ColorFormat.ARGB8888);

            return DecodeBmp(data, imageFormat, width, height, pltt, firstTransparent, tex4x4Data);
        }

        public static Rgba8Bitmap DecodeBmp(ReadOnlySpan<byte> data, ImageFormat imageFormat, int width, int height,
            ReadOnlySpan<Color> palette, bool firstTransparent = false, ReadOnlySpan<byte> tex4x4Data = default)
        {
            uint[] pltt = null;
            if (palette != null)
                pltt = GfxUtil.ColorToU32(palette);

            return DecodeBmp(data, imageFormat, width, height, pltt, firstTransparent, tex4x4Data);
        }

        public static Rgba8Bitmap DecodeBmp(ReadOnlySpan<byte> data, ImageFormat imageFormat, int width, int height,
            ReadOnlySpan<uint> palette = default, bool firstTransparent = false,
            ReadOnlySpan<byte> tex4x4Data = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (imageFormat == ImageFormat.None)
                throw new ArgumentException(nameof(imageFormat));
            if (imageFormat != ImageFormat.Direct && palette == null)
                throw new ArgumentNullException(nameof(palette));
            if ((width & 7) != 0)
                throw new ArgumentException(nameof(width));
            if ((height & 7) != 0)
                throw new ArgumentException(nameof(height));

            int length = width * height * DataSize[(int)imageFormat] / 8;

            if (imageFormat == ImageFormat.Comp4x4)
                tex4x4Data = tex4x4Data[..(length >> 1)];
            uint[] bmpdata = DecodeRaw(data[..length], palette, imageFormat, firstTransparent, tex4x4Data);

            if (imageFormat == ImageFormat.Comp4x4)
                bmpdata = GfxUtil.Detile(bmpdata, 4, width, height);

            return new Rgba8Bitmap(width, height, bmpdata);
        }

        private static uint[] DecodeRaw(ReadOnlySpan<byte> pixelData, ReadOnlySpan<uint> palette,
            ImageFormat imageFormat, bool firstTransparent, ReadOnlySpan<byte> tex4x4Data = default)
        {
            switch (imageFormat)
            {
                case ImageFormat.None:
                    throw new Exception("Invalid pixel format");

                case ImageFormat.A3I5:
                {
                    var result = new uint[pixelData.Length];
                    for (int i = 0; i < pixelData.Length; i++)
                    {
                        int a = pixelData[i] >> 5;
                        a = (a << 2) + (a >> 1);

                        result[i] = (palette[pixelData[i] & 0x1F] & ~0xFF000000) | (uint)((a * 255 / 31) << 24);
                    }

                    return result;
                }
                case ImageFormat.Pltt4:
                {
                    var result = new uint[pixelData.Length * 4];
                    for (int i = 0; i < pixelData.Length; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int idx = (pixelData[i] >> (j * 2)) & 0x3;
                            result[i * 4 + j] = idx != 0 || !firstTransparent ? palette[idx] : 0;
                        }
                    }

                    return result;
                }
                case ImageFormat.Pltt16:
                {
                    var result = new uint[pixelData.Length * 2];
                    for (int i = 0; i < pixelData.Length; i++)
                    {
                        int idx = pixelData[i] & 0xF;
                        result[i * 2]     = idx != 0 || !firstTransparent ? palette[idx] : 0;
                        idx               = pixelData[i] >> 4;
                        result[i * 2 + 1] = idx != 0 || !firstTransparent ? palette[idx] : 0;
                    }

                    return result;
                }
                case ImageFormat.Pltt256:
                {
                    var result = new uint[pixelData.Length];
                    for (int i = 0; i < pixelData.Length; i++)
                    {
                        int idx = pixelData[i];
                        result[i] = idx != 0 || !firstTransparent ? palette[idx] : 0;
                    }

                    return result;
                }
                case ImageFormat.Comp4x4:
                    return DecodeRawComp4x4(pixelData, tex4x4Data, palette);
                case ImageFormat.A5I3:
                {
                    var result = new uint[pixelData.Length];
                    for (int i = 0; i < pixelData.Length; i++)
                    {
                        result[i] = (palette[pixelData[i] & 7] & ~0xFF000000) |
                                    (uint)(((pixelData[i] >> 3) * 255 / 31) << 24);
                    }

                    return result;
                }
                case ImageFormat.Direct:
                    return GfxUtil.ConvertColorFormatFromU16(
                        IOUtil.ReadU16Le(pixelData, pixelData.Length / 2),
                        ColorFormat.ABGR1555, ColorFormat.ARGB8888);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static uint[] DecodeRawComp4x4(ReadOnlySpan<byte> pixelData, ReadOnlySpan<byte> tex4x4Data,
            ReadOnlySpan<uint> palette)
        {
            var result    = new uint[pixelData.Length * 4];
            int offset    = 0;
            int offset4x4 = 0;
            for (int i = 0; i < pixelData.Length * 4; i += 4 * 4)
            {
                ushort tex4x4 = IOUtil.ReadU16Le(tex4x4Data[offset4x4..]);
                offset4x4 += 2;
                int  pal = tex4x4 & 0x3FFF;
                bool a   = (tex4x4 & 0x8000) != 0;
                bool pty = (tex4x4 & 0x4000) != 0;
                byte d   = 0;
                for (int j = 0; j < 4 * 4; j++)
                {
                    if ((j & 3) == 0)
                        d = pixelData[offset++];
                    if (!a && !pty)
                        result[i + j] = (d & 3) == 3 ? 0 : palette[(d & 3) + pal * 2];
                    else if (a && !pty)
                        result[i + j] = palette[(d & 3) + pal * 2];
                    else if (!a && pty)
                    {
                        if ((d & 3) <= 1)
                            result[i + j] = palette[(d & 3) + pal * 2];
                        else if ((d & 3) == 2)
                            result[i + j] = GfxUtil.InterpolateColor(palette[0 + pal * 2], 1,
                                palette[1 + pal * 2], 1, ColorFormat.ARGB8888);
                        else
                            result[i + j] = 0;
                    }
                    else
                    {
                        if ((d & 3) <= 1)
                            result[i + j] = palette[(d & 3) + pal * 2];
                        else if ((d & 3) == 2)
                            result[i + j] = GfxUtil.InterpolateColor(palette[0 + pal * 2], 5,
                                palette[1 + pal * 2], 3, ColorFormat.ARGB8888);
                        else
                            result[i + j] = GfxUtil.InterpolateColor(palette[0 + pal * 2], 3,
                                palette[1 + pal * 2], 5, ColorFormat.ARGB8888);
                    }

                    d >>= 2;
                }
            }

            return result;
        }
    }
}