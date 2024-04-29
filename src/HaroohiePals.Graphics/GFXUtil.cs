using System;
using System.Drawing;

namespace HaroohiePals.Graphics
{
    public class GfxUtil
    {
        //public static Bitmap Resize(Bitmap original, int width, int height)
        //{
        //    if (original.Width == width && original.Height == height)
        //        return original;
        //    var res = new Bitmap(width, height);
        //    using (Graphics g = Graphics.FromImage(res))
        //    {
        //        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        g.PixelOffsetMode   = PixelOffsetMode.HighQuality;
        //        g.SmoothingMode     = SmoothingMode.HighQuality;
        //        g.DrawImage(original, 0, 0, width, height);
        //        g.Flush();
        //    }

        //    return res;
        //}

        //public static unsafe Bitmap StripAlpha(Bitmap original)
        //{
        //    var result = new Bitmap(original.Width, original.Height);
        //    var bd = original.LockBits(
        //        new Rectangle(0, 0, original.Width, original.Height),
        //        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        //    var bd2 = result.LockBits(
        //        new Rectangle(0, 0, result.Width, result.Height),
        //        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        //    uint* pOrig = (uint*)bd.Scan0;
        //    uint* pDst  = (uint*)bd2.Scan0;
        //    for (int y = 0; y < original.Height; y++)
        //    {
        //        uint* pOrigLine = pOrig;
        //        uint* pDstLine  = pDst;
        //        for (int x = 0; x < original.Width; x++)
        //            *pDstLine++ = *pOrigLine++ | 0xFF000000;
        //        pOrig += bd.Stride / 4;
        //        pDst  += bd2.Stride / 4;
        //    }

        //    result.UnlockBits(bd2);
        //    original.UnlockBits(bd);
        //    return result;
        //}

        //public static unsafe Bitmap AlphaToGray(Bitmap original)
        //{
        //    var result = new Bitmap(original.Width, original.Height);
        //    var bd = original.LockBits(
        //        new Rectangle(0, 0, original.Width, original.Height),
        //        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        //    var bd2 = result.LockBits(
        //        new Rectangle(0, 0, result.Width, result.Height),
        //        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        //    uint* pOrig = (uint*)bd.Scan0;
        //    uint* pDst  = (uint*)bd2.Scan0;
        //    for (int y = 0; y < original.Height; y++)
        //    {
        //        uint* pOrigLine = pOrig;
        //        uint* pDstLine  = pDst;
        //        for (int x = 0; x < original.Width; x++)
        //        {
        //            uint alpha = *pOrigLine++ >> 24;
        //            *pDstLine++ = 0xFF000000 | alpha | (alpha << 8) | (alpha << 16);
        //        }

        //        pOrig += bd.Stride / 4;
        //        pDst  += bd2.Stride / 4;
        //    }

        //    result.UnlockBits(bd2);
        //    original.UnlockBits(bd);
        //    return result;
        //}

        public static uint[] ConvertColorFormat(ReadOnlySpan<uint> inColor, ColorFormat inputFormat, ColorFormat outputFormat)
        {
            uint[] output = new uint[inColor.Length];
            for (int i = 0; i < inColor.Length; i++)
                output[i] = ConvertColorFormat(inColor[i], inputFormat, outputFormat);
            return output;
        }

        public static uint[] ConvertColorFormatFromU16(ReadOnlySpan<ushort> inColor, ColorFormat inputFormat,
            ColorFormat outputFormat)
        {
            uint[] output = new uint[inColor.Length];
            for (int i = 0; i < inColor.Length; i++)
                output[i] = ConvertColorFormat(inColor[i], inputFormat, outputFormat);
            return output;
        }

        public static ushort[] ConvertColorFormatToU16(ReadOnlySpan<uint> inColor, ColorFormat inputFormat,
            ColorFormat outputFormat)
        {
            ushort[] output = new ushort[inColor.Length];
            for (int i = 0; i < inColor.Length; i++)
                output[i] = (ushort)ConvertColorFormat(inColor[i], inputFormat, outputFormat);
            return output;
        }

        public static ushort[] ConvertColorFormatU16(ReadOnlySpan<ushort> inColor, ColorFormat inputFormat,
            ColorFormat outputFormat)
        {
            ushort[] output = new ushort[inColor.Length];
            for (int i = 0; i < inColor.Length; i++)
                output[i] = (ushort)ConvertColorFormat(inColor[i], inputFormat, outputFormat);
            return output;
        }

        public static uint ConvertColorFormat(uint inColor, ColorFormat inputFormat, ColorFormat outputFormat)
        {
            if (inputFormat == outputFormat)
                return inColor;
            //From color format to components:
            uint a, mask;
            if (inputFormat.ASize == 0)
                a = 255;
            else
            {
                mask = ~(0xFFFFFFFFu << inputFormat.ASize);
                a    = ((((inColor >> inputFormat.AShift) & mask) * 255u) + mask / 2) / mask;
            }

            mask = ~(0xFFFFFFFFu << inputFormat.RSize);
            uint r = ((((inColor >> inputFormat.RShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << inputFormat.GSize);
            uint g = ((((inColor >> inputFormat.GShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << inputFormat.BSize);
            uint b = ((((inColor >> inputFormat.BShift) & mask) * 255u) + mask / 2) / mask;
            return ToColorFormat(a, r, g, b, outputFormat);
        }

        public static uint ToColorFormat(int r, int g, int b, ColorFormat outputFormat)
            => ToColorFormat(255u, (uint)r, (uint)g, (uint)b, outputFormat);

        public static uint ToColorFormat(int a, int r, int g, int b, ColorFormat outputFormat)
            => ToColorFormat((uint)a, (uint)r, (uint)g, (uint)b, outputFormat);

        public static uint ToColorFormat(uint r, uint g, uint b, ColorFormat outputFormat)
            => ToColorFormat(255u, r, g, b, outputFormat);

        public static uint ToColorFormat(uint a, uint r, uint g, uint b, ColorFormat outputFormat)
        {
            uint result = 0;
            uint mask;
            if (outputFormat.ASize != 0)
            {
                mask   =  ~(0xFFFFFFFFu << outputFormat.ASize);
                result |= ((a * mask + 127u) / 255u) << outputFormat.AShift;
            }

            mask   =  ~(0xFFFFFFFFu << outputFormat.RSize);
            result |= ((r * mask + 127u) / 255u) << outputFormat.RShift;
            mask   =  ~(0xFFFFFFFFu << outputFormat.GSize);
            result |= ((g * mask + 127u) / 255u) << outputFormat.GShift;
            mask   =  ~(0xFFFFFFFFu << outputFormat.BSize);
            result |= ((b * mask + 127u) / 255u) << outputFormat.BShift;
            return result;
        }

        public static uint SetAlpha(uint color, uint a, ColorFormat format)
        {
            uint result = color;
            if (format.ASize == 0)
                return color;
            uint mask = ~(0xFFFFFFFFu << format.ASize);
            result &= ~(mask << format.AShift);
            result |= ((a * mask + 127u) / 255u) << format.AShift;
            return result;
        }

        public static uint InterpolateColor(uint colorA, int factorA, uint colorB, int factorB, ColorFormat format)
        {
            uint aa, ra, ga, ba;
            uint mask;
            if (format.ASize == 0)
                aa = 255;
            else
            {
                mask = ~(0xFFFFFFFFu << format.ASize);
                aa   = ((((colorA >> format.AShift) & mask) * 255u) + mask / 2) / mask;
            }

            mask = ~(0xFFFFFFFFu << format.RSize);
            ra   = ((((colorA >> format.RShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << format.GSize);
            ga   = ((((colorA >> format.GShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << format.BSize);
            ba   = ((((colorA >> format.BShift) & mask) * 255u) + mask / 2) / mask;
            uint ab, rb, gb, bb;
            if (format.ASize == 0)
                ab = 255;
            else
            {
                mask = ~(0xFFFFFFFFu << format.ASize);
                ab   = ((((colorB >> format.AShift) & mask) * 255u) + mask / 2) / mask;
            }

            mask = ~(0xFFFFFFFFu << format.RSize);
            rb   = ((((colorB >> format.RShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << format.GSize);
            gb   = ((((colorB >> format.GShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << format.BSize);
            bb   = ((((colorB >> format.BShift) & mask) * 255u) + mask / 2) / mask;
            return ToColorFormat(
                (uint)((aa * factorA + ab * factorB) / (factorA + factorB)),
                (uint)((ra * factorA + rb * factorB) / (factorA + factorB)),
                (uint)((ga * factorA + gb * factorB) / (factorA + factorB)),
                (uint)((ba * factorA + bb * factorB) / (factorA + factorB)), format);
        }

        public static uint[] ColorToU32(ReadOnlySpan<Color> colors)
        {
            var result = new uint[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                result[i] = (uint)colors[i].ToArgb();
            return result;
        }

        public static Color[] U32ToColor(ReadOnlySpan<uint> colors)
        {
            var result = new Color[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                result[i] = Color.FromArgb((int)colors[i]);
            return result;
        }

        public static uint[] Detile(ReadOnlySpan<uint> data, int tileSize, int width, int height)
        {
            uint[] result = new uint[width * height];
            int    offset = 0;
            for (int y = 0; y < height; y += tileSize)
                for (int x = 0; x < width; x += tileSize)
                    for (int y2 = 0; y2 < tileSize; y2++)
                        for (int x2 = 0; x2 < tileSize; x2++)
                            result[(y + y2) * width + x + x2] = data[offset++];
            return result;
        }
    }
}