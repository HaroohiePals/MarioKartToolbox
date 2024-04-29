using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj
{
    public static class PathInterpolate
    {
        private static void HermiteSplineCalcCoef(double t, out double a1, out double a3, out double a4, out double a5)
        {
            a1 =  (1 - t) * (1 - t) * (1 - t);
            a3 =  3 * (1 - t) * t;
            a4 =  a3 * t;
            a5 =  t * t * t;
            a3 *= (1 - t);
        }

        private static void HermiteSplineCalcCoef2(double t, out double a1, out double a3, out double a4,
            out double tpow3, out double a6, out double a7, out double a8, out double a9)
        {
            a6    =  (1 - t) * (1 - t);
            a1    =  a6 * (1 - t);
            a9    =  t * t;
            tpow3 =  a9 * t;
            a3    =  3 * (1 - t) * t;
            a6    *= 3;
            a9    *= 3;
            a8    =  2 * a3;
            a7    =  a6 - a8;
            a8    -= a9;
            a6    =  -a6;
            a4    =  a3 * t;
            a3    *= (1 - t);
        }

        private static Vector3d ApplyCoefXYZ(Pathwalker.PwPathPart part, double c0, double c1, double c2, double c3)
            => part.P0 * c0 + part.P1 * c1 + part.P2 * c2 + part.P3 * c3;

        private static Vector3d ApplyCoefXZ(Pathwalker.PwPathPart part, double c0, double c1, double c2, double c3)
        {
            var result = part.P0 * c0 + part.P1 * c1 + part.P2 * c2 + part.P3 * c3;
            result.Y = 0;
            return result;
        }

        public static Vector3d InterpolateXYZ(Pathwalker.PwPathPart part, double t)
        {
            HermiteSplineCalcCoef(t, out double c0, out double c1, out double c2, out double c3);
            return ApplyCoefXYZ(part, c0, c1, c2, c3);
        }

        public static void pw_20D939C(Pathwalker.PwPathPart part, double a2, double a3, out Vector3d a4,
            out Vector3d a5)
        {
            HermiteSplineCalcCoef2(a2, out double coef1, out double coef2, out double coef3, out double coef4,
                out double a6, out double a7, out double a8, out double a9);
            a4 =  ApplyCoefXYZ(part, coef1, coef2, coef3, coef4);
            a5 =  ApplyCoefXYZ(part, a6, a7, a8, a9);
            a5 *= a3;
        }

        public static Vector3d InterpolateXZ(Pathwalker.PwPathPart part, double t)
        {
            HermiteSplineCalcCoef(t, out double c0, out double c1, out double c2, out double c3);
            return ApplyCoefXZ(part, c0, c1, c2, c3);
        }

        public static void pw_20D9270_XZ(Pathwalker.PwPathPart part, double a2, double a3, out Vector3d a4,
            out Vector3d a5)
        {
            HermiteSplineCalcCoef2(a2, out double coef1, out double coef2, out double coef3, out double coef4,
                out double a6, out double a7, out double a8, out double a9);
            a4 =  ApplyCoefXZ(part, coef1, coef2, coef3, coef4);
            a5 =  ApplyCoefXZ(part, a6, a7, a8, a9);
            a5 *= a3;
        }

        public static Vector3d InterpolateXZLinearY(Pathwalker.PwPathPart part, double t)
        {
            HermiteSplineCalcCoef(t, out double c0, out double c1, out double c2, out double c3);
            var result = ApplyCoefXZ(part, c0, c1, c2, c3);
            result.Y =  part.P0.Y * (1 - t);
            result.Y += part.P3.Y * t;
            return result;
        }
    }
}