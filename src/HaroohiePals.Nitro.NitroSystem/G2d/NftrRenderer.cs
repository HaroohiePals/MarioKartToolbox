using HaroohiePals.Graphics;
using System.Drawing;

namespace HaroohiePals.Nitro.NitroSystem.G2d
{
    public static class NftrRenderer
    {
        private static int GetStringWidth(Nftr font, int hSpace, string text, int stringOffset)
            => GetStringWidth(font, hSpace, text, ref stringOffset);

        private static int GetStringWidth(Nftr font, int hSpace, string text, ref int stringOffset)
        {
            int width = 0;
            while (stringOffset < text.Length)
            {
                char c = text[stringOffset++];
                if (c == '\r')
                    continue;
                if (c == '\n')
                    break;
                width += font.GetCharWidth(c) + hSpace;
            }

            if (width > 0)
                width -= hSpace;
            return width;
        }


        public static int GetTextHeight(Nftr font, int vSpace, string text)
        {
            int lines = 1;
            foreach (var c in text)
                if (c == '\n')
                    lines++;
            return lines * (font.FontInformation.LineFeed + vSpace) - vSpace;
        }

        public static int GetTextWidth(Nftr font, int hSpace, string text, int stringOffset = 0)
        {
            int width = 0;
            while (stringOffset < text.Length)
            {
                int lineWidth = GetStringWidth(font, hSpace, text, ref stringOffset);
                if (lineWidth > width)
                    width = lineWidth;
            }

            return width;
        }

        private static byte ReadBits(byte[] data, int offset, int nrBits)
        {
            int baseOffset = offset / 8;
            int bitOffset  = offset % 8;
            if (bitOffset + nrBits > 8)
            {
                int  a     = 8 - bitOffset;
                int  b     = nrBits - a;
                byte adata = (byte)((data[baseOffset] >> (8 - bitOffset - a)) & ((1 << a) - 1));
                byte bdata = (byte)((data[baseOffset + 1] >> (8 - b)) & ((1 << b) - 1));
                return (byte)((adata << b) | bdata);
            }

            return (byte)((data[baseOffset] >> (8 - bitOffset - nrBits)) & ((1 << nrBits) - 1));
        }

        private static int DrawChar(Nftr font, char c, Rgba8Bitmap dst, int x, int y, Color[] palette, int baseColor)
        {
            ushort glyphIdx = font.GetGlyphIdxFromCharacterCode(c);
            byte[] glyph    = font.CharacterGlyphs.Glyphs[glyphIdx];
            int    stride   = font.CharacterGlyphs.CellWidth * font.CharacterGlyphs.Bpp;
            int    mask     = (1 << font.CharacterGlyphs.Bpp) - 1;
            var    cw       = font.GetCharWidthsFromGlyphIdx(glyphIdx);
            x += cw.Left;
            for (int y2 = 0; y2 < font.CharacterGlyphs.CellHeight; y2++)
            {
                for (int x2 = 0; x2 < font.CharacterGlyphs.CellWidth; x2++)
                {
                    if (x + x2 < 0 || x + x2 >= dst.Width || y + y2 < 0 || y + y2 >= dst.Height)
                        continue;
                    int pos = y2 * stride + x2 * font.CharacterGlyphs.Bpp;
                    //int shift = (8 - (pos % 8)) - font.CharacterGlyphs.Bpp;
                    int val = ReadBits(glyph, pos, font.CharacterGlyphs.Bpp); //(glyph[pos / 8] >> shift) & mask;
                    if (val == 0)
                        continue;
                    if (baseColor + val >= palette.Length)
                        continue;
                    dst[x + x2, y + y2] = (uint)palette[baseColor + val].ToArgb();
                }
            }

            return cw.GetCharWidth();
        }

        public static Rgba8Bitmap DrawTextRect(Nftr font, int hSpace, int vSpace, int w, int h, XOrigin hAlignment,
            YOrigin vAlignment, Color[] palette, int baseColor, string text)
        {
            var result = new Rgba8Bitmap(w, h);
            int x      = 0, y = 0;
            if (vAlignment == YOrigin.Bottom)
                y += h - GetTextHeight(font, vSpace, text);
            else if (vAlignment == YOrigin.Center)
                y += (h + 1) / 2 - (GetTextHeight(font, vSpace, text) + 1) / 2;

            int linefeed     = font.FontInformation.LineFeed + vSpace;
            int py           = y;
            int stringOffset = 0;
            while (stringOffset < text.Length)
            {
                int px = x;

                if (hAlignment == XOrigin.Right)
                    px += w - GetStringWidth(font, hSpace, text, stringOffset);
                else if (hAlignment == XOrigin.Center)
                    px += (w + 1) / 2 - (GetStringWidth(font, hSpace, text, stringOffset) + 1) / 2;

                {
                    while (stringOffset < text.Length)
                    {
                        char c = text[stringOffset++];
                        if (c == '\r')
                            continue;
                        if (c == '\n')
                            break;
                        px += DrawChar(font, c, result, px, py, palette, baseColor);
                        px += hSpace;
                    }
                }
                py += linefeed;
            }

            return result;
        }
    }
}