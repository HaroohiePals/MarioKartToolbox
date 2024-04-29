using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdArea : NkmdSection<NkmdArea.AreaEntry>
    {
        public const uint AREASignature = 0x41455241;

        public NkmdArea()
            : base(AREASignature, true) { }

        public NkmdArea(EndianBinaryReaderEx er)
            : base(er, AREASignature, true) { }

        public class AreaEntry : NkmdSectionEntry
        {
            public AreaEntry() { }

            public AreaEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d LengthVector;

            [Fx32]
            public Vector3d XVector, YVector, ZVector;
            public short   Param0;
            public short   Param1;

            public short EnemyPointId;

            public MkdsAreaShapeType Shape;
            public sbyte         LinkedCame;
            public MkdsAreaType      AreaType;

            public ushort Unknown10; //unverified

            public byte Unknown11;

            public Vector3d[] GetCube()
            {
                var XVec = XVector * LengthVector.X * 100f;
                var YVec = YVector * LengthVector.Y * 100f;
                var ZVec = ZVector * LengthVector.Z * 100f;

                var BasePoint = Position - XVec / 2 - ZVec / 2;

                var XPoint = BasePoint + XVec;
                var YPoint = BasePoint + YVec;
                var ZPoint = BasePoint + ZVec;

                var XYPoint = BasePoint + XVec + YVec;
                var XZPoint = BasePoint + XVec + ZVec;
                var YZPoint = BasePoint + YVec + ZVec;

                var XYZPoint = BasePoint + XVec + YVec + ZVec;

                return new[] { BasePoint, XPoint, YPoint, ZPoint, XYPoint, XZPoint, YZPoint, XYZPoint };
            }
        }
    }
}