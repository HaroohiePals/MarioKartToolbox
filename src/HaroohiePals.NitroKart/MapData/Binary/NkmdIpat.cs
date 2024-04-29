using HaroohiePals.IO;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdIpat : NkmdSection<NkmdIpatEpatEntry>
    {
        public const uint IPATSignature = 0x54415049;

        public NkmdIpat()
            : base(IPATSignature, true) { }

        public NkmdIpat(EndianBinaryReaderEx er)
            : base(er, IPATSignature, true) { }
    }
}