using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdPoit : NkmdSection<NkmdPoit.PoitEntry>
    {
        public const uint POITSignature = 0x54494F50;

        public NkmdPoit()
            : base(POITSignature, true) { }

        public NkmdPoit(EndianBinaryReaderEx er)
            : base(er, POITSignature, true) { }

        public class PoitEntry : NkmdSectionEntry
        {
            public PoitEntry()
            {
                Position = new Vector3d(0, 0, 0);
                Index = 0;
                Unknown1 = 0;
                Duration = 0;
                Unknown2 = 0;
            }

            public PoitEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector3d Position;

            public byte  Index;
            public byte  Unknown1;
            public short Duration;
            public uint  Unknown2;
        }
    }
}