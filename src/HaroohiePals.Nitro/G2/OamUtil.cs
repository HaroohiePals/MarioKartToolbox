using System.Collections.Generic;
using System.Numerics;

namespace HaroohiePals.Nitro.G2
{
    public class OamUtil
    {
        public enum ObjVramMode
        {
            Mode32K  = 0,
            Mode64K  = 1,
            Mode128K = 2,
            Mode256K = 3
        }

        private readonly struct ObjectSize
        {
            public ObjectSize(byte widthShift, byte heightShift)
            {
                WidthShift  = widthShift;
                HeightShift = heightShift;
            }

            public readonly byte WidthShift;
            public readonly byte HeightShift;
        }

        private static readonly ObjectSize[,] Objs =
        {
            { new(0, 0), new(1, 0), new(2, 0), new(2, 0) },
            { new(0, 1), new(1, 1), new(2, 1), new(2, 1) },
            { new(0, 2), new(1, 2), new(2, 2), new(3, 2) },
            { new(0, 2), new(1, 2), new(2, 3), new(3, 3) }
        };

        private static ObjectSize GetMaxObjectSize(int w, int h)
        {
            int logW = (w >= 8) ? 3 : BitOperations.Log2((uint)w);
            int logH = (h >= 8) ? 3 : BitOperations.Log2((uint)h);
            return Objs[logH, logW];
        }

        private static readonly GxOamShape[,] Shape =
        {
            { GxOamShape.Shape8x8, GxOamShape.Shape16x8, GxOamShape.Shape32x8, 0 },
            { GxOamShape.Shape8x16, GxOamShape.Shape16x16, GxOamShape.Shape32x16, 0 },
            { GxOamShape.Shape8x32, GxOamShape.Shape16x32, GxOamShape.Shape32x32, GxOamShape.Shape64x32 },
            { 0, 0, GxOamShape.Shape32x64, GxOamShape.Shape64x64 }
        };

        private static GxOamShape ObjSizeToShape(ObjectSize pSize)
            => Shape[pSize.HeightShift, pSize.WidthShift];

        public static GxOamAttr[] ArrangeObj1d(int areaWidth, int areaHeight, int x, int y, GxOamColorMode color,
            uint charName, ObjVramMode vramMode)
        {
            var  result           = new List<GxOamAttr>();
            var  pSize            = GetMaxObjectSize(areaWidth, areaHeight);
            int  objWidthShift    = pSize.WidthShift;
            int  objHeightShift   = pSize.HeightShift;
            uint objWidthMaskInv  = ~0u << objWidthShift;
            uint objHeightMaskInv = ~0u << objHeightShift;
            uint areaAWidth       = (uint)areaWidth & objWidthMaskInv;
            uint areaAHeight      = (uint)areaHeight & objHeightMaskInv;
            uint charNameUnit     = color == GxOamColorMode.Color16 ? 1u : 2u;

            // area A
            {
                var  shape        = ObjSizeToShape(pSize);
                int  xNum         = areaWidth >> objWidthShift;
                int  yNum         = areaHeight >> objHeightShift;
                uint charNameSize = ((charNameUnit << objWidthShift) << objHeightShift) >> (int)vramMode;
                int  ox, oy;

                for (oy = 0; oy < yNum; ++oy)
                {
                    int py = y + (oy << objHeightShift) * 8;

                    for (ox = 0; ox < xNum; ++ox)
                    {
                        int px = x + (ox << objWidthShift) * 8;

                        result.Add(new GxOamAttr
                        {
                            X         = px,
                            Y         = py,
                            Shape     = shape,
                            CharName  = charName,
                            ColorMode = color
                        });

                        charName += charNameSize;
                    }
                }
            }

            // area B
            if (areaAWidth < areaWidth)
            {
                uint areaBWidth  = (uint)areaWidth - areaAWidth;
                uint areaBHeight = areaAHeight;
                int  px          = x + (int)areaAWidth * 8;
                int  py          = y;

                result.AddRange(ArrangeObj1d((int)areaBWidth, (int)areaBHeight, px, py, color, charName, vramMode));
                charName += (charNameUnit * areaBWidth * areaBHeight) >> (int)vramMode;
            }

            // area C
            if (areaAHeight < areaHeight)
            {
                uint areaCWidth  = areaAWidth;
                uint areaCHeight = (uint)areaHeight - areaAHeight;
                int  px          = x;
                int  py          = y + (int)areaAHeight * 8;

                result.AddRange(ArrangeObj1d((int)areaCWidth, (int)areaCHeight, px, py, color, charName, vramMode));

                charName += (charNameUnit * areaCWidth * areaCHeight) >> (int)vramMode;
            }

            // area D
            if (areaAWidth < areaWidth && areaAHeight < areaHeight)
            {
                uint areaDWidth  = (uint)areaWidth - areaAWidth;
                uint areaDHeight = (uint)areaHeight - areaAHeight;
                int  px          = x + (int)areaAWidth * 8;
                int  py          = y + (int)areaAHeight * 8;

                result.AddRange(ArrangeObj1d((int)areaDWidth, (int)areaDHeight, px, py, color, charName, vramMode));
            }

            return result.ToArray();
        }
    }
}