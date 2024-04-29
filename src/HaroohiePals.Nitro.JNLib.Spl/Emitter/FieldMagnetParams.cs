using HaroohiePals.IO;
using OpenTK.Mathematics;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class FieldMagnetParams
    {
        public FieldMagnetParams()
        {
        }

        public FieldMagnetParams(EndianBinaryReader reader)
        {
            MagnetPos = reader.ReadVecFx32();
            MagnetPower = reader.ReadFx16();
            reader.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.WriteVecFx32(MagnetPos);
            ew.WriteFx16(MagnetPower);
            ew.Write((ushort)0);
        }

        public Vector3d MagnetPos { get; set; }

        [XmlAttribute]
        public double MagnetPower { get; set; }
    }
}