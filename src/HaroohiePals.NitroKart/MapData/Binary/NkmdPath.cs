using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdPath : NkmdSection<NkmdPath.PathEntry>
    {
        public const uint PATHSignature = 0x48544150;

        public NkmdPath()
            : base(PATHSignature, true) { }

        public NkmdPath(EndianBinaryReaderEx er)
            : base(er, PATHSignature, true) { }

        public class PathEntry : NkmdSectionEntry
        {
            public PathEntry()
            {
                Index  = 0;
                Loop   = false;
                NrPoit = 0;
            }

            public PathEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            public byte Index;

            [Type(FieldType.U8)]
            public bool Loop;

            public short NrPoit;
        }
    }
}