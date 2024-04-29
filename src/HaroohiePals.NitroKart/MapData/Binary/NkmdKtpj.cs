using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapData.Binary
{
    public class NkmdKtpj : NkmdSection<NkmdKtpj.KtpjEntry>
    {
        public const uint KTPJSignature = 0x4A50544B;

        private ushort _version = 37;

        public NkmdKtpj(ushort version = 37) 
            : base(KTPJSignature, true)
        {
            _version = version;
        }

        public NkmdKtpj(EndianBinaryReaderEx er, ushort version)
        {
            _version = version;
            uint signature = er.Read<uint>();
            if (signature != KTPJSignature)
                throw new SignatureNotCorrectException(signature, KTPJSignature, er.BaseStream.Position - 4);
            uint nrEntries = er.Read<uint>();
            for (int i = 0; i < nrEntries; i++)
                Entries.Add(new KtpjEntry(er, version));
        }

        public override void Write(EndianBinaryWriterEx er)
        {
            er.Write(KTPJSignature);
            er.Write((uint)Entries.Count);
            foreach (var entry in Entries)
                entry.Write(er, _version);
        }

        public class KtpjEntry : NkmdSectionEntry
        {
            public KtpjEntry()
            {
                Position     = new Vector3d(0, 0, 0);
                Rotation     = new Vector3d(0, 0, 0);
                EnemyPointID = 0;
                ItemPointID  = 0;
                Index        = 0;
            }

            public KtpjEntry(EndianBinaryReaderEx er, ushort version)
            {
                Position = er.ReadVecFx32();
                Rotation = er.ReadVecFx32();
                //if (version <= 34)
                //{
                //    //It is a vector in this version
                //    double yangle = (double)System.Math.Atan2(Rotation.Z, Rotation.X);
                //    Rotation = new Vector3(0, MathUtil.RadToDeg(yangle), 0);
                //}

                EnemyPointID = er.Read<short>();
                ItemPointID  = er.Read<short>();
                if (version >= 32)
                    Index = er.Read<int>();
            }

            public override void Write(EndianBinaryWriterEx er) => Write(er, 37);

            public void Write(EndianBinaryWriterEx er, ushort version)
            {
                er.WriteVecFx32(Position);
                //if (version <= 34)
                //{
                //    er.WriteFx32((double)System.Math.Cos(MathUtil.DegToRad(Rotation.Y)));
                //    er.WriteFx32(0);
                //    er.WriteFx32((double)System.Math.Sin(MathUtil.DegToRad(Rotation.Y)));
                //}
                //else
                    er.WriteVecFx32(Rotation);

                er.Write(EnemyPointID);
                er.Write(ItemPointID);
                if (version >= 32)
                    er.Write(Index);
            }

            [Fx32]
            public Vector3d Position;

            [Fx32]
            public Vector3d Rotation;

            public short EnemyPointID;
            public short ItemPointID;
            public int   Index;
        }
    }
}