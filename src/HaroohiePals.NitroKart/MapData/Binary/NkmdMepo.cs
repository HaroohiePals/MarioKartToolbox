using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdMepo : NkmdSection<NkmdMepo.MepoEntry>
    {
        public const uint MEPOSignature = 0x4F50454D;

        public NkmdMepo()
            : base(MEPOSignature, false) { }

        public NkmdMepo(EndianBinaryReaderEx er)
            : base(er, MEPOSignature, false) { }

        public NkmdIpoi ToIpoi()
        {
            var ipoi = new NkmdIpoi();
            foreach (var entry in Entries)
                ipoi.Entries.Add(entry.ToIpoiEntry());
            return ipoi;
        }

        public class MepoEntry : NkmdSectionEntry
        {
            public MepoEntry() { }

            public MepoEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            public NkmdIpoi.IpoiEntry ToIpoiEntry()
                => new NkmdIpoi.IpoiEntry
                {
                    Position = Position,
                    Radius   = Radius
                };

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public double Radius;

            public ushort Unknown0;
            public ushort Unknown1;
            public uint   Unknown2;
        }
    }
}