using HaroohiePals.IO;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.Mathematics;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace HaroohiePals.KCollision
{
    public abstract class KclPrism
    {
        public double Height;
        public ushort PosIdx;
        public ushort FNrmIdx;
        public ushort ENrm1Idx;
        public ushort ENrm2Idx;
        public ushort ENrm3Idx;
        public ushort Attribute;

        public abstract void Write(EndianBinaryWriterEx er);

        public KclPrismData GetData(Kcl kcl)
        {
            return new KclPrismData()
            {
                Height      = Height,
                Position    = kcl.PosData[PosIdx],
                FaceNormal  = kcl.NrmData[FNrmIdx],
                EdgeNormal1 = kcl.NrmData[ENrm1Idx],
                EdgeNormal2 = kcl.NrmData[ENrm2Idx],
                EdgeNormal3 = kcl.NrmData[ENrm3Idx],
                Attribute   = Attribute
            };
        }

        public Triangle ToTriangle(Kcl kcl)
            => GetData(kcl).ToTriangle();

        [Flags]
        private enum KCHitSphereClassification
        {
            None  = 0,
            Plane = 1 << 4,
            Edge  = (1 << 5),
            Edge1 = Edge | 0,
            Edge2 = Edge | 1,
            Edge3 = Edge | 2,
            Vtx   = (1 << 6),
            Vtx1  = Vtx | 0,
            Vtx2  = Vtx | 1,
            Vtx3  = Vtx | 2
        };

        private static double CalculateVertexSquareDistance(double dot1, double dot2, double dot3, in Vector3d a,
            in Vector3d b)
        {
            double aFactor = (dot1 * dot2 - dot3) / (dot1 * dot1 - 1.0);
            double bFactor = dot2 - (dot1 * aFactor);

            var result = a * aFactor + b * bFactor;

            return result.LengthSquared;
        }

        public bool IntersectSphere(Kcl kcl, in Vector3d position, double radius, double thickness)
        {
            var localPos = position - kcl.PosData[PosIdx];

            var    normal1 = kcl.NrmData[ENrm1Idx];
            double dotNrm1 = Vector3d.Dot(localPos, normal1);
            if (dotNrm1 >= radius)
                return false;

            var    normal2 = kcl.NrmData[ENrm2Idx];
            double dotNrm2 = Vector3d.Dot(localPos, normal2);
            if (dotNrm2 >= radius)
                return false;

            var    normal3 = kcl.NrmData[ENrm3Idx];
            double dotNrm3 = Vector3d.Dot(localPos, normal3) - Height;
            if (dotNrm3 >= radius)
                return false;

            double dotFaceNrm = Vector3d.Dot(localPos, kcl.NrmData[FNrmIdx]);

            if (radius - dotFaceNrm < 0)
                return false;

            if (this is Sm64dsKclPrism)
            {
                // thickness is simply penetration depth of the center point here, the sphere radius is not taken into account
                if (-dotFaceNrm > thickness)
                    return false;
            }
            else
            {
                if (radius - dotFaceNrm > thickness)
                    return false;
            }

            double sqRadius = radius * radius;
            double sqrt     = 0;

            Vector3d normalC = new(0);
            Vector3d normalD = new(0);

            double maxVal = dotNrm1;
            if (dotNrm2 >= maxVal)
                maxVal = dotNrm2;
            if (dotNrm3 >= maxVal)
                maxVal = dotNrm3;

            KCHitSphereClassification classification;

            double t1 = double.MaxValue;
            if (maxVal < 0)
            {
                // All three of our dot products are < 0. We're on the prism.
                classification = KCHitSphereClassification.Plane;
            }
            else if (maxVal == dotNrm1)
            {
                // dotNrm1 is the maximum. Pick other edge.
                if (dotNrm2 >= dotNrm3)
                {
                    // Our edges are 1 and 2.
                    normalC = normal1;
                    normalD = normal2;

                    t1 = Vector3d.Dot(normalC, normalD);
                    if (dotNrm2 >= t1 * dotNrm1)
                    {
                        classification = KCHitSphereClassification.Vtx1;
                        goto classified;
                    }
                }
                else
                {
                    // Our edges are 3 and 1.
                    normalC = normal3;
                    normalD = normal1;

                    t1 = Vector3d.Dot(normalC, normalD);
                    if (dotNrm3 >= t1 * dotNrm1)
                    {
                        classification = KCHitSphereClassification.Vtx3;
                        goto classified;
                    }
                }

                if (dotNrm1 > dotFaceNrm)
                    return false;

                classification = KCHitSphereClassification.Edge1;
                sqrt           = sqRadius - (dotNrm1 * dotNrm1);
            }
            else if (maxVal == dotNrm2)
            {
                // dotNrm2 is the maximum. Pick other edge.
                if (dotNrm1 >= dotNrm3)
                {
                    // Our edges are 1 and 2.
                    normalC = normal1;
                    normalD = normal2;

                    t1 = Vector3d.Dot(normalC, normalD);
                    if (dotNrm1 >= t1 * dotNrm2)
                    {
                        classification = KCHitSphereClassification.Vtx1;
                        goto classified;
                    }
                }
                else
                {
                    // Our edges are 2 and 3.
                    normalC = normal2;
                    normalD = normal3;

                    t1 = Vector3d.Dot(normalC, normalD);
                    if (dotNrm3 >= t1 * dotNrm2)
                    {
                        classification = KCHitSphereClassification.Vtx2;
                        goto classified;
                    }
                }

                if (dotNrm2 > dotFaceNrm)
                    return false;

                classification = KCHitSphereClassification.Edge2;
                sqrt           = sqRadius - (dotNrm2 * dotNrm2);
            }
            else
            {
                // dotNrm3 is the maximum. Pick other edge.
                if (dotNrm1 >= dotNrm2)
                {
                    // Our edges are 3 and 1.
                    normalC = normal3;
                    normalD = normal1;

                    t1 = Vector3d.Dot(normalC, normalD);
                    if (dotNrm1 >= t1 * dotNrm3)
                    {
                        classification = KCHitSphereClassification.Vtx3;
                        goto classified;
                    }
                }
                else
                {
                    // Our edges are 2 and 3.
                    normalC = normal2;
                    normalD = normal3;

                    t1 = Vector3d.Dot(normalC, normalD);
                    if (dotNrm2 >= t1 * dotNrm3)
                    {
                        classification = KCHitSphereClassification.Vtx2;
                        goto classified;
                    }
                }

                if (dotNrm3 > dotFaceNrm)
                    return false;

                classification = KCHitSphereClassification.Edge3;
                sqrt           = sqRadius - (dotNrm3 * dotNrm3);
            }

            classified:
            // At this point, everything should be classified.
            Debug.Assert(classification != KCHitSphereClassification.None);

            double distance;

            if ((classification & (KCHitSphereClassification.Edge | KCHitSphereClassification.Vtx)) != 0)
            {
                if ((classification & KCHitSphereClassification.Vtx) != 0)
                {
                    double dot2, dot3;
                    if (classification == KCHitSphereClassification.Vtx1)
                    {
                        dot2 = dotNrm2;
                        dot3 = dotNrm1;
                    }
                    else if (classification == KCHitSphereClassification.Vtx2)
                    {
                        dot2 = dotNrm3;
                        dot3 = dotNrm2;
                    }
                    else //if (dst.classification == KC_HIT_SPHERE_CLASS_VTX3)
                    {
                        dot2 = dotNrm1;
                        dot3 = dotNrm3;
                    }

                    double squareDistance = CalculateVertexSquareDistance(t1, dot2, dot3, normalC, normalD);

                    if (squareDistance > (dotFaceNrm * dotFaceNrm) || squareDistance >= sqRadius)
                        return false;

                    sqrt = sqRadius - squareDistance;
                }

                distance = System.Math.Sqrt(sqrt) - dotFaceNrm;
            }
            else //if (dst.classification == KC_HIT_SPHERE_CLASS_PLANE)
            {
                distance = radius - dotFaceNrm;
            }

            // dst.classification = classification;

            if (distance < 0)
                return false;

            if (this is Sm64dsKclPrism)
            {
                if (distance - radius > thickness)
                    return false;
            }
            else
            {
                if (distance > thickness)
                    return false;
            }

            return true;
        }
    }
}