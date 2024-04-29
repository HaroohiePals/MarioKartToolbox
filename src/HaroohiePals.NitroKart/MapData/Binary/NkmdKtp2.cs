using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdKtp2 : NkmdSection<NkmdKtp2.Ktp2Entry>
    {
        public const uint KTP2Signature = 0x3250544B;

        public NkmdKtp2()
            : base(KTP2Signature, false) { }

        public NkmdKtp2(EndianBinaryReaderEx er)
            : base(er, KTP2Signature, false) { }

        public class Ktp2Entry : NkmdSectionEntry
        {
            public Ktp2Entry()
            {
                Position = Vector3d.Zero;
                Rotation = Vector3d.Zero;
            }

            public Ktp2Entry(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
                er.Read<uint>();
            }

            public override void Write(EndianBinaryWriterEx er)
            {
                er.WriteObject(this);
                er.Write(0xFFFFFFFF);
            }

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;
        }
    }
}