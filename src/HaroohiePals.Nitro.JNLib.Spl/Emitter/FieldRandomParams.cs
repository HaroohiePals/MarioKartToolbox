using HaroohiePals.IO;
using OpenTK.Mathematics;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class FieldRandomParams
    {
        public FieldRandomParams()
        {
        }

        public FieldRandomParams(EndianBinaryReader reader)
        {
            Strength = reader.ReadVecFx16();
            Interval = reader.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.WriteVecFx16(Strength);
            ew.Write(Interval);
        }

        public Vector3d Strength { get; set; }

        [XmlAttribute]
        public ushort Interval { get; set; }
    }
}