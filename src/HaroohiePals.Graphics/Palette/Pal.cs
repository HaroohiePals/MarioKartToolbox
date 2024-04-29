using HaroohiePals.IO;
using System.Drawing;
using System.IO;
using System.Text;

namespace HaroohiePals.Graphics.Palette
{
    public class Pal
    {
        public const uint RiffSignature = 0x46464952;

        public Pal(Color[] colors)
        {
            Signature   = RiffSignature;
            PaletteData = new PalData(colors);
        }

        public Pal(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Pal(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                Signature   = er.ReadSignature(RiffSignature);
                FileSize    = er.Read<uint>();
                PaletteData = new PalData(er);
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriter(m, Endianness.LittleEndian);
            er.Write(RiffSignature);
            er.Write(0u);
            PaletteData.Write(er);
            er.BaseStream.Position = 0x4;
            er.Write((uint)(er.BaseStream.Length - 8));
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        public uint    Signature;
        public uint    FileSize;
        public PalData PaletteData;

        public class PalData
        {
            public PalData(Color[] colors)
            {
                Signature = "PAL data";
                Version   = 0x300;
                NrColors  = (ushort)colors.Length;
                Palette   = colors;
            }

            public PalData(EndianBinaryReader er)
            {
                Signature = er.ReadString(Encoding.ASCII, 8);
                if (Signature != "PAL data")
                    throw new SignatureNotCorrectException(Signature, "PAL data", er.BaseStream.Position - 8);
                BlockSize = er.Read<uint>();
                Version   = er.Read<ushort>();
                NrColors  = er.Read<ushort>();
                Palette   = new Color[NrColors];
                for (int i = 0; i < Palette.Length; i++)
                    Palette[i] = Color.FromArgb(er.Read<int>());
            }

            public void Write(EndianBinaryWriter er)
            {
                er.Write("PAL data", Encoding.ASCII, false);
                er.Write((uint)(4 + Palette.Length * 4));
                er.Write(Version);
                er.Write((ushort)Palette.Length);
                for (int i = 0; i < Palette.Length; i++)
                    er.Write(Palette[i].ToArgb());
            }

            public string  Signature;
            public uint    BlockSize;
            public ushort  Version;
            public ushort  NrColors;
            public Color[] Palette;
        }
    }
}