using HaroohiePals.Mathematics;
using OpenTK.Mathematics;
using System.Numerics;

namespace HaroohiePals.KCollision
{
    public static class KclOctreeGenerator
    {
        public class Params
        {
            public double PrismThickness;
            public double SphereRadius;
            public int    MaxRootSize;
            public int    MinCubeSize;
            public int    TargetPrismsPerLeaf;
            public bool   UseRoundCubePrismTest    = true;
            public bool   UseSmartDepthCompression = true;
            public int    SmartCompressionLimit    = -1;
            public int    SmartCompressionDelta    = 10;
        }

        private static (Vector3d min, Vector3d max) ComputeBounds(IEnumerable<Triangle> triangles, Params octreeParams)
        {
            var min = new Vector3d(double.MaxValue);
            var max = new Vector3d(double.MinValue);

            foreach (var tri in triangles)
            {
                min = Vector3d.ComponentMin(min, tri.PointA);
                min = Vector3d.ComponentMin(min, tri.PointB);
                min = Vector3d.ComponentMin(min, tri.PointC);

                max = Vector3d.ComponentMax(max, tri.PointA);
                max = Vector3d.ComponentMax(max, tri.PointB);
                max = Vector3d.ComponentMax(max, tri.PointC);

                var nrm = tri.Normal;

                var a2 = tri.PointA - nrm * octreeParams.PrismThickness;
                var b2 = tri.PointB - nrm * octreeParams.PrismThickness;
                var c2 = tri.PointC - nrm * octreeParams.PrismThickness;

                min = Vector3d.ComponentMin(min, a2);
                min = Vector3d.ComponentMin(min, b2);
                min = Vector3d.ComponentMin(min, c2);

                max = Vector3d.ComponentMax(max, a2);
                max = Vector3d.ComponentMax(max, b2);
                max = Vector3d.ComponentMax(max, c2);
            }

            min -= new Vector3d(octreeParams.SphereRadius);
            max += new Vector3d(octreeParams.SphereRadius);

            return (min, max);
        }

        public static KclOctree Generate(IList<Triangle> triangles, Params octreeParams)
        {
            var octree = new KclOctree();

            var (min, max) = ComputeBounds(triangles, octreeParams);

            var size = max - min;

            uint sizeX = BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(size.X));
            uint sizeY = BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(size.Y));
            uint sizeZ = BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(size.Z));

            uint minSize = Math.Min(Math.Min(sizeX, sizeY), sizeZ);

            octree.RootCubeSize =
                (int)BitOperations.RoundUpToPowerOf2((uint)Math.Min(minSize, octreeParams.MaxRootSize));
            octree.MinPos    = min;
            octree.XNodes    = (int)Math.Max(1, sizeX / octree.RootCubeSize);
            octree.YNodes    = (int)Math.Max(1, sizeY / octree.RootCubeSize);
            octree.ZNodes    = (int)Math.Max(1, sizeZ / octree.RootCubeSize);
            octree.RootNodes = new KclOctreeNode[octree.XNodes * octree.YNodes * octree.ZNodes];

            octree.PrismThickness = octreeParams.PrismThickness;
            octree.SphereRadius   = octreeParams.SphereRadius;

            var idTris = triangles.Select((tri, id) => ((ushort)id, tri)).ToArray();

            Parallel.For(0, octree.XNodes * octree.YNodes * octree.ZNodes, i =>
            {
                int x = i % octree.XNodes;
                int y = (i / octree.XNodes) % octree.YNodes;
                int z = (i / octree.XNodes) / octree.YNodes;

                octree.RootNodes[i] = GenerateNode(octreeParams, idTris,
                    min + octree.RootCubeSize * new Vector3d(x, y, z), octree.RootCubeSize);
            });

            // int i = 0;
            // for (int z = 0; z < octree.ZNodes; z++)
            // {
            //     for (int y = 0; y < octree.YNodes; y++)
            //     {
            //         for (int x = 0; x < octree.XNodes; x++)
            //         {
            //             octree.RootNodes[i++] = GenerateNode(octreeParams, idTris,
            //                 min + octree.RootCubeSize * new Vector3d(x, y, z), octree.RootCubeSize);
            //         }
            //     }
            // }

            Contract(octree);

            return octree;
        }

        private static void Contract(KclOctree octree)
        {
            var intermediateNodes = new List<KclOctreeNode>();
            var queue             = new Queue<KclOctreeNode>();
            foreach (var node in octree.RootNodes)
                queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Children != null)
                {
                    intermediateNodes.Add(node);

                    foreach (var subNode in node.Children)
                        queue.Enqueue(subNode);
                }
            }

            bool change = true;
            while (change)
            {
                change = false;
                for (int j = 0; j < intermediateNodes.Count; j++)
                {
                    var node = intermediateNodes[j];
                    if (!node.Children.All(n =>
                            n.IsLeaf && n.Prisms.ToHashSet().SetEquals(node.Children[0].Prisms)))
                        continue;
                    node.Prisms   = node.Children[0].Prisms;
                    node.Children = null;
                    intermediateNodes.Remove(node);
                    change = true;
                    j--;
                }
            }
        }

        private static KclOctreeNode GenerateNode(Params octreeParams,
            IEnumerable<(ushort id, Triangle triangle)> triangles, Vector3d minPos, int size)
        {
            var node = new KclOctreeNode();

            var contents = new List<(ushort, Triangle)>();
            if (octreeParams.UseRoundCubePrismTest)
            {
                foreach (var v in triangles)
                {
                    if (PrismRoundedBoxTest(octreeParams, v.triangle, minPos, size))
                        contents.Add(v);
                }
            }
            else
            {
                var centerPos = minPos + new Vector3d(size / 2.0);
                //estimate the range of the sphere and the thickness of the prisms by enlarging the box
                double newSize = size + Math.Max(octreeParams.SphereRadius, octreeParams.PrismThickness) * 2;
                var    newPos  = centerPos - new Vector3d(newSize / 2.0);

                foreach (var v in triangles)
                {
                    if (TriCubeOverlap(v.triangle, newPos, newSize))
                        contents.Add(v);
                }
            }

            if (size > octreeParams.MinCubeSize && contents.Count > octreeParams.TargetPrismsPerLeaf)
            {
                node.Prisms   = null;
                node.Children = new KclOctreeNode[8];

                int childSize = size >> 1;

                int i = 0;
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            node.Children[i++] = GenerateNode(octreeParams, contents,
                                minPos + childSize * new Vector3d(x, y, z), childSize);
                        }
                    }
                }

                if (octreeParams.UseSmartDepthCompression &&
                    (octreeParams.SmartCompressionLimit == -1 || contents.Count < octreeParams.SmartCompressionLimit) &&
                    node.Children.All(a => a.IsLeaf))
                {
                    // int min       = node.Children.Min(a => a.Prisms.Length);
                    // int max       = node.Children.Max(a => a.Prisms.Length);
                    int avg = node.Children.Select(a => a.Prisms.Length).Sum() / 8;
                    // int decreased = node.Children.Select(a => a.Prisms.Length < contents.Count ? 1 : 0).Sum();

                    if (contents.Count - avg < octreeParams.SmartCompressionDelta)
                    {
                        node.Children = null;
                        node.Prisms   = contents.Select(c => c.Item1).ToArray();
                    }

                    // if (max >= octreeParams.TargetPrismsPerLeaf * 2.5)
                    // {
                    //     if (contents.Count - max <= 5)
                    //     {
                    //         node.Children = null;
                    //         node.Prisms   = contents.Select(c => c.Item1).ToArray();
                    //     }
                    // }
                    // else if (max > octreeParams.TargetPrismsPerLeaf)
                    // {
                    //     if (max / (double)contents.Count >= octreeParams.SmartCompressionThreshold)//0.7
                    //     {
                    //         node.Children = null;
                    //         node.Prisms   = contents.Select(c => c.Item1).ToArray();
                    //     }
                    // }
                }
            }
            else
            {
                node.Children = null;
                node.Prisms   = contents.Select(c => c.Item1).ToArray();
            }

            return node;
        }

        private static bool AxisTest(double a1, double a2, double b1, double b2, double c1, double c2, double half)
        {
            double p = a1 * b1 + a2 * b2;
            double q = a1 * c1 + a2 * c2;
            double r = half * (Math.Abs(a1) + Math.Abs(a2));
            return Math.Min(p, q) > r || Math.Max(p, q) < -r;
        }

        //Based on this algorithm: http://jgt.akpeters.com/papers/AkenineMoller01/tribox.html
        private static bool TriCubeOverlap(Triangle t, Vector3d minPos, double size)
        {
            double halfSize = size / 2.0;
            var    position = minPos + new Vector3d(halfSize);

            var v0 = t.PointA - position;
            var v1 = t.PointB - position;
            var v2 = t.PointC - position;

            if (Math.Min(Math.Min(v0.X, v1.X), v2.X) > halfSize ||
                Math.Max(Math.Max(v0.X, v1.X), v2.X) < -halfSize)
                return false;
            if (Math.Min(Math.Min(v0.Y, v1.Y), v2.Y) > halfSize ||
                Math.Max(Math.Max(v0.Y, v1.Y), v2.Y) < -halfSize)
                return false;
            if (Math.Min(Math.Min(v0.Z, v1.Z), v2.Z) > halfSize ||
                Math.Max(Math.Max(v0.Z, v1.Z), v2.Z) < -halfSize)
                return false;

            var    nrm = t.Normal;
            double d   = Vector3d.Dot(nrm, v0);
            double r   = halfSize * (Math.Abs(nrm.X) + Math.Abs(nrm.Y) + Math.Abs(nrm.Z));

            if (d > r || d < -r)
                return false;

            var e = v1 - v0;
            if (AxisTest(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, halfSize))
                return false;
            if (AxisTest(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, halfSize))
                return false;
            if (AxisTest(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, halfSize))
                return false;

            e = v2 - v1;
            if (AxisTest(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, halfSize))
                return false;
            if (AxisTest(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, halfSize))
                return false;
            if (AxisTest(e.Y, -e.X, v0.X, v0.Y, v1.X, v1.Y, halfSize))
                return false;

            e = v0 - v2;
            if (AxisTest(e.Z, -e.Y, v0.Y, v0.Z, v1.Y, v1.Z, halfSize))
                return false;
            if (AxisTest(-e.Z, e.X, v0.X, v0.Z, v1.X, v1.Z, halfSize))
                return false;
            if (AxisTest(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, halfSize))
                return false;

            return true;
        }

        private static readonly Vector3d[] BoxVertices =
        {
            // Front
            (-0.5, -0.5, 0.5),
            (0.5, -0.5, 0.5),
            (0.5, 0.5, 0.5),
            (-0.5, 0.5, 0.5),
            (-0.5, -0.5, -0.5),
            (0.5, -0.5, -0.5),
            (0.5, 0.5, -0.5),
            (-0.5, 0.5, -0.5)
        };

        private static readonly uint[] BoxIndices =
        {
            // front
            0, 1, 2,
            2, 3, 0,
            // right
            1, 5, 6,
            6, 2, 1,
            // back
            7, 6, 5,
            5, 4, 7,
            // left
            4, 0, 3,
            3, 7, 4,
            // bottom
            4, 5, 1,
            1, 0, 4,
            // top
            3, 2, 6,
            6, 7, 3
        };

        private static bool PointInAACube(Vector3d point, Vector3d minPos, double size)
        {
            return point.X >= minPos.X && point.X <= minPos.X + size &&
                   point.Y >= minPos.Y && point.Y <= minPos.Y + size &&
                   point.Z >= minPos.Z && point.Z <= minPos.Z + size;
        }

        private static void SegPoints(out Vector3d vec,
            out Vector3d x, out Vector3d y, // closest points
            Vector3d p, Vector3d a,         // seg 1 origin, vector
            Vector3d q, Vector3d b)         // seg 2 origin, vector
        {
            Vector3d tmp;

            var    t     = q - p;
            double aDotA = Vector3d.Dot(a, a);
            double bDotB = Vector3d.Dot(b, b);
            double aDotB = Vector3d.Dot(a, b);
            double aDotT = Vector3d.Dot(a, t);
            double bDotT = Vector3d.Dot(b, t);

            // t parameterizes ray P,A 
            // u parameterizes ray Q,B 

            double t2, u;

            // compute t for the closest point on ray P,A to
            // ray Q,B

            double denom = aDotA * bDotB - aDotB * aDotB;

            t2 = (aDotT * bDotB - bDotT * aDotB) / denom;

            // clamp result so t is on the segment P,A

            if ((t2 < 0) || double.IsNaN(t2))
                t2 = 0;
            else if (t2 > 1)
                t2 = 1;

            // find u for point on ray Q,B closest to point at t

            u = (t2 * aDotB - bDotT) / bDotB;

            // if u is on segment Q,B, t and u correspond to 
            // closest points, otherwise, clamp u, recompute and
            // clamp t 

            if ((u <= 0) || double.IsNaN(u))
            {
                y = q;

                t2 = aDotT / aDotA;

                if ((t2 <= 0) || double.IsNaN(t2))
                {
                    x   = p;
                    vec = q - p;
                }
                else if (t2 >= 1)
                {
                    x   = p + a;
                    vec = q - x;
                }
                else
                {
                    x   = p + a * t2;
                    tmp = Vector3d.Cross(t, a);
                    vec = Vector3d.Cross(a, tmp);
                }
            }
            else if (u >= 1)
            {
                y = q + b;

                t2 = (aDotB + aDotT) / aDotA;

                if ((t2 <= 0) || double.IsNaN(t2))
                {
                    x   = p;
                    vec = y - p;
                }
                else if (t2 >= 1)
                {
                    x   = p + a;
                    vec = y - x;
                }
                else
                {
                    x   = p + a * t2;
                    t   = y - p;
                    tmp = Vector3d.Cross(t, a);
                    vec = Vector3d.Cross(a, tmp);
                }
            }
            else
            {
                y = q + b * u;

                if ((t2 <= 0) || double.IsNaN(t2))
                {
                    x   = p;
                    tmp = Vector3d.Cross(t, b);
                    vec = Vector3d.Cross(b, tmp);
                }
                else if (t2 >= 1)
                {
                    x   = p + a;
                    t   = q - x;
                    tmp = Vector3d.Cross(t, b);
                    vec = Vector3d.Cross(b, tmp);
                }
                else
                {
                    x   = p + a * t2;
                    vec = Vector3d.Cross(a, b);
                    if (Vector3d.Dot(vec, t) < 0)
                        vec *= -1;
                }
            }
        }

        private static double TriDist(out Vector3d p, out Vector3d q, Triangle s, Triangle t)
        {
            p = new(0);
            q = new(0);

            // Compute vectors along the 6 sides

            var      sv = new Vector3d[3];
            var      tv = new Vector3d[3];
            Vector3d vec;

            sv[0] = s.PointB - s.PointA;
            sv[1] = s.PointC - s.PointB;
            sv[2] = s.PointA - s.PointC;

            tv[0] = t.PointB - t.PointA;
            tv[1] = t.PointC - t.PointB;
            tv[2] = t.PointA - t.PointC;

            // For each edge pair, the vector connecting the closest points 
            // of the edges defines a slab (parallel planes at head and tail
            // enclose the slab). If we can show that the off-edge vertex of 
            // each triangle is outside of the slab, then the closest points
            // of the edges are the closest points for the triangles.
            // Even if these tests fail, it may be helpful to know the closest
            // points found, and whether the triangles were shown disjoint

            Vector3d v;
            Vector3d z;
            Vector3d minP = new(0), minQ = new(0);
            double   mindd;
            bool     shownDisjoint = false;

            mindd = double
                .MaxValue; // (S.PointA - T.PointA).Dot(S.PointA - T.PointA) + 1; // Set first minimum safely high

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // Find closest points on edges i & j, plus the 
                    // vector (and distance squared) between these points

                    SegPoints(out vec, out p, out q, s[i], sv[i], t[j], tv[j]);

                    v = q - p;
                    double dd = Vector3d.Dot(v, v);

                    // Verify this closest point pair only if the distance 
                    // squared is less than the minimum found thus far.

                    if (dd <= mindd)
                    {
                        minP  = p;
                        minQ  = q;
                        mindd = dd;

                        z = s[(i + 2) % 3] - p;
                        double a = Vector3d.Dot(z, vec);
                        z = t[(j + 2) % 3] - q;
                        double b = Vector3d.Dot(z, vec);

                        if ((a <= 0) && (b >= 0))
                            return Math.Sqrt(dd);

                        double p2 = Vector3d.Dot(v, vec);

                        if (a < 0)
                            a = 0;
                        if (b > 0)
                            b = 0;
                        if ((p2 - a + b) > 0)
                            shownDisjoint = true;
                    }
                }
            }

            // No edge pairs contained the closest points.  
            // either:
            // 1. one of the closest points is a vertex, and the
            //    other point is interior to a face.
            // 2. the triangles are overlapping.
            // 3. an edge of one triangle is parallel to the other's face. If
            //    cases 1 and 2 are not true, then the closest points from the 9
            //    edge pairs checks above can be taken as closest points for the
            //    triangles.
            // 4. possibly, the triangles were degenerate.  When the 
            //    triangle points are nearly colinear or coincident, one 
            //    of above tests might fail even though the edges tested
            //    contain the closest points.

            // First check for case 1

            Vector3d sn;
            double   snl;
            sn  = Vector3d.Cross(sv[0], sv[1]); // Compute normal to S triangle
            snl = Vector3d.Dot(sn, sn);         // Compute square of length of normal

            // If cross product is long enough,

            if (snl > 1e-15)
            {
                // Get projection lengths of T points

                var tp = new double[3];

                v     = s.PointA - t.PointA;
                tp[0] = Vector3d.Dot(v, sn);

                v     = s.PointA - t.PointB;
                tp[1] = Vector3d.Dot(v, sn);

                v     = s.PointA - t.PointC;
                tp[2] = Vector3d.Dot(v, sn);

                // If Sn is a separating direction,
                // find point with smallest projection

                int point = -1;
                if ((tp[0] > 0) && (tp[1] > 0) && (tp[2] > 0))
                {
                    if (tp[0] < tp[1])
                        point = 0;
                    else
                        point = 1;
                    if (tp[2] < tp[point])
                        point = 2;
                }
                else if ((tp[0] < 0) && (tp[1] < 0) && (tp[2] < 0))
                {
                    if (tp[0] > tp[1])
                        point = 0;
                    else
                        point = 1;
                    if (tp[2] > tp[point])
                        point = 2;
                }

                // If Sn is a separating direction, 

                if (point >= 0)
                {
                    shownDisjoint = true;

                    // Test whether the point found, when projected onto the 
                    // other triangle, lies within the face.

                    v = t[point] - s.PointA;
                    z = Vector3d.Cross(sn, sv[0]);
                    if (Vector3d.Dot(v, z) > 0)
                    {
                        v = t[point] - s.PointB;
                        z = Vector3d.Cross(sn, sv[1]);
                        if (Vector3d.Dot(v, z) > 0)
                        {
                            v = t[point] - s.PointC;
                            z = Vector3d.Cross(sn, sv[2]);
                            if (Vector3d.Dot(v, z) > 0)
                            {
                                // T[point] passed the test - it's a closest point for 
                                // the T triangle; the other point is on the face of S

                                p = t[point] + sn * (tp[point] / snl);
                                q = t[point];
                                return (p - q).Length;
                            }
                        }
                    }
                }
            }

            Vector3d tn;
            double   tnl;
            tn  = Vector3d.Cross(tv[0], tv[1]);
            tnl = Vector3d.Dot(tn, tn);

            if (tnl > 1e-15)
            {
                var sp = new double[3];

                v     = t.PointA - s.PointA;
                sp[0] = Vector3d.Dot(v, tn);

                v     = t.PointA - s.PointB;
                sp[1] = Vector3d.Dot(v, tn);

                v     = t.PointA - s.PointC;
                sp[2] = Vector3d.Dot(v, tn);

                int point = -1;
                if ((sp[0] > 0) && (sp[1] > 0) && (sp[2] > 0))
                {
                    if (sp[0] < sp[1])
                        point = 0;
                    else
                        point = 1;
                    if (sp[2] < sp[point])
                        point = 2;
                }
                else if ((sp[0] < 0) && (sp[1] < 0) && (sp[2] < 0))
                {
                    if (sp[0] > sp[1])
                        point = 0;
                    else
                        point = 1;
                    if (sp[2] > sp[point])
                        point = 2;
                }

                if (point >= 0)
                {
                    shownDisjoint = true;

                    v = s[point] - t.PointA;
                    z = Vector3d.Cross(tn, tv[0]);
                    if (Vector3d.Dot(v, z) > 0)
                    {
                        v = s[point] - t.PointB;
                        z = Vector3d.Cross(tn, tv[1]);
                        if (Vector3d.Dot(v, z) > 0)
                        {
                            v = s[point] - t.PointC;
                            z = Vector3d.Cross(tn, tv[2]);
                            if (Vector3d.Dot(v, z) > 0)
                            {
                                p = s[point];
                                q = s[point] + tn * (sp[point] / tnl);
                                return (p - q).Length;
                            }
                        }
                    }
                }
            }

            // Case 1 can't be shown.
            // If one of these tests showed the triangles disjoint,
            // we assume case 3 or 4, otherwise we conclude case 2, 
            // that the triangles overlap.

            if (shownDisjoint)
            {
                p = minP;
                q = minQ;
                return Math.Sqrt(mindd);
            }
            else
                return 0;
        }

        private static bool TriangleRoundedBoxTest(Params octreeParams, Triangle t, Vector3d minPos, double size)
        {
            var    center  = minPos + new Vector3d(size / 2.0);
            double minDist = double.MaxValue;
            for (int j = 0; j < BoxIndices.Length; j += 3)
            {
                var boxTri = new Triangle(BoxVertices[BoxIndices[j]] * size + center,
                    BoxVertices[BoxIndices[j + 1]] * size + center,
                    BoxVertices[BoxIndices[j + 2]] * size + center);
                double dist = TriDist(out var p, out var q, t, boxTri);

                if (dist == 0 || PointInAACube(p, minPos, size))
                    return true;

                minDist = Math.Min(minDist, dist);
            }

            return minDist <= octreeParams.SphereRadius;
        }

        private static bool PrismRoundedBoxTest(Params octreeParams, Triangle triangle, Vector3d minPos, double size)
        {
            var nrm = triangle.Normal;
            var triangle2 = new Triangle(
                triangle.PointA - nrm * octreeParams.PrismThickness,
                triangle.PointB - nrm * octreeParams.PrismThickness,
                triangle.PointC - nrm * octreeParams.PrismThickness
            );

            bool overlap = TriangleRoundedBoxTest(octreeParams, triangle, minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams, triangle2, minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams,
                    new Triangle(triangle.PointA, triangle2.PointA, triangle2.PointB), minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams,
                    new Triangle(triangle.PointA, triangle2.PointB, triangle.PointB), minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams,
                    new Triangle(triangle.PointB, triangle2.PointB, triangle2.PointC), minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams,
                    new Triangle(triangle.PointB, triangle2.PointC, triangle.PointC), minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams,
                    new Triangle(triangle.PointC, triangle2.PointC, triangle2.PointA), minPos, size);

            if (!overlap)
                overlap = TriangleRoundedBoxTest(octreeParams,
                    new Triangle(triangle.PointC, triangle2.PointA, triangle.PointA), minPos, size);

            return overlap;
        }
    }
}