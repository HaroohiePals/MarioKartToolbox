using HaroohiePals.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class AlphaAnimParams
    {
        public AlphaAnimParams()
        {
        }

        public AlphaAnimParams(EndianBinaryReader er)
        {
            ushort tmp = er.Read<ushort>();
            InitialAlpha = (byte)(tmp & 0x1F);
            PeakAlpha = (byte)(tmp >> 5 & 0x1F);
            EndingAlpha = (byte)(tmp >> 10 & 0x1F);
            Randomness = er.Read<byte>() / 256f;
            Loop = (er.Read<byte>() & 1) == 1;
            InEndTime = er.Read<byte>() / 256f;
            OutStartTime = er.Read<byte>() / 256f;
            er.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.Write((ushort)(
                InitialAlpha & 0x1Fu |
                (PeakAlpha & 0x1Fu) << 5 |
                (EndingAlpha & 0x1Fu) << 10));
            ew.Write(FloatHelper.ToByte(Randomness, nameof(Randomness)));
            ew.Write((byte)(Loop ? 1 : 0));
            ew.Write(FloatHelper.ToByte(InEndTime, nameof(InEndTime)));
            ew.Write(FloatHelper.ToByte(OutStartTime, nameof(OutStartTime)));
            ew.Write((ushort)0);
        }

        public byte InitialAlpha { get; set; }
        public byte PeakAlpha { get; set; }
        public byte EndingAlpha { get; set; }
        public float Randomness { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool Loop { get; set; }

        /// <summary>
        /// Moment at which PeakAlpha is reached as a value between 0 and 1
        /// </summary>
        public float InEndTime { get; set; }

        /// <summary>
        /// Moment at which PeakAlpha starts to interpolate towards EndingAlpha
        /// as a value between 0 and 1
        /// </summary>
        public float OutStartTime { get; set; }
    }
}