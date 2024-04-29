using HaroohiePals.IO;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class FieldSpinParams
    {
        public enum SpinType
        {
            X = 0,
            Y,
            Z
        }

        public FieldSpinParams()
        {
        }

        public FieldSpinParams(EndianBinaryReader reader)
        {
            Rotation = reader.Read<short>() * 360f / 65536f;
            Type = (SpinType)reader.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.Write((ushort)System.Math.Round(Rotation * 65536f / 360f));
            ew.Write((ushort)Type);
        }

        /// <summary>
        /// The amount of rotation applied in degrees
        /// </summary>
        [XmlAttribute]
        public float Rotation { get; set; }

        /// <summary>
        /// The axis around which the rotation is performed
        /// </summary>
        [XmlAttribute]
        public SpinType Type { get; set; }
    }
}