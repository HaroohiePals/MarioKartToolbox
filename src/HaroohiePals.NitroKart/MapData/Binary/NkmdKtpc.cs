using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdKtpc : NkmdSection<NkmdKtpc.KtpcEntry>
    {
        public const uint KTPCSignature = 0x4350544B;

        public NkmdKtpc()
            : base(KTPCSignature, false) { }

        public NkmdKtpc(EndianBinaryReaderEx er)
            : base(er, KTPCSignature, false) { }


        public class KtpcEntry : NkmdSectionEntry
        {
            public KtpcEntry()
            {
                Position = Vector3d.Zero;
                Rotation = Vector3d.Zero;
                NextMEPO = -1;
                Index    = -1;
            }

            public KtpcEntry(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);
            }

            public override void Write(EndianBinaryWriterEx er)
            {
                er.WriteObject(this);
            }

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;

            public short NextMEPO;

            public short Index;
        }
    }
}