using HaroohiePals.Mathematics;
using HaroohiePals.Nitro.Fx;
using OpenTK.Mathematics;

namespace HaroohiePals.KCollision
{
    public class KclPrismData
    {
        public double   Height;
        public Vector3d Position;
        public Vector3d FaceNormal;
        public Vector3d EdgeNormal1;
        public Vector3d EdgeNormal2;
        public Vector3d EdgeNormal3;
        public ushort   Attribute;

        public KclPrismData() { }

        public KclPrismData(Triangle triangle, ushort attribute)
        {
            Position    = triangle.PointA;
            FaceNormal  = triangle.Normal;
            EdgeNormal1 = -Vector3d.Cross(triangle.PointC - triangle.PointA, FaceNormal).Normalized();
            EdgeNormal2 = Vector3d.Cross(triangle.PointB - triangle.PointA, FaceNormal).Normalized();
            EdgeNormal3 = Vector3d.Cross(triangle.PointC - triangle.PointB, FaceNormal).Normalized();
            Height      = Vector3d.Dot(triangle.PointC - triangle.PointA, EdgeNormal3);
            Attribute   = attribute;
        }

        public Triangle ToTriangle()
        {
            var a      = Position;
            var crossA = Vector3d.Cross(EdgeNormal1, FaceNormal);
            var crossB = Vector3d.Cross(EdgeNormal2, FaceNormal);
            var b      = a + crossB * Height / Vector3d.Dot(crossB, EdgeNormal3);
            var c      = a + crossA * Height / Vector3d.Dot(crossA, EdgeNormal3);
            if (double.IsInfinity(b.X) || double.IsNaN(b.X) ||
                double.IsInfinity(b.Y) || double.IsNaN(b.Y) ||
                double.IsInfinity(b.Z) || double.IsNaN(b.Z))
                b = a;
            if (double.IsInfinity(c.X) || double.IsNaN(c.X) ||
                double.IsInfinity(c.Y) || double.IsNaN(c.Y) ||
                double.IsInfinity(c.Z) || double.IsNaN(c.Z))
                c = a;
            return new Triangle(a, b, c);
        }

        /// <summary>
        /// Generates a KCL prism from the given triangle and attribute and rounds all coordinates to valid fx32 values
        /// </summary>
        /// <param name="triangle">The triangle to convert</param>
        /// <param name="attribute">The attributes for the prism</param>
        /// <param name="minimizeError">When true all 3 rotations of the triangle vertices are tested to find the configuration
        /// that yields the smallest error when converted back to a triangle</param>
        /// <returns></returns>
        public static KclPrismData GenerateFx32(Triangle triangle, ushort attribute, bool minimizeError = false)
        {
            if (!minimizeError)
            {
                var prism = new KclPrismData(triangle, attribute);
                prism.Height      = Fx32Util.Fix(prism.Height);
                prism.Position    = Fx32Util.Fix(prism.Position);
                prism.FaceNormal  = Fx32Util.Fix(prism.FaceNormal);
                prism.EdgeNormal1 = Fx32Util.Fix(prism.EdgeNormal1);
                prism.EdgeNormal2 = Fx32Util.Fix(prism.EdgeNormal2);
                prism.EdgeNormal3 = Fx32Util.Fix(prism.EdgeNormal3);
                return prism;
            }

            var prismA = GenerateFx32(triangle, attribute);
            var triA   = prismA.ToTriangle();
            double errorA = (triangle.PointA - triA.PointA).LengthSquared +
                            (triangle.PointB - triA.PointB).LengthSquared +
                            (triangle.PointC - triA.PointC).LengthSquared;

            var prismB = GenerateFx32(new(triangle.PointB, triangle.PointC, triangle.PointA), attribute);
            var triB   = prismB.ToTriangle();
            double errorB = (triangle.PointB - triB.PointA).LengthSquared +
                            (triangle.PointC - triB.PointB).LengthSquared +
                            (triangle.PointA - triB.PointC).LengthSquared;

            var prismC = GenerateFx32(new(triangle.PointC, triangle.PointA, triangle.PointB), attribute);
            var triC   = prismC.ToTriangle();
            double errorC = (triangle.PointC - triC.PointA).LengthSquared +
                            (triangle.PointA - triC.PointB).LengthSquared +
                            (triangle.PointB - triC.PointC).LengthSquared;

            if (errorA <= errorB && errorA <= errorC)
                return prismA;

            if (errorB <= errorA && errorB <= errorC)
                return prismB;

            return prismC;
        }
    }
}