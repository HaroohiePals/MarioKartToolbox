using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdIpatEpatEntry : NkmdSectionEntry
    {
        public NkmdIpatEpatEntry()
        {
            Next     = new byte[] { 0xFF, 0xFF, 0xFF };
            Previous = new byte[] { 0xFF, 0xFF, 0xFF };
        }

        public NkmdIpatEpatEntry(EndianBinaryReaderEx er)
        {
            er.ReadObject(this);
            er.ReadPadding(4);
        }

        public override void Write(EndianBinaryWriterEx er)
        {
            er.WriteObject(this);
            er.WritePadding(4);
        }

        public short StartIndex;
        public short Length;

        [ArraySize(3)]
        public byte[] Next;

        [ArraySize(3)]
        public byte[] Previous;
    }
}