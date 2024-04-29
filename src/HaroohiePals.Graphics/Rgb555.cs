using System;
using System.Drawing;
using System.Xml.Serialization;

namespace HaroohiePals.Graphics
{
    public struct Rgb555
    {
        public static Rgb555 Black => new Rgb555(0, 0, 0);
        public static Rgb555 White => new Rgb555(31, 31, 31);

        public Rgb555(int r, int g, int b)
        {
            if (r < 0 || r > 31)
                throw new ArgumentOutOfRangeException(nameof(r));
            if (g < 0 || g > 31)
                throw new ArgumentOutOfRangeException(nameof(g));
            if (b < 0 || b > 31)
                throw new ArgumentOutOfRangeException(nameof(b));
            R = r;
            G = g;
            B = b;
        }

        public Rgb555(ushort value)
        {
            R = value & 0x1F;
            G = (value >> 5) & 0x1F;
            B = (value >> 10) & 0x1F;
        }

        public Rgb555(Color value)
        {
            R = value.R * 31 / 255;
            G = value.G * 31 / 255;
            B = value.B * 31 / 255;
        }

        [XmlAttribute("r")]
        public int R { get; set; }

        [XmlAttribute("g")]
        public int G { get; set; }

        [XmlAttribute("b")]
        public int B { get; set; }

        [XmlIgnore]
        public ushort Packed => (ushort) (R | (G << 5) | (B << 10));

        public Color Color => Color.FromArgb((R & 0x1F) * 255 / 31, (G & 0x1F) * 255 / 31, (B & 0x1F) * 255 / 31);

        public static implicit operator Rgb555(ushort value) => new Rgb555(value);
        public static implicit operator ushort(Rgb555 color) => color.Packed;
        public static implicit operator Color(Rgb555 color) => color.Color;
    }
}