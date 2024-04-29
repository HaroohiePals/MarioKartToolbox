using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d
{
    public static class G3dUtil
    {
        public static readonly byte[,] PivotUtil = new byte[9, 4]
        {
            { 4, 5, 7, 8 },
            { 3, 5, 6, 8 },
            { 3, 4, 6, 7 },

            { 1, 2, 7, 8 },
            { 0, 2, 6, 8 },
            { 0, 1, 6, 7 },

            { 1, 2, 4, 5 },
            { 0, 2, 3, 5 },
            { 0, 1, 3, 4 }
        };

        public static Matrix3d DecodePivotRotation(
            uint pivotIdx, bool pivotNeg, bool signRevC, bool signRevD, double a, double b)
        {
            var mtx = new Matrix3d();

            void set(int idx, double value) => mtx[idx / 3, idx % 3] = value;

            set((int)pivotIdx, pivotNeg ? -1f : 1f);

            set(PivotUtil[pivotIdx, 0], a);
            set(PivotUtil[pivotIdx, 1], b);

            set(PivotUtil[pivotIdx, 2], signRevC ? -b : b);

            set(PivotUtil[pivotIdx, 3], signRevD ? -a : a);

            return mtx;
        }
    }
}