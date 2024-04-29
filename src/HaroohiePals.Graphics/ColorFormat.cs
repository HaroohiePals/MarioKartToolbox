namespace HaroohiePals.Graphics
{
    public class ColorFormat
    {
        public readonly int AShift, ASize, RShift, RSize, GShift, GSize, BShift, BSize;

        public ColorFormat(int aShift, int aSize, int rShift, int rSize, int gShift, int gSize, int bShift, int bSize)
        {
            AShift = aShift;
            ASize = aSize;
            RShift = rShift;
            RSize = rSize;
            GShift = gShift;
            GSize = gSize;
            BShift = bShift;
            BSize = bSize;
        }

        public int NrBytes => (int)System.Math.Ceiling((ASize + RSize + GSize + BSize) / 8f);

        //The naming is based on the bit order when read out in the correct endianness
        public static readonly ColorFormat ARGB8888 = new(24, 8, 16, 8, 8, 8, 0, 8);

        public static readonly ColorFormat ARGB3444 = new(12, 3, 8, 4, 4, 4, 0, 4);

        public static readonly ColorFormat RGBA8888 = new(0, 8, 24, 8, 16, 8, 8, 8);

        public static readonly ColorFormat RGBA4444 = new(0, 4, 12, 4, 8, 4, 4, 4);

        public static readonly ColorFormat RGB888 = new(0, 0, 16, 8, 8, 8, 0, 8);

        public static readonly ColorFormat RGB565 = new(0, 0, 11, 5, 5, 6, 0, 5);

        public static readonly ColorFormat ARGB1555 = new(15, 1, 10, 5, 5, 5, 0, 5);
        public static readonly ColorFormat XRGB1555 = new(0, 0, 10, 5, 5, 5, 0, 5);

        public static readonly ColorFormat ABGR1555 = new(15, 1, 0, 5, 5, 5, 10, 5);
        public static readonly ColorFormat XBGR1555 = new(0, 0, 0, 5, 5, 5, 10, 5);

        public static readonly ColorFormat RGBA5551 = new(0, 1, 11, 5, 6, 5, 1, 5);
    }
}
