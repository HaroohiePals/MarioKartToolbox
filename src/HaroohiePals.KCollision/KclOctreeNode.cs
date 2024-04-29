using HaroohiePals.IO;

namespace HaroohiePals.KCollision
{
    public class KclOctreeNode
    {
        public KclOctreeNode() { }

        public KclOctreeNode(EndianBinaryReaderEx er)
        {
            uint data       = er.Read<uint>();
            bool isLeaf     = (data >> 31) == 1;
            uint dataOffset = data & 0x7FFFFFFF;
            long curPos     = er.JumpRelative(dataOffset);
            {
                if (isLeaf)
                {
                    // todo: how can we read this so we can always write it back in a matching way
                    er.Read<ushort>(); // skip initial zero
                    var prismList = new List<ushort>();
                    while (true)
                    {
                        ushort prism = er.Read<ushort>();
                        if (prism == 0)
                            break;
                        prismList.Add((ushort)(prism - 1));
                    }

                    Prisms = prismList.ToArray();
                }
                else
                {
                    er.BeginChunk();
                    Children = new KclOctreeNode[8];
                    for (int i = 0; i < 8; i++)
                        Children[i] = new KclOctreeNode(er);
                    er.EndChunk();
                }
            }
            er.BaseStream.Position = curPos;
        }

        public KclOctreeNode[] Children;
        public ushort[]        Prisms;

        public bool IsLeaf => Prisms != null;
    }
}