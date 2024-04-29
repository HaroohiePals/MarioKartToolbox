using OpenTK.Mathematics;
using System;

namespace HaroohiePals.Mathematics
{
    public class Triangle
    {
        public Triangle(Vector3d a, Vector3d b, Vector3d c)
        {
            PointA = a;
            PointB = b;
            PointC = c;
        }

        public Vector3d PointA;
        public Vector3d PointB;
        public Vector3d PointC;

        public Vector3d Normal => Vector3d.Cross(PointB - PointA, PointC - PointA).Normalized();
        public double   Area   => Vector3d.Cross(PointB - PointA, PointC - PointA).Length * 0.5;

        public ref Vector3d this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return ref PointA;
                    case 1:
                        return ref PointB;
                    case 2:
                        return ref PointC;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}