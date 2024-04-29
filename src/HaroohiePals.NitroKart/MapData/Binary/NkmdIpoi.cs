using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdIpoi : NkmdSection<NkmdIpoi.IpoiEntry>
    {
        public const uint IPOISignature = 0x494F5049;

        private ushort _version = 37;

        public NkmdIpoi(ushort version = 37) : base(IPOISignature, true)
        {
            _version = version;
        }

        public NkmdIpoi(EndianBinaryReaderEx er, ushort version)
        {
            _version = version;
            uint signature = er.Read<uint>();
            if (signature != IPOISignature)
                throw new SignatureNotCorrectException(signature, IPOISignature, er.BaseStream.Position - 4);
            uint nrEntries = er.Read<uint>();
            for (int i = 0; i < nrEntries; i++)
                Entries.Add(new IpoiEntry(er, version));
        }

        public override void Write(EndianBinaryWriterEx er)
        {
            er.Write(IPOISignature);
            er.Write((uint)Entries.Count);
            foreach (var entry in Entries)
                entry.Write(er, _version);

            if (Entries.Count == 0)
                er.Write(0);
        }

        public class IpoiEntry : NkmdSectionEntry
        {
            public IpoiEntry() { }

            public IpoiEntry(EndianBinaryReaderEx er, ushort version)
            {
                er.ReadObject(this);
                if (version >= 34)
                {
                    RecalculationIndex = er.Read<byte>();
                    er.ReadPadding(4);
                }
            }

            public override void Write(EndianBinaryWriterEx er) => Write(er, 37);

            public void Write(EndianBinaryWriterEx er, ushort version)
            {
                er.WriteObject(this);
                if (version >= 34)
                {
                    er.Write(RecalculationIndex);
                    er.WritePadding(4);
                }
            }

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public double Radius;

            [Ignore]
            public byte RecalculationIndex;
        }
    }
}