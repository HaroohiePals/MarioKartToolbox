using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class FloatKclPrism : KclPrism
    {
        public FloatKclPrism(EndianBinaryReaderEx er)
        {
            Height    = er.Read<float>();
            PosIdx    = er.Read<ushort>();
            FNrmIdx   = er.Read<ushort>();
            ENrm1Idx  = er.Read<ushort>();
            ENrm2Idx  = er.Read<ushort>();
            ENrm3Idx  = er.Read<ushort>();
            Attribute = er.Read<ushort>();
        }

        public override void Write(EndianBinaryWriterEx er)
        {
            er.Write((float)Height);
            er.Write(PosIdx);
            er.Write(FNrmIdx);
            er.Write(ENrm1Idx);
            er.Write(ENrm2Idx);
            er.Write(ENrm3Idx);
            er.Write(Attribute);
        }
    }
}
