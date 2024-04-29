using HaroohiePals.KCollision;
using OpenTK.Mathematics;

namespace HaroohiePals.KclViewer;

internal class OctreeNodeEx
{
    public OctreeNodeEx(OctreeNodeEx parent, in Vector3d minPos, int size)
    {
        Parent = parent;
        MinPos = minPos;
        Size   = size;
    }

    public readonly OctreeNodeEx Parent;

    public readonly Vector3d MinPos;
    public readonly int     Size;

    public OctreeNodeEx[,,] Children;

    public ushort[] Prisms;
    // public PrismList        PrismList;

    public bool IsLeaf => Prisms != null;

    private static OctreeNodeEx FromKclOctreeNode(OctreeNodeEx parent, KclOctreeNode srcNode, in Vector3d minPos, int size)
    {
        var result = new OctreeNodeEx(parent, minPos, size);
        if (!srcNode.IsLeaf)
        {
            result.Children = new OctreeNodeEx[2, 2, 2];

            size >>= 1;
            int idx = 0;
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        var subMinPos = minPos + (size * x, size * y, size * z);
                        result.Children[z, y, x]        = FromKclOctreeNode(result, srcNode.Children[idx], subMinPos, size);
                        idx++;
                    }
                }
            }
        }
        else
        {
            result.Prisms = srcNode.Prisms;
        }

        return result;
    }

    public static OctreeNodeEx[,,] FromKcl(Kcl kcl)
    {
        var octree = kcl.GetOctree();

        var rootNodes = new OctreeNodeEx[kcl.OctreeZNodes, kcl.OctreeYNodes, kcl.OctreeXNodes];

        int idx = 0;
        for (int z = 0; z < kcl.OctreeZNodes; z++)
        {
            for (int y = 0; y < kcl.OctreeYNodes; y++)
            {
                for (int x = 0; x < kcl.OctreeXNodes; x++)
                {
                    var minPos = kcl.AreaMinPos + (kcl.OctreeRootCubeSize * x, kcl.OctreeRootCubeSize * y, kcl.OctreeRootCubeSize * z);
                    rootNodes[z, y, x] = FromKclOctreeNode(null, octree.RootNodes[idx], minPos, kcl.OctreeRootCubeSize);
                    idx++;
                }
            }
        }

        return rootNodes;
    }
}