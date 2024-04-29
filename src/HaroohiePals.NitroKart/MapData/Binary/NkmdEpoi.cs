using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdEpoi : NkmdSection<NkmdEpoi.EpoiEntry>
    {
        public const uint EPOISignature = 0x494F5045;

        public NkmdEpoi()
            : base(EPOISignature, true) { }

        public NkmdEpoi(EndianBinaryReaderEx er)
            : base(er, EPOISignature, true) { }

        public class EpoiEntry : NkmdSectionEntry
        {
            public enum DriftValue : short
            {
                NoAction,
                StartLeftPowerslide,
                StartLeftDrift,
                StartRightPowerslide,
                StartRightDrift,
                EndDrift
            }

            public EpoiEntry() { }

            public EpoiEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public double Radius;

            public DriftValue Drifting;

            public ushort Unknown0;
            public uint   Unknown1;
        }
    }
}