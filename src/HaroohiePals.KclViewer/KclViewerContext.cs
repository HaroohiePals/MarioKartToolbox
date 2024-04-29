using HaroohiePals.KCollision;
using HaroohiePals.Mathematics;
using OpenTK.Mathematics;

namespace HaroohiePals.KclViewer;

internal sealed class KclViewerContext
{
    public readonly Kcl       Collision;
    public readonly KclOctree Octree;

    public readonly OctreeNodeEx[,,] OctreeRootNodes;

    public readonly int              LeafCount;
    public readonly float[]          LeafHistogram;
    public readonly OctreeNodeEx[][] LeafHistogramEntries;

    public readonly int MinCubeSize;

    public readonly Triangle[] Triangles;

    // public readonly (OctreeNodeEx nodeA, OctreeNodeEx nodeB, OctreeNodeEx nodeC) NodesForTriVertices;

    public KclViewerContext(Kcl collision)
    {
        Collision       = collision;
        Octree          = Collision.GetOctree();
        OctreeRootNodes = OctreeNodeEx.FromKcl(Collision);
        Triangles       = collision.PrismData.Select(p => p.ToTriangle(Collision)).ToArray();

        var histogram        = new List<int>();
        var histogramEntries = new List<List<OctreeNodeEx>>();
        int leafCount        = 0;

        int minSize = collision.OctreeRootCubeSize;

        var queue = new Queue<OctreeNodeEx>(OctreeRootNodes.Cast<OctreeNodeEx>());
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node.IsLeaf)
            {
                if (node.Prisms.Length >= histogram.Count)
                {
                    int count = node.Prisms.Length - histogram.Count + 1;
                    histogram.AddRange(Enumerable.Repeat(0, count));
                    for (int i = 0; i < count; i++)
                        histogramEntries.Add(new List<OctreeNodeEx>());
                }

                histogram[node.Prisms.Length]++;
                histogramEntries[node.Prisms.Length].Add(node);
                leafCount++;

                if (node.Size < minSize)
                    minSize = node.Size;
            }
            else
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            queue.Enqueue(node.Children[z, y, x]);
                        }
                    }
                }
            }
        }

        LeafCount            = leafCount;
        LeafHistogram        = histogram.Select(c => (float)c).ToArray();
        LeafHistogramEntries = histogramEntries.Select(e => e.ToArray()).ToArray();

        MinCubeSize = minSize;
    }

    public OctreeNodeEx FindNodeForPoint(Vector3d point)
    {
      var localPos = point - Collision.AreaMinPos;
      int intX    = (int)localPos.X ;
      int intY    = (int)localPos.Y ;
      int intZ    = (int)localPos.Z;

      if ((intX & Collision.AreaXWidthMask) != 0 ||
          (intY & Collision.AreaYWidthMask) != 0 ||
          (intZ & Collision.AreaZWidthMask) != 0)
          return null;

      int shift = (int)Collision.BlockWidthShift;

      var node = OctreeRootNodes[intZ >> shift, intY >> shift, intX >> shift];

      while (!node.IsLeaf)
      {
          shift--;
          node = node.Children[(intZ >> shift) & 1, (intY >> shift) & 1, (intX >> shift) & 1];
      }

      return node;
    }
}