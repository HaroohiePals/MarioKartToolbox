using HaroohiePals.IO;
using HaroohiePals.Nitro;
using OpenTK.Mathematics;

namespace HaroohiePals.KCollision.Formats
{
    public class MkdsKcl : Kcl
    {
        public static KclOctreeGenerator.Params DefaultOctreeParams => new()
        {
            MaxRootSize       = 4096,
            MinCubeSize       = 32,
            PrismThickness    = 30,
            SphereRadius      = 25,
            TargetPrismsPerLeaf = 10,
            SmartCompressionLimit = 25
        };

        public MkdsKcl() : base(Endianness.LittleEndian) { }

        public MkdsKcl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public MkdsKcl(Stream stream)
            : base(Endianness.LittleEndian)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                uint posDataOffset   = er.Read<uint>();
                uint nrmDataOffset   = er.Read<uint>();
                uint prismDataOffset = er.Read<uint>() + 0x10;
                uint blockDataOffset = er.Read<uint>();
                PrismThickness    = er.ReadFx32();
                AreaMinPos        = er.ReadVecFx32();
                AreaXWidthMask    = er.Read<uint>();
                AreaYWidthMask    = er.Read<uint>();
                AreaZWidthMask    = er.Read<uint>();
                BlockWidthShift   = er.Read<uint>();
                AreaXBlocksShift  = er.Read<uint>();
                AreaXYBlocksShift = er.Read<uint>();
                SphereRadius      = er.ReadFx32();

                er.BaseStream.Position = posDataOffset;
                PosData                = new Vector3d[(nrmDataOffset - posDataOffset) / 0xC];
                for (int i = 0; i < PosData.Length; i++)
                    PosData[i] = er.ReadVecFx32();

                er.BaseStream.Position = nrmDataOffset;
                NrmData                = new Vector3d[(prismDataOffset - nrmDataOffset) / 6];
                for (int i = 0; i < NrmData.Length; i++)
                    NrmData[i] = er.ReadVecFx16();

                er.BaseStream.Position = prismDataOffset;
                PrismData              = new KclPrism[(blockDataOffset - prismDataOffset) / 0x10];
                for (int i = 0; i < PrismData.Length; i++)
                    PrismData[i] = new Fx32KclPrism(er);

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
                er.WriteFx32(PrismThickness);
                er.WriteVecFx32(AreaMinPos);
                er.Write(AreaXWidthMask);
                er.Write(AreaYWidthMask);
                er.Write(AreaZWidthMask);
                er.Write(BlockWidthShift);
                er.Write(AreaXBlocksShift);
                er.Write(AreaXYBlocksShift);
                er.WriteFx32(SphereRadius);

                er.WriteCurposRelative(0);
                foreach (var v in PosData)
                    er.WriteVecFx32(v);

                er.WriteCurposRelative(4);
                foreach (var v in NrmData)
                    er.WriteVecFx16(v);
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