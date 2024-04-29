using HaroohiePals.IO;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    public class FieldCollisionParams
    {
        public enum CollisionBehavior
        {
            Die = 0,
            Bounce
        }

        public FieldCollisionParams()
        {
        }

        public FieldCollisionParams(EndianBinaryReader reader)
        {
            CollisionPlaneY = reader.ReadFx32();
            BounceCoef = reader.ReadFx16();
            Behavior = (CollisionBehavior)(reader.Read<ushort>() & 3);
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.WriteFx32(CollisionPlaneY);
            ew.WriteFx16(BounceCoef);
            ew.Write((ushort)((uint)Behavior & 3));
        }

        [XmlAttribute]
        public double CollisionPlaneY { get; set; }

        [XmlAttribute]
        public double BounceCoef { get; set; }

        [XmlAttribute]
        public CollisionBehavior Behavior { get; set; }
    }
}