using HaroohiePals.KCollision;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.MarioKartToolbox.KCollision;
using OpenTK.Mathematics;

namespace HaroohiePals.MarioKartToolbox.Extensions
{
    internal static class MkdsKclExtensions
    {
        public static bool FindIntersection(this MkdsKcl kcl, Vector3d point, out Vector3d intersection)
            => kcl.FindIntersection(point, out intersection, out var _);

        public static bool FindIntersection(this MkdsKcl kcl, Vector3d point, out Vector3d intersection, out KclPrism prism)
            => kcl.FindIntersection(point, (0, -1, 0), out intersection, out prism);

        public static bool FindIntersection(this MkdsKcl kcl, Vector3d rayStart, Vector3d rayDir, out Vector3d intersection)
            => kcl.FindIntersection(rayStart, rayDir, out intersection, out var _);

        public static bool FindIntersection(this MkdsKcl kcl, Vector3d rayStart, Vector3d rayDir, out Vector3d intersection, out KclPrism kclPlane)
        {
            try
            {
                Vector3d closest = (0, 0, 0);
                double closestDist = double.MaxValue;
                bool found = false;
                kclPlane = null;
                foreach (var p in kcl.PrismData)
                {
                    MkdsKclPrismAttribute type = p.Attribute;

                    var t = p.ToTriangle(kcl);
                    var n = t.Normal;
                    double l = Vector3d.Dot(n, t.PointA - rayStart);
                    double nl = Vector3d.Dot(n, rayDir);
                    double tt = l / nl;

                    if (tt <= 0 || tt >= closestDist)
                        continue;

                    //see if point lies within the triangle
                    var point = rayStart + rayDir * tt;
                    if (double.IsNaN(point.X) || double.IsNaN(point.Y) || double.IsNaN(point.Z))
                        continue;

                    if (Vector3d.Dot(n, Vector3d.Cross(t.PointB - t.PointA, point - t.PointA)) <= 0 ||
                        Vector3d.Dot(n, Vector3d.Cross(t.PointC - t.PointB, point - t.PointB)) <= 0 ||
                        Vector3d.Dot(n, Vector3d.Cross(t.PointA - t.PointC, point - t.PointC)) <= 0)
                        continue;

                    closest = point;
                    closestDist = tt;
                    found = true;
                    kclPlane = p;
                }

                intersection = closest;

                return found;
            }
            catch
            {
                kclPlane = null;
                intersection = (0, 0, 0);
                return false;
            }
        }
    }
}
