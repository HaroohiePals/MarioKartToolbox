using HaroohiePals.IO;

namespace HaroohiePals.KCollision.Formats
{
    public class Sm64dsKclPrism : KclPrism
    {
        public Sm64dsKclPrism(EndianBinaryReaderEx er)
        {
            Height    = er.Read<int>() / 65536.0;
            PosIdx    = er.Read<ushort>();
            FNrmIdx   = er.Read<ushort>();
            ENrm1Idx  = er.Read<ushort>();
            ENrm2Idx  = er.Read<ushort>();
            ENrm3Idx  = er.Read<ushort>();
            Attribute = er.Read<ushort>();
        }

        public override void Write(EndianBinaryWriterEx er)
        {
            er.Write((int)System.Math.Round(Height * 65536.0));
            er.Write(PosIdx);
            er.Write(FNrmIdx);
            er.Write(ENrm1Idx);
            er.Write(ENrm2Idx);
            er.Write(ENrm3Idx);
            er.Write(Attribute);
        }
    }
}