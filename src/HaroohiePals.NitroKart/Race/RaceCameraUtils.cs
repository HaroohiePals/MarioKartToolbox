using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.Race;

public static class RaceCameraUtils
{
    public static Matrix4 CalculateProjection(CameraMode mode, double fovSin, double fovCos, double frustumNear = 0.25 * 16f, double skyFrustumFar = 1600 * 16f, double aspectRatio = 1.3333, bool isMirror = false)
    {
        double frustumTop = frustumNear * (fovSin / fovCos);
        double frustumBottom = -frustumTop;
        double frustumLeft = (-frustumTop) * aspectRatio;
        double top = frustumTop;
        double bottom = frustumBottom;

        double dword_21656B8 = 0.15;

        switch (mode)
        {
            case CameraMode.DoubleTop:
                top *= 2;
                bottom = -top;
                frustumTop = top * (dword_21656B8 + 1);
                frustumBottom = top * dword_21656B8;
                break;
            case CameraMode.DoubleBottom:
                bottom *= 2;
                top = -bottom;
                frustumBottom = bottom * (dword_21656B8 + 1);
                frustumTop = bottom * dword_21656B8;
                break;
        }

        if (isMirror)
            frustumLeft = -frustumLeft;

        double frustumRight = -frustumLeft;

        return Matrix4.CreatePerspectiveOffCenter(
            (float)frustumLeft, (float)frustumRight,
            (float)frustumBottom, (float)frustumTop,
            (float)frustumNear, (float)skyFrustumFar);
    }
    public static void CalculateFovSinCos(ushort begin, ushort end, double progress, double speed, out double resultSin, out double resultCos)
    {
        double fovBeginSin = MathF.Sin(MathHelper.DegreesToRadians(begin));
        double fovBeginCos = MathF.Cos(MathHelper.DegreesToRadians(begin));

        resultSin = fovBeginSin;
        resultCos = fovBeginCos;

        if (speed > 0)
        {
            double fovEndSin = MathF.Sin(MathHelper.DegreesToRadians(end));
            double fovEndCos = MathF.Cos(MathHelper.DegreesToRadians(end));

            resultSin = ((fovEndSin - fovBeginSin) * progress) + fovBeginSin;
            resultCos = ((fovEndCos - fovBeginCos) * progress) + fovBeginCos;
        }
    }

    public static Vector3d CalculateTarget(Vector3d begin, Vector3d end, double progress, double speed)
    {
        var result = begin;

        if (speed > 0)
        {
            double x = ((end.X - begin.X) * progress) + begin.X;
            double y = ((end.Y - begin.Y) * progress) + begin.Y;
            double z = ((end.Z - begin.Z) * progress) + begin.Z;

            result = new(x, y, z);
        }

        return result;
    }
}
