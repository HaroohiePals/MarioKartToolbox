using HaroohiePals.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class TexAnimParams
    {
        public TexAnimParams()
        {
        }

        public TexAnimParams(EndianBinaryReader reader)
        {
            Frames = reader.Read<byte>(8);
            FrameCount = reader.Read<byte>();
            Frames = Frames.Take(FrameCount).ToArray();
            FrameDuration = reader.Read<byte>() / 256f;
            ushort tmp = reader.Read<ushort>();
            IsRandom = (tmp & 1) == 1;
            Loop = (tmp >> 1 & 1) == 1;
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            FrameCount = (byte)Frames.Length;
            if (Frames.Length > 8)
                throw new Exception("Max number of tex anim frames is 8!");
            ew.Write(Frames, 0, Frames.Length);
            if (Frames.Length != 8)
                ew.Write(new byte[8 - Frames.Length], 0, 8 - Frames.Length);
            ew.Write(FrameCount);
            ew.Write(FloatHelper.ToByte(FrameDuration, nameof(FrameDuration)));
            ew.Write((ushort)(
                (IsRandom ? 1u : 0) |
                (Loop ? 1u : 0) << 1));
        }

        [XmlIgnore]
        public byte[] Frames { get; set; } = new byte[8];

        [XmlArray("Frames")]
        [XmlArrayItem("Frame")]
        public List<string> FrameTextureNames { get; set; } = new List<string>();

        [XmlIgnore]
        public byte FrameCount { get; set; }

        [XmlAttribute]
        public float FrameDuration { get; set; }

        /// <summary>
        /// When true particles use a random texture from the list of frames
        /// instead of performing a texture animation
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsRandom { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool Loop { get; set; }
    }
}