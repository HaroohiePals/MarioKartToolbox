using HaroohiePals.IO;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdEpat : NkmdSection<NkmdIpatEpatEntry>
    {
        public const uint EPATSignature = 0x54415045;

        public NkmdEpat()
            : base(EPATSignature, true) { }

        public NkmdEpat(EndianBinaryReaderEx er)
            : base(er, EPATSignature, true) { }
    }
}