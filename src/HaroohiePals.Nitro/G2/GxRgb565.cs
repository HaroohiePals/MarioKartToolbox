using System.Drawing;

namespace HaroohiePals.Nitro.G2
{
    public struct GxRgb565
    {
        private int _r, _g, _b;

        public GxRgb565(ushort packed)
        {
            _r = packed & 0x1F;
            _g = (((packed >> 5) & 0x1F) << 1) | (packed >> 15);
            _b = (packed >> 10) & 0x1F;
        }

        public GxRgb565(Color color)
        {
            _r = (color.R * 31 + 128) / 255;
            _g = (color.G * 63 + 128) / 255;
            _b = (color.B * 31 + 128) / 255;
        }

        public GxRgb565(int r, int g, int b)
        {
            _r = 0;
            _g = 0;
            _b = 0;
            R = r;
            G = g;
            B = b;
        }

        public int R
        {
            get => _r;
            set
            {
                int v = value;
                if (v < 0)
                    v = 0;
                if (v > 31)
                    v = 31;
                _r = v;
            }
        }

        public int G
        {
            get => _g;
            set
            {
                int v = value;
                if (v < 0)
                    v = 0;
                if (v > 63)
                    v = 63;
                _g = v;
            }
        }

        public int B
        {
            get => _b;
            set
            {
                int v = value;
                if (v < 0)
                    v = 0;
                if (v > 31)
                    v = 31;
                _b = v;
            }
        }

        public ushort Packed => (ushort)(_r | ((_g >> 1) << 5) | (_b << 10) | ((_g & 1) << 15));

        public Color Color => Color.FromArgb((_r * 255 + 16) / 31, (_g * 255 + 32) / 63, (_b * 255 + 16) / 31);

        public static implicit operator GxRgb565(ushort color) => new GxRgb565(color);
    }
}
