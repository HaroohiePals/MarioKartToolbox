using HaroohiePals.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class ScaleAnimParams
    {
        public ScaleAnimParams()
        {
        }

        public ScaleAnimParams(EndianBinaryReader reader)
        {
            InitialScale = reader.ReadFx16();
            IntermediateScale = reader.ReadFx16();
            EndingScale = reader.ReadFx16();
            InEndTime = reader.Read<byte>() / 256f;
            OutStartTime = reader.Read<byte>() / 256f;
            Loop = (reader.Read<ushort>() & 1) == 1;
            reader.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.WriteFx16(InitialScale);
            ew.WriteFx16(IntermediateScale);
            ew.WriteFx16(EndingScale);
            ew.Write(FloatHelper.ToByte(InEndTime, nameof(InEndTime)));
            ew.Write(FloatHelper.ToByte(OutStartTime, nameof(OutStartTime)));
            ew.Write((ushort)(Loop ? 1 : 0));
            ew.Write((ushort)0);
        }

        public double InitialScale { get; set; } = 1;
        public double IntermediateScale { get; set; } = 1;
        public double EndingScale { get; set; } = 1;
        public double InEndTime { get; set; }
        public double OutStartTime { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool Loop { get; set; }
    }
}