using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdObji : NkmdSection<NkmdObji.ObjiEntry>
    {
        public const uint OBJISignature = 0x494A424F;

        public NkmdObji()
            : base(OBJISignature, false) { }

        public NkmdObji(EndianBinaryReaderEx er)
            : base(er, OBJISignature, false) { }

        public class ObjiEntry : NkmdSectionEntry
        {
            public ObjiEntry()
            {
                Position             = new Vector3d(0, 0, 0);
                Rotation             = new Vector3d(0, 0, 0);
                Scale                = new Vector3d(1, 1, 1);
                ObjectId             = (ushort)MkdsMapObjectId.Itembox; //itembox
                PathId               = -1;
                Settings             = new short[7];
                EnableInactivePeriod = false;
                TTVisible            = true;
            }

            public ObjiEntry(EndianBinaryReaderEx er)
            {
                er.ReadObject(this);

                ushort visibility = er.Read<ushort>();

                //Parse clip area IDs
                int digits = visibility % 10000;
                ClipAreaIds = new[]
                {
                    (ushort)(digits % 10),
                    (ushort)(digits % 100 / 10),
                    (ushort)(digits % 1000 / 100),
                    (ushort)(digits % 10000 / 1000)
                };

                //Parse enable inactive period
                EnableInactivePeriod = visibility / 10000 == 1;

                TTVisible = er.Read<uint>() != 0;
            }

            public override void Write(EndianBinaryWriterEx er)
            {
                er.WriteObject(this);

                ushort visibility = (ushort)(EnableInactivePeriod ? 10000 : 0);
                if (ClipAreaIds == null || ClipAreaIds.Length != 4)
                    throw new Exception();

                visibility = (ushort)
                    (ClipAreaIds[0] % 10 +
                     ClipAreaIds[1] % 10 * 10 +
                     ClipAreaIds[2] % 10 * 100 +
                     ClipAreaIds[3] % 10 * 1000 + visibility / 10000 * 10000);

                er.Write(visibility);

                er.Write((uint)(TTVisible ? 1 : 0));
            }

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;

            [Fx32]
            public Vector3d Scale;

            public ushort ObjectId;
            public short  PathId;

            [ArraySize(7)]
            public short[] Settings;

            [Ignore]
            public bool EnableInactivePeriod;

            [Ignore]
            public ushort[] ClipAreaIds = new ushort[4];

            [Ignore]
            public bool TTVisible;
        }
    }
}