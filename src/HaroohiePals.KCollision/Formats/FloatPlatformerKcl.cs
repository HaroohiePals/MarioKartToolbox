using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.KCollision.Formats
{
    public class FloatPlatformerKcl : Kcl
    {
        protected FloatPlatformerKcl(Stream stream, Endianness endianness)
            : base(endianness)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness))
            {
                uint posDataOffset = er.Read<uint>();
                uint nrmDataOffset = er.Read<uint>();
                uint prismDataOffset = er.Read<uint>() + 0x10;
                uint blockDataOffset = er.Read<uint>();
                PrismThickness = er.Read<float>();
                AreaMinPos.X = er.Read<float>();
                AreaMinPos.Y = er.Read<float>();
                AreaMinPos.Z = er.Read<float>();
                AreaXWidthMask = er.Read<uint>();
                AreaYWidthMask = er.Read<uint>();
                AreaZWidthMask = er.Read<uint>();
                BlockWidthShift = er.Read<uint>();
                AreaXBlocksShift = er.Read<uint>();
                AreaXYBlocksShift = er.Read<uint>();

                er.BaseStream.Position = posDataOffset;
                PosData = new Vector3d[(nrmDataOffset - posDataOffset) / 0xC];
                for (int i = 0; i < PosData.Length; i++)
                {
                    PosData[i].X = er.Read<float>();
                    PosData[i].Y = er.Read<float>();
                    PosData[i].Z = er.Read<float>();
                }

                er.BaseStream.Position = nrmDataOffset;
                NrmData = new Vector3d[(prismDataOffset - nrmDataOffset) / 0xC];
                for (int i = 0; i < NrmData.Length; i++)
                {
                    NrmData[i].X = er.Read<float>();
                    NrmData[i].Y = er.Read<float>();
                    NrmData[i].Z = er.Read<float>();
                }

                er.BaseStream.Position = prismDataOffset;
                PrismData = new KclPrism[(blockDataOffset - prismDataOffset) / 0x10];
                for (int i = 0; i < PrismData.Length; i++)
                    PrismData[i] = new FloatKclPrism(er);

                er.BaseStream.Position = blockDataOffset;
                Octree = er.Read<byte>((int)(er.BaseStream.Length - blockDataOffset));
            }
        }

        public override void Write(Stream stream)
        {
            using (var er = new EndianBinaryWriterEx(stream, Endianness))
            {
                er.Write(0u);
                er.Write(0u);
                er.Write(0u);
                er.Write(0u);
                er.Write((float)PrismThickness);
                er.Write((float)AreaMinPos.X);
                er.Write((float)AreaMinPos.Y);
                er.Write((float)AreaMinPos.Z);
                er.Write(AreaXWidthMask);
                er.Write(AreaYWidthMask);
                er.Write(AreaZWidthMask);
                er.Write(BlockWidthShift);
                er.Write(AreaXBlocksShift);
                er.Write(AreaXYBlocksShift);

                er.WriteCurposRelative(0);
                foreach (var v in PosData)
                {
                    er.Write((float)v.X);
                    er.Write((float)v.Y);
                    er.Write((float)v.Z);
                }

                er.WriteCurposRelative(4);
                foreach (var v in NrmData)
                {
                    er.Write((float)v.X);
                    er.Write((float)v.Y);
                    er.Write((float)v.Z);
                }

                er.WriteCurposRelative(8, -0x10);
                foreach (var p in PrismData)
                    p.Write(er);

                er.WriteCurposRelative(0xC);
                er.Write(Octree);
            }
        }
    }
}