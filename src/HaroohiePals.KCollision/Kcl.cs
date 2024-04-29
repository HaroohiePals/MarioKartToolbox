using HaroohiePals.IO;
using HaroohiePals.KCollision.Formats;
using OpenTK.Mathematics;
using System.Buffers.Binary;
using System.Numerics;

namespace HaroohiePals.KCollision
{
    public abstract class Kcl
    {
        public readonly Endianness Endianness;

        public double   PrismThickness;
        public Vector3d AreaMinPos;
        public uint     AreaXWidthMask;
        public uint     AreaYWidthMask;
        public uint     AreaZWidthMask;
        public uint     BlockWidthShift;
        public uint     AreaXBlocksShift;
        public uint     AreaXYBlocksShift;
        public double   SphereRadius;

        public Vector3d[] PosData;
        public Vector3d[] NrmData;
        public KclPrism[] PrismData;
        public byte[]     Octree;

        protected Kcl(Endianness endianness)
        {
            Endianness = endianness;
        }

        public byte[] Write()
        {
            using (var m = new MemoryStream())
            {
                Write(m);

                return m.ToArray();
            }
        }

        public abstract void Write(Stream stream);

        public KclOctree GetOctree() => new(this);

        public void SetOctree(KclOctree octree, KclOctree.CompressionMethod compressionMethod)
        {
            if (octree.XNodes == 0 || !BitOperations.IsPow2(octree.XNodes))
                throw new ArgumentException(nameof(octree.XNodes));

            if (octree.YNodes == 0 || !BitOperations.IsPow2(octree.YNodes))
                throw new ArgumentException(nameof(octree.YNodes));

            if (octree.ZNodes == 0 || !BitOperations.IsPow2(octree.ZNodes))
                throw new ArgumentException(nameof(octree.ZNodes));

            if (octree.RootCubeSize == 0 || !BitOperations.IsPow2(octree.RootCubeSize))
                throw new ArgumentException(nameof(octree.RootCubeSize));

            AreaMinPos      = octree.MinPos;
            BlockWidthShift = (uint)BitOperations.Log2((uint)octree.RootCubeSize);
            AreaXWidthMask  = 0xFFFFFFFFu << BitOperations.Log2((uint)(octree.XNodes * octree.RootCubeSize));
            AreaYWidthMask  = 0xFFFFFFFFu << BitOperations.Log2((uint)(octree.YNodes * octree.RootCubeSize));
            AreaZWidthMask  = 0xFFFFFFFFu << BitOperations.Log2((uint)(octree.ZNodes * octree.RootCubeSize));

            AreaXBlocksShift  = (uint)BitOperations.Log2((uint)octree.XNodes);
            AreaXYBlocksShift = AreaXBlocksShift + (uint)BitOperations.Log2((uint)octree.YNodes);

            PrismThickness = octree.PrismThickness;
            SphereRadius   = octree.SphereRadius;

            Octree = octree.Write(Endianness, compressionMethod);
        }

        public int OctreeXNodes       => (int)((~AreaXWidthMask >> (int)BlockWidthShift) + 1);
        public int OctreeYNodes       => (int)((~AreaYWidthMask >> (int)BlockWidthShift) + 1);
        public int OctreeZNodes       => (int)((~AreaZWidthMask >> (int)BlockWidthShift) + 1);
        public int OctreeRootCubeSize => 1 << (int)BlockWidthShift;

        public static Kcl Load(byte[] data)
            => Load(new MemoryStream(data, false));

        public static Kcl Load(Stream stream)
        {
            var tmp = new byte[4];
            if (stream.Read(tmp) < 4)
                throw new Exception("Invalid kcl file");

            // Platformer kcl files have a 0x38 byte header because they do not have a sphere radius field
            // DS and 3DS files are little endian, Wii files are big endian
            // DS files use fixed point values, 3DS and Wii files use floating point values

            Kcl kcl = null;
            if (IOUtil.ReadU32Le(tmp) == 0x38)
            {
                stream.Position = 0x10;
                if (stream.Read(tmp) < 4)
                    throw new Exception("Invalid kcl file");
                stream.Position = 0;
                if (IOUtil.ReadU32Le(tmp) > 100000000 && BinaryPrimitives.ReadSingleLittleEndian(tmp) < 10000)
                    kcl = new Sm3dlKcl(stream);
                else
                    kcl = new Sm64dsKcl(stream);
            }
            else if (IOUtil.ReadU32Le(tmp) == 0x3C)
            {
                stream.Position = 0x10;
                if (stream.Read(tmp) < 4)
                    throw new Exception("Invalid kcl file");
                stream.Position = 0;
                if (IOUtil.ReadU32Le(tmp) > 100000000 && BinaryPrimitives.ReadSingleLittleEndian(tmp) < 10000)
                    kcl = new Mk7Kcl(stream);
                else
                    kcl = new MkdsKcl(stream);
            }
            else if (IOUtil.ReadU32Be(tmp) == 0x38)
            {
                stream.Position = 0;
                kcl             = new SmgKcl(stream);
            }
            else if (IOUtil.ReadU32Be(tmp) == 0x3C)
            {
                stream.Position = 0;
                kcl             = new MkwiiKcl(stream);
            }
            else
                throw new Exception("Could not recognize kcl format");

            return kcl;
        }
    }
}