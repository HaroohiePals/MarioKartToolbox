using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdKtps : NkmdSection<NkmdKtps.KtpsEntry>
    {
        public const uint KTPSSignature = 0x5350544B;

        public NkmdKtps()
            : base(KTPSSignature, false) { }

        public NkmdKtps(EndianBinaryReaderEx er)
            : base(er, KTPSSignature, false) { }

        public class KtpsEntry : NkmdSectionEntry
        {
            public KtpsEntry()
            {
                Position = new Vector3d(0, 0, 0);
                Rotation = new Vector3d(0, 0, 0);
                Index    = -1;
            }

            public KtpsEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;

            public ushort Padding = 0xFFFF;

            public short Index;
        }
    }
}