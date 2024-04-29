using HaroohiePals.IO;
using System.Drawing;
using System.IO;

namespace HaroohiePals.Graphics.Palette
{
    public class Act
    {
        public Act(Color[] colors)
        {
            Palette = colors;
        }

        public Act(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Act(Stream stream)
        {
            using (var er = new EndianBinaryReader(stream, Endianness.LittleEndian))
            {
                Palette = new Color[256];
                for (int i = 0; i < 256; i++)
                    Palette[i] = Color.FromArgb(er.Read<byte>(), er.Read<byte>(), er.Read<byte>());
                if (er.BaseStream.Position != er.BaseStream.Length)
                {
                    NrColorsUsed     = er.Read<ushort>();
                    TransparentColor = er.Read<ushort>();
                }
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            for (int i = 0; i < 256; i++)
            {
                er.Write(Palette[i].R);
                er.Write(Palette[i].G);
                er.Write(Palette[i].B);
            }

            if (NrColorsUsed != 256 || TransparentColor != 0xFFFF)
            {
                er.Write(NrColorsUsed);
                er.Write(TransparentColor);
            }

            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public Color[] Palette;
        public ushort  NrColorsUsed     = 256;
        public ushort  TransparentColor = 0xFFFF;
    }
}