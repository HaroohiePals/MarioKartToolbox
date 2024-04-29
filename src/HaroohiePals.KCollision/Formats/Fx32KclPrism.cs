using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class Fx32KclPrism : KclPrism
    {
        public Fx32KclPrism() { }

        public Fx32KclPrism(EndianBinaryReaderEx er)
        {
            Height    = er.ReadFx32();
            PosIdx    = er.Read<ushort>();
            FNrmIdx   = er.Read<ushort>();
            ENrm1Idx  = er.Read<ushort>();
            ENrm2Idx  = er.Read<ushort>();
            ENrm3Idx  = er.Read<ushort>();
            Attribute = er.Read<ushort>();
        }

        public override void Write(EndianBinaryWriterEx er)
        {
            er.WriteFx32(Height);
            er.Write(PosIdx);
            er.Write(FNrmIdx);
            er.Write(ENrm1Idx);
            er.Write(ENrm2Idx);
            er.Write(ENrm3Idx);
            er.Write(Attribute);
        }
    }
}