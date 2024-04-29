using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdKtpm : NkmdSection<NkmdKtpm.KtpmEntry>
    {
        public const uint KTPMSignature = 0x4D50544B;

        public NkmdKtpm()
            : base(KTPMSignature, false) { }

        public NkmdKtpm(EndianBinaryReaderEx er)
            : base(er, KTPMSignature, false) { }

        public class KtpmEntry : NkmdSectionEntry
        {
            public KtpmEntry()
            {
                Position = new Vector3d(0, 0, 0);
                Rotation = new Vector3d(0, 0, 0);
                Unknown  = 0xFFFF;
                Index    = 1;
            }

            public KtpmEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;

            public ushort Unknown;
            public short  Index;
        }
    }
}