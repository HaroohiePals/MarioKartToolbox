using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.KCollision.Formats
{
    public class Sm64dsKcl : Kcl
    {
        public Sm64dsKcl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Sm64dsKcl(Stream stream)
            : base(Endianness.LittleEndian)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                uint posDataOffset   = er.Read<uint>();
                uint nrmDataOffset   = er.Read<uint>();
                uint prismDataOffset = er.Read<uint>() + 0x10;
                uint blockDataOffset = er.Read<uint>();
                PrismThickness    = er.Read<int>() / 65536.0;
                AreaMinPos.X      = er.Read<int>() / 64.0;
                AreaMinPos.Y      = er.Read<int>() / 64.0;
                AreaMinPos.Z      = er.Read<int>() / 64.0;
                AreaXWidthMask    = er.Read<uint>();
                AreaYWidthMask    = er.Read<uint>();
                AreaZWidthMask    = er.Read<uint>();
                BlockWidthShift   = er.Read<uint>();
                AreaXBlocksShift  = er.Read<uint>();
                AreaXYBlocksShift = er.Read<uint>();

                er.BaseStream.Position = posDataOffset;
                PosData                = new Vector3d[(nrmDataOffset - posDataOffset) / 0xC];
                for (int i = 0; i < PosData.Length; i++)
                {
                    PosData[i].X = er.Read<int>() / 64.0;
                    PosData[i].Y = er.Read<int>() / 64.0;
                    PosData[i].Z = er.Read<int>() / 64.0;
                }

                er.BaseStream.Position = nrmDataOffset;
                NrmData                = new Vector3d[(prismDataOffset - nrmDataOffset) / 6];
                for (int i = 0; i < NrmData.Length; i++)
                {
                    NrmData[i].X = er.Read<short>() / 1024.0;
                    NrmData[i].Y = er.Read<short>() / 1024.0;
                    NrmData[i].Z = er.Read<short>() / 1024.0;
                }

                er.BaseStream.Position = prismDataOffset;
                PrismData              = new KclPrism[(blockDataOffset - prismDataOffset) / 0x10];
                for (int i = 0; i < PrismData.Length; i++)
                    PrismData[i] = new Sm64dsKclPrism(er);

                er.BaseStream.Position = blockDataOffset;
                Octree                 = er.Read<byte>((int)(er.BaseStream.Length - blockDataOffset));
            }
        }

        public override void Write(Stream stream)
        {
            using (var er = new EndianBinaryWriterEx(stream, Endianness.LittleEndian))
            {
                er.Write(0u);
                er.Write(0u);
                er.Write(0u);
                er.Write(0u);
                er.Write((int)Math.Round(PrismThickness * 65536.0));
                er.Write((int)Math.Round(AreaMinPos.X * 64.0));
                er.Write((int)Math.Round(AreaMinPos.Y * 64.0));
                er.Write((int)Math.Round(AreaMinPos.Z * 64.0));
                er.Write(AreaXWidthMask);
                er.Write(AreaYWidthMask);
                er.Write(AreaZWidthMask);
                er.Write(BlockWidthShift);
                er.Write(AreaXBlocksShift);
                er.Write(AreaXYBlocksShift);

                er.WriteCurposRelative(0);
                foreach (var v in PosData)
                {
                    er.Write((int)Math.Round(v.X * 64.0));
                    er.Write((int)Math.Round(v.Y * 64.0));
                    er.Write((int)Math.Round(v.Z * 64.0));
                }

                er.WriteCurposRelative(4);
                foreach (var v in NrmData)
                {
                    er.Write((short)Math.Round(v.X * 1024.0));
                    er.Write((short)Math.Round(v.Y * 1024.0));
                    er.Write((short)Math.Round(v.Z * 1024.0));
                }

                er.WritePadding(4);

                er.WriteCurposRelative(8, -0x10);
                foreach (var p in PrismData)
                    p.Write(er);

                er.WriteCurposRelative(0xC);
                er.Write(Octree);
            }
        }
    }
}