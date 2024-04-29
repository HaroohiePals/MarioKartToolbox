using HaroohiePals.IO;
using OpenTK.Mathematics;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class FieldConvergenceParams
    {
        public FieldConvergenceParams()
        {
        }

        public FieldConvergenceParams(EndianBinaryReader reader)
        {
            ConvergencePos = reader.ReadVecFx32();
            ConvergenceRatio = reader.ReadFx16();
            reader.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.WriteVecFx32(ConvergencePos);
            ew.WriteFx16(ConvergenceRatio);
            ew.Write((ushort)0);
        }

        public Vector3d ConvergencePos { get; set; }

        [XmlAttribute]
        public double ConvergenceRatio { get; set; }
    }
}