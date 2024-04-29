using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdMepa : NkmdSection<NkmdMepa.MepaEntry>
    {
        public const uint MEPASignature = 0x4150454D;

        public NkmdMepa()
            : base(MEPASignature, false) { }

        public NkmdMepa(EndianBinaryReaderEx er)
            : base(er, MEPASignature, false) { }

        public class MepaEntry : NkmdSectionEntry
        {
            public MepaEntry()
            {
                Next = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                Previous = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            }

            public MepaEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            public short StartIndex;
            public short Length;

            [ArraySize(8)]
            public byte[] Next;

            [ArraySize(8)]
            public byte[] Previous;
        }
    }
}