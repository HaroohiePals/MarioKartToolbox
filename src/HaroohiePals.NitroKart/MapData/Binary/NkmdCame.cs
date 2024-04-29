using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public partial class NkmdCame : NkmdSection<NkmdCame.CameEntry>
    {
        public const uint CAMESignature = 0x454D4143;

        public NkmdCame()
            : base(CAMESignature, true) { }

        public NkmdCame(EndianBinaryReaderEx er)
            : base(er, CAMESignature, true) { }

        public void UpdateSinCos()
        {
            foreach (var entry in Entries)
                entry.UpdateSinCos();
        }

        public class CameEntry : NkmdSectionEntry
        {
            public CameEntry()
            {
                FovBegin    = 30;
                FovEnd      = 30;
                LinkedRoute = -1;
                UpdateSinCos();
            }

            public CameEntry(EndianBinaryReaderEx er) => er.ReadObject(this);

            public override void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;

            [Fx32]
            public Vector3d Target1;

            [Fx32]
            public Vector3d Target2;

            public ushort FovBegin;

            [Fx16]
            public double FovBeginSin;

            [Fx16]
            public double FovBeginCos;

            public ushort FovEnd;

            [Fx16]
            public double FovEndSin;

            [Fx16]
            public double FovEndCos;

            [Fx16]
            public double          FovSpeed;
            public MkdsCameraType      Type;
            public short           LinkedRoute;

            [Fx16]
            public double          PathSpeed;

            [Fx16]
            public double          TargetSpeed;
            public short           Duration;
            public short           NextCamera;
            public MkdsCameIntroCamera FirstIntroCamera;

            public byte ShowAwardTrail;

            public void UpdateSinCos()
            {
                FovBeginSin = System.Math.Sin(MathHelper.DegreesToRadians(FovBegin));
                FovBeginCos = System.Math.Cos(MathHelper.DegreesToRadians(FovBegin));
                FovEndSin   = System.Math.Sin(MathHelper.DegreesToRadians(FovEnd));
                FovEndCos   = System.Math.Cos(MathHelper.DegreesToRadians(FovEnd));
            }
        }
    }
}