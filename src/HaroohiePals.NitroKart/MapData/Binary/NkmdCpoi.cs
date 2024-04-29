using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdCpoi : NkmdSection<NkmdCpoi.CpoiEntry>
    {
        public const uint CPOISignature = 0x494F5043;

        public NkmdCpoi()
            : base(CPOISignature, true) { }

        public NkmdCpoi(EndianBinaryReaderEx er)
            : base(er, CPOISignature, true) { }

        public void UpdateDistancesAndSinCos()
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].UpdateDistance(i + 1 < Entries.Count ? Entries[i + 1] : null);
                Entries[i].UpdateSinCos();
            }
        }

        public class CpoiEntry : NkmdSectionEntry
        {
            public CpoiEntry()
            {
                UpdateSinCos();
                GotoSection  = -1;
                StartSection = -1;
                KeyPointId   = -1;
            }

            public CpoiEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector2d Point1;

            [Fx32]
            public Vector2d Point2;

            [Fx32]
            public double Sin;

            [Fx32]
            public double Cos;

            [Fx32]
            public double Distance;

            public short GotoSection;

            public short StartSection;

            public short KeyPointId;

            public byte RespawnId;

            public byte Flags;

            public void UpdateSinCos()
            {
                double a = System.Math.Atan((Point1.Y - Point2.Y) / (Point1.X - Point2.X));
                Sin = System.Math.Sin(System.Math.Abs(a));
                Cos = System.Math.Cos(System.Math.Abs(a));
                if (Point1.Y - Point2.Y > 0)
                    Sin = -Sin;
                if (Point1.X - Point2.X < 0)
                    Cos = -Cos;
            }

            public void UpdateDistance(CpoiEntry next)
            {
                if (GotoSection != -1 || next == null)
                {
                    Distance = -1;
                    return;
                }

                Distance = ((next.Point1 + next.Point2) / 2 - (Point1 + Point2) / 2).Length;
                //Distance = ((next.Point1 + next.Point2) / 2 - (Point1 + Point2) / 2).Length;
                /*if (GotoSection != -1)
                {
                    Distance = -1;
                    return;
                }
                UpdateSinCos();
                next.UpdateSinCos();
                Distance =
                    next.Cosine * next.Point2.Y + next.Sine * next.Point2.X/* + 0.5f/ -
                    Cosine * Point1.Y - Sine * Point1.X/* + 0.5f/;*/
            }
        }
    }
}